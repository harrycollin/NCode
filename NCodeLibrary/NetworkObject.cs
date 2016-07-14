using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCode
{
    /// <summary>
    /// A base class which can be inherited to provide the fundementals for saving etc. 
    /// This is not for attaching to GameObjects and is simply used as a container to save data. 
    /// These are sent to the server for saving and should be serializable. 
    /// </summary>
    [Serializable]
    public class NetworkObject
    {
        public Guid GUID { get; set; }
        public int prefabid { get; set; }
        public int thumbnailid { get; set; }
        public string position { get; set; }
        public string rotation { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string displayName { get; set; }
        public int LastChannelID { get; set; }
    }
}
