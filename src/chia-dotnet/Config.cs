﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using YamlDotNet.Serialization;

namespace chia.dotnet
{
    /// <summary>
    /// Represents a chia config yaml file and its contents. 
    /// Used to find the uri and ssl certs needed to connect 
    /// </summary>
    public sealed class Config
    {
        private readonly string _chiaRootPath;

        internal Config(string chiaRootPath, dynamic config)
        {
            _chiaRootPath = chiaRootPath;
            Contents = config;
        }

        /// <summary>
        /// The contents of the config yaml
        /// </summary>
        public dynamic Contents { get; init; }

        /// <summary>
        /// Creates an <see cref="EndpointInfo"/> from the named service section
        /// </summary>
        /// <param name="serviceName">The setion name in the config file. Use 'daemon' for the root config that include 'self_hostname'; i.e. the local daemon</param>
        /// <returns>An <see cref="EndpointInfo"/> that can be used to connect to the given service's RPC interface</returns>
        public EndpointInfo GetEndpoint(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            // this allows a ServicesNames member to used
            serviceName = serviceName.Replace("chia_", "");

            UriBuilder builder = new();
            dynamic ssl;

            // any daemon connection uses wss:
            if (serviceName == "ui") // this is the daemon that the ui connects to
            {
                builder.Scheme = "wss";
                dynamic section = Contents.ui;
                builder.Host = section.daemon_host;
                builder.Port = Convert.ToInt32(section.daemon_port);
                ssl = section.daemon_ssl;
            }
            else if (serviceName is "daemon" or "chia plots create") // this is the local daemon
            {
                builder.Scheme = "wss";
                builder.Host = Contents.self_hostname;
                builder.Port = Convert.ToInt32(Contents.daemon_port);
                ssl = Contents.daemon_ssl;
            }
            else // all other endpoints are https direct connections
            {
                builder.Scheme = "https";
                builder.Host = Contents.self_hostname;

                var d = Contents as IDictionary<string, object>;
                dynamic section = d[serviceName];

                builder.Port = Convert.ToInt32(section.rpc_port);
                ssl = section.ssl;
            }

            return new EndpointInfo
            {
                Uri = builder.Uri,
                CertPath = Path.Combine(_chiaRootPath, ssl.private_crt),
                KeyPath = Path.Combine(_chiaRootPath, ssl.private_key)
            };
        }

        /// <summary>
        /// The OS specific default location of the chia root folder (respects CHIA_ROOT)
        /// </summary>
        public static string DefaultRootPath => Environment.GetEnvironmentVariable("CHIA_ROOT") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".chia", "mainnet");

        /// <summary>
        /// Opens a chia config yaml file
        /// </summary>
        /// <param name="fullPath">The full filesystem path to the config file</param>
        /// <returns>The config <see cref="Config"/> instance</returns>
        public static Config Open(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"confgi file {fullPath} not found");
            }
            using var input = new StreamReader(fullPath);
            var deserializer = new DeserializerBuilder()
                .WithTagMapping("tag:yaml.org,2002:set", typeof(YamlSet<object>))
                .Build();
            var yamlObject = deserializer.Deserialize(input);
            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            var json = serializer.Serialize(yamlObject);
            dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            var chiaRoot = Directory.GetParent(Path.GetDirectoryName(fullPath));

            return new Config(chiaRoot.FullName, config);
        }

        /// <summary>
        /// Opens the <see cref="Config"/> from <see cref="DefaultRootPath"/> plus 'config' and 'config.yaml'
        /// </summary>
        /// <returns>The user's chia install <see cref="Config"/> instance</returns>
        public static Config Open()
        {
            return Open(Path.Combine(DefaultRootPath, "config", "config.yaml"));
        }

        // sigh... YAML
        // https://stackoverflow.com/questions/32757084/yamldotnet-how-to-handle-set
        private class YamlSet<T> : HashSet<T>, IDictionary<T, object>
        {
            void IDictionary<T, object>.Add(T key, object value)
            {
                _ = Add(key);
            }

            bool IDictionary<T, object>.ContainsKey(T key)
            {
                throw new NotImplementedException();
            }

            ICollection<T> IDictionary<T, object>.Keys => throw new NotImplementedException();

            bool IDictionary<T, object>.Remove(T key)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<T, object>.TryGetValue(T key, out object value)
            {
                throw new NotImplementedException();
            }

            ICollection<object> IDictionary<T, object>.Values => throw new NotImplementedException();

            object IDictionary<T, object>.this[T key] { get => throw new NotImplementedException(); set => _ = Add(key); }

            void ICollection<KeyValuePair<T, object>>.Add(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<T, object>>.Clear()
            {
                throw new NotImplementedException();
            }

            bool ICollection<KeyValuePair<T, object>>.Contains(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<T, object>>.CopyTo(KeyValuePair<T, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            int ICollection<KeyValuePair<T, object>>.Count => Count;

            bool ICollection<KeyValuePair<T, object>>.IsReadOnly => throw new NotImplementedException();

            bool ICollection<KeyValuePair<T, object>>.Remove(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            IEnumerator<KeyValuePair<T, object>> IEnumerable<KeyValuePair<T, object>>.GetEnumerator()
            {
                IDictionary<T, object> dict = new Dictionary<T, object>();
                var keys = new T[Count];
                CopyTo(keys);
                foreach (var k in keys)
                {
                    dict.Add(k, null);
                }
                return dict.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
