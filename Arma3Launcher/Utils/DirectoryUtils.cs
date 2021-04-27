using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Arma3Launcher.Utils
{
    static class DirectoryUtils
    {
        static void DeleteEmptyDirectories(this DirectoryInfo directory)
        {
            foreach (var d in directory.GetDirectories())
            {
                DeleteEmptyDirectories(d);
            }
            if (directory.GetFiles().Length == 0 && directory.GetDirectories().Length == 0)
            {
                directory.Delete();
            }
        }
    }
}
