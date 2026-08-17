global using SIFT = OpenCvSharp.Features2D.SIFT;

namespace DatasetStudio.WinForms.Services;

/// <summary>
/// Small compile-time compatibility helpers for the OpenCvSharp4 API used by DatasetStudio.
/// Keeps version-specific type resolution out of the WinForms pages and alignment workflow.
/// </summary>
internal static class OpenCvSharp4Compatibility
{
    internal delegate double ContourAreaSelector(
        IEnumerable<OpenCvSharp.Point> contour,
        bool oriented);

    /// <summary>
    /// Disambiguates Cv2.ContourArea overloads when a Point[][] contour collection is sorted.
    /// OpenCvSharp exposes ContourArea(contour, oriented = false) as a two-parameter method,
    /// so the delegate intentionally mirrors that signature.
    /// </summary>
    public static IOrderedEnumerable<OpenCvSharp.Point[]> OrderByDescending(
        this OpenCvSharp.Point[][] source,
        ContourAreaSelector keySelector) =>
        System.Linq.Enumerable.OrderByDescending(source, contour => keySelector(contour, false));
}
