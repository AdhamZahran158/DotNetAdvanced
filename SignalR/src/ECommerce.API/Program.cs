using ECommerce.API.Hubs;
using ECommerce.API.RealTime;
using ECommerce.Application;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Utilities;
using ECommerce.Infrastructure.Utilities.Redis;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
builder.Services.AddSingleton<IPdfJobQueue, PdfJobQueue>();
builder.Services.AddHostedService<BillPdfCreator>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(ApplicationAssemblyMarker).Assembly);
});

var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        redisConnectionString!));

builder.Services.AddSingleton<
    IProductViewCounter,
    ProductViewCounter>();

builder.Services.AddHostedService<ProductViewFlushWorker>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IChatMessageStore, ChatMessageStore>();
builder.Services.AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.MapHub<OrderHub>("/hubs/orders");

app.Run();
