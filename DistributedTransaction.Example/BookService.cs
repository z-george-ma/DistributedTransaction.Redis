using DistributedTransaction.Core;
using DistributedTransaction.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DistributedTransaction.Extensions.TaskExtension;

namespace DistributedTransaction.Example
{
    public class BookService
    {
        private readonly ICache cache;

        public BookService(ICache cache)
        {
            this.cache = cache;
        }

        public async Task<string> MakePurchase(string userId, string bookId)
        {
            var transaction = new Transaction(cache);

            (string Message, decimal Price) ret = await transaction
              .Use<User>(userId, true)     // optimistic concurrency lock on getting user
              .Use<Book>(bookId, true)     // optimistic concurrency lock on getting book
              .Map((ITransaction tx, User user, Book book) =>
              {
                  if (user.Balance < book.Price)
                  {
                      return ("Insufficient user balance", book.Price);
                  }

                  if (book.SKU <= 0)
                  {
                      return ("Out of stock", book.Price);
                  }

                  user.Balance -= book.Price;
                  book.SKU--;
                  tx.Update(user);
                  tx.Update(book);
                  return (null, book.Price);
              });

            if (ret.Message != null)
            {
                return ret.Message;
            }

            try
            {
                if (await Purchase(userId, bookId, ret.Price))
                {
                    return "OK";
                }
                
                await transaction.Discard();
                return "Internal error, rolled back";
            }
            catch
            {
                await transaction.Discard();
                throw;  // fatal error, inconsistent state
            }

        }

        private async Task<bool> Purchase(string userId, string bookId, decimal price)
        {
            var tcc = new TryConfirmCancel();

            tcc.Try(
                Retry(() => EnqueueUpdateUserBalance(userId, price))
                .When<Exception>(RetryPolicy.Immediate(3))
            ).Cancel(
                Retry(() => EnqueueUpdateUserBalance(userId, -price))
                .When<Exception>(RetryPolicy.Immediate(3))
            );

            tcc.Try(
                Retry(() => EnqueueUpdateBookSKU(bookId, 1))
                .When<Exception>(RetryPolicy.Immediate(3))
            ).Cancel(
                Retry(() => EnqueueUpdateBookSKU(bookId, -1))
                .When<Exception>(RetryPolicy.Immediate(3))
            );

            var orderId = Guid.NewGuid().ToString("N");
            tcc.Try(
                Retry(() => EnqueueCreateOrder(orderId, userId, bookId, price))
                .When<Exception>(RetryPolicy.Immediate(3))
            ).Cancel(
                Retry(() => EnqueueCancelOrder(orderId))
                .When<Exception>(RetryPolicy.Immediate(3))
            );
            
            var ret = await tcc.Confirm();
            return ret.All(e => e == null);
        }

        private async Task<bool> EnqueueUpdateUserBalance(string userId, decimal price)
        {
            return true;
        }

        private async Task<bool> EnqueueUpdateBookSKU(string bookId, int number)
        {
            return true;
        }

        private async Task<bool> EnqueueCreateOrder(string orderId, string userId, string bookId, decimal price)
        {
            return true;
        }

        private async Task<bool> EnqueueCancelOrder(string orderId)
        {
            return true;
        }
    }
}
