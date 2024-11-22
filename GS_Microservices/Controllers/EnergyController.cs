using Microsoft.AspNetCore.Mvc;
using GS_Microservices.Services;
using GS_Microservices.Models;

namespace GS_Microservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyController : ControllerBase
    {
        private readonly EnergyService _energyService;
        private readonly RedisCacheService _redisCache;

        public EnergyController(EnergyService energyService, RedisCacheService redisCache)
        {
            _energyService = energyService;
            _redisCache = redisCache;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { message = "Service is up and running" });
        }

        [HttpPost("consumo")]
        public async Task<IActionResult> PostConsumo([FromBody] Consumo consumo)
        {
            if (consumo == null || consumo.Valor <= 0)
            {
                return BadRequest("Consumo inválido.");
            }

            var result = await _energyService.AddConsumoAsync(consumo);
            return CreatedAtAction(nameof(GetConsumo), new { id = result.Id }, result);
        }

        [HttpGet("consumo/{id}")]
        public async Task<IActionResult> GetConsumo(string id)
        {

            var cacheData = _redisCache.GetCache<string>(id);
            if (cacheData != null)
            {
                return Ok(new { cached = true, data = cacheData });
            }


            var consumo = await _energyService.GetConsumoAsync(id);
            if (consumo == null)
            {
                return NotFound();
            }


            _redisCache.SetCache(id, consumo.ToString(), TimeSpan.FromMinutes(5));

            return Ok(consumo);
        }
    }
}
