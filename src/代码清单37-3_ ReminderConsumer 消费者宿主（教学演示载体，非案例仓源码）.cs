public sealed class ReminderConsumer : BackgroundService
{
    private readonly ConnectionFactory _factory = new() { HostName = "localhost" };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _factory.CreateConnectionAsync(stoppingToken);
        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "sample-task-reminders",
            durable: false, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            await RemindAsync(message); // 教学示意：办提醒，把到期消息送到承办人跟前
            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "sample-task-reminders",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    private Task RemindAsync(string message) => Task.CompletedTask;
}