﻿using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace aweXpect.Migration.Example.Tests;

public class Class1
{
	[Fact]
	public void Test1()
	{
#pragma warning disable aweXpectM002
		true.Should().BeTrue();
#pragma warning restore aweXpectM002

#pragma warning disable aweXpectM003
		Assert.True(true);
#pragma warning restore aweXpectM003
	}

	[Fact]
	public async Task X()
	{
		int[] subject = [];
		await Expect.That(subject).All().Satisfy(x => x == 3);
	}
}
