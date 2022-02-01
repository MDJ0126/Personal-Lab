using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading;

namespace Network
{
    public class WebManager : Singleton<WebManager>
    {
        private Semaphore semaphore = new Semaphore(0, 10);

        public static readonly string BASE_URL = "https://api.upbit.com/v1/";

        private List<ProtocolHandler> handlers = new List<ProtocolHandler>();

        private RestClient restClient = new RestClient(BASE_URL);

        protected override void Install()
        {
            // Add Handlers
            //this.Handlers.Add(new HandlerTest());
        }

        protected override void Release()
        {
            this.handlers.Clear();
            this.handlers = null;
        }

        /// <summary>
        /// 요청 프로세스
        /// </summary>
        /// <param name="request"></param>
        /// <param name="onResponse"></param>
        private async void Request(RestRequest request, Action<RestResponse> onResponse)
        {
            // 뮤텍스 취득할 때까지 대기
            semaphore.WaitOne();

            // Request
            request.AddHeader("Authorization", GetAuthToken());
            RestResponse response = await restClient.ExecuteAsync(request);
            
            // Response
            onResponse?.Invoke(response);

            // 뮤텍스 해제
            semaphore.Release();
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
        private string GetAuthToken(string parameter = "")
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