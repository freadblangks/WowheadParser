using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WOWSharp.Community
{
	/// <summary>
	///   A Blizzard's battle.net community API client
	/// </summary>
	public abstract class ApiClient : ApiResponse
    {
        private const string RegionNameHttpHeader = "X-WOWSharpProxy-Region";
        private const string ApiUrlHttpHeader = "X-WOWSharpProxy-Url";
        private const string LocaleHttpHeader = "X-WOWSharpProxy-Locale";
        private static Uri _proxyUri;
        /// <summary>
        /// Causes the API to throw serialization exceptions when 
        /// the JSON returned by Blizzard's API contains a property 
        /// not found in the class being deserialized. 
        /// This is useful to detect changes by Blizzard API since they have the habit of changing 
        /// things without announcing it. 
        /// This property is set to true by unit tests
        /// </summary>
        public static bool TestMode
        {
            get;
            set;
        }

        /// <summary>
        ///   Reference date for Unix time
        /// </summary>
        private static readonly DateTime _unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///   The API key used to authenticate the request
        /// </summary>
        private readonly string _apiKey = "USRTT87wBB5fgG5VRLyWvMq2WZExpOq6e9";

        /// <summary>
        ///   An object that implements _cacheManager
        /// </summary>
        private readonly ICacheManager _cacheManager;

        /// <summary>
        ///   Gets the locale which is used to get item names
        /// </summary>
        private string _locale;

        /// <summary>
        ///   Gets the region to which this ApiClient connects
        /// </summary>
        private Region _region;

        /// <summary>
        ///   Constructor. Initializes a new instance of the ApiClient class
        /// </summary>
        /// <remarks>
        ///   The default constructor will use the default region and locale determined by the current thread's culture.
        /// </remarks>
        protected ApiClient()
            : this((Region) null, null, null)
        {
        }

        /// <summary>
        ///   Constructor. Initializes a new instance of the ApiClient class
        /// </summary>
        /// <param name="region"> Regional battle.net Community website to which the ApiClient should connect to perform request. </param>
        protected ApiClient(Region region)
            : this(region, null, null)
        {
        }

        /// <summary>
        ///   Constructor. Initializes a new instance of the ApiClient class
        /// </summary>
        /// <param name="region"> Regional battle.net Community website to which the ApiClient should connect to perform request. </param>
        /// <param name="locale"> the locale to use for retrieving data </param>
        protected ApiClient(Region region, string locale)
            : this(region, locale, null)
        {
        }

        /// <summary>
        ///   Constructor. Initializes a new instance of the ApiClient class
        /// </summary>
        /// <param name="region"> Regional battle.net Community website to which the ApiClient should connect to perform request. </param>
        /// <param name="locale"> the locale to use for retrieving data </param>
        /// <param name="cacheManager"> Cache manager to cache data </param>
        protected ApiClient(Region region, string locale, ICacheManager cacheManager)
        {
			Region = region;

			if (region == null)
			{
				Region = Region.US;
			}

            Locale = Region.GetSupportedLocale(locale);

            if (cacheManager == null)
                cacheManager = new FileCacheManager();

            _cacheManager = cacheManager;
        }

        /// <summary>
        ///   Constructor. Initializes a new instance of the ApiClient class
        /// </summary>
        /// <param name="region"> Regional battle.net Community website to which the ApiClient should connect to perform request. </param>
        /// <param name="apiKey"> Application key used to authenticate requests sent by the ApiClient </param>
        /// <param name="locale"> The locale to use to perform request (item names, class names, etc are retrieved in the locale specified) </param>
        /// <remarks>
        ///   Only Locales supported by the regional website that the ApiClient is connecting to are supported. If a wrong local is passed, default language is used.
        /// </remarks>
        protected ApiClient(string region, string locale, string apiKey)
            : this(Region.GetRegion(region), locale, apiKey, null)
        {
        }

        /// <summary>
        ///   Constructor. Initializes a new instance of the ApiClient class
        /// </summary>
        /// <param name="region"> Regional battle.net Community website to which the ApiClient should connect to perform request. </param>
        /// <param name="apiKey"> Application key used to authenticate requests sent by the ApiClient </param>
        /// <param name="locale"> The locale to use to perform request (item names, class names, etc are retrieved in the locale specified) </param>
        /// <param name="cacheManager"> Cache manager to cache data </param>
        /// <remarks>
        ///   Only Locales supported by the regional website that the ApiClient is connecting to are supported. If a wrong local is passed, default language is used.
        /// </remarks>
        protected ApiClient(string region, string locale, string apiKey, ICacheManager cacheManager)
            : this(Region.GetRegion(region), locale, apiKey, cacheManager)
        {
        }

        /// <summary>
        ///   Constructor. Initializes a new instance of the ApiClient class
        /// </summary>
        /// <param name="region"> Regional battle.net Community website to which the ApiClient should connect to perform request. </param>
        /// <param name="apiKey"> Application key used to authenticate requests sent by the ApiClient </param>
        /// <param name="locale"> The locale to use to perform request (item names, class names, etc are retrieved in the locale specified) </param>
        /// <param name="cacheManager"> Cache manager to cache data </param>
        /// <remarks>
        ///   Only Locales supported by the regional website that the ApiClient is connecting to are supported. If a wrong local is passed, default language is used.
        /// </remarks>
        protected ApiClient(Region region, string locale, string apiKey, ICacheManager cacheManager)
            : this(region, locale, cacheManager)
        {
            _apiKey = apiKey;
        }

        /// <summary>
        ///   Gets or sets proxy Host address. 
        ///   In case of running in silverlight without elevated privilates a proxy hosted on the host web application
        ///   is needed to perform requests
        /// </summary>
        public static Uri ProxyUri
        {
            get
            {
                return _proxyUri;
            }
            set
            {
                _proxyUri = value;
            }
        }

        /// <summary>
        ///   Gets the region to which this ApiClient connects
        /// </summary>
        public Region Region
        {
            get
            {
                return _region;
            }
            private set
            {
                _region = value;
            }
        }

        /// <summary>
        ///   Gets the locale which is used to get item names
        /// </summary>
        public string Locale
        {
            get
            {
                return _locale;
            }
            private set
            {
                _locale = value;
            }
        }

        /// <summary>
        ///   Creates a GET request
        /// </summary>
        /// <param name="client">HttpClient object</param>
        /// <param name="path">request relative url</param>
        /// <returns>Request Url.</returns>
        private Uri SetupRequest(HttpClient client, string path)
        {
            if (ProxyUri != null)
            {
                client.DefaultRequestHeaders.Add(RegionNameHttpHeader, Region.Name);
                client.DefaultRequestHeaders.Add(ApiUrlHttpHeader, path);
                client.DefaultRequestHeaders.Add(LocaleHttpHeader, Locale);

                return ProxyUri;
            }
            else
            {
                // Blizzard recommends that SSL is used when authenticating using the API key
                var uri = new Uri("https://" + Region.Host + path);

                uri = !string.IsNullOrEmpty(uri.Query)
                              ? new Uri(uri + "&namespace=" + "static-us")
                              : new Uri(uri + "?namespace=" + "static-us");

                if (!uri.AbsolutePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    uri = !string.IsNullOrEmpty(uri.Query)
                              ? new Uri(uri + "&locale=" + Locale.Replace('-', '_'))
                              : new Uri(uri + "?locale=" + Locale.Replace('-', '_'));
                }


					uri = !string.IsNullOrEmpty(uri.Query)
						  ? new Uri(uri + "&access_token=" + _apiKey)
						  : new Uri(uri + "?access_token=" + _apiKey);

                return uri;
            }
        }

        /// <summary>
        /// Performs Http Get request asynchronously
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="path">relative URL of the object to get</param>
        /// <param name="objectToRefresh">object to refresh</param>
        /// <returns>A task object for the async HTTP get</returns>
        internal async Task<T> GetAsync<T>(string path, T objectToRefresh) where T : class
        {
            var objResult = await GetAsync(path, typeof(T), objectToRefresh).ConfigureAwait(false);
            return (T)objResult;
        }

        /// <summary>
        /// Performs Http Get request asynchronously
        /// </summary>
        /// <param name="path">relative URL of the object to get</param>
        /// <param name="objectType">object type</param>
        /// <param name="objectToRefresh">object to refresh</param>
        /// <returns>A task object for the async HTTP get</returns>
        internal async Task<object> GetAsync(string path, Type objectType, object objectToRefresh)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            if (_cacheManager != null && objectToRefresh == null)
            {
                object cachedObject = await GetObjectFromCacheAsync(path, objectType);

                if (cachedObject != null)
                {
                    return cachedObject;
                }
            }

            var handler = new HttpClientHandler();
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var client = new HttpClient(handler))
            {
                var objectApiResponse = objectToRefresh as ApiResponse;
                // Set If-Modified-Since header
                if (objectApiResponse != null && path == objectApiResponse.Path && objectApiResponse.LastModifiedUtc != DateTime.MinValue)
                {
                    client.DefaultRequestHeaders.IfModifiedSince = objectApiResponse.LastModifiedUtc;
                }
				
                var uri = SetupRequest(client, path);
                try
                {
                    var responseMessage = await client.GetAsync(uri).ConfigureAwait(false);

                    if (responseMessage.StatusCode == HttpStatusCode.NotModified)
                    {
                        return objectToRefresh;
                    }
                    else if (responseMessage.IsSuccessStatusCode)
                    {
                        object obj;
                        if (typeof(ApiResponse).IsAssignableFrom(objectType))
                        {
                            obj = await DeserializeResponse(path, objectType, responseMessage);
                            ((ApiResponse)obj).Path = path;
                            ((ApiResponse)obj).LastModifiedUtc = DateTime.UtcNow;
                        }
                        else
                        {
                            obj = await responseMessage.Content.ReadAsByteArrayAsync();
                        }
                        if (_cacheManager != null)
                        {
                            try
                            {
                                await _cacheManager.AddDataAsync(Region.Name + "/" + Locale + "/" + path, obj).ConfigureAwait(false);
                            }
                            catch (CacheManagerException)
                            {
                                // if we failed to add item to cache, swallow and return normally
                            }
                        }
                        return obj;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Deserializers
        /// </summary>
        /// <param name="path"></param>
        /// <param name="objectType"></param>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        private async Task<object> DeserializeResponse(string path, Type objectType, HttpResponseMessage responseMessage)
        {
            var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            
            var val = await responseMessage.Content.ReadAsStringAsync();

            if (val == null)
                return null;

            if (val.Contains("\"code\":404"))
                return null;

            return JsonConvert.DeserializeObject(val, objectType);
        }

        /// <summary>
        /// Creates a JSON serializer to serialize objects
        /// </summary>
        /// <param name="responseMessage">Http response message</param>
        /// <returns>Json serializer</returns>
        internal virtual JsonSerializer CreateJsonSerializer(HttpResponseMessage responseMessage)
        {
            // Store information about serialization context 
            // So that JsonApiResponseConverter would correctly 
            // populate ApiClient property for ApiResponse object
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JsonApiResponseConverter(this, responseMessage));

            // This is needed for Testing to make sure that new data added by blizzard is not missed
            // Blizzard has the habit of adding new things to the API without an announcement
            // And without a change of documentation
            if (TestMode)
            {
                serializer.MissingMemberHandling = MissingMemberHandling.Error;
            }

            return serializer;
        }
        
        

        /// <summary>
        /// Tries to fetch an object from cache
        /// </summary>
        /// <param name="path">object path</param>
        /// <returns>The cached object if found or null otherwise.</returns>
        private async Task<object> GetObjectFromCacheAsync(string path, Type objectType)
        {
            object cachedObject = null;
            try
            {
                cachedObject = await _cacheManager.LookupDataAsync(Region.Name
                                                        + "/" + Locale + "/" + path, objectType);
            }
            catch (CacheManagerException)
            {
                // if cacheManager fail swallow and continue without caching
                return null;
            }
            return cachedObject;
        }

        /// <summary>
        ///   Gets the Utc DateTime object from the Unix time returned by the API
        /// </summary>
        /// <param name="value"> time value returned by API </param>
        /// <returns> Utc time object </returns>
        internal static DateTime GetUtcDateFromUnixTimeMilliseconds(long value)
        {
			return _unixStartDate.AddMilliseconds(value);
        }

        /// <summary>
        ///   Gets the Unit time value from the Date time object
        /// </summary>
        /// <param name="date"> date time object </param>
        /// <returns> Unix time value </returns>
        internal static long GetUnixTimeFromDateMilliseconds(DateTime date)
        {
            return (long) Math.Round((date - _unixStartDate).TotalMilliseconds, 0);
        }

        /// <summary>
        ///   Gets the Utc DateTime object from the Unix time returned by the API
        /// </summary>
        /// <param name="value"> time value returned by API </param>
        /// <returns> Utc time object </returns>
        internal static DateTime GetUtcDateFromUnixTimeSeconds(long value)
        {
			return _unixStartDate.AddSeconds(value);
        }

        /// <summary>
        ///   Gets the Unit time value from the Date time object
        /// </summary>
        /// <param name="date"> date time object </param>
        /// <returns> Unix time value </returns>
        internal static long GetUnixTimeFromDateSeconds(DateTime date)
        {
            return (long)Math.Round((date - _unixStartDate).TotalSeconds, 0);
        }

        #region Nested type: AuthenticationSupport

        /// <summary>
        ///   Authentication support class
        /// </summary>
        private class AuthenticationSupport
        {
            /// <summary>
            ///   System.Security.Cryptography.HMACSHA1.ComputeHash
            /// </summary>
            public MethodInfo ComputeHashMethod;

            /// <summary>
            ///   System.Security.Cryptography.HMACSHA1
            /// </summary>
            public Type HashType;
        }

        #endregion
    }
}