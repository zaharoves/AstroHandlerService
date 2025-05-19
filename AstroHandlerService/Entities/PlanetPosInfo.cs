using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities.Enums;

namespace AstroHandlerService.Entities
{
    public class PlanetPosInfo
    {
        public PlanetEnum Planet { get; set; }
        public double AbsolutDegrees { get; set; }

        public ZodiacEnum Zodiac { get; set; }
        public double ZodiacDegrees { get; set; }

        public PlanetPosInfo(PlanetEnum planetEnum)
        {
            Planet = planetEnum;
        }
    }
}
