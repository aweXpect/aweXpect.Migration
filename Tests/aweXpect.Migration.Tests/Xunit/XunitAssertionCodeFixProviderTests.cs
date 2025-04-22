using Verifier =
	aweXpect.Migration.Tests.Verifiers.CSharpCodeFixVerifier<aweXpect.Migration.Analyzers.XunitAssertionAnalyzer,
		aweXpect.Migration.Analyzers.XunitAssertionCodeFixProvider>;

namespace aweXpect.Migration.Tests.Xunit;

public class XunitAssertionCodeFixProviderTests
{
	[Theory]
	[InlineData("Assert.True(false)",
		"Expect.That(false).IsTrue()")]
	[InlineData("Assert.False(true)",
		"Expect.That(true).IsFalse()")]
	[InlineData("Assert.True(false, \"foo\")",
		"Expect.That(false).IsTrue().Because(\"foo\")")]
	[InlineData("Assert.False(true, \"foo\")",
		"Expect.That(true).IsFalse().Because(\"foo\")")]
	[InlineData("Assert.Null(new object())",
		"Expect.That(new object()).IsNull()")]
	[InlineData("Assert.NotNull(new object())",
		"Expect.That(new object()).IsNotNull()")]
	[InlineData("Assert.Equal(1, 2)",
		"Expect.That(2).IsEqualTo(1)")]
	[InlineData("Assert.NotEqual(1, 2)",
		"Expect.That(2).IsNotEqualTo(1)")]
	[InlineData("Assert.Contains(\"oo\", \"foo\")",
		"Expect.That(\"foo\").Contains(\"oo\")")]
	[InlineData("Assert.DoesNotContain(\"oo\", \"foo\")",
		"Expect.That(\"foo\").DoesNotContain(\"oo\")")]
	[InlineData("Assert.StartsWith(\"fo\", \"foo\")",
		"Expect.That(\"foo\").StartsWith(\"fo\")")]
	[InlineData("Assert.EndsWith(\"oo\", \"foo\")",
		"Expect.That(\"foo\").EndsWith(\"oo\")")]
	[InlineData("Assert.Empty(\"foo\")",
		"Expect.That(\"foo\").IsEmpty()")]
	[InlineData("Assert.NotEmpty(\"foo\")",
		"Expect.That(\"foo\").IsNotEmpty()")]
	[InlineData("Assert.Distinct([1, 2,])",
		"Expect.That([1, 2,]).AreAllUnique()")]
	[InlineData("Assert.Same(new ArgumentException(), new NullReferenceException())",
		"Expect.That(new NullReferenceException()).IsSameAs(new ArgumentException())")]
	[InlineData("Assert.NotSame(new ArgumentException(), new NullReferenceException())",
		"Expect.That(new NullReferenceException()).IsNotSameAs(new ArgumentException())")]
	[InlineData("Assert.IsAssignableFrom<ArgumentException>(new Exception())",
		"Expect.That(new Exception()).Is<ArgumentException>()")]
	[InlineData("Assert.IsAssignableFrom(typeof(ArgumentException), new Exception())",
		"Expect.That(new Exception()).Is(typeof(ArgumentException))")]
	[InlineData("Assert.IsNotAssignableFrom<ArgumentException>(new Exception())",
		"Expect.That(new Exception()).IsNot<ArgumentException>()")]
	[InlineData("Assert.IsNotAssignableFrom(typeof(ArgumentException), new Exception())",
		"Expect.That(new Exception()).IsNot(typeof(ArgumentException))")]
	[InlineData("Assert.IsType<ArgumentException>(new Exception())",
		"Expect.That(new Exception()).IsExactly<ArgumentException>()")]
	[InlineData("Assert.IsType(typeof(ArgumentException), new Exception())",
		"Expect.That(new Exception()).IsExactly(typeof(ArgumentException))")]
	[InlineData("Assert.IsNotType<ArgumentException>(new Exception())",
		"Expect.That(new Exception()).IsNotExactly<ArgumentException>()")]
	[InlineData("Assert.IsNotType(typeof(ArgumentException), new Exception())",
		"Expect.That(new Exception()).IsNotExactly(typeof(ArgumentException))")]
	[InlineData("Assert.Fail(\"foo\")",
		"Fail.Test(\"foo\")")]
	[InlineData("Assert.Skip(\"foo\")",
		"Skip.Test(\"foo\")")]
	[InlineData("Assert.ThrowsAny<ArgumentException>((Action)(() => {}))",
		"Expect.That((Action)(() => {})).Throws<ArgumentException>()")]
	[InlineData("Assert.ThrowsAnyAsync<ArgumentException>((Func<Task>)(() => Task.CompletedTask))",
		"Expect.That((Func<Task>)(() => Task.CompletedTask)).Throws<ArgumentException>()")]
	[InlineData("Assert.Throws<ArgumentException>((Action)(() => {}))",
		"Expect.That((Action)(() => {})).ThrowsExactly<ArgumentException>()")]
	[InlineData("Assert.ThrowsAsync<ArgumentException>((Func<Task>)(() => Task.CompletedTask))",
		"Expect.That((Func<Task>)(() => Task.CompletedTask)).ThrowsExactly<ArgumentException>()")]
	[InlineData("Assert.Throws(typeof(ArgumentException), (Action)(() => {}))",
		"Expect.That((Action)(() => {})).ThrowsExactly(typeof(ArgumentException))")]
	[InlineData("Assert.ThrowsAsync(typeof(ArgumentException), (Func<Task>)(() => Task.CompletedTask))",
		"Expect.That((Func<Task>)(() => Task.CompletedTask)).ThrowsExactly(typeof(ArgumentException))")]
	public async Task ShouldApplyCodeFix(string xunitAssertion, string aweXpectExpectation) => await Verifier
		.VerifyCodeFixAsync(
			$$"""
			  using System;
			  using System.Threading.Tasks;
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
			  using System;
			  using System.Threading.Tasks;
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
