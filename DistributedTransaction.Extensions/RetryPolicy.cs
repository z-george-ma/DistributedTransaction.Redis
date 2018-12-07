using System;
using System.Threading.Tasks;

namespace DistributedTransaction.Extensions
{
    public class RetryPolicy
    {
        private readonly Func<bool> shouldRetry;
        private readonly Action incrementRetryCount;
        private readonly Func<Task> delay;

        public RetryPolicy(Func<bool> shouldRetry, Action incrementRetryCount, Func<Task> delay)
        {
            this.shouldRetry = shouldRetry;
            this.incrementRetryCount = incrementRetryCount;
            this.delay = delay;
        }

        public bool ShouldRetry => shouldRetry();
        public void IncrementRetryCount() => incrementRetryCount();
        public Task Delay => delay();

        public static RetryPolicy Immediate(int maxRetryCount)
        {
            int retryCount = 0;
            return new RetryPolicy(
                shouldRetry: () => retryCount < maxRetryCount,
                incrementRetryCount: () => retryCount++,
                delay: () => Task.FromResult(true)
            );
        }

        public static RetryPolicy Constant(int millisecondsBackoff, int maxRetryCount)
        {
            int retryCount = 0;
            return new RetryPolicy(
                shouldRetry: () => retryCount < maxRetryCount,
                incrementRetryCount: () => retryCount++,
                delay: () => Task.Delay(millisecondsBackoff)
            );
        }

        public static RetryPolicy Random(int millisecondsBackoffMin, int millisecondsBackoffMax, int maxRetryCount)
        {
            var random = new Random();

            int retryCount = 0;
            return new RetryPolicy(
                shouldRetry: () => retryCount < maxRetryCount,
                incrementRetryCount: () => retryCount++,
                delay: () => Task.Delay(random.Next(millisecondsBackoffMin, millisecondsBackoffMax))
            );
        }
    }
}
