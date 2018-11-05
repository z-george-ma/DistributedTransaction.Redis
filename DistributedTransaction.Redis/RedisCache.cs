using DistributedTransaction.Core;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTransaction.Redis
{
    public class RedisCache : ICache
    {
        private readonly Lazy<ConnectionMultiplexer> connection;
        public RedisCache(ConfigurationOptions options)
        {
            this.connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options), false);
        }

        public RedisCache(string connectionString)
        {
            this.connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString), false);
        }

        public IBlockingLockScope BeginBlockingLockScope(string lockKey, int ttl) =>
            new BlockingLockScope(this.connection.Value, lockKey, ttl);

        public INonBlockingLockScope BeginNonBlockingLockScope() =>
            new NonBlockingLockScope(this.connection.Value);

        public Task Delete(string key) => this.connection.Value.GetDatabase().KeyDeleteAsync(key);

        public Task<bool> Exists<IData>(string key) => this.connection.Value.GetDatabase().KeyExistsAsync(key);

        public Task Extend(string key, int ttl) => this.connection.Value.GetDatabase().KeyExpireAsync(key, TimeSpan.FromMilliseconds(ttl));

        public async Task<IData[]> Get(string[] keys, Type[] types)
        {
            var ret = await this.connection.Value.GetDatabase().StringGetAsync(keys.Cast<RedisKey>().ToArray());
            return ret.Select((x, i) => ToObject(x, types[i])).ToArray();
        }

        private static IData ToObject(string value, Type type)
        {
            if (value == null)
                return null;

            return (IData)JsonConvert.DeserializeObject(value);
        }
        
    }
}
