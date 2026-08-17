using System.Text.Json;
using OpenCvSharp;
using OpenCvSharp.XFeatures2D;

namespace DatasetStudio.WinForms.Services;

public sealed class AlignmentPreviewResult
{
    public bool Success { get; set; }
    public string AlignedPath { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int FeatureMatches { get; set; }
    public int FeatureInliers { get; set; }
    public double FeatureInlierRatio { get; set; }
    public double? EccScore { get; set; }
    public string Error { get; set; } = string.Empty;

    public string Summary =>
        !Success
            ? $"FAILED: {Error}"
            : FeatureMatches > 0
                ? $"{Method} · inlier {FeatureInlierRatio:P1}" +
                  (EccScore.HasValue ? $" · ECC {EccScore.Value:F3}" : string.Empty)
                : $"{Method}" + (EccScore.HasValue ? $" · ECC {EccScore.Value:F3}" : string.Empty);
}

public sealed class ReferenceBuildResult
{
    public string ReferencePath { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public double DetectedAngleDeg { get; set; }
}

public static class ProductAlignmentService
{
    private sealed record AlignmentPreset(
        string Name,
        int MaxDim,
        int Features,
        double RatioTest,
        int MinMatches,
        int MinInliers,
        double MinInlierRatio,
        double RansacThreshold,
        double EccAccept,
        bool RequireEcc);

    private sealed class LocatedProduct : IDisposable
    {
        public Rect Bbox { get; }
        public Point2f Center { get; }
        public double AngleDeg { get; }
        public double Area { get; }
        public Mat Mask { get; }

        public LocatedProduct(Rect bbox, Point2f center, double angleDeg, double area, Mat mask)
        {
            Bbox = bbox;
            Center = center;
            AngleDeg = angleDeg;
            Area = area;
            Mask = mask;
        }

        public void Dispose() => Mask.Dispose();
    }

    private const int ForegroundThreshold = 238;
    private const double BorderMarginRatio = 0.004;
    private const double MinComponentAreaRatio = 0.00002;
    private const double CloseKernelRatio = 0.006;
    private const double CropPaddingRatio = 0.08;
    private const double MinScale = 0.70;
    private const double MaxScale = 1.30;
    private const double DefaultEccAccept = 0.75;

    private static readonly AlignmentPreset[] Presets =
    [
        new("sift_affine", 1800, 5000, 0.72, 12, 8, 0.25, 5.0, DefaultEccAccept, false),
        new("recovery_detail", 2600, 8000, 0.76, 10, 8, 0.20, 6.0, 0.80, true),
        new("recovery_relaxed", 3200, 12000, 0.80, 8, 6, 0.15, 8.0, 0.80, true),
        new("recovery_ultra", 3600, 16000, 0.82, 8, 6, 0.12, 10.0, 0.85, true)
    ];

    public static ReferenceBuildResult CreateReferenceFromGood(string sourcePath, string targetPath)
    {
        using var source = ReadColor(sourcePath);
        using var located = LocateProduct(source);
        var rotation = Math.Clamp(-located.AngleDeg, -90.0, 90.0);
        using var rotated = RotateBound(source, located.Center, rotation);
        using var rotatedLocation = LocateProduct(rotated);
        using var crop = CropWithPadding(rotated, rotatedLocation.Bbox, CropPaddingRatio);

        if (crop.Empty())
            throw new InvalidOperationException("参考图生成失败：产品裁剪结果为空。");

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        if (!Cv2.ImWrite(targetPath, crop))
            throw new IOException($"无法写入参考图：{targetPath}");

        return new ReferenceBuildResult
        {
            ReferencePath = targetPath,
            Width = crop.Width,
            Height = crop.Height,
            DetectedAngleDeg = located.AngleDeg
        };
    }

    public static AlignmentPreviewResult AlignToReference(
        string sourcePath,
        string referencePath,
        string alignedOutputPath)
    {
        try
        {
            using var image = ReadColor(sourcePath);
            using var reference = ReadColor(referencePath);

            foreach (var preset in Presets)
            {
                var result = TryFeatureAlign(image, reference, preset);
                if (result is null)
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(alignedOutputPath)!);
                if (!Cv2.ImWrite(alignedOutputPath, result.Value.Aligned))
                {
                    result.Value.Aligned.Dispose();
                    throw new IOException($"无法写入对齐缓存：{alignedOutputPath}");
                }

                result.Value.Aligned.Dispose();
                return new AlignmentPreviewResult
                {
                    Success = true,
                    AlignedPath = alignedOutputPath,
                    Method = result.Value.Method,
                    FeatureMatches = result.Value.Matches,
                    FeatureInliers = result.Value.Inliers,
                    FeatureInlierRatio = result.Value.InlierRatio,
                    EccScore = result.Value.EccScore
                };
            }

            var fallback = ForegroundFallback(image, reference);
            Directory.CreateDirectory(Path.GetDirectoryName(alignedOutputPath)!);
            if (!Cv2.ImWrite(alignedOutputPath, fallback.Aligned))
            {
                fallback.Aligned.Dispose();
                throw new IOException($"无法写入对齐缓存：{alignedOutputPath}");
            }

            fallback.Aligned.Dispose();
            return new AlignmentPreviewResult
            {
                Success = true,
                AlignedPath = alignedOutputPath,
                Method = fallback.Method,
                EccScore = fallback.EccScore
            };
        }
        catch (Exception ex)
        {
            return new AlignmentPreviewResult
            {
                Success = false,
                AlignedPath = string.Empty,
                Method = "failed",
                Error = ex.Message
            };
        }
    }

    public static void WriteMetadata(string path, AlignmentPreviewResult result)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static AlignmentPreviewResult? ReadMetadata(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<AlignmentPreviewResult>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    private static (Mat Aligned, string Method, int Matches, int Inliers, double InlierRatio, double? EccScore)?
        TryFeatureAlign(Mat image, Mat reference, AlignmentPreset preset)
    {
        Mat? matrix = null;
        Mat? featureAligned = null;
        Mat? refined = null;
        try
        {
            using var refGray = ResizeForFeatures(reference, preset.MaxDim, out var refScale);
            using var imgGray = ResizeForFeatures(image, preset.MaxDim, out var imgScale);
            using var sift = SIFT.Create(
                nFeatures: preset.Features,
                contrastThreshold: 0.02,
                edgeThreshold: 12);
            using var refDesc = new Mat();
            using var imgDesc = new Mat();

            sift.DetectAndCompute(refGray, null, out var refKeypoints, refDesc);
            sift.DetectAndCompute(imgGray, null, out var imgKeypoints, imgDesc);

            if (refDesc.Empty() || imgDesc.Empty() || refKeypoints.Length < 4 || imgKeypoints.Length < 4)
                return null;

            using var matcher = new BFMatcher(NormTypes.L2, crossCheck: false);
            var pairs = matcher.KnnMatch(refDesc, imgDesc, 2);
            var good = pairs
                .Where(pair => pair.Length >= 2 && pair[0].Distance < preset.RatioTest * pair[1].Distance)
                .Select(pair => pair[0])
                .ToArray();

            if (good.Length < preset.MinMatches)
                return null;

            var refPoints = good
                .Select(m => new Point2f(
                    refKeypoints[m.QueryIdx].Pt.X / (float)refScale,
                    refKeypoints[m.QueryIdx].Pt.Y / (float)refScale))
                .ToArray();
            var imgPoints = good
                .Select(m => new Point2f(
                    imgKeypoints[m.TrainIdx].Pt.X / (float)imgScale,
                    imgKeypoints[m.TrainIdx].Pt.Y / (float)imgScale))
                .ToArray();

            using var from = InputArray.Create(imgPoints);
            using var to = InputArray.Create(refPoints);
            using var inlierMask = new Mat();
            matrix = Cv2.EstimateAffinePartial2D(
                from,
                to,
                inlierMask,
                RobustEstimationAlgorithms.RANSAC,
                preset.RansacThreshold,
                5000,
                0.999,
                50);

            if (matrix is null || matrix.Empty())
                return null;

            var inliers = Cv2.CountNonZero(inlierMask);
            var inlierRatio = inliers / (double)Math.Max(1, good.Length);
            if (inliers < preset.MinInliers || inlierRatio < preset.MinInlierRatio)
                return null;

            var a = matrix.At<double>(0, 0);
            var b = matrix.At<double>(0, 1);
            var scale = Math.Sqrt(a * a + b * b);
            if (scale < MinScale || scale > MaxScale)
                return null;

            featureAligned = new Mat();
            Cv2.WarpAffine(
                image,
                featureAligned,
                matrix,
                reference.Size(),
                InterpolationFlags.Linear,
                BorderTypes.Constant,
                Scalar.All(255));

            var ecc = EccRefine(reference, featureAligned, preset.RequireEcc ? 320 : 200);
            refined = ecc.Refined;

            var eccOk = ecc.Score.HasValue && ecc.Score.Value >= preset.EccAccept;
            if (preset.RequireEcc && !eccOk)
                return null;

            Mat output;
            string method;
            if (eccOk)
            {
                output = refined;
                refined = null;
                method = preset.Name + "+ecc";
            }
            else
            {
                output = featureAligned;
                featureAligned = null;
                method = preset.Name;
            }

            return (output, method, good.Length, inliers, inlierRatio, ecc.Score);
        }
        catch
        {
            return null;
        }
        finally
        {
            matrix?.Dispose();
            featureAligned?.Dispose();
            refined?.Dispose();
        }
    }

    private static (Mat Aligned, string Method, double EccScore) ForegroundFallback(Mat image, Mat reference)
    {
        using var located = LocateProduct(image);
        var baseRotation = -located.AngleDeg;
        var rotations = new[] { baseRotation, baseRotation + 90, baseRotation + 180, baseRotation + 270 }
            .Select(NormalizeAngle)
            .DistinctBy(x => Math.Round(x, 4))
            .ToArray();

        Mat? best = null;
        double bestScore = double.NegativeInfinity;
        double bestRotation = 0;

        foreach (var rotation in rotations)
        {
            Mat? candidate = null;
            Mat? refined = null;
            try
            {
                candidate = ForegroundCandidate(image, located, rotation, reference.Size());
                var ecc = EccRefine(reference, candidate, 350);
                refined = ecc.Refined;
                if (!ecc.Score.HasValue || ecc.Score.Value <= bestScore)
                    continue;

                best?.Dispose();
                best = refined;
                refined = null;
                bestScore = ecc.Score.Value;
                bestRotation = rotation;
            }
            catch
            {
                // Try the remaining quadrant candidates.
            }
            finally
            {
                candidate?.Dispose();
                refined?.Dispose();
            }
        }

        if (best is null || bestScore < DefaultEccAccept)
        {
            best?.Dispose();
            throw new InvalidOperationException(
                $"产品配准失败：前景回退最佳 ECC={bestScore:F3}，低于 {DefaultEccAccept:F2}。");
        }

        return (best, $"foreground_quadrant+ecc({bestRotation:F1}°)", bestScore);
    }

    private static (Mat Refined, double? Score) EccRefine(Mat reference, Mat moving, int iterations)
    {
        using var template = EccReady(reference);
        using var input = EccReady(moving);
        using var warp = new Mat(2, 3, MatType.CV_32F, Scalar.All(0));
        warp.Set<float>(0, 0, 1f);
        warp.Set<float>(1, 1, 1f);

        try
        {
            var criteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.Count, iterations, 1e-6);
            var score = Cv2.FindTransformECC(
                template,
                input,
                warp,
                MotionTypes.Affine,
                criteria,
                null,
                5);
            var refined = new Mat();
            Cv2.WarpAffine(
                moving,
                refined,
                warp,
                reference.Size(),
                InterpolationFlags.Linear | InterpolationFlags.WarpInverseMap,
                BorderTypes.Constant,
                Scalar.All(255));
            return (refined, score);
        }
        catch
        {
            return (moving.Clone(), null);
        }
    }

    private static Mat EccReady(Mat image)
    {
        using var gray = new Mat();
        if (image.Channels() == 1)
            image.CopyTo(gray);
        else
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

        using var blurred = new Mat();
        Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);
        var normalized = new Mat();
        Cv2.Normalize(blurred, normalized, 0.0, 1.0, NormTypes.MinMax, MatType.CV_32F);
        return normalized;
    }

    private static Mat ResizeForFeatures(Mat image, int maxDim, out double scale)
    {
        scale = Math.Min(1.0, maxDim / (double)Math.Max(image.Width, image.Height));
        using var resized = new Mat();
        if (scale < 0.9999)
        {
            Cv2.Resize(
                image,
                resized,
                new Size(
                    Math.Max(1, (int)Math.Round(image.Width * scale)),
                    Math.Max(1, (int)Math.Round(image.Height * scale))),
                0,
                0,
                InterpolationFlags.Area);
        }
        else
        {
            image.CopyTo(resized);
        }

        var gray = new Mat();
        if (resized.Channels() == 1)
            resized.CopyTo(gray);
        else
            Cv2.CvtColor(resized, gray, ColorConversionCodes.BGR2GRAY);
        return gray;
    }

    private static Mat ForegroundCandidate(Mat image, LocatedProduct located, double rotation, Size targetSize)
    {
        using var rotated = Math.Abs(rotation) > 1e-3
            ? RotateBound(image, located.Center, rotation)
            : image.Clone();
        using var rotatedLocation = LocateProduct(rotated);
        using var crop = CropWithPadding(rotated, rotatedLocation.Bbox, CropPaddingRatio);
        if (crop.Empty())
            throw new InvalidOperationException("前景回退裁剪为空。");

        var output = new Mat();
        Cv2.Resize(
            crop,
            output,
            targetSize,
            0,
            0,
            crop.Width > targetSize.Width ? InterpolationFlags.Area : InterpolationFlags.Cubic);
        return output;
    }

    private static LocatedProduct LocateProduct(Mat image)
    {
        var mask = BuildForegroundMask(image);
        using var contourSource = mask.Clone();
        Cv2.FindContours(
            contourSource,
            out Point[][] contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxNone);

        if (contours.Length == 0)
        {
            mask.Dispose();
            throw new InvalidOperationException("未找到产品轮廓，请检查光照或前景阈值。");
        }

        var contour = contours.OrderByDescending(Cv2.ContourArea).First();
        var bbox = Cv2.BoundingRect(contour);
        var moments = Cv2.Moments(contour);
        var center = Math.Abs(moments.M00) > 1e-6
            ? new Point2f((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00))
            : new Point2f(bbox.X + bbox.Width / 2f, bbox.Y + bbox.Height / 2f);
        var angle = PrincipalAngleDeg(contour);
        return new LocatedProduct(bbox, center, angle, Cv2.ContourArea(contour), mask);
    }

    private static Mat BuildForegroundMask(Mat image)
    {
        using var gray = new Mat();
        if (image.Channels() == 1)
            image.CopyTo(gray);
        else
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

        using var blurred = new Mat();
        Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);

        using var thresholded = new Mat();
        Cv2.Threshold(blurred, thresholded, ForegroundThreshold, 255, ThresholdTypes.BinaryInv);

        using var thresholdForContours = thresholded.Clone();
        Cv2.FindContours(
            thresholdForContours,
            out Point[][] components,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        using var filtered = new Mat(thresholded.Rows, thresholded.Cols, MatType.CV_8UC1, Scalar.All(0));
        var borderMargin = Math.Max(1, (int)(Math.Min(image.Width, image.Height) * BorderMarginRatio));
        var minArea = Math.Max(25.0, image.Width * image.Height * MinComponentAreaRatio);

        foreach (var contour in components)
        {
            var area = Cv2.ContourArea(contour);
            if (area < minArea) continue;
            var rect = Cv2.BoundingRect(contour);
            var touchesBorder =
                rect.X <= borderMargin ||
                rect.Y <= borderMargin ||
                rect.Right >= image.Width - borderMargin ||
                rect.Bottom >= image.Height - borderMargin;
            if (touchesBorder) continue;
            Cv2.DrawContours(filtered, new[] { contour }, -1, Scalar.All(255), Cv2.FILLED);
        }

        var k = Odd((int)Math.Round(Math.Min(image.Width, image.Height) * CloseKernelRatio));
        using var closeKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(k, k));
        using var closed = new Mat();
        Cv2.MorphologyEx(filtered, closed, MorphTypes.Close, closeKernel, iterations: 2);

        var dilateK = Odd(Math.Max(3, k / 2));
        using var dilateKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(dilateK, dilateK));
        using var merged = new Mat();
        Cv2.Dilate(closed, merged, dilateKernel);

        using var mergedForContours = merged.Clone();
        Cv2.FindContours(
            mergedForContours,
            out Point[][] contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);
        if (contours.Length == 0)
            throw new InvalidOperationException("未找到产品前景，请调整光照或选择更清晰的 GOOD 原图。");

        var largest = contours.OrderByDescending(Cv2.ContourArea).First();
        var finalMask = new Mat(merged.Rows, merged.Cols, MatType.CV_8UC1, Scalar.All(0));
        Cv2.DrawContours(finalMask, new[] { largest }, -1, Scalar.All(255), Cv2.FILLED);
        return finalMask;
    }

    private static double PrincipalAngleDeg(Point[] contour)
    {
        if (contour.Length < 2) return 0.0;
        var meanX = contour.Average(p => (double)p.X);
        var meanY = contour.Average(p => (double)p.Y);
        double cxx = 0, cyy = 0, cxy = 0;
        foreach (var p in contour)
        {
            var dx = p.X - meanX;
            var dy = p.Y - meanY;
            cxx += dx * dx;
            cyy += dy * dy;
            cxy += dx * dy;
        }

        var angle = 0.5 * Math.Atan2(2.0 * cxy, cxx - cyy) * 180.0 / Math.PI;
        while (angle >= 90.0) angle -= 180.0;
        while (angle < -90.0) angle += 180.0;
        return angle;
    }

    private static Mat RotateBound(Mat image, Point2f center, double angle)
    {
        using var matrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);
        var cos = Math.Abs(matrix.At<double>(0, 0));
        var sin = Math.Abs(matrix.At<double>(0, 1));
        var newWidth = Math.Max(1, (int)Math.Ceiling(image.Height * sin + image.Width * cos));
        var newHeight = Math.Max(1, (int)Math.Ceiling(image.Height * cos + image.Width * sin));

        matrix.Set<double>(0, 2, matrix.At<double>(0, 2) + newWidth / 2.0 - center.X);
        matrix.Set<double>(1, 2, matrix.At<double>(1, 2) + newHeight / 2.0 - center.Y);

        var rotated = new Mat();
        Cv2.WarpAffine(
            image,
            rotated,
            matrix,
            new Size(newWidth, newHeight),
            InterpolationFlags.Linear,
            BorderTypes.Constant,
            Scalar.All(255));
        return rotated;
    }

    private static Mat CropWithPadding(Mat image, Rect bbox, double paddingRatio)
    {
        var padX = (int)Math.Round(bbox.Width * paddingRatio);
        var padY = (int)Math.Round(bbox.Height * paddingRatio);
        var x0 = Math.Max(0, bbox.X - padX);
        var y0 = Math.Max(0, bbox.Y - padY);
        var x1 = Math.Min(image.Width, bbox.Right + padX);
        var y1 = Math.Min(image.Height, bbox.Bottom + padY);
        var rect = new Rect(x0, y0, Math.Max(1, x1 - x0), Math.Max(1, y1 - y0));
        return new Mat(image, rect).Clone();
    }

    private static Mat ReadColor(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("图片不存在。", path);
        var image = Cv2.ImRead(path, ImreadModes.Color);
        if (image.Empty())
        {
            image.Dispose();
            throw new InvalidDataException($"无法读取图片：{path}");
        }
        return image;
    }

    private static int Odd(int value)
    {
        value = Math.Max(3, value);
        return value % 2 == 1 ? value : value + 1;
    }

    private static double NormalizeAngle(double angle)
    {
        var value = (angle + 180.0) % 360.0;
        if (value < 0) value += 360.0;
        return value - 180.0;
    }
}
