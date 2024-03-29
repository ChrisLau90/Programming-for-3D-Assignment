using System;

namespace FuelCell
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DD3Game game = new DD3Game())
            {
                game.Run();
            }
        }
    }
#endif
}
/// REFERENCES:
/// [0] FuelCell game code http://msdn.microsoft.com/en-us/library/dd940288.aspx
/// date accessed 18th April 2012

