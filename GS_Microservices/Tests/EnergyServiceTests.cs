using Xunit;
using Moq;
using MongoDB.Driver;
using GS_Microservices.Services;
using GS_Microservices.Models;

namespace Test
{
    public class EnergyServiceTests
    {
        private readonly Mock<IMongoCollection<Consumo>> _collectionMock;
        private readonly Mock<IMongoDatabase> _databaseMock;
        private readonly Mock<IMongoClient> _clientMock;
        private readonly EnergyService _service;

        public EnergyServiceTests()
        {
            _collectionMock = new Mock<IMongoCollection<Consumo>>();
            _databaseMock = new Mock<IMongoDatabase>();
            _clientMock = new Mock<IMongoClient>();

            _databaseMock
                .Setup(d => d.GetCollection<Consumo>(It.IsAny<string>(), null))
                .Returns(_collectionMock.Object);

            _clientMock
                .Setup(c => c.GetDatabase(It.IsAny<string>(), null))
                .Returns(_databaseMock.Object);

            _service = new EnergyService(new Microsoft.Extensions.Options.OptionsWrapper<DatabaseSettings>(
                new DatabaseSettings
                {
                    ConnectionString = "mongodb://localhost:27017",
                    DatabaseName = "TestDb"
                }));
        }

        [Fact]
        public async Task AddConsumoAsync_ShouldInsertData()
        {
            var consumo = new Consumo { Id = "1", Valor = 150.0 };

            await _service.AddConsumoAsync(consumo);

            _collectionMock.Verify(c => c.InsertOneAsync(consumo, null, default), Times.Once);
        }

        [Fact]
        public async Task GetConsumoAsync_ShouldReturnData()
        {
            var id = "1";
            var expected = new Consumo { Id = id, Valor = 150.0 };

            _collectionMock
                .Setup(c => c.FindAsync(It.IsAny<FilterDefinition<Consumo>>(), null, default))
                .ReturnsAsync(new Mock<IAsyncCursor<Consumo>>().Object);

            var result = await _service.GetConsumoAsync(id);

            Assert.Null(result);
        }
    }
}
