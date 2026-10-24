dotnet new web -o ChatLab
# 浏览器客户端脚本：官方分发的浏览器版单文件 signalr.min.js，
# 复制进 wwwroot/js/，页面以静态引用加载（获取口径见上文交代）
mkdir -p ChatLab/wwwroot/js
cp signalr.min.js ChatLab/wwwroot/js/signalr.min.js