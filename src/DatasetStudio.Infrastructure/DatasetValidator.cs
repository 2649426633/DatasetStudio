using DatasetStudio.Core;

namespace DatasetStudio.Infrastructure;

public sealed class DatasetValidator
{
    public IReadOnlyList<ValidationItem> Validate(
        CatalogRepository repository,
        int referenceWidth = 0,
        int referenceHeight = 0)
    {
        var images = repository.LoadImages();
        var rois = repository.LoadRois();
        var items = new List<ValidationItem>();

        var trainGood = images.Count(x => x.Split == DatasetSplit.Train && x.Truth == ImageTruth.Good);
        var testGood = images.Count(x => x.Split == DatasetSplit.Test && x.Truth == ImageTruth.Good);
        var testNg = images.Count(x => x.Split == DatasetSplit.Test && x.Truth == ImageTruth.Ng);
        var unclassified = images.Count(x => !x.IsClassified);
        var ngWithoutRoi = images.Count(x => x.Truth == ImageTruth.Ng && x.GetDefectRoiIds().Count == 0);
        var missingSource = images.Count(x => !File.Exists(x.SourcePath));

        var roiIds = rois.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknownRoi = images
            .Where(x => x.Truth == ImageTruth.Ng)
            .SelectMany(x => x.GetDefectRoiIds())
            .Count(id => !roiIds.Contains(id));

        var duplicateHashes = repository.GetDuplicateHashGroups();
        var trainTestLeak = duplicateHashes.Count(group =>
            group.Any(x => x.Split == DatasetSplit.Train) &&
            group.Any(x => x.Split == DatasetSplit.Test));
        var duplicateRoiIds = rois.Count - roiIds.Count;
        var outOfBounds = referenceWidth > 0 && referenceHeight > 0
            ? rois.Count(r =>
                r.X < 0 || r.Y < 0 || r.Width <= 0 || r.Height <= 0 ||
                r.X + r.Width > referenceWidth || r.Y + r.Height > referenceHeight)
            : 0;

        items.Add(CheckTrainGood(trainGood));
        items.Add(CheckCount("测试 GOOD", testGood));
        items.Add(CheckCount("测试 NG", testNg));
        items.Add(new ValidationItem(
            "Train/Test 重复图片", trainTestLeak,
            trainTestLeak == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            trainTestLeak == 0 ? "未发现 SHA-256 数据泄漏" : "同一 SHA-256 同时出现在 Train 与 Test，禁止导出"));
        items.Add(new ValidationItem(
            "重复 SHA256", duplicateHashes.Count,
            duplicateHashes.Count == 0 ? ValidationSeverity.Ok : ValidationSeverity.Warning,
            duplicateHashes.Count == 0 ? "未发现完全重复图片" : "存在重复图片，请确认是否为有意保留"));
        items.Add(new ValidationItem(
            "未分类图片", unclassified,
            unclassified == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            unclassified == 0 ? "全部图片已处理" : "仍有图片未分类；请分类或设置 Ignore"));
        items.Add(new ValidationItem(
            "NG 无 ROI 标签", ngWithoutRoi,
            ngWithoutRoi == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            ngWithoutRoi == 0 ? "所有 NG 均已标明异常 ROI" : "NG 必须指定至少一个异常 ROI"));
        items.Add(new ValidationItem(
            "不存在的 ROI ID", unknownRoi,
            unknownRoi == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            unknownRoi == 0 ? "标签中的 ROI 均存在" : "部分图片引用了已删除或不存在的 ROI"));
        items.Add(new ValidationItem(
            "ROI 超出参考图范围", outOfBounds,
            outOfBounds == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            referenceWidth <= 0 || referenceHeight <= 0
                ? "尚未设置有效参考图尺寸；设置 reference_aligned.png 后会检查 ROI 边界"
                : outOfBounds == 0 ? "ROI 坐标全部位于标准参考图内" : "部分 ROI 超出 reference_aligned.png 标准坐标范围"));
        items.Add(new ValidationItem(
            "ROI ID 重复", duplicateRoiIds,
            duplicateRoiIds == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            duplicateRoiIds == 0 ? "ROI ID 唯一" : "ROI ID 存在重复"));
        items.Add(new ValidationItem(
            "源图片不存在", missingSource,
            missingSource == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            missingSource == 0 ? "数据库记录对应的源文件均存在" : "部分源图片已被移动或删除，禁止导出"));

        return items;
    }

    public bool CanExport(
        CatalogRepository repository,
        out IReadOnlyList<ValidationItem> items,
        int referenceWidth = 0,
        int referenceHeight = 0)
    {
        items = Validate(repository, referenceWidth, referenceHeight);
        return items.All(x => x.Severity != ValidationSeverity.Error);
    }

    private static ValidationItem CheckTrainGood(int value)
    {
        if (value < 2)
        {
            return new ValidationItem(
                "训练 GOOD",
                value,
                ValidationSeverity.Error,
                "ProductAlignInspector 建立 Memory Bank 至少需要 2 张不同的 GOOD 原图；建议第一轮准备 10 张以上。");
        }

        if (value < 10)
        {
            return new ValidationItem(
                "训练 GOOD",
                value,
                ValidationSeverity.Warning,
                "可以进行流程验证，但少于 10 张只适合作为 smoke test；建议补充不同正常波动的 GOOD 原图。");
        }

        return new ValidationItem("训练 GOOD", value, ValidationSeverity.Ok, "数量满足 ProductAlignInspector 第一轮训练建议");
    }

    private static ValidationItem CheckCount(string name, int value) =>
        new(
            name,
            value,
            value > 0 ? ValidationSeverity.Ok : ValidationSeverity.Warning,
            value > 0 ? "数量正常" : "当前数量为 0，建议补充数据");
}
