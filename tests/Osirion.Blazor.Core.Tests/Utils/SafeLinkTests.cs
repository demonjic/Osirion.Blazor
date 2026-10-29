using Osirion.Blazor.Core.Utils;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Utils;

// Guards the link rule the components already apply, now that it is public for links rendered outside them.
public class SafeLinkTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/contact")]
    [InlineData("https://cal.example/book")]
    [InlineData("http://cal.example/book")]
    [InlineData("mailto:hello@example.com")]
    [InlineData("tel:+381111222333")]
    [InlineData("TEL:+381111222333")]
    [InlineData("#pricing")]
    [InlineData("#section-2")]
    public void IsSafe_SitePathFragmentOrHttpMailtoOrTelAddress_ReturnsTrue(string url)
    {
        SafeLink.IsSafe(url).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("contact")]
    [InlineData("//cal.example/book")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,hello")]
    [InlineData("JavaScript:alert(1)")]
    [InlineData("vbscript:msgbox(1)")]
    [InlineData("tel:")]
    [InlineData("#")]
    [InlineData("# top")]
    [InlineData("#a	b")]
    [InlineData("///cal.example")]
    public void IsSafe_OtherValue_ReturnsFalse(string? url)
    {
        SafeLink.IsSafe(url).ShouldBeFalse();
    }
}
