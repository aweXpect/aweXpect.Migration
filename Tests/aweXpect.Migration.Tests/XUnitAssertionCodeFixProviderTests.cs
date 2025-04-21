using Verifier = aweXpect.Migration.Tests.Verifiers.CSharpCodeFixVerifier<aweXpect.Migration.Analyzers.XUnitAssertionAnalyzer,
		aweXpect.Migration.Analyzers.XunitAssertionCodeFixProvider>;

namespace aweXpect.Migration.Tests;

public class XunitAssertionCodeFixProviderTests
{
	[Fact]
	public async Task ShouldApplyCodeFix() => await Verifier
		.VerifyCodeFixAsync(
			"""
			using aweXpect;
			using Xunit;

			public class MyClass
			{
			    [Theory]
			    [InlineData("bar")]
			    public void MyTest(string expected)
			    {
			        string subject = "foo";
			        
			        [|Assert.Equal(expected, subject)|];
			    }
			}
			""",
			"""
			using aweXpect;
			using Xunit;

			public class MyClass
			{
			    [Theory]
			    [InlineData("bar")]
			    public void MyTest(string expected)
			    {
			        string subject = "foo";
			        
			        Expect.That(subject).IsEqualTo(expected);
			    }
			}
			"""
		);
}
