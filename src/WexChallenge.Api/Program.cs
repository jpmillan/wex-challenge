using Microsoft.EntityFrameworkCore;
using WexChallenge.Api.Data;
using WexChallenge.Api.Endpoints;
using WexChallenge.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IExchangeRateService, TreasuryExchangeRateService>();

var app = builder.Build();

// apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => "WEX Card API is running");
app.MapCardEndpoints();
app.MapTransactionEndpoints();

app.Run();

// needed for WebApplicationFactory in integration tests
public partial class Program { }
