psql -U postgres
# 口令在你安装 PostgreSQL 时自行设置, 输入时屏幕不回显是正常现象

CREATE DATABASE orderdemo;
\c orderdemo

-- BIGSERIAL 是 PostgreSQL 对「bigint 自增主键」的简写, 相当于标准 SQL 里
-- 「声明 bigint 再挂序列默认值」两句合一, 属于方言提供的便利写法
CREATE TABLE orders (
    id       BIGSERIAL     PRIMARY KEY,
    order_no TEXT          NOT NULL,
    customer TEXT,
    amount   NUMERIC(10,2)
);

-- 种子数据显式给 id = 1, 让下一节的 WHERE id = 1 查询必定命中
INSERT INTO orders (id, order_no, customer, amount)
VALUES (1, 'ORD-1001', '阿里山茶行', 1280.00);