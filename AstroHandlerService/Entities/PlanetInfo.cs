using AstroHandlerService.Enums;
using ProtoBuf;

namespace AstroHandlerService.Entities
{
    [ProtoContract]
    public class PlanetInfo
    {
        [ProtoMember(1)]
        public PlanetEnum Planet { get; }

        ///Take angles 0 -> 360
        ///where zero - 0 angles of <see cref="ZodiacEnum.Aries"/>
        public double AbsolutAngles { get; }

        [ProtoMember(2)]
        public ZodiacEnum Zodiac { get; }

        [ProtoMember(3)]
        public double ZodiacAngles { get; }

        public double Speed { get; set; }

        [ProtoMember(4)]
        public bool IsRetro => Speed < 0;

        public PlanetInfo(PlanetEnum planetEnum, double absolutAngles)
        {
            absolutAngles = absolutAngles >= 360 ? 0 : absolutAngles;

            Planet = planetEnum;
            AbsolutAngles = absolutAngles;
            
            int zodiacInt = (int)absolutAngles / Constants.ZODIAC_ANGLES;
            Zodiac = (ZodiacEnum)Enum.GetValues(typeof(ZodiacEnum)).GetValue(zodiacInt);

            ZodiacAngles = absolutAngles % Constants.ZODIAC_ANGLES;
        }

        public PlanetInfo(PlanetEnum planetEnum, double absolutAngles, double speed)
        {
            absolutAngles = absolutAngles >= 360 ? 0 : absolutAngles;

            Planet = planetEnum;
            AbsolutAngles = absolutAngles;

            int zodiacInt = (int)absolutAngles / Constants.ZODIAC_ANGLES;
            Zodiac = (ZodiacEnum)Enum.GetValues(typeof(ZodiacEnum)).GetValue(zodiacInt);

            ZodiacAngles = absolutAngles % Constants.ZODIAC_ANGLES;

            Speed = speed;
        }
    }
}
