global using SIFT = OpenCvSharp.Features2D.SIFT;

namespace DatasetStudio.WinForms.Services;

/// <summary>
/// Small compile-time compatibility helpers for the OpenCvSharp4 API used by DatasetStudio.
/// Keeps version-specific type resolution out of the WinForms pages and alignment workflow.
/// </summary>
internal static class OpenCvSharp4Compatibility
{
    /// <summary>
    /// Disambiguates Cv2.ContourArea overloads when a Point[][] contour collection is sorted.
    /// </summary>
    public static IOrderedEnumerable<OpenCvSharp.Point[]> OrderByDescending(
        this OpenCvSharp.Point[][] source,
        Func<OpenCvSharp.Point[], double> keySelector) =>
        System.Linq.Enumerable.OrderByDescending(source, keySelector);
}
