using BenchmarkDotNet.Attributes;

namespace aweXpect.Migration.Benchmarks;

/// <summary>
///     This is a dummy benchmark in the Migration template.
/// </summary>
public partial class HappyCaseBenchmarks
{
	[Benchmark]
	public TimeSpan Dummy_aweXpect()
		=> TimeSpan.FromSeconds(10);
}
