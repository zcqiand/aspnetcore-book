namespace ApiLab;

// ── 遗留模块(第20章改造对照用) ─────────────────────────────────────────
// 模拟"接手来的前任留下的客户模块": RPC 风格动词路径, 一堆失范惯例。
// 仅第20章实验期临时挂载; 改造完成后不再注册, 本文件存档留作对照。
public static class LegacyCustomerEndpoints
{
    // 内存态存储: 进程内 static 列表, 零数据库零迁移
    private static readonly List<LegacyCustomer> _customers = new();
    private static int _nextId = 1;

    public static void MapLegacyCustomers(this WebApplication app)
    {
        // 失范点1: 动词入路径, GET 干删除的活; 失范点2: 路径无版本
        app.MapGet("/getCustomerById", (int id) =>
        {
            var c = _customers.FirstOrDefault(x => x.Id == id);
            // 失范点3: 查无此人也回 200, 错误信息混在正文里
            return c is null
                ? Results.Ok(new { success = false, message = $"客户 {id} 不存在" })
                : Results.Ok(new { success = true, data = c });
        });

        app.MapGet("/getAllCustomers", () => Results.Ok(new { success = true, data = _customers }));

        app.MapPost("/createCustomer", (LegacyCustomer input) =>
        {
            input.Id = _nextId++;
            _customers.Add(input);
            // 失范点4: 新建也回 200, 无 Location, 无 201
            return Results.Ok(new { success = true, message = $"客户 {input.Name} 创建成功", data = input });
        });

        // 失范点5: 删除用 GET, 且语义只藏在动词里
        app.MapGet("/removeCustomer", (int id) =>
        {
            var c = _customers.FirstOrDefault(x => x.Id == id);
            if (c is null)
            {
                return Results.Ok(new { success = false, message = $"客户 {id} 不存在, 删除失败" });
            }
            _customers.Remove(c);
            return Results.Ok(new { success = true, message = $"客户 {id} 删除成功" });
        });
    }
}

// 遗留客户形态: 无验证注解, 字段随便传
public class LegacyCustomer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}