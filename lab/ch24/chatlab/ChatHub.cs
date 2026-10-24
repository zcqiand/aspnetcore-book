using Microsoft.AspNetCore.SignalR;

namespace ChatLab;

// Hub 是客户端与服务端之间的双向枢纽：客户端能调用这里声明的公开方法，
// 服务端能通过 Clients.* 主动往连接上推消息——两个方向共用这一条持久连接。
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    // 客户端调用的唯一入口：把一条发言广播给当前所有连接（含发言者本人，
    // 这样浏览器页面上自己发的消息也走服务端绕一圈回来，双向路径都能看到）。
    public async Task SendMessage(string user, string message)
    {
        // 空消息直接以 HubException 拒收：这个异常会原样传回发起调用的客户端，
        // 其余连接不受牵连，是 Hub 内向外报错的标准通道。
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
        {
            throw new HubException("发言人与消息内容都不能为空");
        }

        _logger.LogInformation("收到 {User} 的发言, 转播给全体连接", user);
        await Clients.All.SendAsync("ReceiveMessage", user, message, DateTimeOffset.Now);
    }

    // 连接生命周期的两端：进出都留日志。连接就是 Hub 的状态面，
    // 每个连接自带一个 ConnectionId, 断线重连后会换新 id（见重连实验）。
    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("连接进入: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("连接离开: {ConnectionId} ({Reason})",
            Context.ConnectionId,
            exception is null ? "正常断开" : exception.GetType().Name);
        return base.OnDisconnectedAsync(exception);
    }
}
