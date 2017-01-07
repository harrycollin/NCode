using KleosTypes.Virtual;
using NCode;
using System;
using System.Collections.Generic;

namespace KleosTypes.Items
{
    [Serializable]
    public class MobilePhone : NetworkObject
    {
        public int PinCode;
        public bool PhoneLocked = true;
        public List<Contact> Contacts = new List<Contact>();
        public SimCard simCard;

        /// <summary>
        /// Removes the simcard from this phone.
        /// </summary>
        /// <returns></returns>
        public SimCard RemoveSimcard()
        {
            SimCard card = null;
            if(simCard != null)
            {
                card = simCard;
                simCard = null;
            }
            return card;
        }

        /// <summary>
        /// Checks if a simcard is inserted.
        /// </summary>
        /// <returns></returns>
        public bool ContainsSimCard()
        {
            return simCard == null ? false : true;
        }


        /// <summary>
        /// Returns the number of calls left available on this phone.
        /// </summary>
        /// <returns></returns>
        public int RemainingCalls()
        {
            return simCard != null ? simCard.CallsLeft : 0;
        }

        /// <summary>
        /// Returns the number of texts left available on this phone.
        /// </summary>
        /// <returns></returns>
        public int RemainingTexts()
        {
            return simCard != null ? simCard.TextsLeft : 0;
        }

        /// <summary>
        /// Top up the simcard on this phone.
        /// </summary>
        public void TopUp(int Calls, int Texts)
        {
            if(simCard != null)
            {
                simCard.CallsLeft += Calls;
                simCard.TextsLeft += Texts;
            }
        }

    }
}
