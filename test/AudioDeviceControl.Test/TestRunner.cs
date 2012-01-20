
using NUnit.ConsoleRunner;
using System.Reflection;
using System;

namespace AudioDeviceControl.Test
{
    class TestRunner
    {
        static void Main(string[] args)
        {
            Runner.Main(new string[] { Assembly.GetExecutingAssembly().Location, });

            Console.WriteLine("Press <enter> to quit.");
            Console.ReadLine();
        }
    }
}
