using DistributedTransaction.Core;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransaction.Example
{
    public class UserDataSource : IDataSource<User>
    {
        public int Timeout => 60000;

        public async Task<User> Get(string id, CancellationToken cancellationToken)
        {
            // api call
            await Task.Delay(2000);
            return new User
            {
                Id = id,
                CacheTTL = 300000, // 5 minutes
                Balance = 100m
            };
        }
    }
}
