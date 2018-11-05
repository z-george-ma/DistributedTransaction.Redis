using DistributedTransaction.Core;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace DistributedTransaction.Redis
{
    internal class NonBlockingLockScope : INonBlockingLockScope
    {
        private readonly StackExchange.Redis.ITransaction transaction;

        public NonBlockingLockScope(ConnectionMultiplexer connection)
        {
            this.transaction = connection.GetDatabase().CreateTransaction();
        }

        public Task<bool> Execute() => this.transaction.ExecuteAsync();

        public void Set(string key, IData value) => 
            this.transaction.StringSetAsync(key, FromObject(value), TimeSpan.FromMilliseconds(value.CacheTTL));

        public void When(string key, IData value) =>
            this.transaction.AddCondition(Condition.StringEqual(key, FromObject(value)));

        public void When(string key, string value) =>
            this.transaction.AddCondition(Condition.StringEqual(key, value));

        public void WhenNotExist(string key) =>
            this.transaction.AddCondition(Condition.KeyNotExists(key));

        private static string FromObject(IData value) =>
            JsonConvert.SerializeObject(value);

    }
}
