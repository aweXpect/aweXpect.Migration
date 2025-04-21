using System.Collections.Immutable;
using aweXpect.Migration.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace aweXpect.Migration.Xunit;

/// <summary>
///     An analyzer that flags most xunit assertions.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class XUnitAssertionAnalyzer : ConcurrentDiagnosticAnalyzer
{
	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
		ImmutableArray.Create(Rules.XUnitAssertionRule);

	/// <inheritdoc />
	protected override void InitializeInternal(AnalysisContext context)
	{
		context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
	}
    
	private void AnalyzeOperation(OperationAnalysisContext context)
	{
		if (context.Operation is not IInvocationOperation invocationOperation)
		{
			return;
		}

		var methodSymbol = invocationOperation.TargetMethod;

		var fullyQualifiedNonGenericMethodName = methodSymbol.GloballyQualifiedNonGeneric();

		if (fullyQualifiedNonGenericMethodName.StartsWith("global::Xunit.Assert."))
		{
			context.ReportDiagnostic(
				Diagnostic.Create(Rules.XUnitAssertionRule, context.Operation.Syntax.GetLocation())
			);   
		}
	}
}
