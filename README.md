# aweXpect.Migration

[![Nuget](https://img.shields.io/nuget/v/aweXpect.Migration)](https://www.nuget.org/packages/aweXpect.Migration)
[![Build](https://github.com/aweXpect/aweXpect.Migration/actions/workflows/build.yml/badge.svg)](https://github.com/aweXpect/aweXpect.Migration/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=aweXpect_aweXpect.Migration&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=aweXpect_aweXpect.Migration)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=aweXpect_aweXpect.Migration&metric=coverage)](https://sonarcloud.io/summary/overall?id=aweXpect_aweXpect.Migration)

Migration helpers from other assertion libraries to [aweXpect](https://github.com/aweXpect/aweXpect).

## Xunit

1. Add the `aweXpect.Migration` package reference in addition to the `aweXpect` in the test project.
2. All usages of `Assert` will be marked with an `aweXpectM003` warning: "Xunit assertions should be migrated to aweXpect"
3. Most warnings can be automatically fixed with a code fix provider
4. Fix the remaining warnings manually
5. Remove the `aweXpect.Migration` package


## FluentAssertions

1. Add the `aweXpect.Migration` package reference in addition to the `aweXpect` in the test project.
   Add the following global using statements in the test project:
   ```csharp
   global using System.Threading.Tasks;`
   global using aweXpect;
   ```
2. All usages of `.Should()` will be marked with an `aweXpectM002` warning: "fluentassertions should be migrated to aweXpect"
3. Most warnings can be automatically fixed with a code fix provider
4. Fix all occurrences of `aweXpect0001`: "Expectations must be awaited or verified"
5. Fix the remaining warnings manually
6. Remove the `aweXpect.Migration` package
