// 契约测试示意：验回包形状与契约相符，不验业务数据对错
test("参数界面列表回包守约", async () => {
  const res = await fetch(`${base}/api/param-interfaces?page=1&pageSize=20`);
  const body = await res.json();
  expect(body).toHaveProperty("items");
  expect(body).toHaveProperty("page");
  expect(body).toHaveProperty("pageSize");
  expect(body).toHaveProperty("total");
});