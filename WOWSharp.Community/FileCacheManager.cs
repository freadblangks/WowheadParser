using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;

namespace WOWSharp.Community
{
    public class FileCacheManager : ICacheManager
    {
        
#if DEBUG
        public static string CacheDirectory = "C:\\WoWTools\\Wowheadparser\\cache\\";
#else
        public static string CacheDirectory = ".\\cache\\";
#endif
        
        public async Task AddDataAsync(string key, object value)
        {
            await Task.Run(() =>
                {
                key = GetCacheFilePath(key);

                if (File.Exists(key))
                    File.Delete(key);

                if (value is string strVal)
                {
                    File.WriteAllText(key, strVal);
                }
                else
                {
                    File.WriteAllText(key, JsonConvert.SerializeObject(value));
                }
            });
        }

        public async Task<object> LookupDataAsync(string key, System.Type objectType)
        {
            key = GetCacheFilePath(key);
            string val = null;
            
            await Task.Run(() =>
            {
                if (File.Exists(key))
                {
                    val = File.ReadAllText(key);
                }
            });

            if (val == null)
                return null;

            if (objectType == typeof(string))
                return val as string;

            // Execute the command and get the value
            return JsonConvert.DeserializeObject(val.ToString(), objectType);
        }

        private string GetCacheFilePath(string key)
        {
            var urlDest = key.LastIndexOf("/", StringComparison.Ordinal);

            if (urlDest == -1)
                urlDest = key.LastIndexOf("\\", StringComparison.Ordinal);
            
            if (urlDest != -1)
                key = key.Substring(urlDest + 1);

            if (!Directory.Exists(CacheDirectory))
                Directory.CreateDirectory(CacheDirectory);

            return Path.Combine(CacheDirectory, MakeFileNameSafe(key));
        }

        private string MakeFileNameSafe(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '-');
            }

            return fileName;
        }
    }
}
