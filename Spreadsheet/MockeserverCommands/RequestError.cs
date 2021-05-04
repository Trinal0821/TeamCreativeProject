using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MockserverCommands
{
    [JsonObject(MemberSerialization.OptIn)]

    public class RequestError
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "cellSelected")]
        private string messageType;
        [JsonProperty]
        public string cellName;
        [JsonProperty]
        public string message;

        public RequestError(string messageType, string cellName, string message)
        {
            this.messageType = messageType;
            this.cellName = cellName;
            this.message = message;
        }

        public string getJsonString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonConvert.SerializeObject("messageType: cellSelected") + "\n");
            sb.Append(JsonConvert.SerializeObject("cellName:" + cellName) + "\n");
            sb.Append(JsonConvert.SerializeObject("message:" + message) + "\n");

            return sb.ToString();
        }
    }
}
