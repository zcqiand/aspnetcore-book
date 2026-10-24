    // @entry M03.F05.I03 — 通过退回：review 态 act，SUBMIT 推进至 approval / RETURN 退回（book anchor）
    public override Task<ICollection<FlowActionResult>> ActFlowReview([FromBody] FlowActionRequest body) =>
        Task.FromResult<ICollection<FlowActionResult>>(_flowService.ActFlowReview(_tenantContext.TenantId, body));
    // @entry M03.F06.I01 (book anchor)
    // @entry M03.F06.I03 — 批准退回：RETURN 将 approval 态退回 review（book anchor）
    public override Task<ICollection<FlowActionResult>> ActFlowApprove([FromBody] FlowActionRequest body) =>
        Task.FromResult<ICollection<FlowActionResult>>(_flowService.ActFlowApprove(_tenantContext.TenantId, body));