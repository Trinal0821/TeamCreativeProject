using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommands
{
    [JsonObject(MemberSerialization.OptIn)]
   public class SelectCell
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "selectCell")]
        public string requestType;
        [JsonProperty]
        public string cellName;

        public SelectCell(string requestType, string cellName)
        {
            this.requestType = requestType;
            this.cellName = cellName;
        }

        public string getJsonString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonConvert.SerializeObject("requestType:selectCell") + "\n");
            sb.Append(JsonConvert.SerializeObject("cellName:" + cellName) + "\n");

            return sb.ToString();

        }
    }
}
