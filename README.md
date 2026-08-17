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

1. **数据集分类**：浏览原图、Train GOOD / Test GOOD / Test NG / Ignore、异常 ROI、缺陷类型、快捷键。
2. **ROI 标定**：只在 `reference_aligned.png` 标准坐标系上绘制 S / E / SPRING / SURFACE ROI。
3. **数据校验**：检查未分类、NG 缺 ROI、未知 ROI、重复 SHA-256、Train/Test 数据泄漏。
4. **导出 / 发布**：staging 复制、SHA-256 校验、生成 `dataset_manifest.csv` / `dataset_report.json`，并可备份后发布到 ProductAlignInspector 目录。

## 数据安全原则

- 原始图片目录只读使用。
- 分类时不复制、不移动、不删除原图，只写 `catalog.db` 标签。
- 真正导出时才复制文件。
- 每次复制后重新计算 SHA-256，源/目标不一致立即终止。
- 发布前备份 DatasetStudio 管理的目标目录。

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

## IAD26 Logo

应用图标使用 `IAD26` 仓库中的同一份 `科准LOGO白.ico`。GitHub Actions 在首次构建时会把该二进制资源复制到 `assets/科准LOGO白.ico` 并提交回本仓库，随后 WinForms 项目通过 `ApplicationIcon` 嵌入 EXE。
