using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities;
using AstroHandlerService.Entities.Enums;

namespace AstroHandlerService.ReturnEntities
{
    public class PlanetMain
    {
        public PlanetEnum Planet { get; }

        public Dictionary<DateTime, List<AspectInfo>> Aspects = new Dictionary<DateTime, List<AspectInfo>>();

        public PlanetMain(PlanetEnum planet) 
        {
            Planet = planet;
        }
    }
}
