public sealed class ReminderScanner(InMemoryTaskStore store, ILogger<ReminderScanner> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 教学示意：定时把到期的样品任务扫一遍，给承办人出提醒
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var task in store.FindDue(DateTimeOffset.Now))
            {
                logger.LogInformation(
                    "样品任务 {TaskId} 已到计划检测日期，请提醒承办人 {Assignee}",
                    task.Id, task.AssigneeName);
                store.MarkReminded(task.Id);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}