﻿using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Network
{
    /// <summary>
    /// 전체 계좌 조회
    /// </summary>
    public class ApiKeyRes : iResponse
    {
        /// <summary>
        /// 사용 중인 엑세스 키
        /// </summary>
        public string access_key;
        /// <summary>
        /// 사용기한
        /// </summary>
        public DateTime expire_at;
    }

    public class HandlerApiKey : ProtocolHandler
    {
        public HandlerApiKey()
        {
            this.URI = new Uri(WebManager.BASE_URL + "api_keys");
            this.Method = Method.Get;
        }

        public void Request(Action<bool> onFinished = null)
        {
            RestRequest req = new RestRequest(URI, Method);
            req.AddHeader("Accept", "application/json");
            base.RequestProcess(req, onFinished);
        }

        protected override void Response(RestRequest req, RestResponse res)
        {
            if (res.IsSuccessful)
            {
                List<ApiKeyRes> accountRes = JsonParser<ApiKeyRes>(res.Content);
            }
            else
            {
                if (res.ErrorMessage != null)
                    Logger.Error(res.ErrorMessage);
            }
        }
    }
}