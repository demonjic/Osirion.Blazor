# OsirionTestimonialCard

Professional testimonial card with profile, rating, quote, and optional LinkedIn and read-more.

Basic usage

```razor
@using Osirion.Blazor.Components

<OsirionTestimonialCard 
    Name="Sarah Johnson"
    Position="Senior Developer"
    Company="Tech Solutions"
    TestimonialText="This platform transformed our workflow."
    ProfileImageUrl="/img/sarah.jpg"
    ShowRating="true"
    Rating="5" />
```

Notes

- Variants: Default, Minimal, Highlighted, Compact.
- Sizes: Small, Normal, Large. ImageSize auto-tunes by size.
- Read more via ReadMoreHref/Text/Variant.
- The read more link takes `--osirion-testimonial-action-color` and `--osirion-testimonial-action-hover-color` through the class `osirion-testimonial-read-more`, not a `style` attribute, so a strict Content Security Policy needs nothing for it.
