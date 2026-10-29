# OsirionBaseSection Component

The `OsirionBaseSection` component is a flexible, framework-agnostic base section component for creating structured content sections with optional headers, descriptions, and background styling.

## Features

- **Framework Agnostic**: Works with Bootstrap, FluentUI, MudBlazor, Radzen, and custom CSS frameworks
- **Flexible Content**: Supports both text-based and custom render fragment content for titles and descriptions
- **Background Support**: Optional background images, colors, and patterns
- **Responsive Design**: Mobile-first responsive design with proper breakpoints
- **Accessibility**: Proper semantic HTML and accessibility considerations
- **SSR Compatible**: Works with both server-side rendering and WebAssembly

## Parameters

### Content Parameters
- `Id` (required): Unique identifier for the section element
- `ChildContent`: Main content to be rendered inside the section
- `Title`: Section title text (optional)
- `TitleContent`: Custom title content (overrides Title when provided)
- `Description`: Section description text (optional)
- `DescriptionContent`: Custom description content (overrides Description when provided)
- `Eyebrow`: a short plain-text label above the title (`osirion-section-eyebrow`), shown only with a title (from 4.3)

### Layout Parameters
- `ContainerClass`: Custom container CSS class (defaults to framework-specific container)
- `TextAlignment`: Text alignment (Left, Center, Right, Justify) - defaults to Center
- `Padding`: Section padding size (None, Small, Medium, Large) - defaults to Medium

### Background Parameters
- `BackgroundImageUrl`: URL for background image (optional)
- `BackgroundColor`: Background color (optional)
- `TextColor`: Text color override (optional)
- `ShowOverlay`: Whether to show overlay on background images for text readability (default: true)
- `BackgroundPattern`: Background pattern type (optional)
- `ShowPattern`: Whether to show the background pattern (default: false)

### Visual Parameters
- `HasDivider`: Whether the section has a divider/shadow effect (default: false)
- `Variant`: the band, `SectionVariant.Default`, `Tinted` (`--osirion-background-secondary`) or `Dark`, which writes
  `data-theme="dark"` (`data-bs-theme` under Bootstrap) on the section so everything inside takes the dark theme's
  tokens; an explicit `Theme` and `BackgroundColor` still win (from 4.3). With neither `Eyebrow` nor `Variant` the
  markup is unchanged.

## Usage Examples

### Basic Section
```razor
<OsirionBaseSection Id="about-section" Title="About Us" Description="Learn more about our company">
    <p>Your content goes here...</p>
</OsirionBaseSection>
```

### Section with Custom Title Content
```razor
<OsirionBaseSection Id="features-section">
    <TitleContent>
        <h2><i class="fas fa-star"></i> Premium Features</h2>
    </TitleContent>
    <DescriptionContent>
        <p>Discover what makes us <strong>special</strong></p>
    </DescriptionContent>
    
    <div class="feature-grid">
        <!-- Your features here -->
    </div>
</OsirionBaseSection>
```

### Section with Background
```razor
<OsirionBaseSection 
    Id="hero-section"
    Title="Welcome to Our Platform"
    Description="Building the future, one line of code at a time"
    BackgroundImageUrl="/images/hero-bg.jpg"
    TextColor="white"
    Padding="SectionPadding.Large"
    HasDivider="true">
    
    <button class="btn btn-primary">Get Started</button>
</OsirionBaseSection>
```

### Section with Pattern
```razor
<OsirionBaseSection 
    Id="tech-section"
    Title="Our Technology"
    BackgroundPattern="BackgroundPatternType.Circuit"
    ShowPattern="true"
    TextAlignment="Alignment.Left"
    Padding="SectionPadding.Medium">
    
    <div class="tech-content">
        <!-- Technology content -->
    </div>
</OsirionBaseSection>
```

### Framework-Specific Container
```razor
<!-- Bootstrap -->
<OsirionBaseSection Id="content" ContainerClass="container-fluid">
    <!-- content -->
</OsirionBaseSection>

<!-- Custom container -->
<OsirionBaseSection Id="content" ContainerClass="my-custom-container">
    <!-- content -->
</OsirionBaseSection>
```

## CSS Classes

The component automatically generates appropriate CSS classes based on your settings:

- `.osirion-section`: Base section class
- `.osirion-section-align-{alignment}`: Text alignment classes
- `.osirion-section-padding-{size}`: Padding size classes
- `.osirion-section-with-background`: Applied when background image is used
- `.osirion-section-with-pattern`: Applied when background pattern is shown
- `.osirion-section-divider`: Applied when HasDivider is true

## Responsive Behavior

- Desktop: Full layout with specified alignment and padding
- Mobile: Automatically centers content for better presentation
- Title underlines: Automatically adjust position based on alignment
- Padding: Reduces on smaller screens for optimal mobile experience

## Framework Integration

The component automatically detects your CSS framework and applies appropriate container classes:

- Bootstrap: `container-xxl`
- FluentUI: `osirion-container`
- MudBlazor: `mud-container`
- Radzen: `rz-container`
- Default: `osirion-container`

You can override this by providing a custom `ContainerClass`.

## Accessibility

- Proper semantic HTML structure with `<section>` element
- Required `id` attribute for navigation and screen readers
- Proper heading hierarchy (uses `<h2>` for titles)
- Color contrast considerations for background overlays
- Reduced motion support for animations

## Browser Support

Compatible with all modern browsers. Uses CSS custom properties (CSS variables) for theming, with fallbacks for older browsers.
