using aweXpect.Migration.Common;
using Verifier =
	aweXpect.Migration.Xunit.Tests.Verifiers.CSharpCodeFixVerifier<aweXpect.Migration.Xunit.XUnitAssertionAnalyzer, aweXpect.Migration.Xunit.XunitAssertionCodeFixProvider>;

namespace aweXpect.Migration.Xunit.Tests;

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
