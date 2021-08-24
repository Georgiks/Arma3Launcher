using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Arma3Launcher.Core
{
    class FileSystemInfoEqualityComparer : IEqualityComparer<FileSystemInfo>
    {
        public bool Equals(FileSystemInfo x, FileSystemInfo y)
        {
            //return EqualityComparer<string>.Default.Equals(x.FullName, y.FullName);
            return StringComparer.InvariantCultureIgnoreCase.Equals(x.FullName, y.FullName);
        }

        public int GetHashCode(FileSystemInfo obj)
        {
            //return EqualityComparer<string>.Default.GetHashCode(obj.FullName);
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.FullName);
        }
    }
}
