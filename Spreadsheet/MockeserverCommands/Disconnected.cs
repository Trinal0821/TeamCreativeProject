using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockserverCommands
{
    [JsonObject(MemberSerialization.OptIn)]
   public class Disconnected
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "disconnected")]
        private string messageType;
        [JsonProperty]
        private int user;

        public Disconnected(string messageType, int user)
        {
            this.messageType = messageType;
            this.user = user;
        }
    }
}
