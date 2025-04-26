using Verifier =
	aweXpect.Migration.Tests.Verifiers.CSharpCodeFixVerifier<aweXpect.Migration.Analyzers.FluentAssertionsAnalyzer,
		aweXpect.Migration.Analyzers.FluentAssertionsCodeFixProvider>;

namespace aweXpect.Migration.Tests.FluentAssertions;

public class FluentAssertionsCodeFixProviderTests
{
	[Theory]
	[MemberData(nameof(GetTestCases))]
	public async Task ShouldApplyCodeFixForSynchronousTestCases(
		string fluentAssertions,
		string aweXpect,
		string arrange,
		bool isAsync) => await Verifier
		.VerifyCodeFixAsync(
			$$"""
			  using System;
			  using System.Collections.Generic;
			  using System.Threading.Tasks;
			  using aweXpect;
			  using FluentAssertions;
			  using Xunit;

			  public class MyClass
			  {
			      [Fact]
			      public {{(isAsync ? "async Task" : "void")}} MyTest()
			      {
			          {{arrange}}
			          
			          {{(isAsync ? "await " : "")}}[|{{fluentAssertions}}|];
			      }
			  }
			  """,
			$$"""
			  using System;
			  using System.Collections.Generic;
			  using System.Threading.Tasks;
			  using aweXpect;
			  using FluentAssertions;
			  using Xunit;

			  public class MyClass
			  {
			      [Fact]
			      public {{(isAsync ? "async Task" : "void")}} MyTest()
			      {
			          {{arrange}}
			          
			          {{(isAsync ? "await " : "")}}{{aweXpect}};
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

	[Fact]
	public async Task ShouldSupportNullablePropertyAccess() => await Verifier
		.VerifyCodeFixAsync(
			"""
			using System;
			using aweXpect;
			using FluentAssertions;
			using Xunit;

			public class MyTestClass
			{
			    [Theory]
			    [InlineData("bar")]
			    public void MyTest(string expected)
			    {
			        MyClass subject = new();
			        
			        [|subject?.Inner?.NewInner()?.Value.Should().Be(expected)|];
			    }
			    private class MyClass
			    {
			        public MyClass NewInner() => new MyClass();
			        public MyClass Inner => null;
			        public string Value => "";
			    }
			}
			""",
			"""
			using System;
			using aweXpect;
			using FluentAssertions;
			using Xunit;

			public class MyTestClass
			{
			    [Theory]
			    [InlineData("bar")]
			    public void MyTest(string expected)
			    {
			        MyClass subject = new();
			        
			        Expect.That(subject?.Inner?.NewInner()?.Value).IsEqualTo(expected);
			    }
			    private class MyClass
			    {
			        public MyClass NewInner() => new MyClass();
			        public MyClass Inner => null;
			        public string Value => "";
			    }
			}
			"""
		);

	public static TheoryData<string, string, string, bool> GetTestCases()
		=> new TheoryData<string, string, string, bool>()
			.AddBasicTestCases()
			.AddBooleanTestCases()
			.AddChronologyTestCases()
			.AddCollectionTestCases()
			.AddExceptionsTestCases()
			.AddNumberTestCases()
			.AddStringTestCases();
}
