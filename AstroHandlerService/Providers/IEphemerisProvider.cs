using AstroHandlerService.Db.Entities;

namespace AstroHandlerService.Providers
{
    public interface IEphemerisProvider
    {
        public void AddEphemerises(IEnumerable<Ephemeris> ephemeris);
    }
}
