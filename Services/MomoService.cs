using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DoAnQuanLyKhachSan.Models.MoMo;

namespace DoAnQuanLyKhachSan.Services
{
    public class MomoService
    {
        private readonly MomoOptionModel _options;

        public MomoService()
        {
            _options = new MomoOptionModel
            {
                MomoApiUrl = ConfigurationManager.AppSettings["MomoApiUrl"],
                SecretKey = ConfigurationManager.AppSettings["SecretKey"],
                AccessKey = ConfigurationManager.AppSettings["AccessKey"],
                ReturnUrl = ConfigurationManager.AppSettings["ReturnUrl"],
                NotifyUrl = ConfigurationManager.AppSettings["NotifyUrl"],
                PartnerCode = ConfigurationManager.AppSettings["PartnerCode"],
                RequestType = ConfigurationManager.AppSettings["RequestType"]
            };

            if (string.IsNullOrEmpty(_options.SecretKey) || string.IsNullOrEmpty(_options.AccessKey))
                throw new Exception("Thiếu cấu hình MoMo trong web.config!");
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(int bookingId, decimal amount, string orderInfo)
        {
            if (string.IsNullOrEmpty(orderInfo))
                orderInfo = $"Thanh toan don {bookingId}";

            string orderId = bookingId.ToString();
            string requestId = Guid.NewGuid().ToString();

            long amountInt = (long)amount; // Ép kiểu long để chắc chắn

            string rawHash =
                $"partnerCode={_options.PartnerCode}&accessKey={_options.AccessKey}&requestId={requestId}&amount={amountInt}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={_options.ReturnUrl}&notifyUrl={_options.NotifyUrl}&requestType={_options.RequestType}";

            string signature = SignSHA256(rawHash, _options.SecretKey);

            var data = new
            {
                partnerCode = _options.PartnerCode,
                accessKey = _options.AccessKey,
                requestId,
                amount = amountInt.ToString(),
                orderId,
                orderInfo,
                returnUrl = _options.ReturnUrl,
                notifyUrl = _options.NotifyUrl,
                requestType = _options.RequestType,
                signature
            };

            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_options.MomoApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine("MoMo Response: " + responseContent);

                return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(responseContent);
            }
        }

        private string SignSHA256(string rawData, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }
    }
}
