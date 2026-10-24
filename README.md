# ASP.NET Core 从入门到项目实践 - 代码清单

## 关于本书

《ASP.NET Core 从入门到项目实践》是一本基线锁定 .NET 8 LTS（ASP.NET Core 8.0 / C# 12）的全景实战书：以 roadmap.sh ASP.NET Core 学习主线为骨架，带你从环境搭建一路走到生产部署，两个真实项目贯穿全书，每章代码可运行（学习路线对标，非官方合作出版物）。

全书 42 章，从中间件管道、依赖注入、EF Core 与 Web API 等核心机制讲起，依次落地 Identity/JWT/OIDC 认证授权体系与 xUnit/Testcontainers 单元+集成测试防线，再进入贯穿项目实战，走完性能压测、容器化、CI/CD 与可观测性的完整生产链路，收于微服务演进路线与 .NET Aspire 展望。你不需要 .NET 背景：具备任意一门后端语言基础即可开读，Java/Spring、Python 或前端背景的转轨工程师、想完成现代化的 .NET Framework 老兵、计算机在校生都适合；第 2 章语法对照与第 32 章概念映射专章帮 Spring Boot 等技术栈开发者平移已有经验，HTTP 与 SQL 的最小必备在第 4 章补齐，全书不从零教编程。

## 本书特点

**第一，版本基线锁定 .NET 8 LTS。** 所有示例基于 .NET 8 LTS（ASP.NET Core 8.0 / C# 12）写作，正文不因版本迭代快速过时，.NET 9/10 的变化以增量专栏呈现。

**第二，两个真实项目贯穿，「可运行」是工程承诺而非宣传语。** 认证能力落在 saas-identity-platform-aspnetcore，全栈贯穿项目落在 lab-management-system-aspnetcore，案例仓均已冻结 tag，clone 即跑。

**第三，决策表驱动的选型教学。** 中间件 vs 过滤器、EF Core vs Dapper、cookie vs JWT——每处关键取舍都给决策表，教你在什么场景选什么方案。

**第四，完整生产链路收尾。** 卷五走完压测数据、容器化、CI/CD 与故障演练，交付一个容器化、可观测、可回滚的真实系统，并给出向微服务演进的路线。

## 案例仓库

| 仓库名 | 说明 |
| :--- | :--- |
| [saas-identity-platform-aspnetcore](https://github.com/zcqiand/saas-identity-platform-aspnetcore) @ v0.3.33-20260928 | 基于 ASP.NET Core 8.0 / .NET 8.0 / C# 12 的 SaaS 多租户统一身份认证平台，覆盖多租户、成员与角色菜单、OAuth/OIDC 客户端管理与登录防护 |
| [lab-management-system-aspnetcore](https://github.com/zcqiand/lab-management-system-aspnetcore) @ v0.2.32-20260928 | 基于 ASP.NET Core 8.0 / .NET 8.0 / C# 12 的建筑工程实验室管理系统贯穿项目，覆盖认证、合同、目录字典与汇总报表等业务模块 |

> 配套案例仓库为独立可跑工程，已冻结 tag，含完整测试与 CI，clone 即跑。

## 代码清单说明

本书所有代码清单均收录于本目录，对应书稿中「代码清单 N-M」标题块。

### 运行环境

```bash
# 环境要求：.NET 8 SDK（案例仓库为 ASP.NET Core 8.0 / C# 12 工程，本书清单与之同版本）
dotnet --list-sdks
# 克隆案例仓库、切到冻结 tag 后还原依赖并启动（以实验室管理系统为例）
git clone https://github.com/zcqiand/lab-management-system-aspnetcore.git
cd lab-management-system-aspnetcore && git checkout v0.2.32-20260928
dotnet restore
dotnet run --project src
# 运行单个清单文件：清单多为案例仓实码摘录，建议放回所属工程运行；独立控制台类清单可新建工程验证
dotnet new console -o demo
# 将清单代码替换 demo/Program.cs 后执行
dotnet run --project demo
# 运行工程自带测试
dotnet test
```

### 目录结构

```
src/
├── 代码清单1-* … 代码清单42-*   # 第 1-42 章，共 245 个清单文件（命名「代码清单N-M_ 描述.扩展名」）
└── extracted_code_manifest.json   # 全部清单索引（title/lang/chapter_file/line/extracted_file/source）
```
