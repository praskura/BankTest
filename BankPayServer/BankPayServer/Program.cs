using System;

namespace BankPayServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server at port 8880!");
            HTTPServer server = new HTTPServer(8880);
            server.Start();
        }
    }
}