﻿namespace chia.dotnet.tests
{
    /// <summary>
    /// Use this class to setup the connection to the daemon under test
    /// </summary>
    internal static class Factory
    {
        /// <summary>
        /// Create a daemon instance from a hardcoded address
        /// </summary>
        /// <returns><see cref="DaemonProxy"/></returns>
        public static WebSocketRpcClient CreateRpcClientFromHardcodedLocation()
        {
            // this is an example using a WSL instance running locally
            var endpoint = new EndpointInfo()
            {
                Uri = new System.Uri("wss://172.26.210.216:55400"),
                CertPath = @"\\wsl$/Ubuntu-20.04/home/don/.chia/mainnet/config/ssl/daemon/private_daemon.crt",
                KeyPath = @"\\wsl$/Ubuntu-20.04/home/don/.chia/mainnet/config/ssl/daemon/private_daemon.key",
                //Uri = new System.Uri("wss://localhost:55400"),
                //CertPath = @"/home/don/.chia/mainnet/config/ssl/daemon/private_daemon.crt",
                //KeyPath = @"/home/don/.chia/mainnet/config/ssl/daemon/private_daemon.key",
            };
            return new WebSocketRpcClient(endpoint);
        }

        /// <summary>
        /// Create a daemon instance from the specified config file
        /// </summary>
        /// <param name="filePath">Full path to the chia config file</param>
        /// <returns><see cref="DaemonProxy"/></returns>
        public static WebSocketRpcClient CreateRpcClientFromConfig(string filePath)
        {
            var config = Config.Open(filePath);
            var endpoint = config.GetEndpoint("daemon");
            return new WebSocketRpcClient(endpoint);
        }
    }
}
