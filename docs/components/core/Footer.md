# OsirionFooter

Flexible footer with sections, social links, bottom links, and docking options.

Basic usage

```razor
@using Osirion.Blazor.Components

<OsirionFooter CompanyName="Acme Inc." CompanyUrl="/">
    <Logo>
        <img src="/logo.svg" alt="Acme" height="24" />
    </Logo>
</OsirionFooter>
```

Sections

```razor
<OsirionFooter 
    Links="@sections" 
    SocialLinks="@social" 
    BottomLinks="@bottom" />

@code {
    private readonly IReadOnlyList<FooterSection> sections = new []
    {
        new FooterSection
        {
            Title = "Product",
            Links = new [] { new FooterLink { Text = "Features", Href = "/features" } }
        }
    };

    private readonly IReadOnlyList<FooterSocialLink> social = new []
    {
        new FooterSocialLink { Text = "LinkedIn", Href = "https://linkedin.com", AriaLabel = "LinkedIn" }
    };

    private readonly IReadOnlyList<FooterLink> bottom = new []
    {
        new FooterLink { Text = "Privacy", Href = "/privacy" }
    };
}
```

Notes

- Variant: default|minimal|centered. GridLayout: auto|4-column|3-column|2-column.
- Docked with DockingMode: fixed|sticky.
- Writes no inline `style` element or attribute of its own, so a strict Content Security Policy needs nothing for it.
