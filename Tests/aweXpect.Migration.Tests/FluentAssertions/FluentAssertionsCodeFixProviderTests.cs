using Verifier =
	aweXpect.Migration.Tests.Verifiers.CSharpCodeFixVerifier<aweXpect.Migration.Analyzers.FluentAssertionsAnalyzer,
		aweXpect.Migration.Analyzers.FluentAssertionsCodeFixProvider>;

namespace aweXpect.Migration.Tests.FluentAssertions;

public class FluentAssertionsCodeFixProviderTests
{
	[Theory]
	[InlineData("false.Should().BeTrue()",
		"Expect.That(false).IsTrue()")]
	[InlineData("true.Should().BeFalse()",
		"Expect.That(true).IsFalse()")]
	[InlineData("2.Should().Be(1)",
		"Expect.That(2).IsEqualTo(1)")]
	[InlineData("2.Should().NotBe(1)",
		"Expect.That(2).IsNotEqualTo(1)")]
	[InlineData("(new object()).Should().BeNull()",
		"Expect.That(new object()).IsNull()")]
	[InlineData("(new object()).Should().NotBeNull()",
		"Expect.That(new object()).IsNotNull()")]
	[InlineData("\"foo\".Should().Contain(\"oo\")",
		"Expect.That(\"foo\").Contains(\"oo\")")]
	[InlineData("\"foo\".Should().StartWith(\"fo\")",
		"Expect.That(\"foo\").StartsWith(\"fo\")")]
	[InlineData("\"foo\".Should().EndWith(\"oo\")",
		"Expect.That(\"foo\").EndsWith(\"oo\")")]
	[InlineData("\"foo\".Should().BeEmpty()",
		"Expect.That(\"foo\").IsEmpty()")]
	[InlineData("\"foo\".Should().NotBeEmpty()",
		"Expect.That(\"foo\").IsNotEmpty()")]
	[InlineData("(new ArgumentException()).Should().BeSameAs(new NullReferenceException())",
		"Expect.That(new ArgumentException()).IsSameAs(new NullReferenceException())")]
	[InlineData("(new ArgumentException()).Should().NotBeSameAs(new NullReferenceException())",
		"Expect.That(new ArgumentException()).IsNotSameAs(new NullReferenceException())")]
	[InlineData("new Exception().Should().BeAssignableTo<ArgumentException>()",
		"Expect.That(new Exception()).Is<ArgumentException>()")]
	[InlineData("new Exception().Should().BeAssignableTo(typeof(ArgumentException))",
		"Expect.That(new Exception()).Is(typeof(ArgumentException))")]
	[InlineData("new Exception().Should().NotBeAssignableTo<ArgumentException>()",
		"Expect.That(new Exception()).IsNot<ArgumentException>()")]
	[InlineData("new Exception().Should().NotBeAssignableTo(typeof(ArgumentException))",
		"Expect.That(new Exception()).IsNot(typeof(ArgumentException))")]
	[InlineData("new Exception().Should().BeOfType<ArgumentException>()",
		"Expect.That(new Exception()).IsExactly<ArgumentException>()")]
	[InlineData("new Exception().Should().BeOfType(typeof(ArgumentException))",
		"Expect.That(new Exception()).IsExactly(typeof(ArgumentException))")]
	[InlineData("new Exception().Should().NotBeOfType<ArgumentException>()",
		"Expect.That(new Exception()).IsNotExactly<ArgumentException>()")]
	[InlineData("new Exception().Should().NotBeOfType(typeof(ArgumentException))",
		"Expect.That(new Exception()).IsNotExactly(typeof(ArgumentException))")]
	[InlineData("((Action)(() => {})).Should().Throw<ArgumentException>()",
		"Expect.That((Action)(() => {})).Throws<ArgumentException>()")]
	[InlineData("((Func<Task>)(() => Task.CompletedTask)).Should().ThrowAsync<ArgumentException>()",
		"Expect.That((Func<Task>)(() => Task.CompletedTask)).Throws<ArgumentException>()")]
	[InlineData("((Action)(() => {})).Should().ThrowExactly<ArgumentException>()",
		"Expect.That((Action)(() => {})).ThrowsExactly<ArgumentException>()")]
	[InlineData("((Func<Task>)(() => Task.CompletedTask)).Should().ThrowExactlyAsync<ArgumentException>()",
		"Expect.That((Func<Task>)(() => Task.CompletedTask)).ThrowsExactly<ArgumentException>()")]
	public async Task ShouldApplyCodeFix(string xunitAssertion, string aweXpectExpectation) => await Verifier
		.VerifyCodeFixAsync(
			$$"""
			  using System;
			  using System.Threading.Tasks;
			  using aweXpect;
			  using FluentAssertions;
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
			  using FluentAssertions;
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
			using FluentAssertions;
			using Xunit;

			public class MyClass
			{
			    [Theory]
			    [InlineData("bar")]
			    public void MyTest(string expected)
			    {
			        string subject = "foo";
			        
			        [|subject.Should().Be(expected)|];
			    }
			}
			""",
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
			        
			        Expect.That(subject).IsEqualTo(expected);
			    }
			}
			"""
		);
}
