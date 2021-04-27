using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Arma3Launcher.Model.Index
{
    public class Modpack
    {
        [XmlAttribute]
        public string ID;
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public string Source;
        [XmlAttribute]
        public string IP;
        [XmlAttribute]
        public string Port;
        [XmlAttribute]
        public string Password;

        public string[] Addons;
    }
}
