using aweXpect.Migration.Common;
using Verifier =
	aweXpect.Migration.Xunit.Tests.Verifiers.CSharpAnalyzerVerifier<aweXpect.Migration.Xunit.XUnitAssertionAnalyzer>;

namespace aweXpect.Migration.Xunit.Tests;

public class XUnitAssertionAnalyzerTests
{
	[Fact]
	public async Task WhenUsingAssertEqual_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
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
			        
			        {|#0:Assert.Equal(expected, subject)|};
			    }
			}
			""",
			Verifier.Diagnostic(Rules.XUnitAssertionRule)
				.WithLocation(0)
		);

	[Fact]
	public async Task WhenUsingAssertTrue_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			"""
			using aweXpect;
			using Xunit;

			public class MyClass
			{
			    [Fact]
			    public void MyTest()
			    {
			        bool subject = true;
			        
			        {|#0:Assert.True(subject)|};
			    }
			}
			""",
			Verifier.Diagnostic(Rules.XUnitAssertionRule)
				.WithLocation(0)
		);
}
