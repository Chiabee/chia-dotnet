using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace chia.dotnet
{
    internal static class Converters
    {
        public static T ToObject<T>(JObject j, string childItem)
        {
            Debug.Assert(j is not null);

            var token = string.IsNullOrEmpty(childItem) ? j : j.GetValue(childItem);

            return ToObject<T>(token);
        }

        public static T ToObject<T>(JToken token)
        {
            Debug.Assert(token is not null);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }

            };

            return token.ToObject<T>(JsonSerializer.Create(serializerSettings));
        }

        public static T ToObject<T>(this string json)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }

        public static IEnumerable<string> ToStrings(dynamic stringEnumerable)
        {
            Debug.Assert(stringEnumerable is not null);

            return ((IEnumerable<dynamic>)stringEnumerable).Select<dynamic, string>(item => item.ToString());
        }

        public static DateTime ToDateTime(this double epoch)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, 0); //from start epoch time
            return start.AddSeconds(epoch); //add the seconds to the start DateTime
        }

        public static string ToJson(this object o)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            return JsonConvert.SerializeObject(o, serializerSettings);
        }
    }
}
