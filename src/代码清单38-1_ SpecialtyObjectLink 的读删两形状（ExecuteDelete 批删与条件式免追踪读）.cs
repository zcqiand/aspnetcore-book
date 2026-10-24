    // @note ch38：写删读查两条路径，写一条 ExecuteDelete 批删、读按 spec 条件式免追踪，一窗对照
    public bool DeleteSpecialtyObject(string spec, string obj) =>
        db.SpecialtyObjectLinks
            .Where(x => x.InspectionSpecialtyCode == spec && x.InspectionObjectCode == obj)
            .ExecuteDelete() > 0;

    public IReadOnlyList<SpecialtyObjectLink> ListSpecialtyObject(string? spec) =>
        string.IsNullOrWhiteSpace(spec)
            ? db.SpecialtyObjectLinks.AsNoTracking().ToList()
            : db.SpecialtyObjectLinks.AsNoTracking().Where(x => x.InspectionSpecialtyCode == spec).ToList();