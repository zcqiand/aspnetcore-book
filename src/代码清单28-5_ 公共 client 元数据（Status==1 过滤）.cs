    public override async Task<OAuthClientPublicInfo> Clients(string clientId)
    {
        var row = await _db.OauthClients
            .Where(a => a.ClientId == clientId && a.Status == 1)
            .FirstOrDefaultAsync();
        if (row is null)
        {
            throw new KeyNotFoundException($"Client '{clientId}' not found");
        }
        return new OAuthClientPublicInfo
        {
            // v0.5.0 NSwag 重 emit：OAuthClientPublicInfo 形状变 { clientId, clientName, status }；
            // Id / Name 字段取消（clientId 是 OAuth 标准命名）。
            ClientId = row.ClientId,
            ClientName = row.ClientName,
            Status = (int)row.Status,
        };
    }