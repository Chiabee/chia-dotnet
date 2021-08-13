﻿using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace chia.dotnet.tests
{
    [TestClass]
    [TestCategory("Integration")]
    [Ignore("Needs a coloured coin wallet")]
    public class ColouredCoinWalletTests
    {
        private static ColouredCoinWallet _theWallet;

        [ClassInitialize]
        public static async Task Initialize(TestContext context)
        {
            var rpcClient = Factory.CreateRpcClientFromHardcodedLocation();
            await rpcClient.Connect();

            var daemon = new DaemonProxy(rpcClient, "unit_tests");            
            await daemon.RegisterService();

            var walletProxy = new WalletProxy(rpcClient, "unit_tests");

            // SET this wallet ID to a coloroured coin wallet 
            _theWallet = new ColouredCoinWallet(2, walletProxy);
            _ = await _theWallet.Login();
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            _theWallet.WalletProxy.RpcClient?.Dispose();
        }

        [TestMethod()]
        public async Task GetColouredCoinName()
        {
            var name = await _theWallet.GetName();

            Assert.IsNotNull(name);
        }

        [TestMethod()]
        public async Task GetColouredCoinColour()
        {
            var colour = await _theWallet.GetColour();

            Assert.IsNotNull(colour);
        }
    }
}
