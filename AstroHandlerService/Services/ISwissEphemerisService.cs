using AstroHandlerService.Entities;
using AstroHandlerService.Enums;
using AstroHandlerService.ReturnEntities;

namespace AstroHandlerService.Services
{
    public interface ISwissEphemerisService
    {
        ChartInfo GetChartData(DateTime dateTime);

        Dictionary<DateTime, ChartInfo> GetData(DateTime startTime, DateTime endTime, TimeSpan interval);

        PlanetInfo GetDayInfo(PlanetEnum planetEnum, DateTime dateTime, out string error);

        List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo transitChart);

        List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo transitChart, params PlanetEnum[] transitPlanets);


        List<AspectInfo> GetMoonAspects(ChartInfo natalChart, DateTime startUtcDate, DateTime endUtcDate);

        Dictionary<DateTime, List<AspectInfo>> ProcessAspects0(ChartInfo birthDayInfo, List<ChartInfo> transitList);

        List<PlanetMain> ProcessAspects(ChartInfo birthDayInfo, List<ChartInfo> transitList);

        void FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval);

        (double[] Info, int Result) GetDataTest(DateTime dateTime);
    }
}
