using Newtonsoft.Json;
using System.Text;

namespace MockserverCommands
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CellSelected
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "cellSelected")]
        private string messageType;
        [JsonProperty]
        public string cellName;
        [JsonProperty]
        public int selector;
        [JsonProperty]
        public string selectorName;

        public CellSelected(string messageType, string cellName, int selector, string selectorName)
        {
            this.messageType = messageType;
            this.cellName = cellName;
            this.selector = selector;
            this.selectorName = selectorName;
        }

        public string getJsonString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonConvert.SerializeObject("messageType: cellSelected") + "\n");
            sb.Append(JsonConvert.SerializeObject("cellName:" + cellName) + "\n");
            sb.Append(JsonConvert.SerializeObject("selector:" + selector) + "\n");
            sb.Append(JsonConvert.SerializeObject("selectorName:" + selectorName) + "\n");

            return sb.ToString();

        }
    }
}
