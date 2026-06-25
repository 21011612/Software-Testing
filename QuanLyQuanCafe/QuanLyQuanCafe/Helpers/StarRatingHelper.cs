using System.Text;

namespace QuanLyQuanCafe.Helpers;

public static class StarRatingHelper
{
    public static string RenderStars(double diem, string sizeClass = "")
    {
        var clamped = Math.Clamp(diem, 0, 5);
        var full = (int)Math.Floor(clamped);
        var half = clamped - full >= 0.35 && full < 5;
        var sb = new StringBuilder();
        sb.Append("<span class=\"star-rating ").Append(sizeClass).Append("\" aria-label=\"")
            .Append(clamped.ToString("0.0")).Append(" trên 5 sao\">");
        for (var i = 1; i <= 5; i++)
        {
            if (i <= full)
                sb.Append("<i class=\"bi bi-star-fill star-on\"></i>");
            else if (i == full + 1 && half)
                sb.Append("<i class=\"bi bi-star-half star-on\"></i>");
            else
                sb.Append("<i class=\"bi bi-star star-off\"></i>");
        }
        sb.Append("</span>");
        return sb.ToString();
    }
}