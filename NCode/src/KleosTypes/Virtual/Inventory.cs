using NCode;
using NCode.Core.BaseClasses;
using NCode.Utilities;
using System;
using System.Collections.Generic;

namespace KleosTypes.Virtual
{
    [Serializable]
    public class Inventory : VirtualObject
    {
        public NCode.Utilities.List<NetworkObject> Items = new NCode.Utilities.List<NetworkObject>();
        public Dictionary<Guid, NetworkObject> ItemDictionary = new Dictionary<Guid, NetworkObject>();

        public int SlotCapacity = 30;
        
        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        public bool AddItem(NetworkObject _item)
        {
            if (Items != null && !ContainsItem(_item))
            {
                Items.Add(_item);
                ItemDictionary.Add(_item.GUID, _item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes an item from the inventory
        /// </summary>
        public bool RemoveItem(NetworkObject _item)
        {
            if (Items != null && ContainsItem(_item))
            {
                for (int i = 0; i < Items.size; i++)
                {
                    if (Items[i].GUID == _item.GUID) Items.RemoveAt(i); break;
                }
                if(ItemDictionary.ContainsKey(_item.GUID)) ItemDictionary.Remove(_item.GUID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks to see if this inventory contains the item.
        /// </summary>
        public bool ContainsItem(NetworkObject _item)
        {
            if (Items != null && ItemDictionary.Count != Items.size)
            {
                foreach (NetworkObject i in Items)
                {
                    if (i.GUID == _item.GUID) return true;                 
                }
                return false;
            }
            else
            {
                if (ItemDictionary.ContainsKey(_item.GUID)) return true;
                return false;
            }

        }
    }
}
