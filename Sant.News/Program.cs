using Hangfire;
using Sant.News.HackerNews;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddScoped<IIdsProcessing, IdsProcessing>();
builder.Services.AddScoped<IStoryDetailsProcessing, StoryDetailsProcessing>();

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
builder.Services.Configure<HackerNewsConnectionOptions>(builder.Configuration.GetSection("HackerNewsConnectionOptions"));

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHangfireDashboard();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
