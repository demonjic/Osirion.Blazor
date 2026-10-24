# HeroSection

Versatile hero section with background or side images, CTA buttons, metadata, and multiple layout variants. SSR-ready and framework-agnostic.

Basic usage

```razor
@using Osirion.Blazor.Components

<HeroSection 
    Title="Welcome"
    Subtitle="Build faster with Osirion"
    Summary="Modern, SSR-first Blazor components"
    PrimaryButtonText="Get Started"
    PrimaryButtonUrl="/docs"
    SecondaryButtonText="GitHub"
    SecondaryButtonUrl="https://github.com/demonjic/Osirion.Blazor" />
```

Background image

```razor
<HeroSection 
    Title="Transform Your Workflow"
    ImageUrl="/images/hero.jpg"
    UseBackgroundImage="true"
    TextColor="#fff"
    MinHeight="80vh"
    Alignment="Alignment.Center" />
```

Notes

- Variants: Hero, Jumbotron, Minimal.
- Alignment controls text alignment. ImagePosition applies when not using background images.
- As a background image, ImageUrl is written into `url('...')` with quotes, backslashes, parentheses and control characters CSS-escaped, so a URL taken from content front matter cannot add style declarations. Other URLs are written unchanged.
- Optional metadata: Author, PublishDate, ReadTime, ShowMetadata.
- The buttons take the framework's large primary and secondary classes (`ButtonClasses.Primary` and
  `ButtonClasses.Secondary`, the same as `OsirionCallToActionSection`). Both carry `osirion-hero-button`, plus
  `osirion-hero-button-primary` or `osirion-hero-button-secondary`. Up to 4.2.0 the secondary button had the outlined
  primary classes and the `-primary` hook; select `osirion-hero-button` for a rule meant for both buttons.
