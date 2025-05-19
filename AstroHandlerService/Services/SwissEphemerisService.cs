using Microsoft.Extensions.Options;
using SwissEphNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Configurations;
using AstroHandlerService.Entities;
using AstroHandlerService.Entities.Db;
using AstroHandlerService.Entities.Enums;
using AstroHandlerService.Migrations;
using AstroHandlerService.Providers;
using AstroHandlerService.ReturnEntities;

namespace AstroHandlerService.Services
{
    public class SwissEphemerisService : ISwissEphemerisService
    {
        // SwissEph.SE_GREG_CAL - указывает на григорианский календарь.
        private readonly int _swissCalendarType = SwissEph.SE_GREG_CAL;
        //SwissEph.SEFLG_TRUEPOS - обеспечивает получение истинного положения, без учета аберрации.
        //SwissEph.SEFLG_SWIEPH - гарантирует использование наиболее точных эфемерид, если они доступны.
        private readonly int _swissPositionType = SwissEph.SEFLG_TRUEPOS | SwissEph.SEFLG_SWIEPH;

        private SwissEph _swissEphemeris = new SwissEph();
        private readonly IEphemerisProvider _ephemerisProvider;

        private readonly Dictionary<PlanetEnum, PlanetOrb> _orbPlanetDict;

        public SwissEphemerisService(
            IOptions<AstroConfig> astroConfiguration,
            IEphemerisProvider ephemerisProvider)
        {
            //Установка пути к файлам эфемерид
            //SwissEph sw = new SwissEph("C\\:\\ephe");
            _swissEphemeris.swe_set_ephe_path(@"C:\SWEPH\EPHE");
            //_swissEphemeris.swe_set_jpl_file("C:\\SWEPH\\EPHE");

            _orbPlanetDict = new Dictionary<PlanetEnum, PlanetOrb>()
            {
                {PlanetEnum.Sun, new PlanetOrb(astroConfiguration.Value.Orbs.Sun)},
                {PlanetEnum.Moon, new PlanetOrb(astroConfiguration.Value.Orbs.Moon)},

                {PlanetEnum.Mercury, new PlanetOrb(astroConfiguration.Value.Orbs.Mercury)},
                {PlanetEnum.Venus, new PlanetOrb(astroConfiguration.Value.Orbs.Venus)},
                {PlanetEnum.Mars, new PlanetOrb(astroConfiguration.Value.Orbs.Mars)},

                {PlanetEnum.Jupiter, new PlanetOrb(astroConfiguration.Value.Orbs.Jupiter)},
                {PlanetEnum.Saturn, new PlanetOrb(astroConfiguration.Value.Orbs.Saturn)},

                {PlanetEnum.Uran, new PlanetOrb(astroConfiguration.Value.Orbs.Uran)},
                {PlanetEnum.Neptune, new PlanetOrb(astroConfiguration.Value.Orbs.Neptune)},
                {PlanetEnum.Pluto, new PlanetOrb(astroConfiguration.Value.Orbs.Pluto)}
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

        public PosInfo GetData(DateTime dateTime)
        {
            return GetDayInfo(dateTime, _swissPositionType, out var error);
        }

        //Нужен ли Dictionary?
        public Dictionary<DateTime, PosInfo> GetData(DateTime startTime, DateTime endTime, TimeSpan interval)
        {
            if (startTime >= endTime)
            {
                return null; ;
            }

            var currentTime = startTime;

            var daysInfo = new Dictionary<DateTime, PosInfo>();

            while (currentTime < endTime)
            {
                var dayInfo = GetDayInfo(currentTime, _swissPositionType, out var error);
                daysInfo.Add(currentTime, dayInfo);

                currentTime = currentTime.Add(interval);
            }

            _swissEphemeris.swe_close();

            return daysInfo;
        }

        public Dictionary<DateTime, List<AspectInfo>> ProcessAspects0(PosInfo birthDayInfo, List<PosInfo> transitList)
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

        public List<PlanetMain> ProcessAspects(PosInfo birthDayInfo, List<PosInfo> transitList)
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

        public Dictionary<DateTime, List<AspectInfo>> ProcessAspects2(PosInfo birthDayInfo, List<PosInfo> transitList)
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
            var saveDbTimeSpan = new TimeSpan(1,0,0,0);

            var startIntervalDate = startDate;
            var endIntervalDate = startIntervalDate.Add(saveDbTimeSpan);

            while (startIntervalDate < endDate)
            {
                var dict = GetData(startIntervalDate, endIntervalDate, interval);

                var ephList = new List<EphemerisDb>();

                foreach (var dtInfo in dict)
                {
                    var ephDb = new EphemerisDb()
                    {
                        Id = long.Parse($"{dtInfo.Key.ToUniversalTime().ToString("yyyyMMddHHmmss")}"),
                        DateTime = dtInfo.Key.ToUniversalTime(),

                        SunDegrees = dtInfo.Value[PlanetEnum.Sun].AbsolutDegrees,
                        MoonDegrees = dtInfo.Value[PlanetEnum.Moon].AbsolutDegrees,

                        MercuryDegrees = dtInfo.Value[PlanetEnum.Mercury].AbsolutDegrees,
                        VenusDegrees = dtInfo.Value[PlanetEnum.Venus].AbsolutDegrees,
                        MarsDegrees = dtInfo.Value[PlanetEnum.Mars].AbsolutDegrees,

                        JupiterDegrees = dtInfo.Value[PlanetEnum.Jupiter].AbsolutDegrees,
                        SaturnDegrees = dtInfo.Value[PlanetEnum.Saturn].AbsolutDegrees,

                        UranDegrees = dtInfo.Value[PlanetEnum.Uran].AbsolutDegrees,
                        NeptuneDegrees = dtInfo.Value[PlanetEnum.Neptune].AbsolutDegrees,
                        PlutoDegrees = dtInfo.Value[PlanetEnum.Pluto].AbsolutDegrees
                    };

                    ephList.Add(ephDb);
                }

                _ephemerisProvider.AddEphemeris(ephList);

                startIntervalDate =startIntervalDate.Add(saveDbTimeSpan);
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

        private PosInfo GetDayInfo(DateTime dateTime, int iflag, out string error)
        {
            error = string.Empty;
            var dayInfo = new PosInfo(dateTime);

            // Преобразует дату и время в юлианскую дату
            double minutePart = (dateTime.Minute / 60.0);
            double secondPart = (dateTime.Second / 3600.0);
            double day = _swissEphemeris.swe_julday(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + minutePart + secondPart, _swissCalendarType) ;

            double day1 = _swissEphemeris.swe_utc_to_jd(dateTime.Year, dateTime.Month, dateTime.Day, 00, 00, 00, _swissCalendarType, new double[2], ref error);

            foreach (var planetEnum in Enum.GetValues(typeof(PlanetEnum)).Cast<PlanetEnum>())
            {
                var planetInfo = GetPlanetInfo(planetEnum, day, iflag, out error);
                dayInfo.Add(planetEnum, planetInfo);
            }

            return dayInfo;
        }

        private PlanetPosInfo GetPlanetInfo(PlanetEnum planetEnum, double day, int iflag, out string error)
        {
            var planetInfo = new PlanetPosInfo(planetEnum);

            var info = new double[6];
            error = string.Empty;

            var result = _swissEphemeris.swe_calc(day, ConvertPlanetToSwiss(planetEnum), iflag, info, ref error);

            if (result != SwissEph.OK)
            {
                //Console.WriteLine($"Ошибка при расчете: {error}");
            }

            // Эклиптическая долгота.
            // Градус угла планеты относительно 0 гр. Овна.
            // Может принимать значения от 0 до 360
            if (info == null || info[0] < 0 || info[0] > 360)
            {
                return null;
            }

            var zodiacInfo = ConverToZodiac(info[0]);

            planetInfo.AbsolutDegrees = info[0];

            planetInfo.Zodiac = zodiacInfo.Zodiac;
            planetInfo.ZodiacDegrees = zodiacInfo.ZodiacDegrees;

            return planetInfo;
        }

        private (ZodiacEnum Zodiac, double ZodiacDegrees) ConverToZodiac(double absDegrees)
        {
            int zodiacInt = (int)absDegrees / Constants.ZODIAC_DEGREES;
            var zodiacDegrees = absDegrees % Constants.ZODIAC_DEGREES;

            var zodiacEnum = (ZodiacEnum)Enum.GetValues(typeof(ZodiacEnum)).GetValue(zodiacInt);

            return (zodiacEnum, zodiacDegrees);
        }

        public AspectInfo GetAspect(PlanetPosInfo natalPlanet, PlanetPosInfo transitPlanet)
        {
            if (!_orbPlanetDict.TryGetValue(natalPlanet.Planet, out PlanetOrb planetOrb))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.None);
            }

            var cornerDegrees = Math.Abs(natalPlanet.AbsolutDegrees - transitPlanet.AbsolutDegrees);

            if (cornerDegrees >= planetOrb.AspectOrbDict[AspectEnum.Conjunction].Min && cornerDegrees <= Constants.CIRCLE_DEGREES ||
               cornerDegrees <= planetOrb.AspectOrbDict[AspectEnum.Conjunction].Max && cornerDegrees >= Constants.ZODIAC_ZERO)
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Conjunction);
            }
            else if (cornerDegrees >= planetOrb.AspectOrbDict[AspectEnum.Sextile].Min && cornerDegrees <= planetOrb.AspectOrbDict[AspectEnum.Sextile].Max)
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Sextile);
            }
            else if (cornerDegrees >= planetOrb.AspectOrbDict[AspectEnum.Square].Min && cornerDegrees <= planetOrb.AspectOrbDict[AspectEnum.Square].Max)
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Square);
            }
            else if (cornerDegrees >= planetOrb.AspectOrbDict[AspectEnum.Trine].Min && cornerDegrees <= planetOrb.AspectOrbDict[AspectEnum.Trine].Max)
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Trine);
            }
            else if (cornerDegrees >= planetOrb.AspectOrbDict[AspectEnum.Opposition].Min && cornerDegrees <= planetOrb.AspectOrbDict[AspectEnum.Opposition].Max)
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Opposition);
            }

            return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.None);
        }
    }
}
