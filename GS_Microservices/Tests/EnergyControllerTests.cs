using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using GS_Microservices.Controllers;
using GS_Microservices.Services;
using GS_Microservices.Models;

namespace Test
{
    public class EnergyControllerTests
    {
        private readonly Mock<EnergyService> _energyServiceMock;
        private readonly Mock<RedisCacheService> _redisCacheMock;
        private readonly EnergyController _controller;

        public EnergyControllerTests()
        {
            _energyServiceMock = new Mock<EnergyService>();
            _redisCacheMock = new Mock<RedisCacheService>();
            _controller = new EnergyController(_energyServiceMock.Object, _redisCacheMock.Object);
        }

        [Fact]
        public void Health_ShouldReturnOk()
        {

            var result = _controller.Health();
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);
            Assert.Contains("Service is up", okResult.Value.ToString());
        }

        [Fact]
        public async Task PostConsumo_ValidData_ShouldReturnCreated()
        {
            var consumo = new Consumo { Id = "1", Valor = 150.0 };

            _energyServiceMock.Setup(s => s.AddConsumoAsync(It.IsAny<Consumo>())).ReturnsAsync(consumo);

            var result = await _controller.PostConsumo(consumo);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(consumo, createdResult.Value);
        }

        [Fact]
        public async Task PostConsumo_InvalidData_ShouldReturnBadRequest()
        {
            var consumo = new Consumo { Id = "1", Valor = 0 };

            var result = await _controller.PostConsumo(consumo);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Consumo inválido.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetConsumo_FromCache_ShouldReturnCachedData()
        {
            var id = "1";
            var cachedData = "{ \"Id\": \"1\", \"Valor\": 150.0 }";

            _redisCacheMock.Setup(c => c.GetCache<string>(id)).Returns(cachedData);

            var result = await _controller.GetConsumo(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Contains("cached", okResult.Value.ToString());
        }

        [Fact]
        public async Task GetConsumo_NotFound_ShouldReturn404()
        {
            var id = "non-existent";

            _redisCacheMock.Setup(c => c.GetCache<string>(id)).Returns((string)null);
            _energyServiceMock.Setup(s => s.GetConsumoAsync(id)).ReturnsAsync((Consumo)null);

            var result = await _controller.GetConsumo(id);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}
