using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities.Enums;

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
