    private static AppDbContext MakeDb(string name)
        => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name).Options);

    [Fact]
    [Trait("Fn", "M04.F04.I04")]
    public async Task MenusPatch_withoutOptionalFields_keepsOriginalValues()
    {
        var db = MakeDb("partial-menu-patch");
        var parentId = Guid.NewGuid();
        db.SysMenus.Add(new DbSysMenu
        {
            Id = Guid.NewGuid(), ClientId = "test-client", ParentId = parentId,
            Title = "old-title", Type = 1, Path = "/old", Icon = "old-icon",
            SortOrder = 7, Status = 1, CreatedAt = DateTime.UtcNow,
        });
        db.SaveChanges();
        var ctrl = new ClientMenusController(db);
        var menuId = db.SysMenus.Single(m => m.ClientId == "test-client").Id.ToString();

        // 只改 title（无关注段），不带 type/status/sortOrder/parentId
        var dto = await ctrl.MenusPatch("test-client", menuId, new UpdateSysMenuRequest
        {
            Title = "new-title",
        });

        Assert.Equal("new-title", dto.Title);
        // 目标字段：不传不改（修前被重置 Type=Directory/Status=0/SortOrder=0/ParentId=Empty）
        Assert.Equal(SysMenuType.Menu, dto.Type);
        Assert.Equal(1, dto.Status);
        Assert.Equal(7, dto.SortOrder);
        Assert.Equal(parentId, dto.ParentId);
    }