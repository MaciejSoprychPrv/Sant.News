using FluentValidation;
using Hangfire;
using Sant.News.HackerNews;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddScoped<IIdsProcessing, IdsProcessing>();
builder.Services.AddScoped<IStoryDetailsProcessing, StoryDetailsProcessing>();
builder.Services.AddValidatorsFromAssemblyContaining<GetHackerNews.Query>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(logger);

builder.Logging.ClearProviders();

builder.Logging.AddSerilog(logger);

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
builder.Services.Configure<HackerNewsConnectionOptions>(builder.Configuration.GetSection("HackerNewsConnectionOptions"));

builder.Services.AddMemoryCache();

var app = builder.Build();
app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHangfireDashboard();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

logger.Information("Hacker News Web API started.");

app.Run();
