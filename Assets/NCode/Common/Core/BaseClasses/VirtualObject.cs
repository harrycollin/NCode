using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCode.BaseClasses
{
    [Serializable]
    public class VirtualObject
    {
        public Guid GUID { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string displayName { get; set; }

    }
}
