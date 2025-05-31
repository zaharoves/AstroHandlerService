using AstroHandlerService.Entities;
using AstroHandlerService.Enums;

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
