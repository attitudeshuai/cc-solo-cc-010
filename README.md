# 持仓台账编辑器

配置驱动的持仓台账维护界面：字段的显示、可否修改、默认值由配置决定，台账数据与配置分开存放。

## How to Run

```bash
docker-compose up --build -d
```

访问地址：http://localhost:8081

## Services

| 服务 | 端口 | 说明 |
|------|------|------|
| frontend | 8081 | Vue 3 前端 + Nginx |
| backend | 8081 (内部) | ASP.NET Core Web API |

## 测试账号

无需登录，直接访问即可使用。

## 业务背景

持仓台账要维护现货和期货两本账，字段跟着业务调整走。原先把列名和可改不可改写死在页面里，每加一个字段都要改代码重新发版，所以改成由配置驱动：表头按配置渲染，明细数据按配置校验。

台账明细的维护方式：在台账上点右键呼出菜单，可新增、修改、锁定/解锁、删除；新增时按配置带出默认值，修改时只放开配置允许改的字段；被锁定的记录不允许修改和删除，用来保护已经核对过的账。台账数据当前保存在服务进程的全局变量里，另有保存到文件的入口。

字段配置存放在后端 Config 目录的 config.json 中，形如：

```json
[
  {
    "table_name": "spotHolding",
    "fields": [
      {"en_name": "code", "zh_name": "代码", "default_value": "", "is_modify": false, "is_show": true},
      {"en_name": "open_date", "zh_name": "开仓日期", "default_value": "0", "is_modify": true, "is_show": true},
      {"en_name": "posi_qty", "zh_name": "持仓数量", "default_value": "0", "is_modify": true, "is_show": true},
      {"en_name": "avail_qty", "zh_name": "可用数量", "default_value": "0", "is_modify": true, "is_show": true},
      {"en_name": "remark_info", "zh_name": "备注", "default_value": "", "is_modify": false, "is_show": false}
    ]
  },
  {
    "table_name": "futuresHolding",
    "fields": [
      {"en_name": "contract_code", "zh_name": "代码", "default_value": "", "is_modify": false, "is_show": true},
      {"en_name": "open_date", "zh_name": "开仓日期", "default_value": "0", "is_modify": true, "is_show": true},
      {"en_name": "posi_qty", "zh_name": "持仓数量", "default_value": "0", "is_modify": true, "is_show": true},
      {"en_name": "avail_qty", "zh_name": "可用数量", "default_value": "0", "is_modify": true, "is_show": true},
      {"en_name": "remark_info", "zh_name": "备注", "default_value": "", "is_modify": false, "is_show": false}
    ]
  }
]
```

明细数据与配置同名，例如 spotHolding.json：

```json
[
  {"code": "600000", "open_date": "20251212", "posi_qty": "100", "avail_qty": "80", "remark_info": "这是备注信息1"},
  {"code": "600001", "open_date": "20251212", "posi_qty": "100", "avail_qty": "80", "remark_info": "这是备注信息2"},
  {"code": "600002", "open_date": "20251212", "posi_qty": "100", "avail_qty": "80", "remark_info": "这是备注信息3"}
]
```

---

## 功能特性

- 根据配置动态生成表格列（支持隐藏列）
- 右键菜单操作：新增 / 修改 / 锁定 / 解锁 / 删除
- 行锁定保护：锁定状态下无法修改或删除
- 新增时显示默认值，修改时根据配置控制可编辑字段
- 数据持久化到 JSON 文件

## 技术栈

- 后端：ASP.NET Core 8.0
- 前端：Vue 3 + 原生 CSS
- 部署：Docker + Nginx

## 配置说明

表格配置文件：`backend/Config/config.json`

| 字段 | 说明 |
|------|------|
| table_name | 表格名称 |
| en_name | 字段英文名 |
| zh_name | 字段中文名（列标题） |
| default_value | 新增时的默认值 |
| is_modify | 是否允许修改 |
| is_show | 是否在表格中显示 |

## 项目结构

```
├── backend/                # ASP.NET Core 后端
│   ├── Config/             # 配置和数据文件
│   ├── Controllers/        # API 控制器
│   ├── Models/             # 数据模型
│   ├── Services/           # 业务逻辑
│   └── Dockerfile
├── frontend/               # Vue 前端
│   ├── index.html
│   ├── nginx.conf
│   └── Dockerfile
└── docker-compose.yml
```

## API 接口

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/table | 获取所有表格名称 |
| GET | /api/table/{name}/config | 获取表格配置 |
| GET | /api/table/{name}/rows | 获取表格数据 |
| POST | /api/table/{name}/rows | 新增行 |
| PUT | /api/table/{name}/rows/{id} | 修改行 |
| DELETE | /api/table/{name}/rows/{id} | 删除行 |
| POST | /api/table/{name}/rows/{id}/lock | 锁定行 |
| POST | /api/table/{name}/rows/{id}/unlock | 解锁行 |
| POST | /api/table/{name}/save | 保存到文件 |
