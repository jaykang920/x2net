# Contributing

x2net is still in its early stage and we need your overall contribution on code
review, bugfix, unit tests, and documentation.

## Keep These in Mind

x2net aims to be a minimal building block. When you think of modifications or
new features, please keep in mind that:

- the library code base should be kept as compact as possible
- changes should not break the conceptual and logical integrity
- x2net has many points to extend (flows, links, queues, etc). Extensions that
can be included into the library code base should not depend on other libraries
than the .NET framework itself. Extensions depending on third-party libraries
have to be in separate repositories.

## Issues

Got a question? Experiencing a problem? Found a bug? Or inspired a new feature 
idea? Please feel free to share them through the
[GitHub Issues](https://github.com/jaykang920/x2net/issues).

When reporting an issue, please be very specific. The more information you
include about the issue, the more likely it is to be resolved.

## Pull Requests

When submitting a pull request to the [GitHub Pull Requests](https://github.com/jaykang920/x2net/pulls),
please make sure that your new or updated code:

- Follows x2 and x2net style guides
- Does not break any existing unit test
- Is accompanied with corresponding unit tests if appropriate

If you are not familiar with pull requests, please read the
[GitHub Help](https://help.github.com/articles/about-pull-requests/) for more
details.
