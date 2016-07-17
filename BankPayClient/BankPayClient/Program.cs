using System;
using System.Net.Sockets;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace BankPayClient
{
    public struct RespObj
    {
        public int order_id { get; set; }
        public int status { get; set; }
    }

    class Program
    {
        //All data is in memory.

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

        public const int INAPPROPRIATE_EXP_DATE = 12;


        public static int Pay(int order_id, String card_number, int expiry_month, int expiry_year, String cvv, UInt64 amount_kop, String cardholder_name = "SOMEBODY")
        {
            return SendPostRequest( "127.0.0.1", "POST /api/pay", "order_id=" + order_id.ToString() + "&card_number=" + card_number + "&expiry_month=" + expiry_month.ToString() + "&expiry_year=" + expiry_year.ToString() + "&cvv=" + cvv + "&cardholder_name=" + cardholder_name + "&amount_kop=" + amount_kop.ToString(), 8880);
        }

        public static int GetStatus(int order_id)
        {
            return SendGetRequest("127.0.0.1", "GET /api/getStatus?order_id=" + order_id.ToString() + " HTTP/1.1\r\nHost: localhost\r\nUser-Agent: bankDemoApp\r\n",  8880);
        }

        public static int Refund(int order_id)
        {
            return SendGetRequest("127.0.0.1", "GET /api/refund?order_id=" + order_id.ToString() + " HTTP/1.1\r\nHost: localhost\r\nUser-Agent: bankDemoApp\r\n", 8880);
        }

        static void Main(string[] args)
        {
            int res = -1;
            Console.WriteLine("");
            Console.WriteLine("\n\nTrying to pay an order #543 (3000000000 kop) with a debit card (9/18) that has 3M RUR...");
            res = Pay(543, "000000000003", 9, 18, "003", 3000000000);

            Console.WriteLine("\n\nTrying to get a status of order #543...");
            res = GetStatus(543);

            Console.WriteLine("\n\nTrying to pay an order #123 (20000 kop) with a credit card (9/16) that has 43900 RUR and 10000 RUR credit limit...");
            res = Pay(123, "000000000001", 9, 16, "001", 20000);

            Console.WriteLine("\n\nTrying to pay an order #123 (20000 kop) with a credit card (9/16) that has 43700 RUR and 10000 RUR credit limit...");
            res = Pay(123, "000000000001", 9, 16, "001", 20000);

            Console.WriteLine("\n\nTrying to pay an order #890 (4500000 kop) with a credit card (9/16) that has 43700 RUR and 10000 RUR credit limit...");
            res = Pay(890, "000000000001", 9, 16, "001", 4500000);

            Console.WriteLine("\n\nTrying to pay an order #891 (4500000 kop) with a credit card (9/16) that has -1300 RUR and 10000 RUR credit limit...");
            res = Pay(891, "000000000001", 9, 16, "001", 4500000);

            Console.WriteLine("\n\nTrying to pay an order #342 (100000 kop) with a debit card (3/16) that has 160 RUR...");
            res = Pay(342, "000000000002", 3, 16, "002", 100000, "SecondHolder");

            Console.WriteLine("\n\nTrying to get a status of order #123...");
            res = GetStatus(123);

            Console.WriteLine("\n\nTrying to refund order #123...");
            res = Refund(123);

            Console.WriteLine("\n\nTrying to refund order #123 again...");
            res = Refund(123);

            Console.WriteLine("\n\nTrying to get a status of order #123...");
            res = GetStatus(123);

            Console.WriteLine("\n\nTrying to pay an order #332 (10000 kop) with a debit card (3/16) that has 160 RUR...");
            res = Pay(332, "000000000002", 3, 16, "002", 10000, "SecondHolder");

            Console.WriteLine("\n\nTrying to get a status of order #342...");
            res = GetStatus(342);

            Console.WriteLine("\n\nTrying to get a status of order #332...");
            res = GetStatus(332);

            Console.WriteLine("\n\nTrying to pay an order #406 (17000 kop) with a credit card (2/17) that has 160 RUR and an unlimited credit limit...");
            res = Pay(406, "000000000005", 2, 17, "005", 17000, "FifthHolder");

            Console.WriteLine("\n\nTrying to pay an order #407 (17000 kop) with a credit card (2/17) that has -10 RUR and an unlimited credit limit...");
            res = Pay(407, "000000000005", 2, 17, "005", 17000, "FifthHolder");

            Console.WriteLine("\n\nTrying to pay an order #408 (17000 kop) with a credit card (2/17) that has -180 RUR and an unlimited credit limit...");
            res = Pay(408, "000000000005", 2, 17, "005", 17000, "FifthHolder");

            Console.WriteLine("\n\nTrying to pay an order #409 (17000 kop) with a non-existent credit card...");
            res = Pay(409, "000000000006", 2, 17, "006", 17000, "SixthHolder");

            Console.WriteLine("\n\nTrying to pay an order #410 (1000 kop) with debit card (10/16). Sending invalid expiry date (10/17)...");
            res = Pay(410, "000000000004", 10, 17, "004", 1000, "FourthHolder");


            Console.ReadKey();
        }

        static void DisplayResponce(RespObj responseObject)
        {
            Console.WriteLine("Order ID: {0}\n", responseObject.order_id);
            string verboseStatus = "";
            switch (responseObject.status)
            {
                case 0:
                    verboseStatus = "OK";
                    break;
                case 1:
                    verboseStatus = "BANK ERROR";
                    break;
                case 2:
                    verboseStatus = "INVALID CARD NUMBER";
                    break;
                case 3:
                    verboseStatus = "INVALID CVV";
                    break;
                case 4:
                    verboseStatus = "CARD EXPIRED";
                    break;
                case 5:
                    verboseStatus = "CARD LIMIT EXCEEDED";
                    break;
                case 6:
                    verboseStatus = "REQUIRED ORDER NUMBER EXISTS";
                    break;
                case 7:
                    verboseStatus = "PAYMENT STATUS IS: DONE";
                    break;
                case 8:
                    verboseStatus = "PAYMENT STATUS IS: NOT DONE";
                    break;
                case 9:
                    verboseStatus = "PAYMENT STATUS IS: REFUNDED";
                    break;
                case 10:
                    verboseStatus = "PAYMENT STATUS IS: NOT FOUND";
                    break;
                case 11:
                    verboseStatus = "ALREADY REFUNDED";
                    break;
                case 12:
                    verboseStatus = "INAPPROPRIATE EXPIRY DATE";
                    break;
            }
            Console.WriteLine("Status: {0}\n", verboseStatus);

        }

        static int SendGetRequest(String server, String message,  Int32 port)
        {
            try
            {
                TcpClient client = new TcpClient(server, port);

                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                

                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}\r\n", message);

                data = new Byte[256];

                String respData = String.Empty;
                Int32 bytes = 0;
                try
                {
                    bytes = stream.Read(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Connection is interrupted.\n"+ex.Message);
                }
                respData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Response string: {0}", respData);

                RespObj responseObject = new RespObj();
                JavaScriptSerializer jss = new JavaScriptSerializer();

                responseObject = jss.Deserialize<RespObj>(respData);
                DisplayResponce(responseObject);
                
                

                stream.Close();
                client.Close();
                return responseObject.status;
            }
            catch (ArgumentNullException e)
            {
                Debug.Write("ArgumentNullException: {0}" + e);
                return -1;
            }
            catch (SocketException e)
            {
                Debug.Write("SocketException: {0}" + e);
                return -1;
            }
        }

        static int SendPostRequest(String server, String message, String body, Int32 port)
        {
            TcpClient client = new TcpClient("127.0.0.1", port);
            NetworkStream stream = client.GetStream();
            String fullMessage = message + body;

            Byte[] byteData = System.Text.Encoding.ASCII.GetBytes(fullMessage);
            
            List<String> header = new List<String>();
            header.Add(message);

            header.Add(" HTTP/1.1 ");
            header.Add("Host: " + "127.0.0.1 ");
            header.Add("User-Agent: " + "bankDemoApp ");
            
            String CONTENT = "\r\n" + body;
            header.Add("Content-Type: " + "text/json ");
            header.Add("Content-Length: " + CONTENT.Length + " ");
            header.Add(CONTENT);

            String dataToSend = "";
            for (int i = 0; i < header.Count; i++)
            {
                dataToSend = dataToSend + header[i] + "\r\n";
            }
            dataToSend = dataToSend + "\r\n";

            Byte[] send = Encoding.ASCII.GetBytes(dataToSend);
            stream.Write(send, 0, send.Length);
            Console.Write("Sent: {0}\r\n", dataToSend);
            stream.Flush();
            Byte[] bytes = new byte[client.ReceiveBufferSize];
            int count = stream.Read(bytes, 0, (int)client.ReceiveBufferSize);
            String data = Encoding.ASCII.GetString(bytes);

            RespObj responseObject = new RespObj();
            JavaScriptSerializer jss = new JavaScriptSerializer();

            int trimIndex = data.LastIndexOf('}');

            data = data.Substring(0, trimIndex + 1);

            responseObject = jss.Deserialize<RespObj>(data);
            DisplayResponce(responseObject);

            stream.Close();
            client.Close();
                        
            return responseObject.status;
        }
    }
}