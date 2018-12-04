using DistributedTransaction.Core;

namespace DistributedTransaction.Example
{
    public class Book : IData
    {
        public string Id { get; set; }
        public int CacheTTL { get; set; }
        public int SKU { get; set; }
        public decimal Price { get; set; }
    }
}
