﻿using Verifier =
	aweXpect.Migration.Tests.Verifiers.CSharpCodeFixVerifier<aweXpect.Migration.Analyzers.XunitAssertionAnalyzer,
		aweXpect.Migration.Analyzers.XunitAssertionCodeFixProvider>;

namespace aweXpect.Migration.Tests.Xunit;

public class XunitAssertionCodeFixProviderTests
{
	[Theory]
	[MemberData(nameof(TestCases.Basic), MemberType = typeof(TestCases))]
	public async Task ShouldApplyCodeFixForBasicTestCases(
		string xunitAssertion,
		string aweXpect,
		string arrange,
		bool isAsync) => await VerifyTestCase(xunitAssertion, aweXpect, arrange, isAsync);

	[Theory]
	[MemberData(nameof(TestCases.Boolean), MemberType = typeof(TestCases))]
	public async Task ShouldApplyCodeFixForBooleanTestCases(
		string xunitAssertion,
		string aweXpect,
		string arrange,
		bool isAsync) => await VerifyTestCase(xunitAssertion, aweXpect, arrange, isAsync);

	[Theory]
	[MemberData(nameof(TestCases.Collections), MemberType = typeof(TestCases))]
	public async Task ShouldApplyCodeFixForCollectionTestCases(
		string xunitAssertion,
		string aweXpect,
		string arrange,
		bool isAsync) => await VerifyTestCase(xunitAssertion, aweXpect, arrange, isAsync);

	[Theory]
	[MemberData(nameof(TestCases.Equality), MemberType = typeof(TestCases))]
	public async Task ShouldApplyCodeFixForEqualityTestCases(
		string xunitAssertion,
		string aweXpect,
		string arrange,
		bool isAsync) => await VerifyTestCase(xunitAssertion, aweXpect, arrange, isAsync);

	[Theory]
	[MemberData(nameof(TestCases.Exceptions), MemberType = typeof(TestCases))]
	public async Task ShouldApplyCodeFixForExceptionTestCases(
		string xunitAssertion,
		string aweXpect,
		string arrange,
		bool isAsync) => await VerifyTestCase(xunitAssertion, aweXpect, arrange, isAsync);

	[Theory]
	[MemberData(nameof(TestCases.Strings), MemberType = typeof(TestCases))]
	public async Task ShouldApplyCodeFixForStringTestCases(
		string xunitAssertion,
		string aweXpect,
		string arrange,
		bool isAsync) => await VerifyTestCase(xunitAssertion, aweXpect, arrange, isAsync);

	[Theory]
	[MemberData(nameof(TestCases.Types), MemberType = typeof(TestCases))]
	public async Task ShouldApplyCodeFixForTypeTestCases(
		string xunitAssertion,
		string aweXpect,
		string arrange,
		bool isAsync) => await VerifyTestCase(xunitAssertion, aweXpect, arrange, isAsync);

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


	private static async Task VerifyTestCase(
		string xunitAssertion,
		string aweXpect,
		string arrange,
		bool isAsync) => await Verifier
		.VerifyCodeFixAsync(
			$$"""
			  using System;
			  using System.Collections.Generic;
			  using System.Threading.Tasks;
			  using aweXpect;
			  using Xunit;

			  public class MyClass
			  {
			      [Fact]
			      public {{(isAsync ? "async Task" : "void")}} MyTest()
			      {
			          {{arrange}}
			          
			          {{(isAsync ? "await " : "")}}[|{{xunitAssertion}}|];
			      }
			  }
			  """,
			$$"""
			  using System;
			  using System.Collections.Generic;
			  using System.Threading.Tasks;
			  using aweXpect;
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
}
