
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5110/notificationsHub")
    .Build();

connection.On<string>("ReceiveNotification", message =>
{
    Console.WriteLine($"New notification: {message}");
});

await connection.StartAsync();
Console.WriteLine("Connected to SignalR hub.");

Console.ReadLine();
