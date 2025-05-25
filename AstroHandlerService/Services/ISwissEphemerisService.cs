using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities;
using AstroHandlerService.Entities.Enums;
using AstroHandlerService.ReturnEntities;

namespace AstroHandlerService.Services
{
    public interface ISwissEphemerisService
    {
        ChartInfo GetChartData(DateTime dateTime);

        Dictionary<DateTime, ChartInfo> GetData(DateTime startTime, DateTime endTime, TimeSpan interval);

        PlanetInfo GetDayInfo(PlanetEnum planetEnum, DateTime dateTime, out string error);

        List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo transitChart);
        List<AspectInfo> GetMoonAspects(PlanetInfo moonInfo, List<ChartInfo> transitChartList);

        Dictionary<DateTime, List<AspectInfo>> ProcessAspects0(ChartInfo birthDayInfo, List<ChartInfo> transitList);

        List<PlanetMain> ProcessAspects(ChartInfo birthDayInfo, List<ChartInfo> transitList);

        void FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval);

        (double[] Info, int Result) GetDataTest(DateTime dateTime);
    }
}
