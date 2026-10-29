using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;
using Osirion.Blazor.Core.Handlers;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Handlers;

/// <summary>
/// What the consent endpoint records for each choice, and how the recorded consent is read back.
/// </summary>
public class CookieConsentHandlerConsentDataTests
{
    private const string ConsentCookieName = "osirion_cookie_consent";

    [Fact]
    public async Task HandleConsentAsync_Accepted_ConsentsToEveryCategory()
    {
        var context = CreateContext(new() { ["consent"] = "accepted", ["returnUrl"] = "/" });

        await CookieConsentHandler.HandleConsentAsync(context);

        var consent = ReadWrittenConsent(context);
        consent.Version.ShouldBe("1.0");
        consent.ConsentType.ShouldBe("accepted");
        consent.Categories.ShouldBe(new Dictionary<string, bool>
        {
            ["necessary"] = true,
            ["analytics"] = true,
            ["marketing"] = true,
            ["preferences"] = true
        }, ignoreOrder: true);
    }

    [Fact]
    public async Task HandleConsentAsync_Declined_ConsentsToNecessaryCookiesOnly()
    {
        var context = CreateContext(new() { ["consent"] = "declined", ["returnUrl"] = "/p" });

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<RedirectHttpResult>().Url.ShouldBe("/p");
        var consent = ReadWrittenConsent(context);
        consent.ConsentType.ShouldBe("declined");
        consent.Categories.ShouldBe(new Dictionary<string, bool>
        {
            ["necessary"] = true,
            ["analytics"] = false,
            ["marketing"] = false,
            ["preferences"] = false
        }, ignoreOrder: true);
    }

    [Fact]
    public async Task HandleConsentAsync_SavePreferences_RecordsThePostedCategoriesAndNecessary()
    {
        var context = CreateContext(new()
        {
            ["consent"] = "save-preferences",
            ["returnUrl"] = "/",
            ["category_analytics"] = "true",
            ["category_marketing"] = "false"
        });

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<RedirectHttpResult>().Url.ShouldBe("/");
        var consent = ReadWrittenConsent(context);
        consent.ConsentType.ShouldBe("save-preferences");
        consent.Categories.ShouldBe(new Dictionary<string, bool>
        {
            ["necessary"] = true,
            ["analytics"] = true,
            ["marketing"] = false
        }, ignoreOrder: true);
    }

    [Fact]
    public async Task HandleConsentAsync_MissingConsentValue_ReturnsBadRequestWithoutCookie()
    {
        var context = CreateContext(new() { ["returnUrl"] = "/" });

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<BadRequest<string>>().Value.ShouldBe("Invalid consent value");
        context.Response.Headers.SetCookie.ToString().ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleConsentAsync_ConsentCookie_IsHttpOnlySecureLaxAndSiteWide()
    {
        var context = CreateContext(new() { ["consent"] = "accepted", ["returnUrl"] = "/" });

        await CookieConsentHandler.HandleConsentAsync(context);

        var setCookie = context.Response.Headers.SetCookie.ToString().ToLowerInvariant();
        setCookie.ShouldContain("httponly");
        setCookie.ShouldContain("secure");
        setCookie.ShouldContain("samesite=lax");
        setCookie.ShouldContain("path=/");
    }

    [Fact]
    public void GetConsentData_WithoutCookie_ReturnsNull()
    {
        CookieConsentHandler.GetConsentData(new DefaultHttpContext()).ShouldBeNull();
    }

    [Fact]
    public void GetConsentData_MalformedCookie_ReturnsNull()
    {
        var context = WithConsentCookie("not json");

        CookieConsentHandler.GetConsentData(context).ShouldBeNull();
    }

    [Fact]
    public void GetConsentData_ValidCookie_ReturnsTheRecordedConsent()
    {
        var context = WithConsentCookie("{\"Version\":\"1.0\",\"ConsentType\":\"accepted\",\"Categories\":{\"analytics\":true}}");

        var consent = CookieConsentHandler.GetConsentData(context).ShouldNotBeNull();

        consent.Version.ShouldBe("1.0");
        consent.ConsentType.ShouldBe("accepted");
        consent.Categories["analytics"].ShouldBeTrue();
    }

    // What osirion-consent.js writes for Save Preferences with Analytics ticked: escaped as the endpoint escapes, with
    // the time in the shape Date.toISOString gives (milliseconds only).
    [Fact]
    public void GetConsentData_CookieWrittenByTheConsentScript_IsRead()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = ConsentCookieName + "=%7B%22Version%22%3A%221.0%22%2C%22ConsentDate%22%3A"
            + "%222026-10-23T17%3A37%3A25.558Z%22%2C%22ConsentType%22%3A%22save-preferences%22%2C%22Categories%22%3A"
            + "%7B%22necessary%22%3Atrue%2C%22analytics%22%3Atrue%7D%7D";

        var consent = CookieConsentHandler.GetConsentData(context).ShouldNotBeNull();

        consent.ConsentType.ShouldBe("save-preferences");
        consent.ConsentDate.ShouldBe(new DateTime(2026, 10, 23, 17, 37, 25, 558, DateTimeKind.Utc));
        CookieConsentHandler.IsCategoryConsented(context, "analytics").ShouldBeTrue();
        CookieConsentHandler.IsCategoryConsented(context, "marketing").ShouldBeFalse();
    }

    [Theory]
    [InlineData("analytics", true)]
    [InlineData("marketing", false)]
    [InlineData("unknown", false)]
    public void IsCategoryConsented_RecordedConsent_ReturnsTheCategoryChoice(string category, bool expected)
    {
        var context = WithConsentCookie("{\"Categories\":{\"analytics\":true,\"marketing\":false}}");

        CookieConsentHandler.IsCategoryConsented(context, category).ShouldBe(expected);
    }

    [Fact]
    public void IsCategoryConsented_WithoutCookie_ReturnsFalse()
    {
        CookieConsentHandler.IsCategoryConsented(new DefaultHttpContext(), "necessary").ShouldBeFalse();
    }

    // Fails before the fix: the cookie is visitor input, and a null category map threw NullReferenceException.
    [Fact]
    public void IsCategoryConsented_CookieWithNullCategories_ReturnsFalse()
    {
        var context = WithConsentCookie("{\"Categories\":null}");

        CookieConsentHandler.IsCategoryConsented(context, "analytics").ShouldBeFalse();
    }

    [Fact]
    public void ClearConsent_ExpiresTheConsentCookie()
    {
        var context = new DefaultHttpContext();

        CookieConsentHandler.ClearConsent(context);

        var setCookie = context.Response.Headers.SetCookie.ToString();
        setCookie.ShouldStartWith($"{ConsentCookieName}=;");
        setCookie.ShouldContain("expires=Thu, 01 Jan 1970", Case.Insensitive);
    }

    private static DefaultHttpContext WithConsentCookie(string json)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{ConsentCookieName}={Uri.EscapeDataString(json)}";
        return context;
    }

    private static CookieConsentData ReadWrittenConsent(HttpContext context)
    {
        var setCookie = context.Response.Headers.SetCookie.ToString();
        var prefix = ConsentCookieName + "=";
        setCookie.ShouldStartWith(prefix);
        var value = setCookie[prefix.Length..setCookie.IndexOf(';', StringComparison.Ordinal)];
        return JsonSerializer.Deserialize<CookieConsentData>(Uri.UnescapeDataString(value)).ShouldNotBeNull();
    }

    private static DefaultHttpContext CreateContext(Dictionary<string, StringValues> form)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(form);
        context.Features.Set<IAntiforgeryValidationFeature>(new ValidAntiforgeryFeature());
        return context;
    }

    private sealed class ValidAntiforgeryFeature : IAntiforgeryValidationFeature
    {
        public bool IsValid => true;

        public Exception? Error => null;
    }
}
