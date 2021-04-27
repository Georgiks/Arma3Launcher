using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Arma3Launcher.Model.Index
{
    public class Addon
    {
        [XmlAttribute]
        public string Name;
        public AddonFile[] Files;
    }
}
