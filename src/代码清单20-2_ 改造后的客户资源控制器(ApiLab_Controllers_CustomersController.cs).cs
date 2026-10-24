using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ApiLab.Controllers;

// ── 第20章改造产物: 规范 REST 的客户资源端点 ───────────────────────────
// 资源名词化(controllers 是"客户"这个资源的归宿), 路径带版本号,
// 状态码各归其位, 错误统一走 ProblemDetails 契约。
[ApiController]
[Route("api/v1/customers")]
[ServiceFilter<AuditLogActionFilter>]
public class CustomersController : ControllerBase
{
    // 内存态存储: 进程内 static 列表, 零数据库零迁移(本章是设计章)
    private static readonly List<Customer> _customers = new();
    private static int _nextId = 1;

    // 列表: GET → 200
    [HttpGet]
    public ActionResult<IEnumerable<Customer>> GetAll()
        => Ok(_customers.OrderBy(c => c.Id));

    // 详情: GET /{id} → 200 / 404(ProblemDetails)
    [HttpGet("{id:int}")]
    public ActionResult<Customer> GetById(int id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        return customer is null
            ? Problem(statusCode: StatusCodes.Status404NotFound,
                title: "资源不存在", detail: $"客户 {id} 不存在。")
            : Ok(customer);
    }

    // 新建: POST → 201 + Location; 重名 → 409; 字段缺失 → 400([ApiController] 自动验证)
    [HttpPost]
    public ActionResult<Customer> Create(Customer input)
    {
        if (_customers.Any(c => c.Name == input.Name))
        {
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "资源冲突", detail: $"客户「{input.Name}」已存在, 不可重名。");
        }

        input.Id = _nextId++;
        _customers.Add(input);
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    // 更新: PUT /{id} → 200, 幂等: 同一请求连发两发, 结果一致; 查无则按该 id 建档
    [HttpPut("{id:int}")]
    public ActionResult<Customer> Update(int id, Customer input)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        if (customer is null)
        {
            input.Id = id;
            _customers.Add(input);
            return Ok(input);
        }

        customer.Name = input.Name;
        customer.Email = input.Email;
        return Ok(customer);
    }

    // 删除: DELETE /{id} → 204; 查无 → 404(ProblemDetails)
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        if (customer is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound,
                title: "资源不存在", detail: $"客户 {id} 不存在, 无从删除。");
        }

        _customers.Remove(customer);
        return NoContent();
    }
}

// 客户资源形态: 特性注解承担输入验证, 缺字段由 [ApiController] 自动回 400
public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "客户名不能为空")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }
}