using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Osirion.Blazor.Core.Handlers;
using Shouldly;
using System.Text;

namespace Osirion.Blazor.Core.Tests.Handlers;

public class CookieConsentHandlerTests
{
    private const string ConsentCookieName = "osirion_cookie_consent";

    [Theory]
    [InlineData("https://evil.example/x")]
    [InlineData("http://localhost/products")]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("\\\\evil.example")]
    [InlineData("evil.example")]
    [InlineData("javascript:alert(1)")]
    [InlineData("/\t/evil.example")]
    [InlineData("/x\r\nSet-Cookie: a=b")]
    [InlineData("~//evil.example")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleConsentAsync_NonLocalReturnUrl_RedirectsToRoot(string returnUrl)
    {
        var context = CreateContext(Form("accepted", returnUrl));

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<RedirectHttpResult>().Url.ShouldBe("/");
    }

    [Fact]
    public async Task HandleConsentAsync_MissingReturnUrl_RedirectsToRoot()
    {
        var context = CreateContext(new Dictionary<string, StringValues> { ["consent"] = "accepted" });

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<RedirectHttpResult>().Url.ShouldBe("/");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/products/access")]
    [InlineData("/products?a=1&b=2")]
    [InlineData("/sr/blog/post")]
    [InlineData("/a%20b")]
    public async Task HandleConsentAsync_LocalReturnUrl_RedirectsToIt(string returnUrl)
    {
        var context = CreateContext(Form("accepted", returnUrl));

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<RedirectHttpResult>().Url.ShouldBe(returnUrl);
    }

    [Fact]
    public async Task HandleConsentAsync_UnescapedNonAsciiReturnUrl_RedirectsToRoot()
    {
        var context = CreateContext(Form("accepted", "/blog/članak"));

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<RedirectHttpResult>().Url.ShouldBe("/");
    }

    [Theory]
    [InlineData("/p", "/p?customize-cookies=true")]
    [InlineData("/p?a=1", "/p?a=1&customize-cookies=true")]
    [InlineData("/p?customize-cookies=true", "/p?customize-cookies=true")]
    [InlineData("/p#top", "/p?customize-cookies=true#top")]
    [InlineData("https://evil.example", "/?customize-cookies=true")]
    public async Task HandleConsentAsync_Customize_RedirectsWithASingleCustomizeFlag(string returnUrl, string expectedUrl)
    {
        var context = CreateContext(Form("customize", returnUrl));

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<RedirectHttpResult>().Url.ShouldBe(expectedUrl);
    }

    [Fact]
    public async Task HandleConsentAsync_Customize_WritesNoConsentCookie()
    {
        var context = CreateContext(Form("customize", "/p"));

        await CookieConsentHandler.HandleConsentAsync(context);

        context.Response.Headers.SetCookie.ToString().ShouldNotContain(ConsentCookieName);
    }

    [Fact]
    public async Task HandleConsentAsync_AcceptedWithStaleCustomizeFlag_RedirectsWithoutTheFlag()
    {
        var context = CreateContext(Form("accepted", "/p?a=1&customize-cookies=true"));

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<RedirectHttpResult>().Url.ShouldBe("/p?a=1");
    }

    [Fact]
    public async Task HandleConsentAsync_Accepted_WritesTheConsentCookie()
    {
        var context = CreateContext(Form("accepted", "/p"));

        await CookieConsentHandler.HandleConsentAsync(context);

        context.Response.Headers.SetCookie.ToString().ShouldContain(ConsentCookieName);
    }

    // The value osirion-consent.js wrote in Microsoft Edge for Accept All. Only the time differs from what the endpoint
    // writes, so consent recorded in the browser reads the same as consent recorded on the server.
    [Fact]
    public async Task HandleConsentAsync_Accepted_WritesTheValueTheConsentScriptWrites()
    {
        const string written = "%7B%22Version%22%3A%221.0%22%2C%22ConsentDate%22%3A%222026-10-23T17%3A37%3A25.558Z%22%2C"
            + "%22ConsentType%22%3A%22accepted%22%2C%22Categories%22%3A%7B%22necessary%22%3Atrue%2C%22analytics%22%3Atrue%2C"
            + "%22marketing%22%3Atrue%2C%22preferences%22%3Atrue%7D%7D";
        var context = CreateContext(Form("accepted", "/p"));

        await CookieConsentHandler.HandleConsentAsync(context);

        var setCookie = context.Response.Headers.SetCookie.ToString();
        setCookie.ShouldStartWith(ConsentCookieName + "=");
        var value = setCookie[(ConsentCookieName.Length + 1)..setCookie.IndexOf(';', StringComparison.Ordinal)];
        var sameTime = System.Text.RegularExpressions.Regex.Replace(
            value, "(%22ConsentDate%22%3A%22).+?(%22%2C)", "${1}2026-10-23T17%3A37%3A25.558Z${2}");
        sameTime.ShouldBe(written);
    }

    [Fact]
    public async Task HandleConsentAsync_UnknownConsentValue_ReturnsBadRequest()
    {
        var context = CreateContext(Form("maybe", "/p"));

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<BadRequest<string>>();
    }

    [Fact]
    public async Task HandleConsentAsync_AntiforgeryValidationFailed_ReturnsBadRequestWithoutBodyOrCookie()
    {
        var antiforgery = Substitute.For<IAntiforgery>();
        var context = CreateContext(Form("accepted", "/p"), antiforgeryVerdict: false, antiforgery);

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<BadRequest>();
        context.Response.Headers.SetCookie.ToString().ShouldBeEmpty();
        await antiforgery.DidNotReceiveWithAnyArgs().IsRequestValidAsync(default!);
    }

    [Fact]
    public async Task HandleConsentAsync_AntiforgeryValidationFailed_ReturnsBadRequestWithoutReadingTheForm()
    {
        // After UseAntiforgery records a failed validation, reading the form throws, which would
        // turn the rejection into a 500.
        const string InvalidTokenMessage = "This form is being accessed with an invalid anti-forgery token.";
        var context = CreateContext(form: null, antiforgeryVerdict: false);
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("consent=accepted&returnUrl=%2Fp"));
        var formFeature = Substitute.For<IFormFeature>();
        formFeature.HasFormContentType.Returns(true);
        formFeature.Form.Returns(_ => throw new InvalidOperationException(InvalidTokenMessage));
        formFeature.ReadFormAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException(InvalidTokenMessage));
        context.Features.Set(formFeature);

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<BadRequest>();
        await formFeature.DidNotReceiveWithAnyArgs().ReadFormAsync(default);
    }

    [Theory]
    [InlineData(true, typeof(RedirectHttpResult))]
    [InlineData(false, typeof(BadRequest))]
    public async Task HandleConsentAsync_WithoutAntiforgeryMiddleware_ValidatesWithTheAntiforgeryService(bool tokenValid, Type expectedResult)
    {
        var antiforgery = Substitute.For<IAntiforgery>();
        var context = CreateContext(Form("accepted", "/p"), antiforgeryVerdict: null, antiforgery);
        antiforgery.IsRequestValidAsync(context).Returns(tokenValid);

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType(expectedResult);
    }

    [Fact]
    public async Task HandleConsentAsync_WithoutAntiforgeryMiddlewareOrService_ReturnsBadRequest()
    {
        var context = CreateContext(Form("accepted", "/p"), antiforgeryVerdict: null);

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<BadRequest>();
    }

    [Fact]
    public async Task HandleConsentAsync_JsonBody_ReturnsBadRequest()
    {
        var context = CreateContext(form: null);
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"consent\":\"accepted\"}"));

        var result = await CookieConsentHandler.HandleConsentAsync(context);

        result.ShouldBeOfType<BadRequest>();
    }

    // Fails before the fix: the cookie always lived for 365 days, whatever ConsentExpiryDays the banner was given.
    [Theory]
    [InlineData("30", 30)]
    [InlineData("730", 730)]
    [InlineData("", 365)]
    [InlineData("0", 365)]
    [InlineData("-5", 365)]
    [InlineData("99999", 365)]
    [InlineData("abc", 365)]
    public async Task HandleConsentAsync_PostedExpiryDays_SetsTheCookieLifetime(string postedDays, int expectedDays)
    {
        var form = Form("accepted", "/");
        form["expiryDays"] = postedDays;
        var context = CreateContext(form);

        await CookieConsentHandler.HandleConsentAsync(context);

        var setCookie = context.Response.Headers.SetCookie.ToString();
        var expires = DateTimeOffset.Parse(
            setCookie.Split(';').Select(part => part.Trim()).Single(part => part.StartsWith("expires=", StringComparison.OrdinalIgnoreCase))["expires=".Length..],
            System.Globalization.CultureInfo.InvariantCulture);
        (expires - DateTimeOffset.UtcNow).TotalDays.ShouldBe(expectedDays, tolerance: 0.01);
    }

    private static Dictionary<string, StringValues> Form(string consent, string returnUrl) => new()
    {
        ["consent"] = consent,
        ["returnUrl"] = returnUrl
    };

    private static DefaultHttpContext CreateContext(
        Dictionary<string, StringValues>? form,
        bool? antiforgeryVerdict = true,
        IAntiforgery? antiforgery = null)
    {
        var services = new ServiceCollection();
        if (antiforgery is not null)
        {
            services.AddSingleton(antiforgery);
        }

        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/cookie-consent";

        if (form is not null)
        {
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.Form = new FormCollection(form);
        }

        if (antiforgeryVerdict is bool isValid)
        {
            context.Features.Set<IAntiforgeryValidationFeature>(new AntiforgeryValidationFeature(isValid));
        }

        return context;
    }

    private sealed class AntiforgeryValidationFeature(bool isValid) : IAntiforgeryValidationFeature
    {
        public bool IsValid => isValid;

        public Exception? Error => isValid ? null : new AntiforgeryValidationException("Invalid token.");
    }
}
