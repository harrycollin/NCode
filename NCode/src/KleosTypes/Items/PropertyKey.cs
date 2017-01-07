using NCode;
using System;

namespace KleosTypes.Items
{
    [Serializable]
    public class PropertyKey : NetworkObject
    {
        public Guid PropertyGuid { get; set; }
    }
}
