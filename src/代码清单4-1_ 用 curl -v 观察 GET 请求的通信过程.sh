curl -v --http1.1 https://example.com

# -v 开启详尽模式, --http1.1 显式钉住 1.1 版; 输出按三类前缀分行, 下面摘关键行(有删节):
# *   Trying example.com:443...           ← 过程行: 先把域名换成 IP, 再发起连接
# * Connected to example.com port 443     ← 过程行: 连接建立, 443 是 HTTPS 默认端口
# > GET / HTTP/1.1                        ← 请求行: 发出去的报文第一行, 方法 GET, 路径 /
# > Host: example.com                     ← 请求头: 告诉服务器要访问哪台主机
# > User-Agent: curl/8.x                  ← 请求头: 报明客户端身份, 版本号随安装而异
# > Accept: */*                           ← 请求头: 表示任何类型的响应都能接收
# < HTTP/1.1 200 OK                       ← 状态行: 200 表示请求成功
# < Content-Type: text/html; charset=UTF-8 ← 响应头: 正文是 UTF-8 编码的 HTML
# < Content-Length: 1256                  ← 响应头: 正文长度, 以字节计
# { 1256 字节的 HTML 正文 }               ← 最后落到正文, 即浏览器渲染的那份页面