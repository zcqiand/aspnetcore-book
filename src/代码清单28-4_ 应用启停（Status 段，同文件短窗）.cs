    public override async Task<ApiClient> Status(string clientId, Body body)
    {
        var e = await _db.OauthClients.FirstOrDefaultAsync(c => c.ClientId == clientId)
            ?? throw new KeyNotFoundException($"client {clientId} not found");
        e.Status = (short)body.Status;
        e.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(e);
    }