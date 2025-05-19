using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroHandlerService.Configurations
{
    public class AstroConfig
    {
        public string SwissEphPath { get; set; }
        public Orbs Orbs { get; set; } = new Orbs();
    }
}
