using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaplestoryAoBUpdater
{
    [JsonObject(MemberSerialization.OptIn)]
    class Address
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public int Type { get; set; }
        [JsonProperty]
        public string AoB { get; set; }
        [JsonProperty]
        public string Comments { get; set; }
    }
}
