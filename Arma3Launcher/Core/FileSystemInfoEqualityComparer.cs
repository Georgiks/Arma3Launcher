using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Arma3Launcher.Core
{
    class FileSystemInfoEqualityComparer : IEqualityComparer<FileSystemInfo>
    {
        public bool Equals([AllowNull] FileSystemInfo x, [AllowNull] FileSystemInfo y)
        {
            return EqualityComparer<string>.Default.Equals(x.FullName, y.FullName);
        }

        public int GetHashCode([DisallowNull] FileSystemInfo obj)
        {
            return EqualityComparer<string>.Default.GetHashCode(obj.FullName);

        }
    }
}
