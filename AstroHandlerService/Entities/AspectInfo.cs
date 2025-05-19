using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities.Enums;

namespace AstroHandlerService.Entities
{
    public class AspectInfo
    {
        public PlanetPosInfo NatalPlanet { get; set; }
        public PlanetPosInfo TransitPlanet { get; set; }
        public AspectEnum Aspect { get; set; }

        public AspectInfo(PlanetPosInfo natalPlanet, PlanetPosInfo transitPlanet, AspectEnum aspect)
        {
            NatalPlanet = natalPlanet;
            TransitPlanet = transitPlanet;
            Aspect = aspect;
        }
    }
}
