using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Arma3Launcher.Model.LauncherConfig
{
    public class ModpackPath
    {
        [XmlAttribute]
        public string ModpackID;
        [XmlText]
        public string Path;
    }
}
