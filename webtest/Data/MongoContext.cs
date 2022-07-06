using Microsoft.Extensions.Options;
using MongoBlogApp.Contracts;
using MongoDB.Driver;

namespace MongoBlogApp.Contracts.Core.Data
{
	public class MongoContext
	{
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoContext(IOptions<DatabaseSettings> dbOptions)
        {
            var settings = dbOptions.Value;
            _client = new MongoClient(settings.ConnectionString);
            _database = _client.GetDatabase(settings.DatabaseName);
        }

        public IMongoClient Client => _client;

        public IMongoDatabase Database => _database;
    }
}

