using AstroHandlerService.Enums;

namespace AstroHandlerService.Entities
{
    public class ChartInfo : Dictionary<PlanetEnum, PlanetInfo>
    {
        public DateTime DateTime { get; set; }

        public ChartInfo(DateTime dateTime) : base()
        {
            DateTime = dateTime;
        }
    }
}
