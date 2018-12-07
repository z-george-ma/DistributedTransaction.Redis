using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTransaction.Extensions
{
    public class TryConfirmCancel
    {
        internal List<(Func<Task> Try, Func<Task> Cancel)> Tasks { get; }
            = new List<(Func<Task> Try, Func<Task> Cancel)>();

        public CancelContext Try(Func<Task> tryFunc) => new CancelContext(this, tryFunc);
        
        public async Task<Exception[]> Confirm(bool continueOnCapturedContext = false)
        {
            var tryTasks = Tasks.Select(x => x.Try()).ToList();
            try
            {
                await Task.WhenAll(tryTasks).ConfigureAwait(continueOnCapturedContext);
            }
            catch
            {
                await Task.WhenAll(tryTasks.Select((x, i) => (x.IsCanceled || x.IsFaulted) ? Tasks[i].Cancel() : Task.CompletedTask)).ConfigureAwait(continueOnCapturedContext);
            }

            return tryTasks.Select(x => x.Exception).ToArray();
        }
    }    
}
