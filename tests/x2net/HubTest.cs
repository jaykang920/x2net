using Xunit;

namespace x2net.tests
{
    public class HubTest
    {
        [Fact]
        public void TestSingleton()
        {
            Hub instance = Hub.Instance;
            Assert.NotNull(instance);
        }
    }
}
