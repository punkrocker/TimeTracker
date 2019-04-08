using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Model;

namespace TT.Util
{
    public class WebCall
    {
        public static string BaseUrl = "http://222222/api/";

        public static string Login = "Login";
        public static string GetProjects = "GetProjects";
        public static string GetCustomerTeams = "GetCustomerTeams";
        public static string CommitProjects = "CommitProjects";

        public static T GetMethod<T>(string methodName, List<KeyValuePair<string, string>> paramlist)
        {
            var param = new StringBuilder();
            if (null != paramlist)
            {
                for (var index = 0; index < paramlist.Count; index++)
                {
                    param.Append(index == 0 ? "?" : "&");

                    param.Append(paramlist[index].Key);
                    param.Append("=");
                    param.Append(paramlist[index].Value);
                }
            }

            using (var client = new HttpClient())
            {
                var result = client.GetAsync(new Uri(
                        BaseUrl + methodName + param,
                        UriKind.Absolute))
                    .Result.Content.ReadAsStringAsync()
                    .Result;
                var resultInfo = AppUtils.JsonDeserialize<T>(result);
                return resultInfo;
            }
        }

        public static string PostMethod<T>(string methodName, T param)
        {
            var requestJson = AppUtils.JsonSerializer(param);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                HttpContent httpContent =
                    new StringContent(requestJson);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var result =
                    client.PostAsync(methodName, httpContent).Result.Content.ReadAsStringAsync().Result;
                return result;
            }
        }

        public static void LoadUrl()
        {
            using (var client = new HttpClient())
            {
                Configuration config =
                    System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                BaseUrl = config.AppSettings.Settings["url"].Value;
            }
        }
    }
}
