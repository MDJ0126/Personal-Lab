using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading;

namespace Network
{
    public class ProtocolManager : Singleton<ProtocolManager>
    {
        public static readonly string BASE_URL = "https://api.upbit.com/v1/";

        private List<ProtocolHandler> handlers = new List<ProtocolHandler>();

        private RestClient restClient = new RestClient(BASE_URL);

        private class RequestInfo
        {
            public RestRequest request;
            public Action<RestResponse> onResponse;
            public RequestInfo(RestRequest request, Action<RestResponse> onResponse)
            {
                this.request = request;
                this.onResponse = onResponse;
            }
        }

        private Queue<RequestInfo> restRequestDelegates = new Queue<RequestInfo>();

        protected override void Install()
        {
            MultiThread.Start(RequestProcess);
            // 여러개를 사용하면 속도가 빨라지는데, 필요할 때 다르게 사용하자
            //MultiThread.Start(RequestProcess);
            //MultiThread.Start(RequestProcess);
            //MultiThread.Start(RequestProcess);
            //MultiThread.Start(RequestProcess);
        }

        protected override void Release()
        {
            this.handlers.Clear();
            this.handlers = null;
        }

        /// <summary>
        /// 요청 등록
        /// </summary>
        /// <param name="request"></param>
        /// <param name="onResponse"></param>
        private void Request(RestRequest request, Action<RestResponse> onResponse)
        {
            restRequestDelegates.Enqueue(new RequestInfo(request, onResponse));
        }

        /// <summary>
        /// 요청 프로세스 (쓰레드)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="onResponse"></param>
        private async void RequestProcess()
        {
            while (true)
            {
                if (restRequestDelegates.Count > 0)
                {
                    // 리스트 꺼내기
                    var dequeue = restRequestDelegates.Dequeue();
                    if (dequeue != null)
                    {
                        // Request
                        RestResponse response = await restClient.ExecuteAsync(dequeue.request);

                        // Response
                        dequeue.onResponse.Invoke(response);
                    }
                }
                else
                {
                    // 반복 대기 (0.1초마다 Queue 확인)
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 핸들러 가져오기
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetHandler<T>() where T : ProtocolHandler, new()
        {
            T protocolHandler;

            var enumerator = Instance.handlers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                protocolHandler = enumerator.Current as T;
                if (protocolHandler != null)
                    return protocolHandler;
            }

            // if handler is null
            protocolHandler = new T();
            protocolHandler.restRequest = Instance.Request;
            Instance.handlers.Add(protocolHandler);
            return protocolHandler;
        }

        /// <summary>
        /// Authorization
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetAuthToken(string parameter = "")
        {
            var payload = new JwtPayload
            {
                { "access_key", Config.ACCESS_KEY },
                { "nonce", Guid.NewGuid().ToString() },
                { "query_hash", parameter },
                { "query_hash_alg", "SHA512" }
            };

            byte[] keyBytes = Encoding.Default.GetBytes(Config.SECRET_KEY);
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes);
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, "HS256");
            var header = new JwtHeader(credentials);
            var secToken = new JwtSecurityToken(header, payload);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(secToken);
            return "Bearer " + jwtToken;
        }
    }
}