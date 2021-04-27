using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Arma3Launcher.Frontend.Console;
using Arma3Launcher.Model.Index;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Arma3Launcher.Core
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Printer printer = new Printer();
            printer.Verbosity = VerboseLevel.INFO;
            Arma3Launcher launcher = new Arma3Launcher(printer);

            Repository repository = launcher.GetRepositoryAsync().GetAwaiter().GetResult();

            // select modpack
            if (repository.Modpacks.Length == 0)
            {
                printer.PrintLine("No modpack found!", VerboseLevel.IMPORTANT);
                return;
            }
            Modpack modpack;
            if (repository.Modpacks.Length > 1)
            {
                for (int i = 0; i < repository.Modpacks.Length; i++)
                {
                    Console.WriteLine($"{i}: {repository.Modpacks[i].Name}");
                }
                Console.WriteLine("Select modpack number: ");
                var r = Console.ReadLine();
                if (int.TryParse(r, out int ri) && ri >= 0 && ri < repository.Modpacks.Length)
                {
                    modpack = repository.Modpacks[ri];
                } else
                {
                    printer.PrintLine("Invalid number entered!");
                    return;
                }
            }
            else
            {
                modpack = repository.Modpacks[0];
            }


            // synchronize modpack
            launcher.Synchronize(repository, modpack);

            // launch arma
            launcher.StartArmaWithModpack(modpack);
        }
    }
}
