using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroHandlerService.Entities.Db
{
    public class EphemerisDb
    {
        public EphemerisDb()
        {
        }

        public long? Id { get; set; }
        public DateTime? DateTime { get; set; }

        public double? SunAngles { get; set; }
        public double? MoonAngles { get; set; }
        public double? MercuryAngles { get; set; }
        public double? VenusAngles { get; set; }
        public double? MarsAngles { get; set; }
        public double? JupiterAngles { get; set; }
        public double? SaturnAngles { get; set; }
        public double? UranAngles { get; set; }
        public double? NeptuneAngles { get; set; }
        public double? PlutoAngles { get; set; }
    }
}
