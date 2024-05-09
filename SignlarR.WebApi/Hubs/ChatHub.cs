using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SignlarR.WebApi.Hubs;

public class ChatHub : Hub
{
    public readonly IServiceProvider _provider;
    public static HubWrapper _hubwrapperInstance;
    public ChatHub(IServiceProvider provider)
    {
        _provider = provider;
    }

    public static HubWrapper GetHubWrapper()
    {
        return _hubwrapperInstance;
    }

    public async Task SendMessage(string message,string group)
    {
        await _hubwrapperInstance.PublishToGroupAsync(group, "ReceiveMessage", message);
    }

    public async Task Initialize(string group)
    {
        if (_hubwrapperInstance == null)
        {
            _hubwrapperInstance = new HubWrapper(_provider.GetRequiredService<IHubContext<ChatHub>>());
        }

        try
        {
            await AddToGroup(group);
            await ConnectAsync();
        }
        catch (Exception e)
        {
            await DisconnectAsync(e);
            await RemoveFromGroup(group);
        }
    }
    private async Task ConnectAsync()
    {
        await Clients.Client(Context.ConnectionId).SendAsync("connected");
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public async Task ConnectIdAsync(string connectionId)
    {
        await Clients.Client(connectionId).SendAsync("connected");
        await base.OnConnectedAsync().ConfigureAwait(false);
    }



    private async Task DisconnectAsync(Exception e)
    {
        await Clients.Client(Context.ConnectionId).SendAsync("disconnected");
        await base.OnDisconnectedAsync(e).ConfigureAwait(false); ;
    }

    public async Task AddToGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
    }

    public async Task RemoveFromGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
    }
    public string GetUserName()
    {
        if (!(Context.User.Identity is ClaimsIdentity claimIdentity)) return string.Empty;
        var groupClaim = claimIdentity.Claims.FirstOrDefault(c => c.Type == "loginName");
        return groupClaim != null ? groupClaim.Value : string.Empty;
    }

}
public interface IHubWrapper
{
    Task PublishToGroupAsync(string group, string message, object data);
    Task PublishToAllAsync(string message, object data);

}

public class HubWrapper : IHubWrapper
{
    private readonly IHubContext<ChatHub> _hubContext;

    public HubWrapper(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishToGroupAsync(string userName, string message, object data)
    {
        await _hubContext.Clients.Group(userName).SendAsync(message, data);
    }

    public async Task PublishToAllAsync(string message, object data)
        => await _hubContext.Clients.All.SendAsync(message, data);

}