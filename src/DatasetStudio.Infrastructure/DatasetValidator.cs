using DatasetStudio.Core;

namespace DatasetStudio.Infrastructure;

public sealed class DatasetValidator
{
    public IReadOnlyList<ValidationItem> Validate(CatalogRepository repository)
    {
        var images = repository.LoadImages();
        var rois = repository.LoadRois();
        var items = new List<ValidationItem>();

        var trainGood = images.Count(x => x.Split == DatasetSplit.Train && x.Truth == ImageTruth.Good);
        var testGood = images.Count(x => x.Split == DatasetSplit.Test && x.Truth == ImageTruth.Good);
        var testNg = images.Count(x => x.Split == DatasetSplit.Test && x.Truth == ImageTruth.Ng);
        var unclassified = images.Count(x => !x.IsClassified);
        var ngWithoutRoi = images.Count(x => x.Truth == ImageTruth.Ng && x.GetDefectRoiIds().Count == 0);
        var roiIds = rois.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknownRoi = images
            .Where(x => x.Truth == ImageTruth.Ng)
            .SelectMany(x => x.GetDefectRoiIds())
            .Count(id => !roiIds.Contains(id));

        var duplicateHashes = repository.GetDuplicateHashGroups();
        var trainTestLeak = duplicateHashes.Count(group =>
            group.Any(x => x.Split == DatasetSplit.Train) &&
            group.Any(x => x.Split == DatasetSplit.Test));

        items.Add(CheckCount("训练 GOOD", trainGood, true));
        items.Add(CheckCount("测试 GOOD", testGood, true));
        items.Add(CheckCount("测试 NG", testNg, true));
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
            unclassified == 0 ? "全部图片已处理" : "仍有图片未分类"));
        items.Add(new ValidationItem(
            "NG 无 ROI 标签", ngWithoutRoi,
            ngWithoutRoi == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            ngWithoutRoi == 0 ? "所有 NG 均已标明异常 ROI" : "NG 必须指定至少一个异常 ROI"));
        items.Add(new ValidationItem(
            "不存在的 ROI ID", unknownRoi,
            unknownRoi == 0 ? ValidationSeverity.Ok : ValidationSeverity.Error,
            unknownRoi == 0 ? "标签中的 ROI 均存在" : "部分图片引用了已删除或不存在的 ROI"));
        items.Add(new ValidationItem(
            "ROI ID 重复", rois.Count - roiIds.Count,
            rois.Count == roiIds.Count ? ValidationSeverity.Ok : ValidationSeverity.Error,
            rois.Count == roiIds.Count ? "ROI ID 唯一" : "ROI ID 存在重复"));

        return items;
    }

    public bool CanExport(CatalogRepository repository, out IReadOnlyList<ValidationItem> items)
    {
        items = Validate(repository);
        return items.All(x => x.Severity != ValidationSeverity.Error);
    }

    private static ValidationItem CheckCount(string name, int value, bool requirePositive) =>
        new(
            name,
            value,
            !requirePositive || value > 0 ? ValidationSeverity.Ok : ValidationSeverity.Warning,
            value > 0 ? "数量正常" : "当前数量为 0，建议补充数据");
}
