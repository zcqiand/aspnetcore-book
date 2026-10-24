dotnet new web -o ConfigLab
cd ConfigLab
# ConfigLab 是与 FirstWebApp(第 5-7 章)、LifetimeLab(第 8-9 章) 并列的独立配置实验项目,
# 不在既有两个项目上追加任何代码, 第 7 章与第 8/9 章搭好的管道、注册与端点保持原样不动,
# 本章所有实验都落在 ConfigLab 目录内
# 后续清单10-2/10-3 的两个 appsettings 文件、清单10-5 的 Program.cs、清单10-6 的五步实验,
# 全部以本目录为根; 零新增 NuGet 包, 本章用到的配置、选项与用户机密能力全部随共享框架与 SDK 内置