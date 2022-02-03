using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Network
{
    /// <summary>
    /// 현재가 정보: 요청 당시 종목의 스냅샷을 반환한다.
    /// </summary>
    public class TickerRes : iResponse
    {
        /// <summary>
        /// 종목 구분 코드
        /// </summary>
        public string market;
        /// <summary>
        /// 최근 거래 일자(UTC)
        /// </summary>
        public DateTime trade_date;
        /// <summary>
        /// 최근 거래 시각(UTC)
        /// </summary>
        public DateTime trade_time;
        /// <summary>
        ///  최근 거래 일자(KST)
        /// </summary>
        public DateTime trade_date_kst;
        /// <summary>
        /// 최근 거래 시각(KST)
        /// </summary>
        public DateTime trade_time_kst;
        /// <summary>
        /// 시가
        /// </summary>
        public double opening_price;
        /// <summary>
        /// 고가
        /// </summary>
        public double high_price;
        /// <summary>
        /// 저가
        /// </summary>
        public double low_price;
        /// <summary>
        /// 종가
        /// </summary>
        public double trade_price;
        /// <summary>
        /// 전일 종가
        /// </summary>
        public double prev_closing_price;
        /// <summary>
        /// EVEN : 보합
        /// RISE : 상승
        /// FALL : 하락
        /// </summary>
        public string change;
        /// <summary>
        /// 변화액의 절대값
        /// </summary>
        public double change_price;
        /// <summary>
        /// 변화율의 절대값
        /// </summary>
        public double change_rate;
        /// <summary>
        /// 부호가 있는 변화액
        /// </summary>
        public double signed_change_price;
        /// <summary>
        /// 부호가 있는 변화율
        /// </summary>
        public double signed_change_rate;
        /// <summary>
        /// 가장 최근 거래량
        /// </summary>
        public double trade_volume;
        /// <summary>
        /// 누적 거래대금(UTC 0시 기준)
        /// </summary>
        public double acc_trade_price;
        /// <summary>
        /// 24시간 누적 거래대금
        /// </summary>
        public double acc_trade_price_24h;
        /// <summary>
        /// 누적 거래량(UTC 0시 기준)
        /// </summary>
        public double acc_trade_volume;
        /// <summary>
        /// 24시간 누적 거래량
        /// </summary>
        public double acc_trade_volume_24h;
        /// <summary>
        /// 52주 신고가
        /// </summary>
        public double highest_52_week_price;
        /// <summary>
        /// 52주 신고가 달성일
        /// </summary>
        public DateTime highest_52_week_date;
        /// <summary>
        /// 52주 신저가
        /// </summary>
        public double lowest_52_week_price;
        /// <summary>
        /// 52주 신저가 달성일
        /// </summary>
        public DateTime lowest_52_week_date;
        /// <summary>
        /// 타임스탬프
        /// </summary>
        public long timestamp;
    }

    public class HandlerTicker : ProtocolHandler
    {
        public HandlerTicker()
        {
            this.URI = new Uri(ProtocolManager.BASE_URL + "ticker?markets=BTC-ETH"); // <- 코드 수정해야함 POST와 GET 방식으로 설계되도록
            this.Method = Method.Get;
        }

        public void Request(Action<bool> onFinished = null)
        {
            RestRequest request = new RestRequest(URI, Method);
            request.AddHeader("Accept", "application/json");
            base.RequestProcess(request, onFinished);
        }

        protected override void Response(RestRequest req, RestResponse res)
        {
            if (res.IsSuccessful)
            {
                List<TickerRes> tickerRes = JsonParser<TickerRes>(res.Content);
            }
            else
            {
                if (res.ErrorMessage != null)
                    Logger.Error(res.ErrorMessage);
            }
        }
    }
}
