    [Fact]
    [Trait("Fn", "M02.F01.I01")]
    public void List_keywordMatchesCodeOrProjectName()
    {
        var store = new InMemoryFlowStore();
        store.SaveContract(C("C-1", "HT-2026-A", "地铁工程"));
        store.SaveContract(C("C-2", "HT-2026-B", "桥梁工程"));
        store.SaveContract(C("C-3", "XX-其他", " unrelated"));
        var service = new ContractService(store);

        var byCode = service.List(Tenant, "ht-2026", null);
        Assert.Equal(2, byCode.Count); // 不敏包含

        var byProject = service.List(Tenant, "桥梁", null);
        Assert.Single(byProject);
    }

    [Fact]
    [Trait("Fn", "M02.F01.I01")]
    public void List_statusFilter()
    {
        var store = new InMemoryFlowStore();
        store.SaveContract(C("C-1", "A", "p1"));
        store.SaveContract(C("C-2", "B", "p2", ContractStatus.Archived));
        var service = new ContractService(store);

        Assert.Single(service.List(Tenant, null, ContractStatus.Archived));
        Assert.Single(service.List(Tenant, null, ContractStatus.Active));
    }