using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedTransaction.Extensions
{
    public static class TaskExtension
    {
        public static async Task<TResult> Then<TResult>(this Task self, Func<Task<TResult>> thenProc, bool continueOnCapturedContext = false)
        {
            await self.ConfigureAwait(continueOnCapturedContext);

            return await thenProc().ConfigureAwait(continueOnCapturedContext);
        }

        public static async Task<TResult> Then<T, TResult>(this Task<T> self, Func<T, Task<TResult>> thenProc, bool continueOnCapturedContext = false)
        {
            var result = await self.ConfigureAwait(continueOnCapturedContext);

            return await thenProc(result).ConfigureAwait(continueOnCapturedContext);
        }

        public static async Task<TResult> Then<TResult>(this Task self, Func<TResult> thenProc, bool continueOnCapturedContext = false)
        {
            await self.ConfigureAwait(continueOnCapturedContext);
            return thenProc();
        }

        public static async Task<TResult> Then<T, TResult>(this Task<T> self, Func<T, TResult> thenProc, bool continueOnCapturedContext = false) =>
            thenProc(await self.ConfigureAwait(continueOnCapturedContext));

        public static async Task<T> Catch<TException, T>(this Task<T> self, Func<TException, Task<T>> catchProc, bool continueOnCapturedContext = false)
            where TException : Exception
        {
            try
            {
                return await self.ConfigureAwait(continueOnCapturedContext);
            }
            catch (TException e)
            {
                return await catchProc(e).ConfigureAwait(continueOnCapturedContext);
            }
        }

        public static Task<T> Catch<TException, T>(this Task<T> self, Func<TException, T> catchProc, bool continueOnCapturedContext = false)
            where TException : Exception =>
            self.Catch((TException e) => Task.FromResult(catchProc(e)), continueOnCapturedContext);

        public static Task<T> Create<T>(Func<CancellationToken, Task<T>> createProc, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            return createProc(cts.Token);
        }

        public static Task Create(Func<CancellationToken, Task> createProc, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            return createProc(cts.Token);
        }

        public static RetryContext<T> Retry<T>(Func<Task<T>> retryProc, bool continueOnCapturedContext = false) =>
            new RetryContext<T>(retryProc, continueOnCapturedContext);

        public static RetryContext<object> Retry(Func<Task> retryProc, bool continueOnCapturedContext = false) =>
            new RetryContext<object>(() => retryProc().Then<object>(() => null, continueOnCapturedContext), continueOnCapturedContext);
        
        public static async Task AsTask(this AutoResetEvent self, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            using (ct.Register(() => { tcs.TrySetCanceled(); }))
            {
                RegisteredWaitHandle rwh = default;
                rwh = ThreadPool.RegisterWaitForSingleObject(
                    self, 
                    delegate (object s, bool timedOut) 
                    { 
                        ((TaskCompletionSource<object>)s).TrySetResult(null); 
                        rwh.Unregister(self); 
                    }, tcs, -1, true);
                await tcs.Task;
            }
        }

        public static async Task AsTask(this AutoResetEvent self)
        {
            var tcs = new TaskCompletionSource<object>();
            RegisteredWaitHandle rwh = default;
            rwh = ThreadPool.RegisterWaitForSingleObject(
                self,
                delegate (object s, bool timedOut)
                {
                    ((TaskCompletionSource<object>)s).TrySetResult(null);
                    rwh.Unregister(self);
                }, tcs, -1, true);
            await tcs.Task;
        }
    }
}
