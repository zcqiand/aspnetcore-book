// 发布端：失败退避重试，教学件用简单循环示意
for (var attempt = 1; attempt <= 3; attempt++)
{
    try
    {
        await publisher.PublishReminderAsync(task, ct);
        break;
    }
    catch (Exception)
    {
        if (attempt == 3) throw; // 三回都投不进，错误上抛交上层处置
        await Task.Delay(TimeSpan.FromSeconds(attempt), ct); // 退避：第几回失败就歇几秒
    }
}

// 消费端：办不动就 BasicNackAsync，让消息回队列另投
consumer.ReceivedAsync += async (_, eventArgs) =>
{
    try
    {
        await HandleAsync(eventArgs);
        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
    }
    catch (Exception)
    {
        await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true);
    }
};