namespace aweXpect.Migration.Api.Tests;

/// <summary>
///     Whenever a test fails, this means that the public API surface changed.
///     If the change was intentional, execute the <see cref="ApiAcceptance.AcceptApiChanges()" /> test to take over the
///     current public API surface. The changes will become part of the pull request and will be reviewed accordingly.
/// </summary>
public sealed class ApiApprovalTests
{
	[Theory]
	[MemberData(nameof(TargetFrameworksTheoryData))]
	public async Task VerifyPublicApiForAweXpectMigrationCommon(string framework)
	{
		const string assemblyName = "aweXpect.Migration.Common";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await That(publicApi).IsEqualTo(expectedApi);
	}

	[Theory]
	[MemberData(nameof(TargetFrameworksTheoryData))]
	public async Task VerifyPublicApiForAweXpectMigrationCommonFluentAssertionsAnalyzers(string framework)
	{
		const string assemblyName = "aweXpect.Migration.FluentAssertions.Analyzers";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await That(publicApi).IsEqualTo(expectedApi);
	}

	[Theory]
	[MemberData(nameof(TargetFrameworksTheoryData))]
	public async Task VerifyPublicApiForAweXpectMigrationCommonFluentAssertionsCodeFixers(string framework)
	{
		const string assemblyName = "aweXpect.Migration.FluentAssertions.CodeFixers";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await That(publicApi).IsEqualTo(expectedApi);
	}

	[Theory]
	[MemberData(nameof(TargetFrameworksTheoryData))]
	public async Task VerifyPublicApiForAweXpectMigrationCommonXunitAnalyzers(string framework)
	{
		const string assemblyName = "aweXpect.Migration.Xunit.Analyzers";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await That(publicApi).IsEqualTo(expectedApi);
	}

	[Theory]
	[MemberData(nameof(TargetFrameworksTheoryData))]
	public async Task VerifyPublicApiForAweXpectMigrationCommonXunitCodeFixers(string framework)
	{
		const string assemblyName = "aweXpect.Migration.Xunit.CodeFixers";

		string publicApi = Helper.CreatePublicApi(framework, assemblyName);
		string expectedApi = Helper.GetExpectedApi(framework, assemblyName);

		await That(publicApi).IsEqualTo(expectedApi);
	}

	public static TheoryData<string> TargetFrameworksTheoryData()
	{
		TheoryData<string> theoryData = new();
		foreach (string targetFramework in Helper.GetTargetFrameworks())
		{
			theoryData.Add(targetFramework);
		}

		return theoryData;
	}
}
