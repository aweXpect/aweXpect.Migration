using Verifier =
	aweXpect.Migration.Tests.Verifiers.CSharpCodeFixVerifier<aweXpect.Migration.Analyzers.XUnitAssertionAnalyzer,
		aweXpect.Migration.Analyzers.XunitAssertionCodeFixProvider>;

namespace aweXpect.Migration.Tests;

public class XunitAssertionCodeFixProviderTests
{
	[Theory]
	[InlineData("Assert.True(false)", "Expect.That(false).IsTrue()")]
	[InlineData("Assert.False(true)", "Expect.That(true).IsFalse()")]
	[InlineData("Assert.Null(new object())", "Expect.That(new object()).IsNull()")]
	[InlineData("Assert.Equal(1, 2)", "Expect.That(2).IsEqualTo(1)")]
	[InlineData("Assert.NotEqual(1, 2)", "Expect.That(2).IsNotEqualTo(1)")]
	[InlineData("Assert.Contains(\"oo\", \"foo\")", "Expect.That(\"foo\").Contains(\"oo\")")]
	public async Task ShouldApplyCodeFix(string xunitAssertion, string aweXpectExpectation) => await Verifier
		.VerifyCodeFixAsync(
			$$"""
			  using aweXpect;
			  using Xunit;

			  public class MyClass
			  {
			      [Fact]
			      public void MyTest()
			      {
			          [|{{xunitAssertion}}|];
			      }
			  }
			  """,
			$$"""
			  using aweXpect;
			  using Xunit;

			  public class MyClass
			  {
			      [Fact]
			      public void MyTest()
			      {
			          {{aweXpectExpectation}};
			      }
			  }
			  """
		);

	[Fact]
	public async Task ShouldApplyCodeFixInTheory() => await Verifier
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
