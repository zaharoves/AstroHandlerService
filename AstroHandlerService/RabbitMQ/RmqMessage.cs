using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroHandlerService.RMQ
{
    [ProtoContract]
    public class RmqMessage
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public DateTime BirthDateTime { get; set; }

        [ProtoMember(3)]
        public DateTime StartDateTime { get; set; }

        [ProtoMember(4)]
        public DateTime EndDateTime { get; set; }
    }
}
