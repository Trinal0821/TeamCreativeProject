using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommands
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UndoCell
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "undoCell")]
        private string requestType;

        public UndoCell(string requestType)
        {
            this.requestType = requestType;
        }
    }
}
