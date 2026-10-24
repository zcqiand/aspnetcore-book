dotnet new web -o ErrorLab
cd ErrorLab
# 本章零第三方包: dotnet new web 模板自带的家当已经够用, 契约服务与异常处理中间件都是框架内置,
# 不执行任何 dotnet add package, 也就没有任何版本号需要交代;
# ErrorLab 独立于前几个实验项目, 互不污染, 本章所有代码与配置都以 ErrorLab 目录为根
# 接下来覆盖模板的两份产物: appsettings.json 换成清单13-2, Program.cs 换成清单13-3