using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Arma3Launcher.Model.Index
{
    [XmlType(TypeName = "File")]
    public class AddonFile
    {
        [XmlAttribute]
        public long LastChange;
        [XmlAttribute]
        public string Path;
        [XmlAttribute]
        public long Size;
    }
}
