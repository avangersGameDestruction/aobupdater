using Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaplestoryAoBUpdater
{
    public partial class attachForm : Form
    {
        public attachForm()
        {
            InitializeComponent();
        }

        public Mem returnMem { get; set; }

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            if (lvProcess.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a process.");
            }
            else
            {
                //SelectedIndices = Position
                ListViewItem item = lvProcess.SelectedItems[0];
                int intPID = int.Parse(item.SubItems[1].Text, System.Globalization.NumberStyles.HexNumber);
                returnMem = new Mem(); //Init Mem
                returnMem.OpenProcess(intPID);
                this.DialogResult = DialogResult.OK;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void AttachForm_Load(object sender, EventArgs e)
        {
            var allProcesses = Process.GetProcesses();
            foreach (var items in allProcesses)
            {
                string[] arr = new string[2];
                arr[0] = items.ProcessName;
                arr[1] = items.Id.ToString("X8");
                ListViewItem itm;
                itm = new ListViewItem(arr);
                lvProcess.Items.Add(itm);
            }
        }
    }
}
