using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Migration.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace aweXpect.Migration.Analyzers;

/// <summary>
///     A code fix provider that migrates most assertions from fluentassertions to aweXpect.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FluentAssertionsCodeFixProvider))]
[Shared]
public class FluentAssertionsCodeFixProvider() : AssertionCodeFixProvider(Rules.FluentAssertionsRule)
{
	/// <inheritdoc />
	protected override async Task<Document> ConvertAssertionAsync(CodeFixContext context,
		ExpressionSyntax expressionSyntax, CancellationToken cancellationToken)
	{
		Document? document = context.Document;

		SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

		if (root is not CompilationUnitSyntax compilationUnit)
		{
			return document;
		}

		ExpressionSyntaxWalker walker = new(expressionSyntax is ConditionalAccessExpressionSyntax);
		walker.Visit(expressionSyntax);
		ExpressionSyntax? actual = walker.Subject;
		if (actual is null || walker.Methods.Count == 0)
		{
			return document;
		}

		SyntaxNode nodeToReplace = expressionSyntax;
		if (expressionSyntax is LambdaExpressionSyntax lambdaExpressionSyntax)
		{
			nodeToReplace = lambdaExpressionSyntax.Body;
		}

		ExpressionSyntax? newExpression = await GetNewExpression(context,
			actual, walker.Methods, expressionSyntax is LambdaExpressionSyntax);

		if (newExpression != null)
		{
			compilationUnit =
				compilationUnit.ReplaceNode(nodeToReplace, newExpression.WithTriviaFrom(expressionSyntax));
		}

		return document.WithSyntaxRoot(compilationUnit);
	}

#pragma warning disable S3776
	private static async Task<ExpressionSyntax?> GetNewExpression(
		CodeFixContext context,
		ExpressionSyntax actual,
		Stack<MethodDefinition> methods,
		bool wrapSynchronously)
	{
		MethodDefinition? mainMethod = methods.Pop();
		MemberAccessExpressionSyntax? memberAccessExpressionSyntax = mainMethod.Method;
		ArgumentSyntax? expected = mainMethod.Arguments.ElementAtOrDefault(0);

		string? methodName = memberAccessExpressionSyntax.Name.Identifier.ValueText;
		string? genericArgs = GetGenericArguments(memberAccessExpressionSyntax.Name);
		bool isGeneric = !string.IsNullOrEmpty(genericArgs);

		ExpressionSyntax? ParseExpressionWithBecause(string expression, int? becauseIndex = null)
			=> ParseExpressionWithBecauseSupport(mainMethod.Arguments, expression, methods, wrapSynchronously,
				becauseIndex);

		return methodName switch
		{
			"Be" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsEqualTo({expected})", 1),
			"NotBe" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotEqualTo({expected})", 1),
			"BeEquivalentTo" => await BeEquivalentTo(context, mainMethod.Arguments, actual, expected,
				methods, wrapSynchronously),
			"NotBeEquivalentTo" => await BeEquivalentTo(context, mainMethod.Arguments, actual, expected,
				methods, wrapSynchronously, true),
			"Contain" => ParseExpressionWithBecause(
				$"Expect.That({actual}).Contains({expected})", 1),
			"NotContain" => ParseExpressionWithBecause(
				$"Expect.That({actual}).DoesNotContain({expected})", 1),
			"StartWith" => ParseExpressionWithBecause(
				$"Expect.That({actual}).StartsWith({expected})", 1),
			"NotStartWith" => ParseExpressionWithBecause(
				$"Expect.That({actual}).DoesNotStartWith({expected})", 1),
			"EndWith" => ParseExpressionWithBecause(
				$"Expect.That({actual}).EndsWith({expected})", 1),
			"NotEndWith" => ParseExpressionWithBecause(
				$"Expect.That({actual}).DoesNotEndWith({expected})", 1),
			"BeEmpty" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsEmpty()", 0),
			"NotBeEmpty" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotEmpty()", 0),
			"BeNullOrEmpty" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNullOrEmpty()", 0),
			"NotBeNullOrEmpty" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotNullOrEmpty()", 0),
			"BeNullOrWhiteSpace" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNullOrWhiteSpace()", 0),
			"NotBeNullOrWhiteSpace" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotNullOrWhiteSpace()", 0),
			"BePositive" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsPositive()", 0),
			"BeNegative" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNegative()", 0),
			"BeGreaterThan" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsGreaterThan({expected})", 1),
			"BeGreaterThanOrEqualTo" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsGreaterThanOrEqualTo({expected})", 1),
			"BeGreaterOrEqualTo" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsGreaterThanOrEqualTo({expected})", 1),
			"BeLessThan" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsLessThan({expected})", 1),
			"BeLessOrEqualTo" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsLessThanOrEqualTo({expected})", 1),
			"BeLessThanOrEqualTo" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsLessThanOrEqualTo({expected})", 1),
			"BeApproximately" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsEqualTo({expected}).Within({mainMethod.Arguments.ElementAtOrDefault(1)})",
				2),
			"BeAfter" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsAfter({expected})", 1),
			"BeOnOrAfter" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsOnOrAfter({expected})", 1),
			"BeBefore" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsBefore({expected})", 1),
			"BeOnOrBefore" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsOnOrBefore({expected})", 1),
			"NotBeAfter" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotAfter({expected})", 1),
			"NotBeOnOrAfter" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotOnOrAfter({expected})", 1),
			"NotBeBefore" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotBefore({expected})", 1),
			"NotBeOnOrBefore" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotOnOrBefore({expected})", 1),
			"NotBeNull" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotNull()", 0),
			"BeNull" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNull()", 0),
			"BeTrue" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsTrue()", 0),
			"BeFalse" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsFalse()", 0),
			"NotBeTrue" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotTrue()", 0),
			"NotBeFalse" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotFalse()", 0),
			"Imply" => ParseExpressionWithBecause(
				$"Expect.That({actual}).Implies({expected})", 1),
			"BeDefined" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsDefined()", 0),
			"NotBeDefined" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotDefined()", 0),
			"HaveValue" => expected is null || expected.Expression.ToString().Contains('\"')
				? ParseExpressionWithBecause(
					$"Expect.That({actual}).IsNotNull()", 0)
				: ParseExpressionWithBecause(
					$"Expect.That({actual}).HasValue({expected})", 1),
			"NotHaveValue" => expected is null || expected.Expression.ToString().Contains('\"')
				? ParseExpressionWithBecause(
					$"Expect.That({actual}).IsNull()", 0)
				: ParseExpressionWithBecause(
					$"Expect.That({actual}).DoesNotHaveValue({expected})", 1),
			"HaveFlag" => ParseExpressionWithBecause(
				$"Expect.That({actual}).HasFlag({expected})", 1),
			"NotHaveFlag" => ParseExpressionWithBecause(
				$"Expect.That({actual}).DoesNotHaveFlag({expected})", 1),
			"BeSameAs" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsSameAs({expected})", 1),
			"NotBeSameAs" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotSameAs({expected})", 1),
			"HaveCount" => ParseExpressionWithBecause(
				$"Expect.That({actual}).HasCount({expected})", 1),
			"BeAssignableTo" => isGeneric
				? ParseExpressionWithBecause(
					$"Expect.That({actual}).Is<{genericArgs}>()", 0)
				: ParseExpressionWithBecause(
					$"Expect.That({actual}).Is({expected})", 1),
			"NotBeAssignableTo" => isGeneric
				? ParseExpressionWithBecause(
					$"Expect.That({actual}).IsNot<{genericArgs}>()", 0)
				: ParseExpressionWithBecause(
					$"Expect.That({actual}).IsNot({expected})", 1),
			"BeOfType" => isGeneric
				? ParseExpressionWithBecause(
					$"Expect.That({actual}).IsExactly<{genericArgs}>()", 0)
				: ParseExpressionWithBecause(
					$"Expect.That({actual}).IsExactly({expected})", 1),
			"NotBeOfType" => isGeneric
				? ParseExpressionWithBecause(
					$"Expect.That({actual}).IsNotExactly<{genericArgs}>()", 0)
				: ParseExpressionWithBecause(
					$"Expect.That({actual}).IsNotExactly({expected})", 1),
			"NotThrow" or "NotThrowAsync" => ParseExpressionWithBecause(
				$"Expect.That({actual}).DoesNotThrow()", 0),
			"Throw" or "ThrowAsync" => isGeneric
				? ParseExpressionWithBecause(
					$"Expect.That({actual}).Throws<{genericArgs}>()", 0)
				: ParseExpressionWithBecause(
					$"Expect.That({actual}).Throws({expected})", 1),
			"ThrowExactly" or "ThrowExactlyAsync" => isGeneric
				? ParseExpressionWithBecause(
					$"Expect.That({actual}).ThrowsExactly<{genericArgs}>()", 0)
				: ParseExpressionWithBecause(
					$"Expect.That({actual}).ThrowsExactly({expected})", 1),
			_ => null,
		};
	}
#pragma warning restore S3776

	private static async Task<ExpressionSyntax?> BeEquivalentTo(CodeFixContext context,
		SeparatedSyntaxList<ArgumentSyntax> argumentListArguments,
		ExpressionSyntax actual, ArgumentSyntax? expected, Stack<MethodDefinition> methods, bool wrapSynchronously,
		bool negated = false)
	{
		SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync();
		ISymbol? actualSymbol = semanticModel.GetSymbolInfo(actual).Symbol;
		if (semanticModel is not null && actualSymbol is not null &&
		    GetType(actualSymbol) is { } actualSymbolType && IsEnumerable(actualSymbolType))
		{
			string expressionSuffix = "";
			int becauseIndex = 1;
			string? secondArgument = argumentListArguments.ElementAtOrDefault(1)?.ToString() ?? "";
			if (!secondArgument.StartsWith("\"") || !secondArgument.EndsWith("\""))
			{
				if (secondArgument.Contains(".WithoutStrictOrdering()"))
				{
					expressionSuffix = ".InAnyOrder()";
				}

				becauseIndex++;
			}

			return ParseExpressionWithBecauseSupport(argumentListArguments,
				$"Expect.That({actual}).{(negated ? "IsNotEqualTo" : "IsEqualTo")}({expected})" + expressionSuffix,
				methods, wrapSynchronously, becauseIndex);
		}

		return ParseExpressionWithBecauseSupport(argumentListArguments,
			$"Expect.That({actual}).{(negated ? "IsNotEquivalentTo" : "IsEquivalentTo")}({expected})",
			methods, wrapSynchronously, 1);
	}

	private static ExpressionSyntax? ParseExpressionWithBecauseSupport(
		SeparatedSyntaxList<ArgumentSyntax> argumentListArguments,
		string expression,
		Stack<MethodDefinition> methods,
		bool wrapSynchronously,
		int? becauseIndex = null)
	{
		if (methods.Count > 0)
		{
			foreach (MethodDefinition? method in methods)
			{
				expression += ParseAdditionalMethodExpression(method);
			}
		}

		if (becauseIndex.HasValue)
		{
			string? because = argumentListArguments.ElementAtOrDefault(becauseIndex.Value)?.ToString();
			if (because != null)
			{
				if (becauseIndex.Value + 1 < argumentListArguments.Count)
				{
					because = $"${because}";
					int index = 0;
					for (int i = becauseIndex.Value + 1; i < argumentListArguments.Count; i++)
					{
						because = because.Replace($"{{{index++}}}", $"{{{argumentListArguments[i]}}}");
					}
				}

				expression += $".Because({because})";
			}
		}

		if (wrapSynchronously)
		{
			expression = $"aweXpect.Synchronous.Synchronously.Verify({expression})";
		}

		return SyntaxFactory.ParseExpression(expression);
	}

	private static string ParseAdditionalMethodExpression(MethodDefinition method)
	{
		string? methodName = method.Method.Name.Identifier.ValueText;
		return methodName switch
		{
			"WithMessage" => $".WithMessage({method.Arguments.ElementAtOrDefault(0)}).AsWildcard()",
			_ => "",
		};
	}

	private static ITypeSymbol? GetType(ISymbol symbol)
	{
		if (symbol is ITypeSymbol typeSymbol)
		{
			return typeSymbol;
		}

		if (symbol is IPropertySymbol propertySymbol)
		{
			return propertySymbol.Type;
		}

		if (symbol is IFieldSymbol fieldSymbol)
		{
			return fieldSymbol.Type;
		}

		if (symbol is ILocalSymbol localSymbol)
		{
			return localSymbol.Type;
		}

		return null;
	}

	private static bool IsEnumerable(ITypeSymbol typeSymbol)
	{
		if (typeSymbol is IArrayTypeSymbol)
		{
			return true;
		}

		if (typeSymbol is INamedTypeSymbol namedTypeSymbol
		    && namedTypeSymbol.GloballyQualifiedNonGeneric() is "global::System.Collections.IEnumerable"
			    or "global::System.Collections.Generic.IEnumerable")
		{
			return true;
		}

		return typeSymbol.AllInterfaces.Any(i => i.GloballyQualified() == "global::System.Collections.IEnumerable");
	}

	private static string GetGenericArguments(ExpressionSyntax expressionSyntax)
	{
		if (expressionSyntax is GenericNameSyntax genericName)
		{
			return string.Join(", ", genericName.TypeArgumentList.Arguments.ToList());
		}

		return string.Empty;
	}

	public class MethodDefinition
	{
		public MethodDefinition(MemberAccessExpressionSyntax method)
		{
			Method = method;
			if (method?.Parent is InvocationExpressionSyntax invocationExpressionSyntax)
			{
				Arguments = invocationExpressionSyntax.ArgumentList.Arguments;
			}
			else
			{
				Arguments = [];
			}
		}

		public MemberAccessExpressionSyntax Method { get; }
		public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }
	}

	private sealed class ExpressionSyntaxWalker(bool isConditional) : SyntaxWalker
	{
		private bool _isConditional = isConditional;
		private bool _isShould;
		private string _subjectString = "";
		public ExpressionSyntax? Subject { get; private set; }

		public Stack<MethodDefinition> Methods { get; } = [];

		public override void Visit(SyntaxNode node)
		{
			if (_isConditional)
			{
				if (_subjectString == "" && node is IdentifierNameSyntax identifierNameSyntax)
				{
					_subjectString = identifierNameSyntax.ToString();
				}

				if (node is MemberBindingExpressionSyntax memberBindingExpressionSyntax)
				{
					_subjectString += "?" + memberBindingExpressionSyntax;
				}

				if (node is ArgumentListSyntax invocationExpressionSyntax)
				{
					_subjectString += invocationExpressionSyntax;
				}
			}

			if (_isShould && node is not ParenthesizedExpressionSyntax)
			{
				Subject = node as ExpressionSyntax;
				_isShould = false;
			}

			if (node is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
			{
				_isShould = memberAccessExpressionSyntax.Name.Identifier.ValueText == "Should";
				if (_isShould)
				{
					if (_isConditional)
					{
						Subject = SyntaxFactory.ParseExpression(
							_subjectString + "?" + memberAccessExpressionSyntax.Expression);
						_isShould = false;
						_isConditional = false;
					}
				}
				else
				{
					Methods.Push(new MethodDefinition(memberAccessExpressionSyntax));
				}
			}

			if (node is not ArgumentSyntax)
			{
				base.Visit(node);
			}
		}
	}
}
