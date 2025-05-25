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
        public string MessageId { get; set; }

        [ProtoMember(2)]
        public DatePickerData DatePickerData { get; set; }
    }

    [ProtoContract]
    public class DatePickerData
    {
        [ProtoMember(1)]
        public DateTime? DateTime { get; set; }

        [ProtoMember(2)]
        public TimeSpan GmtOffset { get; set; }

        [ProtoMember(3)]
        public int MinYearInterval { get; set; }

        [ProtoIgnore]
        public bool IsSaveCommand { get; set; }

        [ProtoIgnore]
        public bool IsCancelCommand { get; set; }

        [ProtoIgnore]
        public bool IsChangeCommand { get; set; }

        public override string ToString()
        {
            if (!DateTime.HasValue)
            {
                return string.Empty;
            }

            var gmtSign = GmtOffset >= TimeSpan.Zero ? "+" : "-";

            return $"{DateTime.Value.ToString("d MMMM yyyyг. HH:mm")} [GMT{gmtSign}{Math.Abs(GmtOffset.Hours)}]";
        }
    }
}
