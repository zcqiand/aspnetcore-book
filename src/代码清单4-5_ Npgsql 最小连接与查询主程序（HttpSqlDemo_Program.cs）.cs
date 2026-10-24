using Npgsql;

// 连接字符串是程序通往数据库的全部凭证, 一行写全, 逐段含义:
//   Host=localhost      数据库服务器在本机
//   Port=5432           PostgreSQL 安装默认端口
//   Database=orderdemo  上一节刚建的库
//   Username/Password   安装时设置的管理员账号, 口令请替换成你自己的
const string connectionString =
    "Host=localhost;Port=5432;Database=orderdemo;Username=postgres;Password=替换为你的口令";

// Npgsql 现行版本推荐经由 NpgsqlDataSource 创建连接: 连接配置集中管理,
// 同一份连接字符串全程序只出现这一次, 后续章节的依赖注入写法也以此为底座
NpgsqlDataSource dataSource = NpgsqlDataSource.Create(connectionString);

try
{
    // 三处 await using 让连接、命令、读取器在离开作用域时自动释放,
    // 数据库连接是稀缺资源, 忘记归还会让连接池慢慢耗尽
    await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();

    // 查询条件此刻是写死的常量; 若将来要把外部输入拼进条件, 必须改用参数化查询防注入,
    // 那是后续章节的正题, 这里先占住「值不落进 SQL 文本」这个位置
    const string sql = "SELECT order_no, customer, amount FROM orders WHERE id = 1";
    await using NpgsqlCommand command = new(sql, connection);
    await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

    // 查不到行不算故障: 连接是通的、语句是对的, 只是数据不在,
    // 走提示分支而非抛异常, 与连接失败严格区分
    if (!reader.HasRows)
    {
        Console.WriteLine("查无此行: orders 表中不存在 id = 1 的记录。");
        return 1;
    }

    await reader.ReadAsync();
    string orderNo = reader.GetString(0);
    string customer = reader.GetString(1);
    decimal amount = reader.GetDecimal(2);
    Console.WriteLine($"查询结果: {orderNo} {customer} {amount:F2} 元");
    return 0;
}
catch (NpgsqlException dbError)
{
    // 连不上库时 Npgsql 统一抛 NpgsqlException, 病因通常只有三类, 逐条提示省去翻文档
    Console.WriteLine("数据库连接或查询失败, 常见原因有三:");
    Console.WriteLine("  1. 库未建: orderdemo 还没创建, 先执行清单4-2 的建库语句;");
    Console.WriteLine("  2. 口令错误: 连接字符串里的 Password 与安装时设置的不一致;");
    Console.WriteLine("  3. 服务未启动: PostgreSQL 服务没有在后台运行。");
    Console.WriteLine($"详细信息: {dbError.Message}");
    return 1;
}
catch (Exception unexpectedError)
{
    // 兜底分支: 非数据库类的意外错误也给出出口, 不让程序带着崩溃痕迹收场
    Console.WriteLine($"发生未预期的错误: {unexpectedError.Message}");
    return 1;
}
finally
{
    // DataSource 持有底层连接池, 程序退出前统一归还
    await dataSource.DisposeAsync();
}