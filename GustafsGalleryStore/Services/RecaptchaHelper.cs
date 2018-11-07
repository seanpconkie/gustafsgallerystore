using System;
using System.IO;
using System.Net;
using GustafsGalleryStore.Helpers;
using Newtonsoft.Json.Linq;

namespace GustafsGalleryStore.Services
{
    public static class RecaptchaHelper
    {
        public static bool IsReCaptchValid(string recaptchaResponse)
        {
            var result = false;
            var captchaResponse = recaptchaResponse;
            var secretKey = MasterStrings.RecaptchaSecretKey;
            var apiUrl = "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}";
            var requestUri = string.Format(apiUrl, secretKey, captchaResponse);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    JObject jResponse = JObject.Parse(stream.ReadToEnd());
                    var isSuccess = jResponse.Value<bool>("success");
                    result = (isSuccess) ? true : false;
                }
            }
            return result;
        }
    }
}
