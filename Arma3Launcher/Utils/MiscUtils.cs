using System;
using System.Collections.Generic;
using System.Text;

namespace Arma3Launcher.Utils
{
    static class MiscUtils
    {
        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = bytes / Math.Pow(1024, place);
            return $"{Math.Sign(byteCount) * num:0.##} {suf[place]}";
        }
    }
}
