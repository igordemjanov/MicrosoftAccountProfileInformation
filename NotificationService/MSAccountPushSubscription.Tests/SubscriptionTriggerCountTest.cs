using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSAccountPushSubscription.Models;
using MSAccountPushSubscription.Repositories;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MSAccountPushSubscription.Tests
{
    [TestClass]
    public class SubscriptionTriggerCountTest
    {
        private ILogger log;
        private DefaultHttpRequest request;
        private PushSubscriptionInformation sub;
        private DocumentClient client;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            log = NullLoggerFactory.Instance.CreateLogger("Dummy");

            sub = new PushSubscriptionInformation();
            sub.EndPoint = "https://dummy_endpoint";
            sub.ExpirationTime = null;
            sub.Keys = new Keys();
            sub.Keys.Authentication = "dummy_authentication";
            sub.Keys.p256dh = "dummy_p256dh";

            Environment.SetEnvironmentVariable("DatabaseId", TestContext.Properties["DatabaseId"].ToString(), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CollectionId", TestContext.Properties["CollectionId"].ToString(), EnvironmentVariableTarget.Process);

            client = new DocumentClient(new Uri(TestContext.Properties["AccountEndpoint"].ToString()), TestContext.Properties["AccountKey"].ToString());
            DocumentDBRepository<PushSubscriptionInformation>.Initialize(client);            
        }

        [TestMethod]
        public async Task NoSubscriptionsTest()
        {
            request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                QueryString = QueryString.Create("", "")
            };
            dynamic response = await SubscriptionTriggerCount.Run(request, client, log);
            Assert.AreEqual(response.Value, 0);
        }

        [TestMethod]
        public async Task SubscriptionsExistTest()
        {
            DocumentDBRepository<PushSubscriptionInformation>.CreateItemAsync(sub).Wait();
            request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                QueryString = QueryString.Create("", "")
            };
            dynamic response = await SubscriptionTriggerCount.Run(request, client, log);
            Assert.AreEqual(response.Value, 1);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            DocumentDBRepository<PushSubscriptionInformation>.Clean();
        }
    }
}
