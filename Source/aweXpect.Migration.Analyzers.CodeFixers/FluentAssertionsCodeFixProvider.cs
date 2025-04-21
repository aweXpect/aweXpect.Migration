using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace aweXpect.Migration.Analyzers;

/// <summary>
///     A code fix provider that migrates most assertions from fluentassertions to aweXpect.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FluentAssertionsCodeFixProvider))]
[Shared]
public class FluentAssertionsCodeFixProvider : CodeFixProvider
{
	/// <inheritdoc />
	public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [Rules.FluentAssertionsRule.Id,];

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
					Resources.aweXpectM002CodeFixTitle,
					c => ConvertAssertionAsync(context, expressionSyntax, c),
					nameof(FluentAssertionsCodeFixProvider)),
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

		ExpressionOrPatternSyntax topmostExpression = expressionSyntax.Expression;
		while (topmostExpression.Parent is ExpressionOrPatternSyntax parent)
		{
			topmostExpression = parent;
		}

		ArgumentSyntax? expected = expressionSyntax.ArgumentList.Arguments.ElementAtOrDefault(0);

		ExpressionSyntaxWalker walker = new();
		walker.Visit(expressionSyntax);
		string? actual = walker.SubjectText;

		string? methodName = memberAccessExpressionSyntax.Name.Identifier.ValueText;

		string? genericArgs = GetGenericArguments(memberAccessExpressionSyntax.Name);

		ExpressionSyntax? newExpression = GetNewExpression(context, memberAccessExpressionSyntax, methodName,
			actual, expected, genericArgs, expressionSyntax.ArgumentList.Arguments);

		if (newExpression != null)
		{
			compilationUnit =
				compilationUnit.ReplaceNode(expressionSyntax, newExpression.WithTriviaFrom(expressionSyntax));
		}

		return document.WithSyntaxRoot(compilationUnit);
	}

	private static ExpressionSyntax? GetNewExpression(CodeFixContext context,
		MemberAccessExpressionSyntax memberAccessExpressionSyntax, string method,
		string? actual, ArgumentSyntax? expected, string genericArgs,
		SeparatedSyntaxList<ArgumentSyntax> argumentListArguments)
	{
		bool isGeneric = !string.IsNullOrEmpty(genericArgs);

		return method switch
		{
			"Be" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsEqualTo({expected})"),
			"NotBe" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNotEqualTo({expected})"),
			"Contain" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).Contains({expected})"),
			"NotContain" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).DoesNotContain({expected})"),
			"StartWith" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).StartsWith({expected})"),
			"EndWith" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).EndsWith({expected})"),
			"BeEmpty" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsEmpty()"),
			"NotBeEmpty" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNotEmpty()"),
			"NotBeNull" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNotNull()"),
			"BeNull" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNull()"),
			"BeTrue" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsTrue()"),
			"BeFalse" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsFalse()"),
			"BeSameAs" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsSameAs({expected})"),
			"NotBeSameAs" => SyntaxFactory.ParseExpression(
				$"Expect.That({actual}).IsNotSameAs({expected})"),
			"BeAssignableTo" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).Is<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).Is({expected})"),
			"NotBeAssignableTo" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsNot<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsNot({expected})"),
			"BeOfType" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsExactly<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsExactly({expected})"),
			"NotBeOfType" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsNotExactly<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).IsNotExactly({expected})"),
			"Throw" or "ThrowAsync" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).Throws<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).Throws({expected})"),
			"ThrowExactly" or "ThrowExactlyAsync" => isGeneric
				? SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).ThrowsExactly<{genericArgs}>()")
				: SyntaxFactory.ParseExpression(
					$"Expect.That({actual}).ThrowsExactly({expected})"),
			_ => null,
		};
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
		public string? SubjectText { get; private set; }

		public override void Visit(SyntaxNode node)
		{
			if (_isShould && node is not ParenthesizedExpressionSyntax)
			{
				SubjectText ??= node.ToString();
				return;
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
