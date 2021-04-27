using System;
using System.Collections.Generic;
using System.Text;

namespace Arma3Launcher.Frontend.Console
{
    class Printer
    {
        public VerboseLevel Verbosity = VerboseLevel.REGULAR;
        public Printer()
        {
        }

        public void PrintLine(string text, VerboseLevel verbosity = VerboseLevel.REGULAR)
        {
            if ((int)Verbosity >= (int)verbosity)
            {
                System.Console.WriteLine(text);
            }
        }

        public void Print(string text, VerboseLevel verbosity = VerboseLevel.REGULAR)
        {
            if ((int)Verbosity >= (int)verbosity)
            {
                System.Console.Write(text);
            }
        }
    }

    enum VerboseLevel
    {
        VERBOSE = 4,
        INFO = 3,
        REGULAR = 2,
        IMPORTANT = 1
    }
}
