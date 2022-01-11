using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequesterLib;

namespace RequesterLibTests
{
    [TestClass]
    public class RequesterTests
    {
        private readonly Requester _requester;

        public RequesterTests()
        {
            _requester = new Requester.Builder().Build();
        }

        [TestMethod]
        public async Task TestGetRequest()
        {
            var response = await _requester.GetStringAsync("https://api.my-ip.io/ip");
            Assert.IsFalse(string.IsNullOrWhiteSpace(response));
        }
    }
}
