using QuanLyQuanCafe.Helpers;

namespace QuanLyQuanCafe.Tests;

public class StarRatingHelperTests
{
    [Fact]
    public void RenderStars_Five_ShouldRenderFiveFilled()
    {
        var html = StarRatingHelper.RenderStars(5);
        Assert.Equal(5, Count(html, "bi-star-fill"));
        Assert.DoesNotContain("bi-star-half", html);
    }

    [Fact]
    public void RenderStars_Zero_ShouldRenderOnlyEmptyStars()
    {
        var html = StarRatingHelper.RenderStars(0);
        Assert.Contains("bi-star star-off", html);
        Assert.DoesNotContain("bi-star-fill", html);
    }

    [Fact]
    public void RenderStars_AboveFive_ShouldClamp()
    {
        var html = StarRatingHelper.RenderStars(8);
        Assert.Equal(5, Count(html, "bi-star-fill"));
        Assert.Contains("5.0 trên 5 sao", html);
    }

    [Fact]
    public void RenderStars_Negative_ShouldClampToZero()
    {
        var html = StarRatingHelper.RenderStars(-2);
        Assert.DoesNotContain("bi-star-fill", html);
        Assert.Contains("0.0 trên 5 sao", html);
    }

    [Fact]
    public void RenderStars_ThreePointFive_ShouldShowHalfStar()
    {
        var html = StarRatingHelper.RenderStars(3.5);
        Assert.Equal(3, Count(html, "bi-star-fill"));
        Assert.Contains("bi-star-half", html);
    }

    [Fact]
    public void RenderStars_ThreePointTwo_ShouldNotShowHalfStar()
    {
        var html = StarRatingHelper.RenderStars(3.2);
        Assert.Equal(3, Count(html, "bi-star-fill"));
        Assert.DoesNotContain("bi-star-half", html);
    }

    [Fact]
    public void RenderStars_One_ShouldRenderOneFilled()
    {
        var html = StarRatingHelper.RenderStars(1);
        Assert.Equal(1, Count(html, "bi-star-fill"));
        Assert.Equal(4, Count(html, "star-off"));
    }

    [Fact]
    public void RenderStars_CustomSizeClass_ShouldApply()
    {
        var html = StarRatingHelper.RenderStars(4, "star-lg");
        Assert.Contains("star-rating star-lg", html);
    }

    [Fact]
    public void RenderStars_ShouldIncludeAriaLabel()
    {
        var html = StarRatingHelper.RenderStars(4.5);
        Assert.Contains("aria-label=\"4.5 trên 5 sao\"", html);
    }

    [Fact]
    public void RenderStars_ShouldWrapInSpanContainer()
    {
        var html = StarRatingHelper.RenderStars(2);
        Assert.StartsWith("<span class=\"star-rating ", html);
        Assert.EndsWith("</span>", html);
    }

    private static int Count(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;
}