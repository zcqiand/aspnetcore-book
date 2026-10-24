namespace Lab.AspNetCore.Tests.TestDoubles;

using System.Collections.Concurrent;
using Lab.AspNetCore.Controllers.Generated;
using Lab.AspNetCore.Data;

/// <summary>
/// B3 流程域内存存储（合同/接样/样品/检测记录），B4 汇总也从这里取数。
/// 语义镜像 springboot JPA repository：tenant 收口 + 各 list 过滤 + 排序；
/// 删除约束：合同被接样引用 RESTRICT、接样删除 CASCADE 下属样品。
/// </summary>
public sealed class InMemoryFlowStore : IFlowStore
{
    private readonly ConcurrentDictionary<(string Tenant, string Id), Contract> _contracts = new();
    private readonly ConcurrentDictionary<(string Tenant, string Id), SampleReceipt> _receipts = new();
    private readonly ConcurrentDictionary<(string Tenant, string Id), Sample> _samples = new();
    private readonly ConcurrentDictionary<(string Tenant, string Id), TestRecord> _records = new();

    private static string N(string? s) => s ?? "";

    // === 合同 M02.F01 ===

    public IReadOnlyList<Contract> FilterContracts(string tenantId, string? keyword, ContractStatus? status) =>
        _contracts.Values
            .Where(c => c.TenantId == tenantId)
            .Where(c => string.IsNullOrEmpty(keyword)
                || (c.ContractCode ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || (c.ProjectName ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .Where(c => status is null || c.Status == status)
            .OrderBy(c => c.CreatedAt)
            .ToList();

    public Contract? FindContract(string tenantId, string id) =>
        _contracts.TryGetValue((tenantId, id), out var c) ? c : null;

    public bool ContractReferenced(string contractId) =>
        _receipts.Values.Any(r => r.ContractId == contractId);

    public void SaveContract(Contract c) => _contracts[(c.TenantId, c.Id)] = c;

    public bool DeleteContract(string tenantId, string id) => _contracts.TryRemove((tenantId, id), out _);