# 以下三条命令都在 ConfigLab 目录内执行
# 第 1 步: 初始化用户机密——init 在 ConfigLab.csproj 里写入一行 <UserSecretsId>…</UserSecretsId>,
#          这串 ID 就是本项目的机密「户头号」, 机密文件按它存放在用户目录而不是项目目录
dotnet user-secrets init

# 第 2 步: 写入一条机密——键名 Report:Title 与清单10-2/10-3 的配置键逐字同形,
#          冒号是配置系统的层级分隔符, 所以机密层覆盖的正是配置树上的 Report:Title 那一个键
dotnet user-secrets set "Report:Title" "机密层报表标题"

# 第 3 步: 查看当前项目的全部机密, 确认第 2 步真的写进去了
dotnet user-secrets list

# 三条命令的共同观察点:
# 1. 机密明文存放在用户目录(Windows 为 %APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json),
#    不进代码仓库, 也不加密——它的保护机制是「作用域隔离」(机密只存在开发者本机用户目录),
#    不是加密; 真正的保密要求(如生产密钥)要交给环境变量或专业密钥服务, 用户机密只管开发期
# 2. 仅 Development 环境默认挂载这层来源, 换到 Production 运行时机密层根本不在配置树里
# 3. secrets.json 与 appsettings 文件一样默认可热重载, 改完免重启即生效(清单10-6 第 3 步实测)