using NCode;
using System;

namespace KleosTypes.Items
{
    [Serializable]
    public class SimCard : NetworkObject
    {
        public long Number;
        public int CallsLeft;
        public int TextsLeft;
    }
}
