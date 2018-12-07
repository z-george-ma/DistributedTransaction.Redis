using System;
using System.Threading.Tasks;

namespace DistributedTransaction.Extensions
{
    public class RetryContext<T>
    {
        private readonly Func<Task<T>> callContext;
        private readonly bool continueOnCapturedContext;

        private Task<T> resolved = null;

        internal RetryContext(Func<Task<T>> callContext, bool continueOnCapturedContext)
        {
            this.callContext = callContext;
            this.continueOnCapturedContext = continueOnCapturedContext;
        }
        
        public Task<T> Invoke() => resolved == null ? callContext.Invoke() : resolved;

        public RetryContext<T> When<TException>(RetryPolicy retryPolicy)
            where TException : Exception =>
            new RetryContext<T>(() => Retry<TException>(this, retryPolicy), continueOnCapturedContext);
        
        private static async Task<T> Retry<TException>(RetryContext<T> retryContext, RetryPolicy retryPolicy)
            where TException : Exception
        {
            TException ex;
            do
            {
                try
                {
                    return await retryContext.Invoke().ConfigureAwait(retryContext.continueOnCapturedContext);
                }
                catch (TException e)
                {
                    ex = e;
                    retryContext.resolved = null;

                    await retryPolicy.Delay.ConfigureAwait(retryContext.continueOnCapturedContext);

                    retryPolicy.IncrementRetryCount();
                }
            }
            while (retryPolicy.ShouldRetry);

            throw ex;
        }

        public static implicit operator Func<Task<T>>(RetryContext<T> self) =>
            () => self.Invoke();
    }
}
