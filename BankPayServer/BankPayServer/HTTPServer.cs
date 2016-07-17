using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BankPayServer
{
    public class HTTPServer
    {
        //All data is in memory.

        //Cards amount - 5 cards, for example
        public const int CARDS_AMOUNT = 5;

        //credit cards.
        Card[] Cards = new Card[CARDS_AMOUNT];

        //orders
        List<Order> orders = new List<Order>();

        //Pay status codes
        public const int OK = 0;
        public const int BANK_ERROR = 1;
        public const int INVALID_CARD_NUMBER = 2;
        public const int INVALID_CVV = 3;
        public const int CARD_EXPIRED = 4;
        public const int LIMIT_EXCEEDED = 5;
        public const int ORDER_NUM_EXISTS = 6;

        //Order status codes
        public const int PAYMENT_DONE = 7;
        public const int PAYMENT_NOT_DONE = 8;
        public const int PAYMENT_REFUNDED = 9;
        public const int PAYMENT_NOT_FOUND = 10;
        public const int ALREADY_REFUNDED = 11;


        public int INAPPROPRIATE_EXP_DATE = 12;

        public const String VERSION = "HTTP/1.1";

        private bool running = false;

        private TcpListener listener;

        //check if card exists. If it exists, the function returns it's index. If not - -1
        private int checkIfCardExists(String number)
        {
            for (int i = 0; i < CARDS_AMOUNT; i++)
            {
                if (Cards[i].number == number)
                    return i;
            }
            return -1;
        }

        private int checkExpDate(Card card, int paramExpMonth, int paramExpYear)
        {
            if (card.expMonth != paramExpMonth || card.expYear != paramExpYear)
                return INAPPROPRIATE_EXP_DATE;
            if (card.expYear + 2000 < DateTime.Now.Year)
                return CARD_EXPIRED;
            else if (card.expYear + 2000 > DateTime.Now.Year)
                return OK;
            else
            {
                if (card.expMonth < DateTime.Now.Month)
                    return CARD_EXPIRED;
                else
                    return OK;
            }
        }

        private bool checkCVV(Card card, String cvv)
        {
            if (card.cvv == cvv)
                return true;
            return false;
        }

        private bool checkLimit(Card card, UInt64 amount_kop)
        {
            if (card.isCredit)
            {
                if (card.creditLimit == 0)
                    return true;
                if (card.limit + card.creditLimit < amount_kop)
                    return false;
                else
                    return true;
            }
            else if (card.limit >= amount_kop)
                return true;
            return false;
        }

        public int Pay(int order_id, String card_number, int expiry_month, int expiry_year, String cvv, UInt64 amount_kop, String cardholder_name = "SOME BODY")
        {
            //checks
            if (isOrderExists(order_id) != null)
                return ORDER_NUM_EXISTS;

            int cardIndex = -2;
            cardIndex = checkIfCardExists(card_number);
            if (cardIndex == -1)
                return INVALID_CARD_NUMBER;


            if (checkExpDate(Cards[cardIndex], expiry_month, expiry_year) == CARD_EXPIRED)
            {
                orders.Add(new Order(order_id, PAYMENT_NOT_DONE, amount_kop, Cards[cardIndex]));
                return CARD_EXPIRED;
            }
            else if (checkExpDate(Cards[cardIndex], expiry_month, expiry_year) == INAPPROPRIATE_EXP_DATE)
            {
                orders.Add(new Order(order_id, PAYMENT_NOT_DONE, amount_kop, Cards[cardIndex]));
                return INAPPROPRIATE_EXP_DATE;
            }


            if (!checkCVV(Cards[cardIndex], cvv))
            {
                orders.Add(new Order(order_id, PAYMENT_NOT_DONE, amount_kop, Cards[cardIndex]));
                return INVALID_CVV;
            }

            if (!checkLimit(Cards[cardIndex], amount_kop))
            {
                orders.Add(new Order(order_id, PAYMENT_NOT_DONE, amount_kop, Cards[cardIndex]));
                return LIMIT_EXCEEDED;
            }

            //if everything is OK

            Cards[cardIndex].limit -= amount_kop;

            orders.Add(new Order(order_id, PAYMENT_DONE, amount_kop, Cards[cardIndex]));

            return OK;
        }

        private Order isOrderExists(int order_id)
        {
            for (int i = 0; i < orders.Count; i++)
                if (orders[i].id == order_id)
                    return orders[i];
            return null;
        }

        public int GetStatus(int order_id)
        {
            //check
            Order requiredOrder = isOrderExists(order_id);
            if (requiredOrder == null)
                return PAYMENT_NOT_FOUND;

            return requiredOrder.status;
        }

        public int Refund(int order_id)
        {
            //check
            Order requiredOrder = isOrderExists(order_id);
            if (requiredOrder == null)
                return PAYMENT_NOT_FOUND;

            if (requiredOrder.status == PAYMENT_REFUNDED)
                return ALREADY_REFUNDED;

            requiredOrder.card.limit += requiredOrder.amountKop;
            requiredOrder.status = PAYMENT_REFUNDED;

            return OK;
        }

        public HTTPServer(int port)
        {
            InitializeCards();
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void InitializeCards()
        {
            Cards[0] = new Card("000000000001", 9, 16, "First Holder", "001", 4390000, true, 1000000);
            Cards[1] = new Card("000000000002", 3, 16, "Second Holder", "002", 16000, false);
            Cards[2] = new Card("000000000003", 9, 18, "Third Holder", "003", 300000000, false);
            Cards[3] = new Card("000000000004", 10, 16, "Fourth Holder", "004", 2700000, false);
            Cards[4] = new Card("000000000005", 2, 17, "Fifth Holder", "005", 320000, true, 0);
        }

        public void Start()
        {
            Thread serverThread = new Thread(new ThreadStart(Run));
            serverThread.Start();
        }

        private void Run()
        {
            running = true;
            listener.Start();
            while (running)
            {
                Console.WriteLine("Waiting for connections...");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected!");

                HandleCLient(client);

                client.Close();
            }

            running = false;

            listener.Stop();
        }

        private void HandleCLient(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream());

            String msg = "";
            while (reader.Peek() != -1)
            {
                msg += reader.ReadLine() + "\n";
            }

            Console.WriteLine("Request: " + msg);

            Request req = Request.GetRequest(msg);
            Response resp = Response.From(req, this);
            resp.Post(client.GetStream());
        }
    }
}