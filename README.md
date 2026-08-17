# DatasetStudio

![build](https://github.com/2649426633/DatasetStudio/actions/workflows/build.yml/badge.svg)

面向工业视觉数据准备的 WinForms 数据集工具。界面布局、浅色工业主题和品牌风格参考同账号仓库 `IAD26`，业务按 DatasetStudio 自己的职责重新设计。

## 开发环境

- Visual Studio 2026
- .NET 8 / `net8.0-windows`
- WinForms
- SQLite (`Microsoft.Data.Sqlite 8.0.29`)
- `System.Text.Json`

打开 `DatasetStudio.slnx`，启动项目选择 `src/DatasetStudio.WinForms/DatasetStudio.WinForms.csproj`。

## V1 页面

1. **数据集分类**：浏览原图、Train GOOD / Test GOOD / Test NG / Ignore、异常 ROI、缺陷类型、备注和快捷键。
2. **ROI 标定**：只在 `reference_aligned.png` 标准坐标系上绘制 S / E / SPRING / SURFACE ROI；支持拖动、四角缩放、键盘微调和表格坐标编辑。
3. **数据校验**：检查未分类、NG 缺 ROI、未知 ROI、ROI 越界、重复 SHA-256、Train/Test 数据泄漏和源文件丢失。
4. **导出 / 发布**：staging 复制、SHA-256 校验、生成 `dataset_manifest.csv` / `dataset_report.json`，并以备份 + staging + SHA256 + 失败回滚方式发布到 ProductAlignInspector 目录。

## 数据安全原则

- 原始图片目录只读使用。
- 分类时不复制、不移动、不删除原图，只写 `catalog.db` 标签。
- 同一路径源文件的 SHA256 如果变化，会自动清除该图片的旧分类，防止旧标签误用于新照片。
- 真正导出时才复制文件。
- 每次复制后重新计算 SHA-256，源/目标不一致立即终止。
- 发布前备份 DatasetStudio 管理的目标目录，先写目标盘 staging，校验后再替换；失败会尝试自动回滚。

## 快捷键

```text
T       Train GOOD
G       Test GOOD
N       Test NG
I       Ignore
1..9    切换前 9 个异常 ROI
Enter   保存 + 下一张
Space   下一张未分类
← / →   上一张 / 下一张
Ctrl+Z  撤销上一笔分类（Split / Truth / DefectType / ROI / Note 一起恢复）
```

备注文本框聚焦时 `Ctrl+Z` 保留为普通文本撤销。

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

NG 导出目录使用具体 ROI truth，例如：

```text
missing_S01
missing_S02
missing_S01+missing_S02
defect_E01
defect_SURFACE01
```

不再依赖 `all_empty`、`missing_screws`、`excess_screws` 这类无法定位具体 ROI 的模糊目录名。

## IAD26 Logo

应用图标使用 `IAD26` 仓库中的同一份 `科准LOGO白.ico`。WinForms 项目通过 `ApplicationIcon` 嵌入 EXE；GitHub Actions 也保留缺失时的自动补齐逻辑。

## CI

GitHub Actions 在 `main` push、面向 `main` 的 Pull Request 以及手动触发时，使用 Windows runner + .NET 8 执行 Restore / Release Build。这样功能分支在合并前就能验证 WinForms 是否可以编译。
