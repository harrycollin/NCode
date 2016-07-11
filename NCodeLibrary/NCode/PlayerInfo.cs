using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCode
{
    [Serializable]
    public class PlayerInfo
    {
        public string ign { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string surname { get; set; }
        public string steamid { get; set; }
        public string position { get; set; }
        public string rotation { get; set; }
    }
}
