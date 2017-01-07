using System;
using KleosTypes.Virtual;
using NCode.Core.BaseClasses;

namespace NCode.KleosTypes.Virtual
{
    [Serializable]
    public class PlayerInfo 
    {
        public CharacterInfo characterInfomation;
        public string ign;
        public string firstName;
        public string middleName;
        public string surname;
        public string steamid;
        public V3 position;
        public V4 rotation;
        public Inventory inventory;
    }
}
