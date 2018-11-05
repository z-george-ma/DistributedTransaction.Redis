using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTransaction.Core
{
    public interface ITransaction
    {
        void Update<T>(T value) where T : IData;
        Task Discard();
    }

    public class Transaction : ITransaction
    {
        private readonly List<(Task Task, Type Type, string Key, bool ConcurrencyLock)> imports;
        private readonly List<(string Key, IData Value)> updates;
        private readonly ICache cache;

        public Transaction(ICache cache)
        {
            this.imports = new List<(Task Task, Type Type, string Key, bool ConcurrencyLock)>();
            this.updates = new List<(string Key, IData Value)>();
            this.cache = cache;
        }

        protected Transaction(Transaction transaction)
        {
            this.imports = transaction.imports;
            this.updates = transaction.updates;
        }

        protected Transaction Import<T>(string id, bool concurrencyLock) where T : IData
        {
            imports.Add((CacheLoader.Instance.Load(Type<T>.DataSource, id), typeof(T), Type<T>.Key(id), concurrencyLock));
            return this;
        }

        protected async Task<T> Map<T>(Func<object[], T> mapper)
        {
            await Task.WhenAll(this.imports.Select(x => x.Task));
            var args = await this.cache.Get(
                this.imports.Select(x => x.Key).ToArray(), 
                this.imports.Select(x => x.Type).ToArray());

            var ret = mapper(args);

            if (this.updates.Any())
            {
                var updateLock = cache.BeginNonBlockingLockScope();
                this.imports.Select((x, i) => 
                {
                    updateLock.When(x.Key, args[i]);
                    return true;
                });

                this.updates.ForEach(x => updateLock.Set(x.Key, x.Value));

                if (!await updateLock.Execute())
                {
                    ret = default(T);
                }
            }

            await Task.WhenAll(this.imports.Select((x, i) => this.cache.Extend(x.Key, args[i].CacheTTL)));

            return ret;
        }

        public Transaction<T> Use<T>(string id, bool concurrencyLock) where T : IData => new Transaction<T>(this.Import<T>(id, concurrencyLock));

        public Task Discard()
        {
            if (!this.updates.Any())
                return Task.CompletedTask;

            return Task.WhenAll(this.updates.Select(x => this.cache.Delete(x.Key)));
        }

        public void Update<T>(T value) where T : IData =>
            this.updates.Add((Type<T>.Key(value.Id), value));

    }
}
