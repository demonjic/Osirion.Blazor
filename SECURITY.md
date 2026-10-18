# Security policy

## Supported versions

Security fixes ship in a new release of the current version line. To receive them, stay on the newest 4.x release of
the Osirion.Blazor packages.

| Version | Security fixes |
| --- | --- |
| Newest 4.x release | Yes |
| Older 4.x releases | Update to the newest 4.x release |
| 3.x and earlier | No |

## Reporting a vulnerability

Do not report a vulnerability in a public issue, pull request or comment.

Report it privately through GitHub: open the repository's **Security** tab and choose **Report a vulnerability**, or go
straight to [the report form](https://github.com/demonjic/Osirion.Blazor/security/advisories/new). Only you and the
maintainers can see the report.

A useful report says:

- which package and version it affects, and the component, option or API involved;
- how to reproduce it, ideally with a small example or a proof of concept;
- what an attacker gains, as far as you can tell.

What happens next: the maintainers confirm the problem in the advisory thread, prepare the fix privately, release it in
a new version, and then publish the advisory. If you want credit, say so in the report and you will be named in the
advisory.

## Scope

In scope are the packages built from `src/` in this repository, including the `dotnet new` templates. The example sites
in `examples/` show how to use the packages; a problem that only exists in how an example is configured is still worth
reporting, but it is not a vulnerability in the packages.

A vulnerability in a dependency belongs with that project first. Tell us as well if the way Osirion.Blazor uses the
dependency makes it exploitable.
