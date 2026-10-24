dotnet new web -o FilterLab
cd FilterLab
# 本章零第三方包: IEndpointFilter 与 AddEndpointFilter 是共享框架内置,
# 不执行任何 dotnet add package, 也就没有任何版本号需要交代;
# FilterLab 独立于前几个实验项目, 仓库从零起, 互不残留, 本章所有代码与配置都以 FilterLab 目录为根
# 接下来覆盖模板的两份产物: appsettings.json 换成清单14-2, Program.cs 换成清单14-3