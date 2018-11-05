using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransaction.Core
{
    public class CacheLoaderConfiguration
    {
        public ICache Cache { get; set; }
        public int LockTTL { get; set; } = 6000;
        public int LockRenewInterval { get; set; } = 5000;
        public int LockCheckInterval { get; set; } = 500;

    }

    public class CacheLoader
    {
        private readonly CacheLoaderConfiguration config;

        internal static CacheLoader Instance;
        public static void Use(CacheLoaderConfiguration config) => Instance = new CacheLoader(config);
        
        internal CacheLoader(CacheLoaderConfiguration config)
        {
            this.config = config;
        }

        internal async Task Load<T>(IDataSource<T> dataSource, string id)
            where T : IData
        {
            if (await LockAndGetData(dataSource, id))
                return;

            var cts = new CancellationTokenSource(dataSource.Timeout);

            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(this.config.LockCheckInterval, cts.Token);

                if (await this.config.Cache.Exists<T>(Type<T>.LockKey(id)))
                    continue;

                if (await this.config.Cache.Exists<T>(Type<T>.Key(id)))
                    return;

                break;
            }
            
            throw new TimeoutException();
        }

        private async Task<bool> LockAndGetData<T>(IDataSource<T> dataSource, string id)
            where T : IData
        {
            using (var readLock = this.config.Cache.BeginBlockingLockScope(Type<T>.LockKey(id), this.config.LockTTL))
            {
                if (await readLock.Acquire())
                    return false;

                var cts = new CancellationTokenSource(dataSource.Timeout);

                var getDataTask = dataSource.Get(id, cts.Token);

                while (!cts.IsCancellationRequested)
                {
                    await Task.WhenAny(getDataTask, Task.Delay(this.config.LockRenewInterval, cts.Token));

                    if (getDataTask.IsCompleted)
                    {
                        var key = Type<T>.Key(id);

                        var saveLock = this.config.Cache.BeginNonBlockingLockScope();
                        saveLock.WhenNotExist(key);
                        saveLock.When(Type<T>.LockKey(id), readLock.LockId);
                        saveLock.Set(key, getDataTask.Result);

                        if (!await saveLock.Execute())
                        {
                            throw new SynchronizationLockException();
                        }

                        return true;
                    }

                    await readLock.Renew();
                }

                throw new TimeoutException();
            }
        }
    }
}