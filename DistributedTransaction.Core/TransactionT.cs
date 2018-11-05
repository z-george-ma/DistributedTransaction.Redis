using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTransaction.Core
{
    public class Transaction<T> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0]));
        public new Transaction<T, TNew> Use<TNew>(string id, bool concurrencyLock) where TNew : IData => new Transaction<T, TNew>(this.Import<TNew>(id, concurrencyLock));
    }

    public class Transaction<T, T1> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1]));
        public new Transaction<T, T1, TNew> Use<TNew>(string id, bool concurrencyLock) where TNew : IData => new Transaction<T, T1, TNew>(this.Import<TNew>(id, concurrencyLock));
    }

    public class Transaction<T, T1, T2> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, T2, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1], (T2)args[2]));
        public new Transaction<T, T1, T2, TNew> Use<TNew>(string id, bool concurrencyLock) where TNew : IData => new Transaction<T, T1, T2, TNew>(this.Import<TNew>(id, concurrencyLock));
    }

    public class Transaction<T, T1, T2, T3> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, T2, T3, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1], (T2)args[2], (T3)args[3]));
        public new Transaction<T, T1, T2, T3, TNew> Use<TNew>(string id, bool concurrencyLock) where TNew : IData => new Transaction<T, T1, T2, T3, TNew>(this.Import<TNew>(id, concurrencyLock));
    }

    public class Transaction<T, T1, T2, T3, T4> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, T2, T3, T4, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4]));
    }

    public class Transaction<T, T1, T2, T3, T4, T5> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, T2, T3, T4, T5, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5]));
    }

    public class Transaction<T, T1, T2, T3, T4, T5, T6> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, T2, T3, T4, T5, T6, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6]));
    }

    public class Transaction<T, T1, T2, T3, T4, T5, T6, T7> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, T2, T3, T4, T5, T6, T7, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7]));
    }

    public class Transaction<T, T1, T2, T3, T4, T5, T6, T7, T8> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, T2, T3, T4, T5, T6, T7, T8, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8]));
    }

    public class Transaction<T, T1, T2, T3, T4, T5, T6, T7, T8, T9> : Transaction
    {
        internal Transaction(Transaction transaction) : base(transaction) { }
        public Task<TResult> Map<TResult>(Func<ITransaction, T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> mapper) => this.Map((object[] args) => mapper(this, (T)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9]));
    }

    // Consider breaking your transaction down into multiple ones if you ever need more than 10 parameters
}
