using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities.Db;
using AstroHandlerService.Services;

namespace AstroHandlerService.Providers
{
    internal class EphemerisProvider : IEphemerisProvider
    {
        public ApplicationContext _appContext { get; set; }

        public EphemerisProvider(
            ApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public void AddEphemeris(IEnumerable<EphemerisDb> ephemeris)
        {
            _appContext.EphemerisSet.AddRange(ephemeris);
            _appContext.SaveChanges();

            // получаем объекты из бд и выводим на консоль
            var ephs = _appContext.EphemerisSet.ToList();


            //Console.WriteLine("Users list:");
            foreach (EphemerisDb eph in ephs)
            {
                //Console.WriteLine($"{eph.Id}.{eph.DateTime} - {eph.SunDegrees}");
            }

        }
    }
}
