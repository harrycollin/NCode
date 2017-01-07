using NCode;
using System;

namespace NCode.KleosTypes.Items
{
    [Serializable]
    public class DebitCard : NetworkObject
    {
        public int AccountNumber;
        public int SortCode;
        public DateTime IssueDate;
        public DateTime ExpiryDate;
        public int PinCode;
        public bool Contactless;

        public DebitCard(Guid Owner, int _accountNumber, int _sortCode, DateTime _issueDate, DateTime _expiryDate, int _pinCode, bool _contactless)
        {
            owner = owner;
            AccountNumber = _accountNumber;
            SortCode = _sortCode;
            IssueDate = _issueDate;
            ExpiryDate = _expiryDate;
            PinCode = _pinCode;
            Contactless = _contactless;
        }
    }
}
