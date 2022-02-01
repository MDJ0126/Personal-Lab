using RestSharp;
using System;

namespace Network
{
    public class HandlerExample : ProtocolHandler
    {
        public HandlerExample()
        {
            this.URI = new Uri("example");
            this.Method = Method.Get;
        }

        public void Request(Action<bool> onFinished)
        {
            RestRequest req = new RestRequest(URI, Method);
            req.AddHeader("Accept", "application/json");

            base.RequestProcess(req, onFinished);
        }

        protected override void Response(RestRequest req, RestResponse res)
        {
            if (res.IsSuccessful)
            {

            }
            else
            {
                if (res.ErrorMessage != null)
                    Logger.Error(res.ErrorMessage);
            }
        }
    }
}
