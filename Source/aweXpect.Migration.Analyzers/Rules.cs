using Microsoft.CodeAnalysis;

namespace aweXpect.Migration.Analyzers;

internal static class Rules
{
	private const string UsageCategory = "Usage";

	public static readonly DiagnosticDescriptor FluentAssertionsRule =
		CreateDescriptor("aweXpectM002", UsageCategory, DiagnosticSeverity.Warning);

	public static readonly DiagnosticDescriptor XUnitAssertionRule =
		CreateDescriptor("aweXpectM003", UsageCategory, DiagnosticSeverity.Warning);


	private static DiagnosticDescriptor CreateDescriptor(string diagnosticId, string category,
		DiagnosticSeverity severity) => new(
		diagnosticId,
		new LocalizableResourceString(diagnosticId + "Title",
			Resources.ResourceManager, typeof(Resources)),
		new LocalizableResourceString(diagnosticId + "MessageFormat", Resources.ResourceManager,
			typeof(Resources)),
		category,
		severity,
		true,
		new LocalizableResourceString(diagnosticId + "Description", Resources.ResourceManager,
			typeof(Resources))
	);
}
