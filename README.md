# DistributedTransaction.Redis
Redis implementation for distributed transaction (C#)

### Introduction - Transactions are hard, let alone distributed ones. 
Let's start with a simple example - an online bookstore. When user purchases book from the bookstore, at the bare minimum, we have to perform two checks. Does user have sufficient balance to make the purchase? Do we have enough stock of the book to fulfill the purchase?

In a plain old SQL based non-distributed system, the data structure will look like this
```
CREATE TABLE Users (
  ID bigint primary key,
  Balance decimal
);

CREATE TABLE Books (
  ID bigint primary key,
  SKU int,
  Price decimal
);
```

A naive implementation will look like this:
```
DECLARE @UserId bigint = 123 -- User ID, input
DECLARE @BookId bigint = 345 -- Book ID, input

IF EXISTS(
  SELECT 1 FROM Users, Books
  WHERE Users.ID = @UserId AND Books.ID = @BookId AND Books.SKU > 0 AND Users.Balance >= Books.Price
) BEGIN
  UPDATE Users 
  SET Balance = Balance - (SELECT Price FROM Books WHERE Id = @BookId)
  WHERE Id = @UserId
  
  UPDATE Books
  SET SKU = SKU - 1
  WHERE Id = @BookId
END
```

Quick and easy, isn't it? Until you observe some spooky behavior occasionally. Yes, you know what I am talking about, [phantom read and non-repeatable read](http://www.ongoinghelp.com/difference-between-dirty-read-non-repeatable-read-and-phantom-read-in-database/). You may choose to increase your transaction isolation level, a trade-off between performance and throughput.

Over time, complexity grows. You want to break your monolithic application apart into small, single-purpose, manageable microservices, each having its own persistence and context boundary. Sounds all nice and beautiful, but...what about transactions? There's no equivalent of `BEGIN TRANSACTION` for APIs.

### One of the many solutions
Assume we have two microservices, user API and book API, to manage users and books respectively. For placing order, we have an order API, where the validation of user balance and book stock will happen. With Redis cache to manage distributed transactions, the workflow will look like this in order service

1. Check if user and book have been loaded in the cache, based on the IDs
2. If not, load user and book from APIs into cache (pessimistic concurrency lock with [SETNX or Redlock](https://redis.io/topics/distlock))
3. Check balance and SKU based on the cached value with [optimistic concurrency lock](https://redis.io/topics/transactions). If order is valid, update the balance in the cache.   
*Note:* apart from updating Redis cache, this step should not have any side effect, e.g. API call or database operation.
4. Enqueue commands to update balance and SKU respectively (eventual consistency).  
*Note:* When user / book APIs update the entity, it needs to follow the same pattern, i.e. make sure the write-through cache is updated before the actual update happens.
5. Any failure will invalidate the cache and trigger a compensation action within a new distributed transaction.

```
  var transaction = new Transaction();        
  var isSuccess = await transaction
	.Use<User>(userId, true)     // Use User from cache or API. Place optimistic concurrency lock on it 
	.Use<Book>(bookId, true)     // Use Book from cache or API. Place optimistic concurrency lock on it
	.Map((ITransaction tx, User user, Book book) => 
    {
      if (user.Balance < book.Price || book.SKU <= 0) {
        return false;
      }
      
      user.Balance -= book.Price;
      book.SKU--;
      tx.Update(user);
      tx.Update(book);
      return true;
    });
    
  try
  {
    // enqueue messages for eventual consistency
    ...
  }
  catch
  {
    transaction.Discard();    // Invalidate cache for User / Book 
    // Compensate action here
    ...
  }
```
