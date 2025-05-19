using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities;
using AstroHandlerService.ReturnEntities;

namespace AstroHandlerService.Services
{
    public interface ISwissEphemerisService
    {
        public PosInfo GetData(DateTime dateTime);

        public Dictionary<DateTime, PosInfo> GetData(DateTime startTime, DateTime endTime, TimeSpan interval);

        public Dictionary<DateTime, List<AspectInfo>> ProcessAspects0(PosInfo birthDayInfo, List<PosInfo> transitList);
        public List<PlanetMain> ProcessAspects(PosInfo birthDayInfo, List<PosInfo> transitList);
        public void FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval);

        public (double[] Info, int Result) GetDataTest(DateTime dateTime);
    }
}
