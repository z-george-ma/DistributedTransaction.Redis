using DistributedTransaction.Core;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace DistributedTransaction.Redis
{
    internal class BlockingLockScope : IBlockingLockScope
    {
        private readonly ConnectionMultiplexer conn;
        private readonly string lockKey;
        private readonly int ttl;
        private bool acquired = false;
        public BlockingLockScope(ConnectionMultiplexer conn, string lockKey, int ttl)
        {
            this.conn = conn;
            this.lockKey = lockKey;
            this.ttl = ttl;
        }

        public string LockId { get; } = Guid.NewGuid().ToString();

        public async Task<bool> Acquire()
        {
            this.acquired = await this.conn.GetDatabase().StringSetAsync(this.lockKey, this.LockId, TimeSpan.FromMilliseconds(this.ttl), When.NotExists);
            return this.acquired;
        }

        public void Dispose()
        {
            if (this.acquired)
            {
                var db = this.conn.GetDatabase();
                var tx = db.CreateTransaction();
                tx.AddCondition(Condition.StringEqual(this.lockKey, this.LockId));
                tx.KeyDeleteAsync(this.lockKey);
                tx.ExecuteAsync(); // or sync?
            }
        }

        public async Task<bool> Renew()
        {
            if (this.acquired)
            {
                var db = this.conn.GetDatabase();
                var tx = db.CreateTransaction();
                tx.AddCondition(Condition.StringEqual(this.lockKey, this.LockId));
                var task = tx.KeyExpireAsync(this.lockKey, TimeSpan.FromMilliseconds(this.ttl));
                return await tx.ExecuteAsync();
            }

            return false;
        }
    }
}
