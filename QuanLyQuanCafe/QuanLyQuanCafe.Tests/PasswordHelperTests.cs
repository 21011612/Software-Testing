using QuanLyQuanCafe.Helpers;

namespace QuanLyQuanCafe.Tests;

public class PasswordHelperTests
{
    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        var hash = PasswordHelper.Hash("MatKhau123!");
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void Hash_ShouldDifferFromPlainText()
    {
        var password = "MatKhau123!";
        Assert.NotEqual(password, PasswordHelper.Hash(password));
    }

    [Fact]
    public void Hash_ShouldTrimInputBeforeHashing()
    {
        var hash1 = PasswordHelper.Hash("  Abc@123  ");
        var hash2 = PasswordHelper.Hash("Abc@123");
        Assert.True(PasswordHelper.Verify("Abc@123", hash1));
        Assert.True(PasswordHelper.Verify("Abc@123", hash2));
    }

    [Fact]
    public void Verify_BcryptHash_ShouldReturnTrueWhenMatch()
    {
        var password = "Abc@123456";
        Assert.True(PasswordHelper.Verify(password, PasswordHelper.Hash(password)));
    }

    [Fact]
    public void Verify_BcryptHash_ShouldReturnFalseWhenMismatch()
    {
        var hash = PasswordHelper.Hash("CorrectPassword123");
        Assert.False(PasswordHelper.Verify("WrongPassword", hash));
    }

    [Fact]
    public void Verify_PlainTextStored_ShouldCompareDirectly()
    {
        Assert.True(PasswordHelper.Verify("khach@123", "khach@123"));
        Assert.False(PasswordHelper.Verify("wrong", "khach@123"));
    }

    [Fact]
    public void Verify_EmptyPassword_ShouldReturnFalse()
    {
        var hash = PasswordHelper.Hash("SomePassword");
        Assert.False(PasswordHelper.Verify("", hash));
    }

    [Fact]
    public void Verify_WhitespacePassword_ShouldReturnFalse()
    {
        var hash = PasswordHelper.Hash("SomePassword");
        Assert.False(PasswordHelper.Verify("   ", hash));
    }

    [Fact]
    public void Verify_NullStoredHash_ShouldReturnFalse() =>
        Assert.False(PasswordHelper.Verify("password", null!));

    [Fact]
    public void Verify_InvalidBcryptHash_ShouldReturnFalse() =>
        Assert.False(PasswordHelper.Verify("password", "not-a-valid-bcrypt-hash"));
}