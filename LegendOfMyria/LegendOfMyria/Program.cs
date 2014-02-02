using System;

namespace LegendOfMyria
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Myria game = new Myria())
            {
                game.Run();
            }
        }
    }
#endif
}

