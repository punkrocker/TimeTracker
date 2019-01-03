using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Model
{
    public class AppUtils
    {
        public static T JsonDeserialize<T>(string jsonString)
        {
            var list = (T)JsonConvert.DeserializeObject(jsonString, typeof(T));
            return list;
        }

        public static string JsonSerializer<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }

        public static string SHA1Hash(string origin)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes_sha1_in = UTF8Encoding.Default.GetBytes(origin);
            byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
            sha1.Clear();
            (sha1 as IDisposable).Dispose();
            return Convert.ToBase64String(bytes_sha1_out);
        }

        public static string ToString(string str)
        {
            return str == null ? string.Empty : Convert.ToString(str);
        }
    }
}
