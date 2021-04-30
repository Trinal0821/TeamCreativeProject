using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommands
{
    [JsonObject(MemberSerialization.OptIn)]

    public class EditCell
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "editCell")]
        public string requestType;
        [JsonProperty]
        public string cellName;
        [JsonProperty]
        public string contents;

        public EditCell(string requestType, string cellName, string cellContents)
        {
            this.requestType = requestType;
            this.cellName = cellName;
            this.contents = cellContents;
        }

        public string getJsonString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonConvert.SerializeObject("requestType:requestType") + "\n");
            sb.Append(JsonConvert.SerializeObject("cellName:" + cellName) + "\n");
            sb.Append(JsonConvert.SerializeObject("cellContents:" + contents) + "\n");

            return sb.ToString();

        }
      
    }
}
