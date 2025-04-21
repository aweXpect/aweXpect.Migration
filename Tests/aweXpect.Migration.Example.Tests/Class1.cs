using Xunit;

namespace aweXpect.Migration.Example.Tests;

public class Class1
{
	[Fact]
	public void Test1()
	{
#pragma warning disable aweXpectM003
		Assert.True(true);
#pragma warning restore aweXpectM003
	}
}
