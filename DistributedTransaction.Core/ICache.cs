using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTransaction.Core
{
    public interface INonBlockingLockScope
    {
        void When(string key, IData value);
        void When(string key, string value);
        void WhenNotExist(string key);
        void Set(string key, IData value);
        Task<bool> Execute();
    }

    public interface IBlockingLockScope : IDisposable
    {
        Task<bool> Acquire();
        string LockId { get; }
        Task<bool> Renew();
    }

    public interface ICache
    {
        IBlockingLockScope BeginBlockingLockScope(string lockKey, int ttl);
        INonBlockingLockScope BeginNonBlockingLockScope();
        Task<IData[]> Get(string[] keys, Type[] types);
        Task Extend(string key, int ttl);
        Task<bool> Exists<IData>(string key);
        Task Delete(string key);
    }
}
