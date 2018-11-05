using System;

namespace DistributedTransaction.Core
{
    public interface IData
    {
        string Id { get; set; }
        int CacheTTL { get; set; }
    }

    public interface IData<TDataSource, T> : IData
        where TDataSource : IDataSource<T>
        where T: IData
    { }

}
