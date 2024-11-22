using Xunit;
using Moq;
using StackExchange.Redis;
using GS_Microservices.Services;

namespace Test
{
    public class RedisCacheServiceTests
    {
        private readonly Mock<IDatabase> _databaseMock;
        private readonly Mock<IConnectionMultiplexer> _connectionMock;
        private readonly RedisCacheService _service;

        public RedisCacheServiceTests()
        {
            _databaseMock = new Mock<IDatabase>();
            _connectionMock = new Mock<IConnectionMultiplexer>();

            _connectionMock
                .Setup(c => c.GetDatabase(It.IsAny<int>(), null))
                .Returns(_databaseMock.Object);

            _service = new RedisCacheService(new Microsoft.Extensions.Options.OptionsWrapper<RedisSettings>(
                new RedisSettings
                {
                    ConnectionString = "localhost:6379"
                }));
        }

        [Fact]
        public void SetCache_ShouldStoreData()
        {
            var key = "key";
            var value = "value";
            var expiry = TimeSpan.FromMinutes(5);

            _service.SetCache(key, value, expiry);

            _databaseMock.Verify(db => db.StringSet(key, value, expiry, false, When.Always, CommandFlags.None), Times.Once);
        }

        [Fact]
        public void GetCache_ShouldRetrieveData()
        {
            var key = "key";
            var expectedValue = "value";

            _databaseMock.Setup(db => db.StringGet(key, CommandFlags.None)).Returns(expectedValue);

            var result = _service.GetCache<string>(key);

            Assert.Equal(expectedValue, result);
        }
    }
}
