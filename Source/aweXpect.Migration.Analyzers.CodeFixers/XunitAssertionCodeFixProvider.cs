using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Migration.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace aweXpect.Migration.Analyzers;

/// <summary>
///     A code fix provider that migrates most xunit assertions to aweXpect.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(XunitAssertionCodeFixProvider))]
[Shared]
public class XunitAssertionCodeFixProvider : CodeFixProvider
{
	/// <inheritdoc />
	public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
		ImmutableArray.Create(Rules.XUnitAssertionRule.Id);

	/// <inheritdoc />
	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	/// <inheritdoc />
	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (Diagnostic? diagnostic in context.Diagnostics)
		{
			TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

			SyntaxNode? root =
				await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			SyntaxNode? diagnosticNode = root?.FindNode(diagnosticSpan);

			if (diagnosticNode is not InvocationExpressionSyntax expressionSyntax)
			{
				return;
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					Resources.aweXpectM003CodeFixTitle,
					c => ConvertAssertionAsync(context, expressionSyntax, c),
					nameof(XunitAssertionCodeFixProvider)),
				diagnostic);
		}
	}

	private static async Task<Document> ConvertAssertionAsync(CodeFixContext context,
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

		ArgumentSyntax? expected = expressionSyntax.ArgumentList.Arguments.ElementAtOrDefault(0);
		ArgumentSyntax? actual = expressionSyntax.ArgumentList.Arguments.ElementAtOrDefault(1) ??
		                         expressionSyntax.ArgumentList.Arguments.ElementAtOrDefault(0);

		string? methodName = memberAccessExpressionSyntax.Name.Identifier.ValueText;

		string? genericArgs = GetGenericArguments(memberAccessExpressionSyntax.Name);

		ExpressionSyntax? newExpression = await GetNewExpression(context, memberAccessExpressionSyntax, methodName,
			actual, expected, genericArgs, expressionSyntax.ArgumentList.Arguments);

		if (newExpression != null)
		{
			compilationUnit =
				compilationUnit.ReplaceNode(expressionSyntax, newExpression.WithTriviaFrom(expressionSyntax));
		}

		return document.WithSyntaxRoot(compilationUnit);
	}

	private static async Task<ExpressionSyntax?> GetNewExpression(CodeFixContext context,
		MemberAccessExpressionSyntax memberAccessExpressionSyntax, string method,
		ArgumentSyntax? actual, ArgumentSyntax? expected, string genericArgs,
		SeparatedSyntaxList<ArgumentSyntax> argumentListArguments)
	{
		bool isGeneric = !string.IsNullOrEmpty(genericArgs);

		return method switch
		{
			"Equal" => await IsEqualTo(context, argumentListArguments, actual, expected),
			"NotEqual" => await IsNotEqualTo(context, argumentListArguments, actual, expected),
			"Contains" => await Contains(context, memberAccessExpressionSyntax, actual, expected),
			"DoesNotContain" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).DoesNotContain({expected})"),
			"StartsWith" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).StartsWith({expected})"),
			"EndsWith" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).EndsWith({expected})"),
			"NotNull" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNotNull()"),
			"Null" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNull()"),
			"True" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsTrue()"),
			"False" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsFalse()"),
			"Same" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsSameAs({expected})"),
			"NotSame" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNotSameAs({expected})"),
			"IsAssignableFrom" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).Is<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).Is({expected})"),
			"IsNotAssignableFrom" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsNot<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsNot({expected})"),
			"All" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).All().Satisfy({expected})"),
			"Single" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).HasSingle()"),
			"IsType" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsExactly<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsExactly({expected})"),
			"IsNotType" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsNotExactly<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsNotExactly({expected})"),
			"Empty" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsEmpty()"),
			"NotEmpty" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNotEmpty()"),
			"Fail" => SyntaxFactory.ParseExpression(
				"Fail.Test()"),
			"Skip" => SyntaxFactory.ParseExpression(
				"Skip.Test()"),
			"Throws" or "ThrowsAsync" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).ThrowsExactly<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).ThrowsExactly({expected})"),
			"ThrowsAny" or "ThrowsAnyAsync" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).Throws<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).Throws({expected})"),
			_ => null,
		};
	}

	private static async Task<ExpressionSyntax> IsNotEqualTo(CodeFixContext context,
		SeparatedSyntaxList<ArgumentSyntax> argumentListArguments,
		ArgumentSyntax? actual, ArgumentSyntax? expected)
	{
		if (argumentListArguments.Count >= 3 && argumentListArguments[2].Expression is LiteralExpressionSyntax
			                                     literalExpressionSyntax
		                                     && decimal.TryParse(literalExpressionSyntax.Token.ValueText, out _))
		{
			return SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNotEqualTo({expected}).Within({literalExpressionSyntax})");
		}

		SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync();

		ISymbol? actualSymbol = semanticModel.GetSymbolInfo(actual!.Expression).Symbol;
		ISymbol? expectedSymbol = semanticModel.GetSymbolInfo(expected!.Expression).Symbol;

		if (actualSymbol is not null && expectedSymbol is not null
		                             && GetType(actualSymbol) is { } actualSymbolType
		                             && GetType(expectedSymbol) is { } expectedSymbolType
		                             && IsEnumerable(actualSymbolType)
		                             && IsEnumerable(expectedSymbolType))
		{
			return SyntaxFactory.ParseExpression($"Expect.That({actual}).IsNotEquivalentTo({expected})");
		}

		return SyntaxFactory.ParseExpression($"Expect.That({actual}).IsNotEqualTo({expected})");
	}

	private static async Task<ExpressionSyntax> IsEqualTo(CodeFixContext context,
		SeparatedSyntaxList<ArgumentSyntax> argumentListArguments,
		ArgumentSyntax? actual, ArgumentSyntax? expected)
	{
		if (argumentListArguments.Count >= 3 && argumentListArguments[2].Expression is LiteralExpressionSyntax
			                                     literalExpressionSyntax
		                                     && decimal.TryParse(literalExpressionSyntax.Token.ValueText, out _))
		{
			return SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsEqualTo({expected}).Within({literalExpressionSyntax})");
		}

		SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync();

		ISymbol? actualSymbol = semanticModel.GetSymbolInfo(actual!.Expression).Symbol;
		ISymbol? expectedSymbol = semanticModel.GetSymbolInfo(expected!.Expression).Symbol;

		if (actualSymbol is not null && expectedSymbol is not null
		                             && GetType(actualSymbol) is { } actualSymbolType
		                             && GetType(expectedSymbol) is { } expectedSymbolType
		                             && IsEnumerable(actualSymbolType)
		                             && IsEnumerable(expectedSymbolType))
		{
			return SyntaxFactory.ParseExpression($"Expect.That({actual}).IsEquivalentTo({expected})");
		}

		return SyntaxFactory.ParseExpression($"Expect.That({actual}).IsEqualTo({expected})");
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

	private static async Task<ExpressionSyntax> Contains(CodeFixContext context,
		MemberAccessExpressionSyntax memberAccessExpressionSyntax, ArgumentSyntax? actual, ArgumentSyntax? expected)
	{
		SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync();

		ISymbol? symbol = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol;

		if (symbol is IMethodSymbol { Parameters.Length: 2, } methodSymbol &&
		    methodSymbol.Parameters[0].Type.Name == "IEnumerable" &&
		    methodSymbol.Parameters[1].Type.Name == "Predicate")
		{
			// Swap them - This overload is the other way around to the other ones.
			(actual, expected) = (expected, actual);
		}

		return SyntaxFactory.ParseExpression($"Expect.That({actual}).Contains({expected})");
	}

	private static string GetGenericArguments(ExpressionSyntax expressionSyntax)
	{
		if (expressionSyntax is GenericNameSyntax genericName)
		{
			return string.Join(", ", genericName.TypeArgumentList.Arguments.ToList());
		}

		return string.Empty;
	}
}
