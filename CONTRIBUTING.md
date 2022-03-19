# Contributing

BibleBot is an open-source project and open to contributions. Please be sure to follow the guidelines below before submitting a pull request.

*This document is derived from [Discord.Net](https://github.com/discord-net/Discord.Net)'s CONTRIBUTING.md.*

## Getting Started

We recommend grabbing one of the "To do" items in our kanban board, instead of coming up with something entirely new. If you're fixing a bug, make an issue first *then* you can make a PR.

If you're looking to introduce a feature, create a Feature Request issue or discuss it with us on the official support server. We won't implement any feature requests without discussion on the matter.

## Pull Request Guidelines

Your commits should not be monolithic, containing multiple major changes in one commit. In your PR description, you do not need to provide change details, **your commit message log should be self-explanatory**.

## Versioning

This project generally follows [Semantic Versioning](https://semver.org/), which we will be more strictly following after v9.2's release. Make sure your commits are SemVer-friendly. **Do not change any version numbers in your PR**, this will be handled by the project maintainers.

We follow the .NET Foundation's [Breaking Change Rules](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/breaking-change-rules.md) for SemVer compliance.

## Code Style

We generally conform to the .NET Foundation's [Coding Style](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md).

### Documentation

Please ensure new public members have sufficient documentation, including but not limited to:

* `<summary>` summarizing the purpose of the method.
* `<param>` or `<typeparam>` explaining the parameter.
* `<return>` explaining the type of the returned member and what it is.
* `<exception>` if the method directly throws an exception.

#### Recommended Reads

* [Official Microsoft Documentation](https://docs.microsoft.com)
* [Sandcastle User Manual](https://ewsoftware.github.io/XMLCommentsGuide/html/4268757F-CE8D-4E6D-8502-4F7F2E22DDA3.htm)
