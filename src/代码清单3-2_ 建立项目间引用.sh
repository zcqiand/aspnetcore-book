dotnet add OrderFlow.App reference OrderFlow.Domain
dotnet add OrderFlow.Tests reference OrderFlow.Domain
dotnet add OrderFlow.Tests reference OrderFlow.App

# 引用方向即依赖方向: App 依赖 Domain, 反过来 Domain 对 App 一无所知, 业务规则因此保持独立
# Tests 双向引用 Domain 与 App: 既可直接测类库内部逻辑, 也能走 App 的公共入口做端到端验证
# 三条命令执行后, 对应的 ProjectReference 条目会自动写进各项目的 csproj, 无需手工编辑