using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockserverCommands
{
    [JsonObject(MemberSerialization.OptIn)]
    class UndoCell
    {
        //Sets all for the edit cell's json property
        [JsonProperty(PropertyName = "selectCell")]
        private string requestType;

        public UndoCell(string requestType)
        {
            this.requestType = requestType;
        }
    }
}
