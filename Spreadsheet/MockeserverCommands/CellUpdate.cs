using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockserverCommands
{
    [JsonObject(MemberSerialization.OptIn)]

    public class CellUpdate
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "cellUpdated")]
        private string messageType;
        [JsonProperty]
        public string cellName;
        [JsonProperty]
        public string contents;

        public CellUpdate(string messageType, string cellName, string cellContents)
        {
            this.messageType = messageType;
            this.cellName = cellName;
            this.contents = cellContents;
        }

        public string getJsonString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonConvert.SerializeObject("messageType: cellUpdated") + "\n");
            sb.Append(JsonConvert.SerializeObject("cellName:" + cellName) + "\n");
            sb.Append(JsonConvert.SerializeObject("cellContents:" + contents) + "\n");

            return sb.ToString();

        }

    }
}
