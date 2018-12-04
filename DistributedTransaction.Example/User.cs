using DistributedTransaction.Core;
using System;

namespace DistributedTransaction.Example
{
    public class User : IData
    {
        public string Id { get; set; }
        public int CacheTTL { get; set; }
        public decimal Balance { get; set; }
    }
}
