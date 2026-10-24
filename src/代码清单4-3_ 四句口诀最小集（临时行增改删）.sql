-- ① INSERT: 插入一条练习用临时行, 预期影响 1 行
-- id 用 9001, 与种子行的 1 拉开距离, 避免与自增序列撞号
INSERT INTO orders (id, order_no, customer, amount)
VALUES (9001, 'ORD-TEST', '练习临时户', 99.50);

-- ② SELECT: 全表一次, 预期 2 行（种子行 + 临时行）
SELECT * FROM orders;
-- 条件查一次, 预期 1 行（只有临时行）
SELECT * FROM orders WHERE order_no = 'ORD-TEST';

-- ③ UPDATE: 只改临时行的金额, 预期影响 1 行
UPDATE orders SET amount = 199.00 WHERE order_no = 'ORD-TEST';

-- ④ DELETE: 删掉临时行, 预期影响 1 行; 种子行全程不动
DELETE FROM orders WHERE order_no = 'ORD-TEST';

-- 回滚式验证: 最后一查应只剩种子行, 预期 1 行, 相当于什么都没发生过
SELECT * FROM orders;