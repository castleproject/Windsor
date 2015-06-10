# Contributing

Information on contributing to this project can be found in the [Castle Project Contributing Guide](https://github.com/castleproject/Home/blob/master/README.md) located in our Home repository.

## Criteria for Patches

Before committing any change make sure all the following items are met:

1. The change makes sense and brings value
1. The code complies with [Castle coding standards](https://github.com/castleproject/Home/blob/master/coding-standards.md)
1. The code compiles in all versions (that currently means .NET 3.5 SP1, .NET 4 (full and client profile), Silverlight 3 and 4, possibly Mono in the (nearish) future)
1. The change has tests, with reasonably good code coverage
1. All tests pass in all versions we build
1. If needed (changes something more significant than fixing a typo or formatting update) `Changes.txt` is updated
1. If the change is breaking, `breakingchanges.txt` is updated
1. The change works with child containers, `NoTrackingReleasePolicy` or other non-standard configuration that may affect it
1. Documentation is accordingly updated
1. Related issues in issue tracker are accordingly updated