
namespace AstroHandlerService.Enums
{
    public enum AspectEnum
    {
        /// <summary>
        /// No aspects
        /// </summary>
        None,
        /// <summary>
        /// Corner between planets = 0 angles
        Conjunction,
        /// <summary>
        /// Corner between planets = 60 angles
        /// </summary>
        Sextile,
        /// <summary>
        /// Corner between planets = 90 angles
        /// </summary>
        Square,
        /// <summary>
        /// Corner between planets = 120 angles
        /// </summary>
        Trine,
        /// <summary>
        /// Corner between planets = 180 angles
        /// </summary>
        Opposition
    }
}
