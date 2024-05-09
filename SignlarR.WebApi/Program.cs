using Microsoft.AspNetCore.Cors.Infrastructure;
using SignlarR.WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//SignalR adding to services
builder.Services.AddSignalR();
builder.Services.AddCors(opt => opt.AddPolicy(name: "CorsPolicy", builder => builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();




app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chat");
});


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/sendnotification", async (string? group, string message) =>
{
    var hub = ChatHub.GetHubWrapper();
    if(hub==null)
    {
        return;
    }
    if (!string.IsNullOrEmpty(group))
    {
        await hub.PublishToGroupAsync(group, "ReceiveMessage", message);
    }
    else
    {
        await hub.PublishToAllAsync("ReceiveMessage", message);
    }
})
.WithName("SendNotification")
.WithOpenApi();
//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}

app.Run();

