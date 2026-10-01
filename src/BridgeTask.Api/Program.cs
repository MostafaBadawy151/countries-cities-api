using BridgeTask.Api.Data;
using BridgeTask.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi()
    .AddApplication(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

await DbInitializer.InitializeAsync(app.Services, seed: app.Configuration.GetValue("Database:Seed", true));

app.Run();

public partial class Program;
