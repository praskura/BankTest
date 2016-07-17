using System;

namespace BankPayServer
{
    public class Card
    {
        private String _number;
        private int _expMonth;
        private int _expYear;
        private String _cardHolderName;
        private String _cvv;
        private UInt64 _limit;
        private bool _isCredit;
        private UInt64 _creditLimit;

        public Card(String number, int expMonth, int expYear, String cardHolderName, String cvv, UInt64 limitRur, bool isCredit, UInt64 creditLimit = 0)
        {
            _number = number;
            _expMonth = expMonth;
            _expYear = expYear;
            _cardHolderName = cardHolderName;
            _cvv = cvv;
            _limit = limitRur;
            _isCredit = isCredit;
            _creditLimit = creditLimit;
        }

        public String number
        {
            get { return _number; }
            set { _number = value; }
        }

        public int expMonth
        {
            get { return _expMonth; }
            set { _expMonth = value; }
        }

        public int expYear
        {
            get { return _expYear; }
            set { _expYear = value; }
        }

        public String cardHolderName
        {
            get { return _cardHolderName; }
            set { _cardHolderName = value; }
        }

        public String cvv
        {
            get { return _cvv; }
            set { _cvv = value; }
        }

        public UInt64 limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        public bool isCredit
        {
            get { return _isCredit; }
            set { _isCredit = value; }
        }

        public UInt64 creditLimit
        {
            get { return _creditLimit; }
            set { _creditLimit = value; }
        }

    }
}