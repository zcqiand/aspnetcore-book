# 版本号以 x 代替, 以读者机器实际解析为准

# 第一道坎: 工具缺失, 直接敲 dotnet ef 时系统找不到命令:
#   无法执行，因为找不到指定的命令或文件
# 解法一行, 全局安装一次:
PS> dotnet tool install --global dotnet-ef

# 第二道坎: 工具有了, 主仓没有 Design 包, migrations add 当场报:
#   Your startup project 'EFLab' doesn't reference Microsoft.EntityFrameworkCore.Design...
# 解法按报错提示补装:
PS> dotnet add package Microsoft.EntityFrameworkCore.Design --version "8.0.*"

# 注: 下行 Done 出自后文 dotnet ef migrations add, 非本命令回显
#   Done. To undo this action, use 'ef migrations remove'

# 第三道坎不报错: dotnet-ef 工具与运行时包的版本错配, 旧工具只多一行告警, 不中断