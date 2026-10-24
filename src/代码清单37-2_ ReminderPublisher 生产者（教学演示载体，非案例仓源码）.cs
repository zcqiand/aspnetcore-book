public sealed class ReminderPublisher
{
    // 教学示意：主机名取本机示意常量，正式部署按环境配置走
    private const string HostName = "localhost";

    public async Task PublishReminderAsync(SampleTaskItem task, CancellationToken ct = default)
    {
        var factory = new ConnectionFactory { HostName = HostName };
        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue: "sample-task-reminders",
            durable: false, exclusive: false, autoDelete: false,
            cancellationToken: ct);

        var body = Encoding.UTF8.GetBytes(
            $"{{\"taskId\":\"{task.Id}\",\"assignee\":\"{task.AssigneeName}\"}}");

        // 投完即回：分派请求等「已投出」，不等「已办完」
        await channel.BasicPublishAsync(
            exchange: string.Empty, routingKey: "sample-task-reminders",
            body: body, cancellationToken: ct);
    }
}