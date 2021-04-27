using Arma3Launcher.Model.Index;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Arma3Launcher.Model.LauncherConfig
{
    public class Config
    {
        public List<ModpackPath> Modpacks;

        public string Arma3Executable;

        public DirectoryInfo GetModpackDirectory(string modpackId)
        {
            if (Modpacks == null)
                return null;

            ModpackPath path = Modpacks.Where(m => m.ModpackID == modpackId).FirstOrDefault();
            if (path == null)
                return null;

            return new DirectoryInfo(path.Path);
        }

        public void SetModpackDirectory(ModpackPath modpackPath)
        {
            if (Modpacks == null)
            {
                Modpacks = new List<ModpackPath>();
            }
            ModpackPath path = Modpacks.Where(m => m.ModpackID == modpackPath.ModpackID).FirstOrDefault();
            if (path == null)
            {
                Modpacks.Add(modpackPath);
            }
            else
            {
                path.Path = modpackPath.Path;
            }
        }
    }
}
