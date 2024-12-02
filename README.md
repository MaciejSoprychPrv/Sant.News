### How It Works

1. **Application Launch**: Run the application in debug mode using IIS Express.
2. **StoriesCount Parameter**: The controller has a single parameter, `storiesCount`, which must be greater than 0. This value should be specified during the request.
3. **Controller File**: The controller is located in the file `GetHackerNews`.
4. **Managing Retry Attempts in Hangfire**: The number of retry attempts can be configured in the `Program` class, with `Attempts = 2` set in the Hangfire configuration.
5. **Logs**: Logs of the application can be found in "logs" folder
6. **Managing Number of Threads**: The number of threads used to query the API `https://hacker-news.firebaseio.com/` can be adjusted in the `Program` class. This is controlled by the `WorkerCount = 10` parameter in the Hangfire server configuration.
   - **Higher `WorkerCount`**: Increases the load on the external API.
   - **Lower `WorkerCount`**: Reduces the load on the external API.
  
   
     

### Project Assumptions

1. **No Database**: All data is stored in cache; no database is implemented. There should be database in real life application to store Hangfire jobs and stories ids and and details.
2. **Hangfire**: Since Hangfire is pretty simple and it does what is requested I implemented it. It has one big shortcoming it creates a lot of traffic on database site. In real life other solutions should considered like: MassTransit with ServiceBus or RabbitMQ. Depending on the volume of data. Only free of charge features and implemented in the solution.
3. **RetryPolicy**: Except for retries done by Hangfire whent it executes jobs there are no retries while storing data. Since it is cache there is no point in doing it. In real life with database I would go for Polly.
4. **Single Controller**: A single controller handles both fetching data from an external API and retrieving data from the cache. Ideally, there should be two separate controllers for these responsibilities.
5. **Hangfire In-Memory**: Hangfire operates entirely in memory since no database is available.
6. **Extension Methods for Configuration**: The content of the `Program` class, especially Hangfire configuration, has been moved to dedicated extension methods.
7. **Cache Expiry**: Cached data is stored for 30 minutes.
8. **No Integration Tests**: Due to the lack of a database, integration tests are not implemented.
9. **Unit Test Preparation for Hangfire**: Unit tests for Hangfire calls are skipped. However, `IBackgroundJobClient _client` is injected to prepare for future testing.
10. **No Exception Service**: There is no dedicated service for handling exceptions. In real life that should be changed.
11. **Returning All Stories**: If a user requests more stories than exist in the collection, all available stories are returned.
12. **Authentication**: It is not implement, in real life I would go for Azure AD if possible.
13. **Load tests**: No load tests were performed. As a result setting number of workers is by trial.
14. **Job status**: Hangfire allows to check the status of the job. I take advantage of this feature in the project. However in real life I would rather go for my own object sth like BusinessOpertion which would be stored in database and would provide me with the status. It gives more flexibility especially if hangfire is to be replaced with sthe else.
