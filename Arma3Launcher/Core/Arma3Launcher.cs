using Arma3Launcher.Frontend.Console;
using Arma3Launcher.Model.Index;
using Arma3Launcher.Model.LauncherConfig;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Arma3Launcher.Core
{
    class Arma3Launcher
    {
        private string RepositoryUrl = @"https://files.417rct.org/Swifty_repos/RavensRegiment/index.xml";
        private string ConfigPath = @"Config\Modpacks.xml";

        private Config CurrentConfig;
        private Printer Printer;

        public Arma3Launcher(Printer printer)
        {
            Printer = printer;
        }

        void LoadConfiguration()
        {
            Printer.PrintLine("Loading configuration...", VerboseLevel.VERBOSE);
            FileInfo fi = new FileInfo(ConfigPath);
            if (!fi.Exists)
            {
                Printer.PrintLine("Configuration file not found.", VerboseLevel.INFO);
                CurrentConfig = new Config();
                return;
            }

            using (FileStream f = fi.OpenRead())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
                CurrentConfig = (Config)xmlSerializer.Deserialize(f);
            }
        }

        void SaveConfiguration()
        {
            Printer.PrintLine("Saving configuration...", VerboseLevel.VERBOSE);
            FileInfo fi = new FileInfo(ConfigPath);
            Directory.CreateDirectory(fi.DirectoryName);

            using (FileStream f = fi.OpenWrite())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
                xmlSerializer.Serialize(f, CurrentConfig ?? new Config());
            }
        }

        public void Synchronize(Repository repository, Modpack modpack)
        {
            if (CurrentConfig == null)
            {
                LoadConfiguration();
            }

            DirectoryInfo directory = CurrentConfig.GetModpackDirectory(modpack.ID);
            if (directory == null)
            {
                Printer.PrintLine("Please select modpack directory:", VerboseLevel.REGULAR);
                directory = PromptModpackDirectory(modpack);
                if (directory == null || !directory.Exists)
                {
                    Printer.PrintLine("Modpack directory invalid!", VerboseLevel.IMPORTANT);
                    return;
                }
                CurrentConfig.SetModpackDirectory(new ModpackPath() { ModpackID = modpack.ID, Path = directory.FullName});
                SaveConfiguration();
            }


            ModpackSynchronizer synchronizer = new ModpackSynchronizer(repository, modpack, directory, Printer);
            synchronizer.Synchronize();
        }

        public void StartArmaWithModpack(Modpack modpack)
        {
            if (CurrentConfig == null)
            {
                LoadConfiguration();
            } 
            if (string.IsNullOrWhiteSpace(CurrentConfig.Arma3Executable))
            {
                Printer.PrintLine("Please select Arma 3 executable:", VerboseLevel.REGULAR);
                FileInfo execFile = PromptArmaExecutable();
                if (execFile == null || !execFile.Exists)
                {
                    Printer.PrintLine("Invalid Arma 3 executable file!");
                    return;
                }
                CurrentConfig.Arma3Executable = execFile.FullName;
                SaveConfiguration();
            }

            string parameters = CreateStartupParameters(modpack);
            Process.Start(new ProcessStartInfo(CurrentConfig.Arma3Executable, parameters));
        }

        string CreateStartupParameters(Modpack modpack)
        {
            DirectoryInfo modpackDirectory = CurrentConfig.GetModpackDirectory(modpack.ID);
            if (modpackDirectory == null || string.IsNullOrWhiteSpace(modpackDirectory.FullName))
            {
                Printer.PrintLine("Could not find modpack directory!", VerboseLevel.IMPORTANT);
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("-connect=").Append(modpack.IP).Append(" ");
            sb.Append("-port=").Append(modpack.Port).Append(" ");
            sb.Append("-password=").Append(modpack.Password).Append(" ");
            string mods = string.Join(';', modpack.Addons.Select(a => $"\"{Path.Combine(modpackDirectory.FullName, a)}\""));
            sb.Append("-mod=").Append(mods);
            return sb.ToString();
        }



        DirectoryInfo PromptModpackDirectory(Modpack modpack)
        {
            using (var dialog = new CommonOpenFileDialog()
            { 
                Title = $"'{modpack.Name}' modpack directory", 
                IsFolderPicker = true,
                EnsureFileExists = true,
                Multiselect = false
            })
            {
                var result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    return new DirectoryInfo(dialog.FileName);
                }

                return null;
            }
        }
        FileInfo PromptArmaExecutable()
        {
            using (var dialog = new CommonOpenFileDialog()
            {
                Title = $"Arma 3 Executable",
                EnsureFileExists = true,
                Multiselect = false,
                DefaultFileName = "arma3_x64.exe"
            })
            {
                dialog.Filters.Add(new CommonFileDialogFilter("Arma 3 Executable", "exe"));

                var result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    return new FileInfo(dialog.FileName);
                }

                return null;
            }
        }

        public async Task<Repository> GetRepositoryAsync()
        {
            Printer.PrintLine("Retrieving repository index...", VerboseLevel.REGULAR);

            byte[] data = await DownloadRepositoryIndexAsync();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Repository));

            Printer.PrintLine("\nRepository retrieved.", VerboseLevel.REGULAR);

            using (MemoryStream stream = new MemoryStream(data))
            {
                return (Repository)xmlSerializer.Deserialize(stream);
            }
        }

        async Task<byte[]> DownloadRepositoryIndexAsync()
        {
            using (var c = new WebClient())
            {
                c.DownloadProgressChanged += IndexDownloadProgressChanged;
                return await c.DownloadDataTaskAsync(RepositoryUrl);
            }
        }

        private void IndexDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write($"\r [{e.ProgressPercentage} %] {e.BytesReceived}/{e.TotalBytesToReceive} B    ");
        }
    }
}
