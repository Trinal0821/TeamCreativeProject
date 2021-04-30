using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommands
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RevertCell
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "revertCell")]
        private string requestType;
        [JsonProperty]
        private string cellName;

        public RevertCell(string requestType, string cellName)
        {
            this.requestType = requestType;
            this.cellName = cellName;
        }
    }
}
