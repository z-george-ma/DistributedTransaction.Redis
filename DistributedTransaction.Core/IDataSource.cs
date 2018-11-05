using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransaction.Core
{
    public interface IDataSource<T>
        where T : IData
    {
        Task<T> Get(string id, CancellationToken cancellationToken);
        int Timeout { get; }
    }
}
