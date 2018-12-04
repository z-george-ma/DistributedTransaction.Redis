using DistributedTransaction.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
              .Use<User>(userId, true)     // true means optimistic concurrency lock on getting User
              .Use<Book>(bookId, true)
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
                await Purchase(userId, bookId, ret.Price);

                return "OK";
            }
            catch
            {
                await transaction.Discard();

                throw; 
            }

        }

        private async Task Purchase(string userId, string bookId, decimal price)
        {
            await EnqueueUpdateUserBalance(userId, price);

            try
            {
                await EnqueueUpdateBookSKU(bookId, 1);

                try
                {
                    await EnqueueCreateOrder(userId, bookId, price);
                }
                catch
                {
                    await EnqueueUpdateBookSKU(bookId, -1);
                }
            }
            catch
            {
                await EnqueueUpdateUserBalance(userId, -price);
                throw;
            }


        }

        private async Task EnqueueUpdateUserBalance(string userId, decimal price)
        {
            return;
        }

        private async Task EnqueueUpdateBookSKU(string bookId, int number)
        {
            return;
        }

        private async Task EnqueueCreateOrder(string userId, string bookId, decimal price)
        {
            return;
        }
    }
}
