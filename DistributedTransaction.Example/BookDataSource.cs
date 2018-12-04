using DistributedTransaction.Core;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransaction.Example
{
    public class BookDataSource : IDataSource<Book>
    {

        public int Timeout => 60000;
        
        public async Task<Book> Get(string id, CancellationToken cancellationToken)
        {
            await Task.Delay(2000);
            return new Book
            {
                Id = id,
                CacheTTL = 300000, // 5 minutes
                SKU = 5
            };
        }
    }
}
