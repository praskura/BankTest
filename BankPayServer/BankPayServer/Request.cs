using System;

namespace BankPayServer
{
    public class Request
    {
        public String Type { get; set; }
        public String URL { get; set; }
        public String Host { get; set; }
        public String Body { get; set; }

        public Request(String type, String url, String host)
        {
            Type = type;
            URL = url;
            Host = host;
        }

        public Request(String type, String url, String host, String body)
        {
            Type = type;
            URL = url;
            Host = host;
            Body = body;
        }

        public static Request GetRequest(String request)
        {
            if (String.IsNullOrEmpty(request))
                return null;

            String[] tokens;
            String type;
            String url;
            String host;
            String body;

            if (request.StartsWith("GET"))
            {
                //handle GET
                tokens = request.Split(' ');
                type = tokens[0];
                url = tokens[1];
                host = tokens[4];
                return new Request(type, url, host);
            }
            else if (request.StartsWith("POST"))
            {
                //handle POST
                tokens = request.Split(' ');
                type = tokens[0];
                url = tokens[1];
                host = tokens[4];
                body = tokens[11];
                return new Request(type, url, host, body);
            }

            return null;
        }
    }
}