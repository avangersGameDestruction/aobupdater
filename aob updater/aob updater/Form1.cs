using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;
using Newtonsoft.Json;
using System.Threading;

namespace MaplestoryAoBUpdater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        enum AoBType
        {
            Address,
            Pointer,
            FollowCall
        }

        Mem memlib;

        private void AttachToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new attachForm())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    memlib = form.returnMem;
                }
            }
        }

        private async void UpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (memlib == null)
            {

                MessageBox.Show("Please attach to a process before updating.");
            }
            else
            {
                tsslStatus.Text = "Running";

                foreach (ListViewItem items in lvAddy.Items)
                {
                    tsslCompletionRate.Text = ((items.Index + 1) / (float)lvAddy.Items.Count).ToString("P0");

                    long myAddress = (await memlib.AoBScan(items.SubItems[3].Text)).FirstOrDefault();

                    if (myAddress == 0)
                    {
                        items.SubItems[1].Text = "0xERROR";
                    }
                    else if ((Enum.TryParse(items.SubItems[2].Text, true, out AoBType result) == true))
                    {
                        switch (result)
                        {
                            case AoBType.Address:
                                items.SubItems[1].Text = "0x" + myAddress.ToString("X8");
                                break;

                            case AoBType.FollowCall:
                                items.SubItems[1].Text = "0x" + GetCall(myAddress).ToString("X8");
                                break;

                            case AoBType.Pointer:
                                items.SubItems[1].Text = "0x" + GetPointer(myAddress).ToString("X8");
                                break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unsupported type");
                    }
                }
                tsslStatus.Text = "Not Running";
            }
        }

        private int GetPointer(long address)
        {
            long actualByte = (address + 0x2);
            byte[] result = memlib.readBytes(actualByte.ToString("X"), 4);
            if (result != null)
            {
                return BitConverter.ToInt32(result, 0);
            }
            return 0;
        }

        private long GetCall(long address)
        {
            long callAddress = address;
            address += 1;
            byte[] result = memlib.readBytes(address.ToString("X"), 4);
            if (result != null)
            {
                return BitConverter.ToInt32(result, 0) + callAddress + 5;
            }
            return 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Populate it with Enum data
            cbType.DataSource = Enum.GetValues(typeof(AoBType));

            if (File.Exists("AoB.json"))
            {
                string existingJson = File.ReadAllText("AoB.json");

                var list = JsonConvert.DeserializeObject<List<Address>>(existingJson);

                foreach (var items in list)
                {
                    AddToList(items.Name, "0xNULL", Enum.GetName(typeof(AoBType), items.Type), items.AoB, items.Comments);
                }
            }
        }

        private void AddToList(string name, string address, string type, string aob, string comments)
        {
            string[] arr = new string[5];
            ListViewItem itm;
            //add items to ListView
            arr[0] = name; //Name
            arr[1] = address; //Address
            arr[2] = type; //Type
            arr[3] = aob; //AoB
            arr[4] = comments; //Comments
            itm = new ListViewItem(arr);
            lvAddy.Items.Add(itm);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Name Field cannot be empty.");
            }
            else if (string.IsNullOrEmpty(cbType.Text))
            {
                MessageBox.Show("Please select the type you want to extract.");
            }
            else if (string.IsNullOrEmpty(txtAoB.Text))
            {
                MessageBox.Show("AoB Field cannot be empty.");
            }
            else
            {
                if (File.Exists("AoB.json"))
                {
                    //Write to AoB JSON and add it to the listview
                    string existingJson = File.ReadAllText("AoB.json");

                    var list = JsonConvert.DeserializeObject<List<Address>>(existingJson);

                    list.Add(new Address { Name = txtName.Text, Type = cbType.SelectedIndex, AoB = txtAoB.Text, Comments = txtComments.Text });

                    var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
                    File.WriteAllText("AoB.json", convertedJson);
                }
                else
                {
                    //Create AoB JSON and add to listview
                    List<Address> addy = new List<Address>
                    {
                        new Address {Name=txtName.Text, Type=cbType.SelectedIndex, AoB=txtAoB.Text, Comments=txtComments.Text }
                    };
                    string json = JsonConvert.SerializeObject(addy, Formatting.Indented);
                    File.WriteAllText("AoB.json", json);
                }
                AddToList(txtName.Text, "0xNULL", cbType.Text, txtAoB.Text, txtComments.Text); //0xNULL as a placeholder
            }
        }

        private void LoadAoBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    lvAddy.Items.Clear();
                    string existingJson = File.ReadAllText(openFileDialog1.FileName);
                    var list = JsonConvert.DeserializeObject<List<Address>>(existingJson);

                    foreach (var items in list)
                    {
                        AddToList(items.Name, "0xNULL", Enum.GetName(typeof(AoBType), items.Type), items.AoB, items.Comments);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void SaveAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (ListViewItem item in lvAddy.Items)
                {
                    File.AppendAllText(saveFileDialog1.FileName, "const unsigned int " + item.SubItems[0].Text + " = " + item.SubItems[1].Text + ";" + Environment.NewLine);
                }
            }
        }

        private void saveJSONToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                List<Address> addresses = new List<Address>();
                foreach (ListViewItem item in lvAddy.Items)
                {
                    addresses.Add(new Address { Name=item.SubItems[0].Text, Type= (int)Enum.Parse(typeof(AoBType), item.SubItems[2].Text),  AoB=item.SubItems[3].Text, Comments=item.SubItems[4].Text });
                }
                string json = JsonConvert.SerializeObject(addresses, Formatting.Indented);
                File.WriteAllText(saveFileDialog1.FileName, json);
            }
        }
    }
}
