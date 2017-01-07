using NCode.Core.BaseClasses;
using System;

namespace KleosTypes.Virtual
{
    [Serializable]
    public class Contact : VirtualObject
    {
        public string Forename;
        public string MiddleName;
        public string Surname;
        public long PhoneNumber;
        public string ID;
    }
}
