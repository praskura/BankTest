using System;
using System.IO;
using System.Net.Sockets;
using System.Web.Script.Serialization;

namespace BankPayServer
{
    public struct RespObj
    {
        public int order_id { get; set; }
        public int status { get; set; }
    }

    public class Response
    {
        private Byte[] data = null;
        private String status;
        private String mime;

        private Response(String status, String mime, Byte[] data)
        {
            this.status = status;
            this.data = data;
            this.mime = mime;
        }

        private static Byte[] createInvalidRequestParamsErrorString()
        {
            string errorMsg = "There are invalid params in the request";
            Byte[] returnData = new Byte[errorMsg.Length];
            returnData = System.Text.Encoding.ASCII.GetBytes(errorMsg);
            return returnData;
        }

        public static Response From(Request request, HTTPServer server)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            String errorMsg;
            String jsonString;
            int res = -1;
            Byte[] returnData;
            RespObj responseObject = new RespObj();
            int orderId;

            if (request == null)
                return null;

            if (request.Type == "GET")
            {
                int lastIndexOfQuestionSymbol = request.URL.LastIndexOf('?');

                if (request.URL.Contains("/api/getStatus?") && request.URL.Contains("order_id="))
                {
                    orderId = -1;
                    if (!int.TryParse(request.URL.Substring(request.URL.LastIndexOf('=') + 1), out orderId))
                    {
                        returnData = createInvalidRequestParamsErrorString();
                        return new Response("400 Bad Request", "text/plain", returnData);
                    }

                    int result = server.GetStatus(orderId);
                    responseObject.order_id = orderId;
                    responseObject.status = result;
                    jsonString = jss.Serialize(responseObject);
                    returnData = new Byte[jsonString.Length];
                    returnData = System.Text.Encoding.ASCII.GetBytes(jsonString);

                    return new Response("200 OK", "application/json", returnData);

                }
                else if (request.URL.Contains("/api/refund?") && request.URL.Contains("order_id="))
                {
                    orderId = -1;
                    if (!int.TryParse(request.URL.Substring(request.URL.LastIndexOf('=') + 1), out orderId))
                    {
                        returnData = createInvalidRequestParamsErrorString();
                        return new Response("400 Bad Request", "text/plain", returnData);
                    }
                    int result = server.Refund(orderId);
                    responseObject.order_id = orderId;
                    responseObject.status = result;
                    jsonString = jss.Serialize(responseObject);
                    returnData = new Byte[jsonString.Length];
                    returnData = System.Text.Encoding.ASCII.GetBytes(jsonString);
                    return new Response("200 OK", "application/json", returnData);
                }
            }
            else if (request.Type == "POST")
            {
                int lastIndexOfEqualsSymbol = request.Body.LastIndexOf('=');
                String[] parameters = request.Body.Split('&');
                orderId = -1;
                int expMonth = -1;
                int expYear = -1;
                String cardNumber = "";
                String cardHolderName = "";
                UInt64 amountKop = 0;
                String cvv = "";

                if (!int.TryParse(parameters[0].Substring(parameters[0].LastIndexOf('=') + 1), out orderId))
                {
                    returnData = createInvalidRequestParamsErrorString();
                    return new Response("400 Bad Request", "text/plain", returnData);
                }
                cardNumber = parameters[1].Substring(parameters[1].LastIndexOf('=') + 1);
                if (!int.TryParse(parameters[2].Substring(parameters[2].LastIndexOf('=') + 1), out expMonth))
                {
                    returnData = createInvalidRequestParamsErrorString();
                    return new Response("400 Bad Request", "text/plain", returnData);
                }
                if (!int.TryParse(parameters[3].Substring(parameters[3].LastIndexOf('=') + 1), out expYear))
                {
                    returnData = createInvalidRequestParamsErrorString();
                    return new Response("400 Bad Request", "text/plain", returnData);
                }
                cvv = parameters[4].Substring(parameters[4].LastIndexOf('=') + 1);
                if (request.Body.Contains("cardholder_name="))
                {
                    cardHolderName = parameters[5].Substring(parameters[5].LastIndexOf('=') + 1);
                    if (!UInt64.TryParse(parameters[6].Substring(parameters[6].LastIndexOf('=') + 1), out amountKop))
                    {
                        returnData = createInvalidRequestParamsErrorString();
                        return new Response("400 Bad Request", "text/plain", returnData);
                    }
                }
                else
                {
                    if (!UInt64.TryParse(parameters[5].Substring(parameters[5].LastIndexOf('=') + 1), out amountKop))
                    {
                        returnData = createInvalidRequestParamsErrorString();
                        return new Response("400 Bad Request", "text/plain", returnData);
                    }
                }

                if (request.Body.Contains("cardholder_name="))
                    res = server.Pay(orderId, cardNumber, expMonth, expYear, cvv, amountKop, cardHolderName);
                else
                    res = server.Pay(orderId, cardNumber, expMonth, expYear, cvv, amountKop);

                //FORM A CORRECT JSON RESPONSE
                responseObject.order_id = orderId;
                responseObject.status = res;

                jsonString = jss.Serialize(responseObject);

                returnData = new Byte[jsonString.Length];
                returnData = System.Text.Encoding.ASCII.GetBytes(jsonString);
                return new Response("200 OK", "application/json", returnData);
            }
            else
            {
                errorMsg = "Required method is not allowed on this server.";
                returnData = new Byte[errorMsg.Length];
                returnData = System.Text.Encoding.ASCII.GetBytes(errorMsg);
                return new Response("405 Method Not Allowed", "text/plain", returnData);
            }
            returnData = createInvalidRequestParamsErrorString();
            return new Response("400 Bad Request", "text/plain", returnData);
        }

        public void Post(NetworkStream stream)
        {
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(String.Format("{0} {1}\r\nServer: {2}\r\nContent-Type: {3}\r\nAccept-Ranges: bytes\r\nContent-Length: {4}\r\n",
                HTTPServer.VERSION, status, "SimpleBankServer", mime, data.Length));
            stream.Write(data, 0, data.Length);
        }
    }
}