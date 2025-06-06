using MongoDB.Driver;
using Telexistence.Models;

namespace Telexistence.Repositories
{
    public class MongoCommandRepository : ICommandRepository
    {
        private readonly IMongoCollection<RobotCommand> _collection;

        public MongoCommandRepository(IConfiguration config)
        {
            var client = new MongoClient(config["Mongo:ConnectionString"]);
            var db = client.GetDatabase("telexistence");
            _collection = db.GetCollection<RobotCommand>("commands");
        }

        public async Task SaveCommandAsync(RobotCommand command) =>
            await _collection.ReplaceOneAsync(
                x => x.Id == command.Id,
                command,
                new ReplaceOptions { IsUpsert = true }
            );

        public async Task<List<RobotCommand>> GetCommandHistoryAsync(string robotId) =>
            await _collection.Find(x => x.RobotId == robotId).ToListAsync();

        public async Task<RobotCommand?> GetCommandByIdAsync(string id) =>
            await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task UpdateCommandAsync(RobotCommand command) =>
            await _collection.ReplaceOneAsync(
                x => x.Id == command.Id,
                command,
                new ReplaceOptions { IsUpsert = false }
            );
    }
}
