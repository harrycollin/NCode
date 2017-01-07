using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCode.Core.BaseClasses
{
    [Serializable]
    public class VirtualObject
    {
        public string name { get; set; }
        public Guid Owner { get; set; }
    }
}
