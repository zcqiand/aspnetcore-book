// 前端侧示意：调用函数与回包类型由生成面给出，形状与契约对齐
import { listParamInterfaces } from "./api/endpoints/param-interfaces";

const result = await listParamInterfaces({ page: 1, pageSize: 20, keyword: "ph" });
// result.items / result.page / result.pageSize / result.total
// 四件套的类型在生成面里，字段名写错，编译期就报