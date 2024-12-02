using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHangfire(c => c
    .UseInMemoryStorage()
    .UseFilter(new AutomaticRetryAttribute { Attempts = 2 }));

var options = new BackgroundJobServerOptions
{
    Queues = new[] { "hackernews" },
    WorkerCount = 10,
};

builder.Services.AddHangfireServer(serverOptions =>
{
    serverOptions.Queues = options.Queues;
    serverOptions.WorkerCount = options.WorkerCount;
});

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
