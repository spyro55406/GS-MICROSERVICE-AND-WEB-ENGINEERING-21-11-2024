using MongoDB.Driver;
using Microsoft.Extensions.Options;
using GS_Microservices.Models;
namespace GS_Microservices.Services
{
    public class EnergyService
    {
        private readonly IMongoCollection<Consumo> _consumos;

        public EnergyService(IOptions<DatabaseSettings> databaseSettings)
        {
            var client = new MongoClient(databaseSettings.Value.ConnectionString);
            var database = client.GetDatabase(databaseSettings.Value.DatabaseName);
            _consumos = database.GetCollection<Consumo>("Consumos");
        }

        public async Task<Consumo> AddConsumoAsync(Consumo consumo)
        {
            await _consumos.InsertOneAsync(consumo);
            return consumo;
        }

        public async Task<Consumo> GetConsumoAsync(string id)
        {
            return await _consumos.Find(consumo => consumo.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Consumo>> GetAllConsumosAsync()
        {
            return await _consumos.Find(consumo => true).ToListAsync();
        }
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
