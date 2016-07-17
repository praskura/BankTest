using System;

namespace BankPayServer
{
    public class Order
    {
        private int _id;
        private int _status;
        private UInt64 _amountKop;
        private Card _card;

        public int id
        {
            get { return _id; }
            set { _id = value; }
        }
        public int status
        {
            get { return _status; }
            set { _status = value; }
        }

        public UInt64 amountKop
        {
            get { return _amountKop; }
            set { _amountKop = value; }
        }

        public Card card
        {
            get { return _card; }
            set { _card = value; }
        }

        public Order(int id, int status, UInt64 amountKop, Card card)
        {
            _id = id;
            _status = status;
            _amountKop = amountKop;
            _card = card;
        }
    }
}