using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Entities;
using AstroHandlerService.Services;

namespace AstroHandlerService.Tests
{
    public static class TestClass
    {
        //public static void Test1()
        //{
        //    Console.WriteLine($"--Test1--Begin--");

        //    var planet1 = new PlanetInfo(PlanetEnum.Sun);
        //    planet1.AbsolutDegrees = 30;
        //    var planet2 = new PlanetInfo(PlanetEnum.Sun);
        //    planet2.AbsolutDegrees = 90;
        //    var aspect = SwissEphemeridService.GetAspect(planet1, planet2);
        //    Console.WriteLine($"{planet1.AbsolutDegrees} - {planet2.AbsolutDegrees} : {aspect}");

        //    planet1 = new PlanetInfo(PlanetEnum.Sun);
        //    planet1.AbsolutDegrees = 30;
        //    planet2 = new PlanetInfo(PlanetEnum.Sun);
        //    planet2.AbsolutDegrees = 150;
        //    aspect = SwissEphemeridService.GetAspect(planet1, planet2);
        //    Console.WriteLine($"{planet1.AbsolutDegrees} - {planet2.AbsolutDegrees} : {aspect}");


        //    planet1 = new PlanetInfo(PlanetEnum.Sun);
        //    planet1.AbsolutDegrees = 30;
        //    planet2 = new PlanetInfo(PlanetEnum.Sun);
        //    planet2.AbsolutDegrees = 120;
        //    aspect = SwissEphemeridService.GetAspect(planet1, planet2);
        //    Console.WriteLine($"{planet1.AbsolutDegrees} - {planet2.AbsolutDegrees} : {aspect}");

        //    planet1 = new PlanetInfo(PlanetEnum.Sun);
        //    planet1.AbsolutDegrees = 30;
        //    planet2 = new PlanetInfo(PlanetEnum.Sun);
        //    planet2.AbsolutDegrees = 210;
        //    aspect = SwissEphemeridService.GetAspect(planet1, planet2);
        //    Console.WriteLine($"{planet1.AbsolutDegrees} - {planet2.AbsolutDegrees} : {aspect}");

        //    planet1 = new PlanetInfo(PlanetEnum.Sun);
        //    planet1.AbsolutDegrees = 0;
        //    planet2 = new PlanetInfo(PlanetEnum.Sun);
        //    planet2.AbsolutDegrees = 360;
        //    aspect = SwissEphemeridService.GetAspect(planet1, planet2);
        //    Console.WriteLine($"{planet1.AbsolutDegrees} - {planet2.AbsolutDegrees} : {aspect}");

        //    Console.WriteLine($"--Test1--End--");
        //}

        //public static void Test2(DateTime startTime, DateTime endTime)
        //{
        //    Console.WriteLine($"--Test2--Begin--");

        //    var sw = new Stopwatch();
        //    sw.Start();

        //    var data = SwissEphemeridService.GetData(startTime, endTime);

        //    sw.Stop();
        //    Console.WriteLine($"Время выполнения: {sw.ElapsedMilliseconds}. Считано данных: {data?.Count}.");

        //    Console.WriteLine($"--Test2--End--");
        //}
    }
}
