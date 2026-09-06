using System.Data;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigRepository : IBasicRepository<Config>
    {
        IDbConnection OpenConnection();
        Config Get(string key);
        Config Upsert(string key, string value);
        void Upsert(string key, string value, IDbConnection connection, IDbTransaction transaction);
    }

    public class ConfigRepository : BasicRepository<Config>, IConfigRepository
    {
        public ConfigRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Config Get(string key)
        {
            return Query(c => c.Key == key).SingleOrDefault();
        }

        public IDbConnection OpenConnection()
        {
            return _database.OpenConnection();
        }

        public Config Upsert(string key, string value)
        {
            var dbValue = Get(key);

            if (dbValue == null)
            {
                return Insert(new Config { Key = key, Value = value });
            }

            dbValue.Value = value;

            return Update(dbValue);
        }

        public void Upsert(string key, string value, IDbConnection connection, IDbTransaction transaction)
        {
            var rowsUpdated = connection.Execute("UPDATE \"Config\" SET \"Value\" = @Value WHERE \"Key\" = @Key", new { Key = key, Value = value }, transaction);

            if (rowsUpdated == 0)
            {
                connection.Execute("INSERT INTO \"Config\" (\"Key\", \"Value\") VALUES (@Key, @Value)", new { Key = key, Value = value }, transaction);
            }
        }
    }
}
