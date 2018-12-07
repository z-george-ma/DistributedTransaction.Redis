using System;
using System.Threading.Tasks;

namespace DistributedTransaction.Extensions
{
    public class CancelContext
    {
        private readonly TryConfirmCancel tcc;
        private readonly Func<Task> tryFunc;

        internal CancelContext(TryConfirmCancel tcc, Func<Task> tryFunc)
        {
            this.tcc = tcc;
            this.tryFunc = tryFunc;
        }

        public void Cancel(Func<Task> cancelFunc)
        {
            tcc.Tasks.Add((tryFunc, cancelFunc));
        }
    }
    
}
