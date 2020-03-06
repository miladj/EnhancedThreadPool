# EnhancedThreadPool
![.NET Core](https://github.com/miladj/EnhancedThreadPool/workflows/.NET%20Core/badge.svg)<br/>
This library add some functionality to default dotnet thread pool.
* Benefits:
  * It is not static so you can have multiple threadpool.
  * It allows you to have different group in a single threadpool.
  
The group feature in threadpool allows you process actions fairly in different groups, for example imagine there is a system that process customer orders and orders enter in system sequentially, if it uses typical threadpool, there is a chance that a customer's orders fill up the threadpool and other customer's must wait until that specific customer's orders is processed. But in this model each customer have a chance to use a thread and one customer can't busy all the thread in the threadpool.