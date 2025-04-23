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
		InvocationExpressionSyntax expressionSyntax, CancellationToken cancellationToken)
	{
		Document? document = context.Document;

		SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

		if (root is not CompilationUnitSyntax compilationUnit)
		{
			return document;
		}

		if (expressionSyntax.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax)
		{
			return document;
		}

		ExpressionOrPatternSyntax topmostExpression = expressionSyntax.Expression;
		while (topmostExpression.Parent is ExpressionOrPatternSyntax parent)
		{
			topmostExpression = parent;
		}

		ArgumentSyntax? expected = expressionSyntax.ArgumentList.Arguments.ElementAtOrDefault(0);

		ExpressionSyntaxWalker walker = new();
		walker.Visit(expressionSyntax);
		ExpressionSyntax? actual = walker.Subject;
		if (actual is null)
		{
			return document;
		}

		string? methodName = memberAccessExpressionSyntax.Name.Identifier.ValueText;

		string? genericArgs = GetGenericArguments(memberAccessExpressionSyntax.Name);

		ExpressionSyntax? newExpression = await GetNewExpression(context,
			methodName, actual, expected, genericArgs, expressionSyntax.ArgumentList.Arguments);

		if (newExpression != null)
		{
			compilationUnit =
				compilationUnit.ReplaceNode(expressionSyntax, newExpression.WithTriviaFrom(expressionSyntax));
		}

		return document.WithSyntaxRoot(compilationUnit);
	}

#pragma warning disable S3776
	private static async Task<ExpressionSyntax?> GetNewExpression(
		CodeFixContext context,
		string method,
		ExpressionSyntax actual,
		ArgumentSyntax? expected,
		string genericArgs,
		SeparatedSyntaxList<ArgumentSyntax> argumentListArguments)
	{
		bool isGeneric = !string.IsNullOrEmpty(genericArgs);

		ExpressionSyntax? ParseExpressionWithBecause(string expression, int? becauseIndex = null)
			=> ParseExpressionWithBecauseSupport(argumentListArguments, expression, becauseIndex);

		return method switch
		{
			"Be" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsEqualTo({expected})", 1),
			"NotBe" => ParseExpressionWithBecause(
				$"Expect.That({actual}).IsNotEqualTo({expected})", 1),
			"BeEquivalentTo" => await BeEquivalentTo(context, argumentListArguments, actual, expected),
			"NotBeEquivalentTo" => await BeEquivalentTo(context, argumentListArguments, actual, expected, true),
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
				$"Expect.That({actual}).IsEqualTo({expected}).Within({argumentListArguments.ElementAtOrDefault(1)})",
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
		ExpressionSyntax actual, ArgumentSyntax? expected, bool negated = false)
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
				becauseIndex);
		}

		return ParseExpressionWithBecauseSupport(argumentListArguments,
			$"Expect.That({actual}).{(negated ? "IsNotEquivalentTo" : "IsEquivalentTo")}({expected})", 1);
	}

	private static ExpressionSyntax? ParseExpressionWithBecauseSupport(
		SeparatedSyntaxList<ArgumentSyntax> argumentListArguments, string expression, int? becauseIndex = null)
	{
		if (becauseIndex.HasValue)
		{
			ArgumentSyntax? because = argumentListArguments.ElementAtOrDefault(becauseIndex.Value);
			if (because != null)
			{
				expression += $".Because({because})";
			}
		}

		return SyntaxFactory.ParseExpression(expression);
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

	private sealed class ExpressionSyntaxWalker : SyntaxWalker
	{
		private bool _isShould;
		public ExpressionSyntax? Subject { get; private set; }

		public override void Visit(SyntaxNode node)
		{
			if (_isShould && node is not ParenthesizedExpressionSyntax)
			{
				Subject = node as ExpressionSyntax;
				_isShould = false;
			}

			if (node is MemberAccessExpressionSyntax identifierNameSyntax &&
			    identifierNameSyntax.Name.Identifier.ValueText == "Should")
			{
				_isShould = true;
			}

			base.Visit(node);
		}
	}
}
