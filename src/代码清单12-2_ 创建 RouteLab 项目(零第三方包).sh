dotnet new web -o RouteLab
cd RouteLab
# 本章零第三方包: dotnet new web 模板自带的家当已经够用,
# 不执行任何 dotnet add package, 也就没有任何版本号需要交代;
# RouteLab 独立于前几个实验项目, 互不污染, 本章所有代码与配置都以 RouteLab 目录为根
# 接下来覆盖模板的两份产物: appsettings.json 换成清单12-3, Program.cs 换成清单12-4