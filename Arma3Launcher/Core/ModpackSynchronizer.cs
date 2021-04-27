using Arma3Launcher.Frontend.Console;
using Arma3Launcher.Model.Index;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Arma3Launcher.Core
{
    class ModpackSynchronizer
    {
        private DirectoryInfo ModpackDirectory;
        private Modpack Modpack;
        private Repository Repository;
        private Printer Printer;

        public ModpackSynchronizer(Repository repository, Modpack modpack, DirectoryInfo modpackDirectory, Printer printer)
        {
            Repository = repository;
            Modpack = modpack;
            ModpackDirectory = modpackDirectory;
            Printer = printer;
        }

        public void Synchronize()
        {
            HashSet<DirectoryInfo> allDirectories = new HashSet<DirectoryInfo>(
                ModpackDirectory.GetDirectories(),
                new FileSystemInfoEqualityComparer()
            );


            foreach (var addon in Modpack.Addons)
            {
                DirectoryInfo directory = new DirectoryInfo(Path.Combine(ModpackDirectory.FullName, addon));
                if (directory.Exists)
                {
                    Printer?.PrintLine($"Addon found: {addon}", VerboseLevel.INFO);
                    allDirectories.Remove(directory);
                }
                else
                {
                    Printer?.PrintLine($"Addon not found: {addon}", VerboseLevel.INFO);
                }
                Addon addonInfo = Repository.Addons.Where((a) => a.Name == addon).FirstOrDefault();
                if (addonInfo == null)
                {
                    Printer?.PrintLine("Addon files not found in repository index!", VerboseLevel.IMPORTANT);
                }
                CheckAddonDirectory(
                        addonInfo,
                        directory,
                        Modpack
                    );
            }
            foreach (var dir in allDirectories)
            {
                Printer?.PrintLine($"Redundant directory found: {dir.Name} - Deleting!", VerboseLevel.REGULAR);
                DeleteDirectoryWithContents(dir);
            }
        }

        void CheckAddonDirectory(Addon addon, DirectoryInfo directory, Modpack modpack)
        {
            Directory.CreateDirectory(directory.FullName);
            HashSet<FileInfo> allFiles = new HashSet<FileInfo>(
                directory.GetFiles("*", SearchOption.AllDirectories),
                new FileSystemInfoEqualityComparer());

            List<AddonFile> filesToDownload = new List<AddonFile>();

            foreach (var f in addon.Files)
            {
                string fPath = Path.Combine(ModpackDirectory.FullName, f.Path);
                FileInfo fi = new FileInfo(fPath);
                if (fi.Exists)
                {
                    //long writeTime = ((DateTimeOffset)fi.LastWriteTimeUtc).ToUnixTimeSeconds();
                    if (fi.Length != f.Size/* || writeTime != f.LastChange*/)
                    {
                        Printer?.PrintLine($"\tSize differs: {fi.Length}/{f.Size} B ({f.Path})", VerboseLevel.REGULAR);
                        filesToDownload.Add(f);
                    }
                    allFiles.Remove(fi);
                }
                else
                {
                    Printer?.PrintLine($"\tFile not found: {f.Path}", VerboseLevel.REGULAR);
                    filesToDownload.Add(f);
                }
            }

            if (filesToDownload.Count > 0)
            {
                ConcurrentAddonDownloader downloader = new ConcurrentAddonDownloader(Printer);
                downloader.DownloadFiles(filesToDownload, modpack.Source, ModpackDirectory).GetAwaiter().GetResult();
            }

            foreach (var f in allFiles)
            {
                Printer?.PrintLine($"\tRedundant file: {f.FullName} - Deleting!", VerboseLevel.REGULAR);
                f.Delete();
            }

            DeleteEmptyDirectories(directory);
        }


        void DeleteDirectoryWithContents(DirectoryInfo directory)
        {
            foreach (var d in directory.EnumerateDirectories())
            {
                DeleteDirectoryWithContents(d);
            }
            foreach (var f in directory.EnumerateFiles())
            {
                f.Delete();
            }
            directory.Delete();
        }

        void DeleteEmptyDirectories(DirectoryInfo directory)
        {
            foreach (var d in directory.GetDirectories())
            {
                DeleteEmptyDirectories(d);
            }
            if (directory.GetFiles().Length == 0 && directory.GetDirectories().Length == 0)
            {
                Printer?.PrintLine($"Deleting empty directory: {directory.FullName}", VerboseLevel.REGULAR);
                directory.Delete();
            }
        }

    }
}
