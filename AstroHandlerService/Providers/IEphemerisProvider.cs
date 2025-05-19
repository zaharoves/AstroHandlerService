using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities.Db;

namespace AstroHandlerService.Providers
{
    public interface IEphemerisProvider
    {
        public void AddEphemeris(IEnumerable<EphemerisDb> ephemeris);
    }
}
