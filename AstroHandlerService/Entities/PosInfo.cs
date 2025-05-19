using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities.Enums;

namespace AstroHandlerService.Entities
{
    public class PosInfo : Dictionary<PlanetEnum, PlanetPosInfo>
    {
        public DateTime DateTime { get; set; }

        public PosInfo(DateTime dateTime) : base()
        {
            DateTime = dateTime;
        }
    }
}
