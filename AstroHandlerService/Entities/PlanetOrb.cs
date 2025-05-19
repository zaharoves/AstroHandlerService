using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities.Enums;
using AstroHandlerService.Providers;

namespace AstroHandlerService.Entities
{
    public class PlanetOrb : Dictionary<AspectEnum, (double Min, double Max)>
    {
        public Dictionary<AspectEnum, (double Min, double Max)> AspectOrbDict { get; }

        public PlanetOrb(double orb)
        {
            AspectOrbDict = GetOrbAspectDict(orb);
        }

        private Dictionary<AspectEnum, (double Min, double Max)> GetOrbAspectDict(double orb)
        {
            var dict = new Dictionary<AspectEnum, (double Min, double Max)>();

            dict.Add(
                AspectEnum.Conjunction,
                (Constants.CIRCLE_DEGREES - orb,
                Constants.CONJUNCTION + orb));

            dict.Add(
                AspectEnum.Sextile,
                (Constants.SEXTILE - orb,
                Constants.SEXTILE + orb));

            dict.Add(
                AspectEnum.Square,
                (Constants.SQUARE - orb,
                Constants.SQUARE + orb));

            dict.Add(
                AspectEnum.Trine,
                ((Constants.TRINE - orb),
                (Constants.TRINE + orb)));

            dict.Add(
                AspectEnum.Opposition,
                (Constants.OPPOSITION - orb,
                Constants.OPPOSITION + orb));

            return dict;
        }
    }
}
