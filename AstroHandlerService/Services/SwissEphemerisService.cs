using Microsoft.Extensions.Options;
using SwissEphNet;
using AstroHandlerService.Configurations;
using AstroHandlerService.Entities;
using AstroHandlerService.Enums;
using AstroHandlerService.Providers;
using AstroHandlerService.ReturnEntities;
using AstroHandlerService.Db.Entities;

namespace AstroHandlerService.Services
{
    public class SwissEphemerisService : ISwissEphemerisService
    {
        // SwissEph.SE_GREG_CAL - указывает на григорианский календарь.
        private readonly int _swissCalendarType = SwissEph.SE_GREG_CAL;
        //SwissEph.SEFLG_TRUEPOS - обеспечивает получение истинного положения, без учета аберрации.
        //SwissEph.SEFLG_SWIEPH - гарантирует использование наиболее точных эфемерид, если они доступны.
        private readonly int _swissPositionType = SwissEph.SEFLG_TRUEPOS | SwissEph.SEFLG_SWIEPH | SwissEph.SEFLG_SPEED;

        private SwissEph _swissEphemeris = new SwissEph();
        private readonly IEphemerisProvider _ephemerisProvider;

        private readonly Dictionary<PlanetEnum, AspectOrbDictionary> _planetsOrbDictionary;

        public SwissEphemerisService(
            IOptions<AstroConfig> astroConfiguration,
            IEphemerisProvider ephemerisProvider)
        {
            //Установка пути к файлам эфемерид
            //SwissEph sw = new SwissEph("C\\:\\ephe");
            _swissEphemeris.swe_set_ephe_path(@"C:\SWEPH\EPHE");
            //_swissEphemeris.swe_set_jpl_file("C:\\SWEPH\\EPHE");

            _planetsOrbDictionary = new Dictionary<PlanetEnum, AspectOrbDictionary>()
            {
                {PlanetEnum.Sun, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Sun)},
                {PlanetEnum.Moon, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Moon)},

                {PlanetEnum.Mercury, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Mercury)},
                {PlanetEnum.Venus, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Venus)},
                {PlanetEnum.Mars, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Mars)},

                {PlanetEnum.Jupiter, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Jupiter)},
                {PlanetEnum.Saturn, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Saturn)},

                {PlanetEnum.Uran, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Uran)},
                {PlanetEnum.Neptune, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Neptune)},
                {PlanetEnum.Pluto, new AspectOrbDictionary(astroConfiguration.Value.Orbs.Pluto)}
            };

            _ephemerisProvider = ephemerisProvider;
        }

        public (double[] Info, int Result) GetDataTest(DateTime dateTime)
        {
            double day = _swissEphemeris.swe_julday(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, _swissCalendarType) + (dateTime.Minute / 60.0) + (dateTime.Second / 3600.0);

            var info = new double[6];
            var error = string.Empty;

            var result = _swissEphemeris.swe_calc(day, ConvertPlanetToSwiss(PlanetEnum.Moon), _swissCalendarType, info, ref error);

            return (info, result);
        }

        public ChartInfo GetChartData(DateTime dateTime)
        {
            return GetDayInfo(dateTime, out var error);
        }

        public List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo transitChart)
        {
            var aspects = new List<AspectInfo>();

            foreach (var natalPlanet in natalChart.Values)
            {
                foreach (var transitPlanet in transitChart.Values)
                {
                    var aspectInfo = GetAspect(natalPlanet, transitPlanet);

                    if (aspectInfo.Aspect != AspectEnum.None)
                    {
                        aspects.Add(aspectInfo);
                    }

                }
            }

            return aspects;
        }

        public List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo transitChart, params PlanetEnum[] transitPlanets)
        {
            var aspects = new List<AspectInfo>();

            foreach (var transitPlanet in transitChart.Values)
            {
                foreach (var natalPlanet in natalChart.Values)
                {

                    if (!transitPlanets.Contains(transitPlanet.Planet))
                    {
                        continue;
                    }
                    var aspectInfo = GetAspect(natalPlanet, transitPlanet);

                    if (aspectInfo.Aspect != AspectEnum.None)
                    {
                        aspects.Add(aspectInfo);
                    }

                }
            }

            return aspects;
        }

        public AspectInfo GetAspect(PlanetInfo natalPlanet, PlanetInfo transitPlanet)
        {
            if (!_planetsOrbDictionary.TryGetValue(natalPlanet.Planet, out var aspectsOrb))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.None);
            }

            var angles = Math.Abs(natalPlanet.AbsolutAngles - transitPlanet.AbsolutAngles);

            if (angles >= aspectsOrb[AspectEnum.Conjunction].Min && angles <= Constants.CIRCLE_ANGLES ||
               angles <= aspectsOrb[AspectEnum.Conjunction].Max && angles >= Constants.ZODIAC_ZERO)
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Conjunction);
            }
            else if (angles >= aspectsOrb[AspectEnum.Sextile].Min && angles <= aspectsOrb[AspectEnum.Sextile].Max ||
                angles >= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Sextile].Max) && angles <= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Sextile].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Sextile);
            }
            else if (angles >= aspectsOrb[AspectEnum.Square].Min && angles <= aspectsOrb[AspectEnum.Square].Max ||
                angles >= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Square].Max) && angles <= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Square].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Square);
            }
            else if (angles >= aspectsOrb[AspectEnum.Trine].Min && angles <= aspectsOrb[AspectEnum.Trine].Max ||
                angles >= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Trine].Max) && angles <= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Trine].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Trine);
            }
            else if (angles >= aspectsOrb[AspectEnum.Opposition].Min && angles <= aspectsOrb[AspectEnum.Opposition].Max ||
                angles >= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Opposition].Max) && angles <= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Opposition].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Opposition);
            }

            return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.None);
        }

        public List<AspectInfo> GetMoonAspects(ChartInfo natalChart, DateTime startUtcDate, DateTime endUtcDate)
        {
            if (startUtcDate >= endUtcDate)
            {
                return new List<AspectInfo>();
            }

            //calculate moon info
            var moonTransitList = new List<PlanetInfo>();

            while (startUtcDate < endUtcDate)
            {
                var moonTransit = GetDayInfo(PlanetEnum.Moon, startUtcDate, out var error);
                moonTransitList.Add(moonTransit);

                startUtcDate = startUtcDate.AddHours(1);
            }

            //calculate aspects
            var aspects = new List<AspectInfo>();

            foreach (var planetEnum in Enum.GetValues(typeof(PlanetEnum)).Cast<PlanetEnum>())
            {
                var natalPlanetInfo = natalChart[planetEnum];

                var moonTransitInfo = new PlanetInfo(PlanetEnum.Moon, 0);

                var aspect = AspectEnum.None;

                foreach (var moonTransit in moonTransitList)
                {
                    var currentAspectInfo = GetAspect(natalPlanetInfo, moonTransit);

                    if (currentAspectInfo.Aspect != AspectEnum.None)
                    {
                        moonTransitInfo = moonTransit;
                        aspect = currentAspectInfo.Aspect;

                        break;
                    }
                }

                if (aspect != AspectEnum.None)
                {
                    aspects.Add(new AspectInfo(natalPlanetInfo, moonTransitInfo, aspect));
                }
            }

            return aspects;
        }










        //Нужен ли Dictionary?
        public Dictionary<DateTime, ChartInfo> GetData(DateTime startTime, DateTime endTime, TimeSpan interval)
        {
            if (startTime >= endTime)
            {
                return null; ;
            }

            var currentTime = startTime;

            var daysInfo = new Dictionary<DateTime, ChartInfo>();

            while (currentTime < endTime)
            {
                var dayInfo = GetDayInfo(currentTime, out var error);
                daysInfo.Add(currentTime, dayInfo);

                currentTime = currentTime.Add(interval);
            }

            _swissEphemeris.swe_close();

            return daysInfo;
        }

        public Dictionary<DateTime, List<AspectInfo>> ProcessAspects0(ChartInfo birthDayInfo, List<ChartInfo> transitList)
        {
            var dict = new Dictionary<DateTime, List<AspectInfo>>();

            //natal planet
            foreach (var natalPlanet in birthDayInfo.Values)
            {
                //transit date time
                foreach (var transit in transitList)
                {
                    //transit planet
                    foreach (var transitPlanet in transit.Values)
                    {
                        var aspect = GetAspect(natalPlanet, transitPlanet);

                        if (aspect.Aspect == AspectEnum.None)
                        {
                            continue;
                        }

                        if (dict.TryGetValue(transit.DateTime, out var aspectList))
                        {
                            aspectList.Add(aspect);
                        }
                        else
                        {
                            dict.Add(transit.DateTime, new List<AspectInfo>() { aspect });
                        }
                    }
                }
            }

            return dict;
        }

        public List<PlanetMain> ProcessAspects(ChartInfo birthDayInfo, List<ChartInfo> transitList)
        {
            var planetMainList = new List<PlanetMain>();

            //natal planet
            foreach (var natalPlanet in birthDayInfo.Values)
            {
                var planetMain = new PlanetMain(natalPlanet.Planet);

                //transit date time
                foreach (var transit in transitList)
                {
                    var aspects = new List<AspectInfo>();

                    //transit planet
                    foreach (var transitPlanet in transit.Values)
                    {
                        var aspect = GetAspect(natalPlanet, transitPlanet);

                        if (aspect.Aspect == AspectEnum.None)
                        {
                            continue;
                        }

                        aspects.Add(aspect);
                    }

                    planetMain.Aspects.Add(transit.DateTime, aspects);
                }

                planetMainList.Add(planetMain);
            }

            return planetMainList;
        }

        public Dictionary<DateTime, List<AspectInfo>> ProcessAspects2(ChartInfo birthDayInfo, List<ChartInfo> transitList)
        {
            var dict = new Dictionary<DateTime, List<AspectInfo>>();

            //natal planet
            foreach (var natalPlanet in birthDayInfo.Values)
            {
                //transit date time
                foreach (var transit in transitList)
                {
                    //transit planet
                    foreach (var transitPlanet in transit.Values)
                    {
                        var aspect = GetAspect(natalPlanet, transitPlanet);

                        if (aspect.Aspect == AspectEnum.None)
                        {
                            continue;
                        }

                        if (dict.TryGetValue(transit.DateTime, out var aspectList))
                        {
                            aspectList.Add(aspect);
                        }
                        else
                        {
                            dict.Add(transit.DateTime, new List<AspectInfo>() { aspect });
                        }
                    }
                }
            }

            return dict;
        }

        public void FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval)
        {
            var saveDbTimeSpan = new TimeSpan(1, 0, 0, 0);

            var startIntervalDate = startDate;
            var endIntervalDate = startIntervalDate.Add(saveDbTimeSpan);

            while (startIntervalDate < endDate)
            {
                var dict = GetData(startIntervalDate, endIntervalDate, interval);

                var ephList = new List<Ephemeris>();

                foreach (var dtInfo in dict)
                {
                    var ephDb = new Ephemeris()
                    {
                        Id = long.Parse($"{dtInfo.Key.ToUniversalTime().ToString("yyyyMMddHHmmss")}"),
                        DateTime = dtInfo.Key.ToUniversalTime(),

                        SunAngles = dtInfo.Value[PlanetEnum.Sun].AbsolutAngles,
                        MoonAngles = dtInfo.Value[PlanetEnum.Moon].AbsolutAngles,

                        MercuryAngles = dtInfo.Value[PlanetEnum.Mercury].AbsolutAngles,
                        VenusAngles = dtInfo.Value[PlanetEnum.Venus].AbsolutAngles,
                        MarsAngles = dtInfo.Value[PlanetEnum.Mars].AbsolutAngles,

                        JupiterAngles = dtInfo.Value[PlanetEnum.Jupiter].AbsolutAngles,
                        SaturnAngles = dtInfo.Value[PlanetEnum.Saturn].AbsolutAngles,

                        UranAngles = dtInfo.Value[PlanetEnum.Uran].AbsolutAngles,
                        NeptuneAngles = dtInfo.Value[PlanetEnum.Neptune].AbsolutAngles,
                        PlutoAngles = dtInfo.Value[PlanetEnum.Pluto].AbsolutAngles
                    };

                    ephList.Add(ephDb);
                }

                _ephemerisProvider.AddEphemerises(ephList);

                startIntervalDate = startIntervalDate.Add(saveDbTimeSpan);
                endIntervalDate = endIntervalDate.Add(saveDbTimeSpan);
            }


        }

        private int ConvertPlanetToSwiss(PlanetEnum planetEnum)
        {
            switch (planetEnum)
            {
                case PlanetEnum.Sun:
                    return SwissEph.SE_SUN;
                case PlanetEnum.Moon:
                    return SwissEph.SE_MOON;
                case PlanetEnum.Mercury:
                    return SwissEph.SE_MERCURY;
                case PlanetEnum.Venus:
                    return SwissEph.SE_VENUS;
                case PlanetEnum.Mars:
                    return SwissEph.SE_MARS;
                case PlanetEnum.Jupiter:
                    return SwissEph.SE_JUPITER;
                case PlanetEnum.Saturn:
                    return SwissEph.SE_SATURN;
                case PlanetEnum.Uran:
                    return SwissEph.SE_URANUS;
                case PlanetEnum.Neptune:
                    return SwissEph.SE_NEPTUNE;
                case PlanetEnum.Pluto:
                    return SwissEph.SE_PLUTO;
                default:
                    return -1;
            }
        }

        private ChartInfo GetDayInfo(DateTime dateTime, out string error)
        {
            error = string.Empty;
            var dayInfo = new ChartInfo(dateTime);

            // Преобразует дату и время в юлианскую дату
            double minutePart = (dateTime.Minute / 60.0);
            double secondPart = (dateTime.Second / 3600.0);
            double day = _swissEphemeris.swe_julday(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + minutePart + secondPart, _swissCalendarType);

            foreach (var planetEnum in Enum.GetValues(typeof(PlanetEnum)).Cast<PlanetEnum>())
            {
                var planetInfo = GetPlanetInfo(planetEnum, day, _swissPositionType, out error);
                dayInfo.Add(planetEnum, planetInfo);
            }

            return dayInfo;
        }

        public PlanetInfo GetDayInfo(PlanetEnum planetEnum, DateTime dateTime, out string error)
        {
            error = string.Empty;

            // Преобразует дату и время в юлианскую дату
            double minutePart = (dateTime.Minute / 60.0);
            double secondPart = (dateTime.Second / 3600.0);
            double day = _swissEphemeris.swe_julday(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + minutePart + secondPart, _swissCalendarType);

            var planetInfo = GetPlanetInfo(planetEnum, day, _swissPositionType, out error);

            return planetInfo;
        }

        private PlanetInfo GetPlanetInfo(PlanetEnum planetEnum, double day, int iflag, out string error)
        {
            var info = new double[6];
            error = string.Empty;

            var result = _swissEphemeris.swe_calc(day, ConvertPlanetToSwiss(planetEnum), iflag, info, ref error);

            if (result != SwissEph.OK)
            {
                //Console.WriteLine($"Ошибка при расчете: {error}");
            }

            var planetInfo = new PlanetInfo(planetEnum, info[0], info[3]);

            return planetInfo;
        }
    }
}
