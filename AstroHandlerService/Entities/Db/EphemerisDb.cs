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

        public double? SunDegrees { get; set; }
        public double? MoonDegrees { get; set; }
        public double? MercuryDegrees { get; set; }
        public double? VenusDegrees { get; set; }
        public double? MarsDegrees { get; set; }
        public double? JupiterDegrees { get; set; }
        public double? SaturnDegrees { get; set; }
        public double? UranDegrees { get; set; }
        public double? NeptuneDegrees { get; set; }
        public double? PlutoDegrees { get; set; }
    }
}
