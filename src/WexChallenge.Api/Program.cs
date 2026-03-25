var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "WEX Card API is running");

app.Run();
