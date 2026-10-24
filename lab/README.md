# lab/ — 章级验证工程

随书章节的独立验证工程按 `lab/chNN/<工程名>/` 存放：一章可含多个工程，每个工程独立可运行。

- 章节正文见主仓 `output/xr-know-*/chapters/`，此处只放随书代码
- 工程自带 `.gitignore`（bin/obj 等可再生目录不入库）
- 章级验证回执（test receipts）放工程内 `test/receipts/`

现有工程：

- `ch24/chatlab/` — ASP.NET Core 8 + SignalR 实时聊天室（xr-know-016 chapter-024 随书验证工程，零 PackageReference）

（2026-10-24 起沿用 flutter-book/lab 的目录约定）
