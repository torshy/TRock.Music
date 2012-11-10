using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TRock.Music.EchoNest
{
    public class EchoNestHelpers
    {
        #region Fields

        public static readonly Uri EchoNestUri = new Uri("http://developer.echonest.com/api/v4/", UriKind.RelativeOrAbsolute);

        #endregion Fields

        #region Methods

        public static Task<dynamic> ExecuteQuery(dynamic options, UriQuery query)
        {
            foreach (string option in options)
            {
                if (options[option].HasValue)
                {
                    if (options[option].Value is IEnumerable<string>)
                    {
                        foreach (var value in ((IEnumerable<string>)options[option].Value))
                        {
                            query.Add(option, value);
                        }
                    }
                    else
                    {
                        query.Add(option, options[option].ToString());
                    }
                }
            }

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            return client
                .GetStringAsync(query.ToString())
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Trace.WriteLine((object)task.Exception);
                        return null;
                    }

                    return JsonConvert.DeserializeObject<dynamic>(task.Result);
                });
        }

        #endregion Methods

        #region Nested Types

        public class Playlist
        {
            #region Fields

            private static readonly Uri Uri = new Uri(EchoNestUri, "playlist/dynamic/");

            #endregion Fields

            #region Methods

            public static dynamic Create(string apiKey, dynamic options)
            {
                var query = new UriQuery(Uri.AbsoluteUri + "create")
                {
                    { "api_key", apiKey }
                };

                return ExecuteQuery(options, query).Result;
            }

            public static dynamic Next(string apiKey, string sessionId)
            {
                var query = new UriQuery(Uri.AbsoluteUri + "next")
                {
                    { "api_key", apiKey }, 
                    { "session_id", sessionId }
                };

                return ExecuteQuery(new DynamicDictionary(), query).Result;
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}