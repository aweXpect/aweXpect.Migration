using Microsoft.CodeAnalysis.Diagnostics;

namespace aweXpect.Migration.Analyzers.Common
{
	/// <summary>
	///     A concurrent <see cref="DiagnosticAnalyzer" />.
	/// </summary>
	public abstract class ConcurrentDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		/// <inheritdoc />
		public sealed override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			InitializeInternal(context);
		}

		/// <summary>
		///     Called once at session start to register actions in the analysis context.
		/// </summary>
		protected abstract void InitializeInternal(AnalysisContext context);
	}
}
