# DatasetStudio

![build](https://github.com/2649426633/DatasetStudio/actions/workflows/build.yml/badge.svg)

面向工业视觉数据准备的 WinForms 数据集工具。界面布局、浅色工业主题和品牌风格参考同账号仓库 `IAD26`，业务按 DatasetStudio 自己的职责重新设计。

当前数据接口直接面向 `ProductAlignInspector`：**原始完整图片 → 标准参考图 / 固定 ROI → Train GOOD / Test GOOD / Test NG 精确 ROI truth → 一键导出**。DatasetStudio 不裁训练 ROI 小图，也不训练 DINO/PatchCore；训练端继续由 ProductAlignInspector 在完整原图上完成产品配准和 ROI 裁剪。

## 开发环境

- Visual Studio 2026
- .NET 8 / `net8.0-windows`
- WinForms
- SQLite (`Microsoft.Data.Sqlite 8.0.29`)
- OpenCvSharp4.Windows
- `System.Text.Json`

打开 `DatasetStudio.slnx`，启动项目选择 `src/DatasetStudio.WinForms/DatasetStudio.WinForms.csproj`。

## V1 页面

1. **数据集分类**：浏览完整原图，分类为 Train GOOD / Test GOOD / Test NG / Ignore；存在参考图时默认显示配准后的标准坐标预览，并在该预览上叠加固定 ROI。按 `V` 可在原图 / 对齐图之间切换。
2. **ROI 标定**：只在 `reference_aligned.png` 标准坐标系上绘制 S / E / SPRING / SURFACE ROI；支持拖动、四角缩放、键盘微调和表格坐标编辑。现有“参考图”入口同时支持“从 GOOD 原图自动创建”或“导入已有 reference_aligned.png”，不改变页面布局。
3. **数据校验**：检查未分类、NG 缺 ROI、未知 ROI、ROI 越界、重复 SHA-256、Train/Test 数据泄漏、源文件丢失，并要求 ProductAlignInspector 训练至少有 2 张 Train GOOD。
4. **导出 / 发布**：staging 复制、SHA-256 校验、生成 `dataset_manifest.csv` / `dataset_report.json`，执行 ProductAlignInspector 目录兼容性检查，并以备份 + staging + SHA256 + 失败回滚方式发布到 ProductAlignInspector 目录。

## 推荐工作流

```text
一堆完整原始图片
    ↓
DatasetStudio 新建项目 / 扫描目录
    ↓
从一张清晰 GOOD 原图创建 reference_aligned.png
    ↓
在 reference_aligned.png 上标定固定 ROI
    ↓
分类图片
    ├─ Train GOOD
    ├─ Test GOOD
    ├─ Test NG + 精确异常 ROI
    └─ Ignore
    ↓
数据校验
    ↓
导出 ProductAlignPackage
    ↓
ProductAlignInspector 训练 / 评估
```

参考图自动创建使用与 ProductAlignInspector 相同思路的前景定位、主方向矫正和标准裁剪。分类页的预览配准采用 SIFT / RANSAC 仿射为主、ECC 质量确认与前景方向回退；成功后才允许给 Test NG 选择标准坐标 ROI。这样避免产品在原图中发生平移或旋转时，把异常错误标到其他 ROI。

## 数据安全原则

- 原始图片目录只读使用。
- 分类时不复制、不移动、不删除原图，只写 `catalog.db` 标签。
- 同一路径源文件的 SHA256 如果变化，会自动清除该图片的旧分类，防止旧标签误用于新照片。
- 对齐预览缓存只写到 DatasetStudio 项目目录下的 `cache\aligned\`，不会写入原始图片目录。
- 更换参考图后自动清除旧对齐缓存。
- 真正导出时才复制文件。
- 每次复制后重新计算 SHA-256，源/目标不一致立即终止。
- 发布前备份 DatasetStudio 管理的目标目录，先写目标盘 staging，校验后再替换；失败会尝试自动回滚。

## 快捷键

```text
T       Train GOOD
G       Test GOOD
N       Test NG
I       Ignore
V       原图 / 对齐预览切换
1..9    切换前 9 个异常 ROI
Enter   保存 + 下一张
Space   下一张未分类
← / →   上一张 / 下一张
Ctrl+Z  撤销上一笔分类（Split / Truth / DefectType / ROI / Note 一起恢复）
```

备注文本框聚焦时 `Ctrl+Z` 保留为普通文本撤销。

## Test NG 标注原则

Test NG 必须先成功配准到 `reference_aligned.png`，之后才能选择异常 ROI。配准失败的图片不允许直接保存精确 ROI truth，建议检查参考图 / 原图，或将无法使用的图片设为 Ignore。

例子：

```text
S01 缺螺丝
→ Test NG
→ DefectType = Missing
→ ROI = S01

S01 + S02 都缺螺丝
→ Test NG
→ DefectType = Missing
→ ROI = S01 + S02

E01 多装零件
→ Test NG
→ DefectType = Excess
→ ROI = E01

SURFACE01 划伤
→ Test NG
→ DefectType = Surface
→ ROI = SURFACE01
```

## ROI 操作

- `S` / `E` / `P` / `A` 工具按钮创建对应 ROI。
- 鼠标左键拖动 ROI 移动位置。
- 选中 ROI 后拖四角控制点改变大小。
- 鼠标滚轮缩放图片；中键/右键拖动画布。
- 方向键微调 1 px，`Shift + 方向键` 微调 10 px。
- 右侧表格可直接编辑 `X / Y / W / H / Enabled`。
- 所有坐标会限制在 `reference_aligned.png` 标准参考图范围内。

## 项目目录

```text
DatasetProject\
├─ project.json
├─ catalog.db
├─ reference\
│  └─ reference_aligned.png
├─ configs\
│  └─ <product>.json
├─ cache\
│  └─ aligned\
└─ exports\
   └─ yyyyMMdd_HHmmss\
```

## 导出目录

```text
ProductAlignPackage\
├─ configs\<product>.json
├─ artifacts\reference\reference_aligned.png
├─ dataset_roi_dino\train\good\
├─ dataset_roi_dino\test\good\
├─ dataset_roi_dino\test\ng\missing_S01+missing_S02\...
├─ dataset_manifest.csv
└─ dataset_report.json
```

Train GOOD 导出的仍然是**完整原图**。ProductAlignInspector 在训练时自行完成配准、固定 ROI 裁剪和每个 ROI 的 GOOD Memory Bank 建立。

NG 导出目录使用 ProductAlignInspector 可直接识别的精确 ROI truth，例如：

```text
missing_S01
missing_S02
missing_S01+missing_S02
defect_E01
defect_SURFACE01
defect_S01
```

目录名只承担 ProductAlignInspector 的精确 ROI truth 接口：

- `Missing` 使用 `missing_<ROI>`。
- `Wrong / Excess / Surface / Other` 统一使用 `defect_<ROI>`。
- 真实 `DefectType` 仍完整保存在 `dataset_manifest.csv`，不会丢失。

不再输出 `wrong_S01` 这种 ProductAlignInspector 当前精确标签解析器无法识别的目录，也不依赖 `all_empty`、`missing_screws`、`excess_screws` 这类无法定位具体 ROI 的模糊目录名。

导出完成前还会再次检查：

- `artifacts/reference/reference_aligned.png` 是否存在；
- `train/good` 是否至少有 2 张完整 GOOD 原图；
- 是否至少有一个启用 ROI；
- 每个 NG 目录是否只使用 `missing_` / `defect_`；
- NG 目录引用的 ROI 是否真实存在并启用。

只有兼容性检查通过后，`dataset_report.json` 才会写入：

```json
{
  "product_align_inspector_compatible": true,
  "dataset_layout": "dataset_roi_dino.v1"
}
```

## IAD26 Logo

应用图标使用 `IAD26` 仓库中的同一份 `科准LOGO白.ico`。WinForms 项目通过 `ApplicationIcon` 嵌入 EXE；GitHub Actions 也保留缺失时的自动补齐逻辑。

## CI

GitHub Actions 在 `main` push、面向 `main` 的 Pull Request 以及手动触发时，使用 Windows runner + .NET 8 执行 Restore / Release Build。
