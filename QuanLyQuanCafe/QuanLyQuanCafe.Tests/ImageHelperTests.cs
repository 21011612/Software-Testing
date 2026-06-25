using QuanLyQuanCafe.Helpers;

namespace QuanLyQuanCafe.Tests;

public class ImageHelperTests
{
    [Fact]
    public void ToUrl_Null_ShouldReturnDefault() =>
        Assert.Equal("/images/Ca_phe_sua_da.jpg", ImageHelper.ToUrl(null));

    [Fact]
    public void ToUrl_Empty_ShouldReturnDefault() =>
        Assert.Equal("/images/Ca_phe_sua_da.jpg", ImageHelper.ToUrl(""));

    [Fact]
    public void ToUrl_Whitespace_ShouldReturnDefault() =>
        Assert.Equal("/images/Ca_phe_sua_da.jpg", ImageHelper.ToUrl("   "));

    [Fact]
    public void ToUrl_RelativePath_ShouldPrefixSlash() =>
        Assert.Equal("/images/Americano_Tiki.jpg", ImageHelper.ToUrl("images/Americano_Tiki.jpg"));

    [Fact]
    public void ToUrl_AbsolutePath_ShouldKeepLeadingSlash() =>
        Assert.Equal("/images/Ca_phe_muoi.jpg", ImageHelper.ToUrl("/images/Ca_phe_muoi.jpg"));

    [Fact]
    public void ToUrl_Backslash_ShouldNormalizeToForwardSlash() =>
        Assert.Equal("/images/Ca_phe_sua_da.jpg", ImageHelper.ToUrl("images\\Ca_phe_sua_da.jpg"));

    [Fact]
    public void ToUrl_TrimmedPath_ShouldNormalize() =>
        Assert.Equal("/images/Tra_cam_da_cocktail.jpg", ImageHelper.ToUrl("  images/Tra_cam_da_cocktail.jpg  "));

    [Fact]
    public void ToUrl_SubfolderImage_ShouldWork() =>
        Assert.Equal("/uploads/sanpham/sp1.jpg", ImageHelper.ToUrl("uploads/sanpham/sp1.jpg"));

    [Fact]
    public void ToUrl_DoubleSlashInput_ShouldPreservePath() =>
        Assert.Equal("//images/test.jpg", ImageHelper.ToUrl("//images/test.jpg"));

    [Fact]
    public void ToUrl_WwwrootStylePath_ShouldConvert() =>
        Assert.Equal("/wwwroot/images/7Up_Lon.jpg", ImageHelper.ToUrl("wwwroot/images/7Up_Lon.jpg"));
}