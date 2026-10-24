    public bool DeleteContract(string tenantId, string id)
    {
        // RESTRICT 语义：service 层已用 ContractReferenced 前置拦截；这里防御性再查一次
        if (ContractReferenced(id))
        {
            throw new InvalidOperationException($"contract {id} is referenced by receipts");
        }

        return db.Contracts.Where(c => c.TenantId == tenantId && c.Id == id).ExecuteDelete() > 0;
    }