using aweXpect.Migration.Analyzers;
using Verifier =
	aweXpect.Migration.Tests.Verifiers.CSharpAnalyzerVerifier<aweXpect.Migration.Analyzers.FluentAssertionsAnalyzer>;

namespace aweXpect.Migration.Tests.FluentAssertions;

public class FluentAssertionsAnalyzerTests
{
	[Fact]
	public async Task WhenUsingAssertEqual_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			"""
			using aweXpect;
			using FluentAssertions;
			using Xunit;

			public class MyClass
			{
			    [Theory]
			    [InlineData("bar")]
			    public void MyTest(string expected)
			    {
			        string subject = "foo";
			        
			        {|#0:subject.Should().Be(expected)|};
			    }
			}
			""",
			Verifier.Diagnostic(Rules.FluentAssertionsRule)
				.WithLocation(0)
		);

	[Fact]
	public async Task WhenUsingAssertTrue_ShouldBeFlagged() => await Verifier
		.VerifyAnalyzerAsync(
			"""
			using aweXpect;
			using FluentAssertions;
			using Xunit;

			public class MyClass
			{
			    [Fact]
			    public void MyTest()
			    {
			        bool subject = true;
			        
			        {|#0:subject.Should().BeTrue()|};
			    }
			}
			""",
			Verifier.Diagnostic(Rules.FluentAssertionsRule)
				.WithLocation(0)
		);
}
