using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroHandlerService.RMQ
{
    [ProtoContract]
    public class RmqMessage2
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public string MessageText { get; set; }
    }
}
