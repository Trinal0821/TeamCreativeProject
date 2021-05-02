using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockserverCommands
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RequestError
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "requestError")]
        private string messageType;
        [JsonProperty]
        public string message;

        public RequestError(string messageType, string message)
        {
            this.messageType = messageType;
            this.message = message;
        }
    }
}
