namespace aweXpect.Migration.Tests.FluentAssertions;

public static class TestCases
{
	/// <summary>
	///     <see href="https://fluentassertions.com/basicassertions/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Basic()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("object subject = new object();",
			"subject.Should().BeNull()",
			"Expect.That(subject).IsNull()");
		theoryData.AddWithBecause("object subject = new object();",
			"subject.Should().NotBeNull()",
			"Expect.That(subject).IsNotNull()");
		theoryData.AddWithBecause("object subject = new object();object expected = new object();",
			"subject.Should().BeSameAs(expected, {0})",
			"Expect.That(subject).IsSameAs(expected)");
		theoryData.AddWithBecause("object subject = new object();object unexpected = new object();",
			"subject.Should().NotBeSameAs(unexpected, {0})",
			"Expect.That(subject).IsNotSameAs(unexpected)");
		theoryData.AddWithBecause("object subject = new Exception();",
			"subject.Should().BeAssignableTo<ArgumentException>({0})",
			"Expect.That(subject).Is<ArgumentException>()");
		theoryData.AddWithBecause("object subject = new Exception();",
			"subject.Should().BeAssignableTo(typeof(ArgumentException), {0})",
			"Expect.That(subject).Is(typeof(ArgumentException))");
		theoryData.AddWithBecause("object subject = new Exception();",
			"subject.Should().NotBeAssignableTo<ArgumentException>({0})",
			"Expect.That(subject).IsNot<ArgumentException>()");
		theoryData.AddWithBecause("object subject = new Exception();",
			"subject.Should().NotBeAssignableTo(typeof(ArgumentException), {0})",
			"Expect.That(subject).IsNot(typeof(ArgumentException))");
		theoryData.AddWithBecause("object subject = new Exception();",
			"subject.Should().BeOfType<ArgumentException>({0})",
			"Expect.That(subject).IsExactly<ArgumentException>()");
		theoryData.AddWithBecause("object subject = new Exception();",
			"subject.Should().BeOfType(typeof(ArgumentException), {0})",
			"Expect.That(subject).IsExactly(typeof(ArgumentException))");
		theoryData.AddWithBecause("object subject = new Exception();",
			"subject.Should().NotBeOfType<ArgumentException>({0})",
			"Expect.That(subject).IsNotExactly<ArgumentException>()");
		theoryData.AddWithBecause("object subject = new Exception();",
			"subject.Should().NotBeOfType(typeof(ArgumentException), {0})",
			"Expect.That(subject).IsNotExactly(typeof(ArgumentException))");
		return theoryData;
	}

	/// <summary>
	///     <see href="https://fluentassertions.com/booleans/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Boolean()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("bool subject = false;",
			"subject.Should().BeTrue({0})",
			"Expect.That(subject).IsTrue()");
		theoryData.AddWithBecause("bool subject = false;",
			"subject.Should().BeFalse({0})",
			"Expect.That(subject).IsFalse()");
		theoryData.AddWithBecause("bool subject = false;bool expected = false;",
			"subject.Should().Imply(expected, {0})",
			"Expect.That(subject).Implies(expected)");
		theoryData.AddWithBecause("bool? subject = false;",
			"subject.Should().NotBeTrue({0})",
			"Expect.That(subject).IsNotTrue()");
		theoryData.AddWithBecause("bool? subject = null;",
			"subject.Should().NotBeFalse({0})",
			"Expect.That(subject).IsNotFalse()");
		return theoryData;
	}

	/// <summary>
	///     <see href="https://fluentassertions.com/collections/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Collection()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("int[] subject = [1, 2,];",
			"subject.Should().HaveCount(1, {0})",
			"Expect.That(subject).HasCount(1)");
		theoryData.AddWithBecause("int[] subject = [1, 2,];",
			"subject.Should().OnlyContain(x => x > 0, {0})",
			"Expect.That(subject).All().Satisfy(x => x > 0)");
		theoryData.AddWithBecause("object[] subject = [];",
			"subject.Should().AllBeAssignableTo<ArgumentException>({0})",
			"Expect.That(subject).All().Are<ArgumentException>()");
		theoryData.AddWithBecause("object[] subject = [];",
			"subject.Should().AllBeAssignableTo(typeof(ArgumentException), {0})",
			"Expect.That(subject).All().Are(typeof(ArgumentException))");
		theoryData.AddWithBecause("object[] subject = [];",
			"subject.Should().AllBeOfType<ArgumentException>({0})",
			"Expect.That(subject).All().AreExactly<ArgumentException>()");
		theoryData.AddWithBecause("object[] subject = [];",
			"subject.Should().AllBeOfType(typeof(ArgumentException), {0})",
			"Expect.That(subject).All().AreExactly(typeof(ArgumentException))");
		return theoryData;
	}

	/// <summary>
	///     <see href="https://fluentassertions.com/collections/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Enums()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("DayOfWeek subject = DayOfWeek.Monday;",
			"subject.Should().BeDefined({0})",
			"Expect.That(subject).IsDefined()");
		theoryData.AddWithBecause("DayOfWeek subject = DayOfWeek.Monday;",
			"subject.Should().NotBeDefined({0})",
			"Expect.That(subject).IsNotDefined()");
		theoryData.AddWithBecause("DayOfWeek? subject = DayOfWeek.Monday;",
			"subject.Should().HaveValue({0})",
			"Expect.That(subject).IsNotNull()");
		theoryData.AddWithBecause("DayOfWeek? subject = DayOfWeek.Monday;",
			"subject.Should().HaveValue(1, {0})",
			"Expect.That(subject).HasValue(1)");
		theoryData.AddWithBecause("DayOfWeek? subject = DayOfWeek.Monday;",
			"subject.Should().NotHaveValue({0})",
			"Expect.That(subject).IsNull()");
		theoryData.AddWithBecause("DayOfWeek? subject = DayOfWeek.Monday;",
			"subject.Should().NotHaveValue(2, {0})",
			"Expect.That(subject).DoesNotHaveValue(2)");
		theoryData.AddWithBecause("DayOfWeek subject = DayOfWeek.Monday;",
			"subject.Should().HaveFlag(DayOfWeek.Tuesday, {0})",
			"Expect.That(subject).HasFlag(DayOfWeek.Tuesday)");
		theoryData.AddWithBecause("DayOfWeek subject = DayOfWeek.Monday;",
			"subject.Should().NotHaveFlag(DayOfWeek.Tuesday, {0})",
			"Expect.That(subject).DoesNotHaveFlag(DayOfWeek.Tuesday)");
		return theoryData;
	}

	/// <summary>
	///     <see href="https://fluentassertions.com/exceptions/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Equivalency()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("object subject = new object();object expected = new object();",
			"subject.Should().BeEquivalentTo(expected, {0})",
			"Expect.That(subject).IsEquivalentTo(expected)");
		theoryData.AddWithBecause("byte[] subject = [];byte[] expected = [];",
			"subject.Should().BeEquivalentTo(expected, {0})",
			"Expect.That(subject).IsEqualTo(expected)");
		theoryData.AddWithBecause("IEnumerable<string> subject = [];string[] expected = [];",
			"subject.Should().BeEquivalentTo(expected, o => o.WithStrictOrdering(), {0})",
			"Expect.That(subject).IsEqualTo(expected)");
		theoryData.AddWithBecause("AggregateException subject = new(); Exception[] expected = [];",
			"subject.InnerExceptions.Should().BeEquivalentTo(expected, o => o.WithStrictOrdering(), {0})",
			"Expect.That(subject.InnerExceptions).IsEqualTo(expected)");
		theoryData.AddWithBecause("int[] subject = [];int[] expected = [];",
			"subject.Should().BeEquivalentTo(expected, o => o.WithoutStrictOrdering(), {0})",
			"Expect.That(subject).IsEqualTo(expected).InAnyOrder()");
		theoryData.AddWithBecause("object subject = new object();object unexpected = new object();",
			"subject.Should().NotBeEquivalentTo(unexpected, {0})",
			"Expect.That(subject).IsNotEquivalentTo(unexpected)");
		theoryData.AddWithBecause("int[] subject = [];int[] unexpected = [];",
			"subject.Should().NotBeEquivalentTo(unexpected, {0})",
			"Expect.That(subject).IsNotEqualTo(unexpected)");
		return theoryData;
	}

	/// <summary>
	///     <see href="https://fluentassertions.com/exceptions/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Exceptions()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("Action callback = () => {};",
			"callback.Should().NotThrow({0})",
			"Expect.That(callback).DoesNotThrow()");
		theoryData.AddWithBecause("Func<Task> callback = () => Task.CompletedTask;",
			"callback.Should().NotThrowAsync({0})",
			"Expect.That(callback).DoesNotThrow()",
			true);
		theoryData.AddWithBecause("Action callback = () => {};",
			"callback.Should().Throw<ArgumentException>({0})",
			"Expect.That(callback).Throws<ArgumentException>()");
		theoryData.AddWithBecause("Action callback = () => {};",
			"callback.Should().ThrowExactly<ArgumentException>({0})",
			"Expect.That(callback).ThrowsExactly<ArgumentException>()");
		theoryData.AddWithBecause("Func<Task> callback = () => Task.CompletedTask;",
			"callback.Should().ThrowAsync<ArgumentException>({0})",
			"Expect.That(callback).Throws<ArgumentException>()",
			true);
		theoryData.AddWithBecause("Func<Task> callback = () => Task.CompletedTask;",
			"callback.Should().ThrowExactlyAsync<ArgumentException>({0})",
			"Expect.That(callback).ThrowsExactly<ArgumentException>()",
			true);
		return theoryData;
	}

	/// <summary>
	///     <see href="https://fluentassertions.com/numerictypes/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Numbers()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("int subject = 1;",
			"subject.Should().BePositive({0})",
			"Expect.That(subject).IsPositive()");
		theoryData.AddWithBecause("int subject = 1;",
			"subject.Should().BeNegative({0})",
			"Expect.That(subject).IsNegative()");
		theoryData.AddWithBecause("int subject = 1;int expected = 2;",
			"subject.Should().BeGreaterThan(expected, {0})",
			"Expect.That(subject).IsGreaterThan(expected)");
		theoryData.AddWithBecause("int subject = 1;int expected = 2;",
			"subject.Should().BeGreaterThanOrEqualTo(expected, {0})",
			"Expect.That(subject).IsGreaterThanOrEqualTo(expected)");
		theoryData.AddWithBecause("int subject = 1;int expected = 2;",
			"subject.Should().BeLessThan(expected, {0})",
			"Expect.That(subject).IsLessThan(expected)");
		theoryData.AddWithBecause("int subject = 1;int expected = 2;",
			"subject.Should().BeLessThanOrEqualTo(expected, {0})",
			"Expect.That(subject).IsLessThanOrEqualTo(expected)");
		theoryData.AddWithBecause("double subject = 1.1;double expected = 1.0;double tolerance = 0.05;",
			"subject.Should().BeApproximately(expected, tolerance, {0})",
			"Expect.That(subject).IsEqualTo(expected).Within(tolerance)");
		theoryData.AddWithBecause("int subject = 1;int[] expected = [2, 3,];",
			"subject.Should().BeOneOf(expected, {0})",
			"Expect.That(subject).IsOneOf(expected)");
		theoryData.AddWithBecause("int subject = 1;",
			"subject.Should().BeOneOf(2, 3, 4)",
			"Expect.That(subject).IsOneOf(2, 3, 4)");
		return theoryData;
	}

	/// <summary>
	///     Legacy expectations on fluentassertions 7.2.x
	/// </summary>
	public static TheoryData<string, string, string, bool> Legacy()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("int subject = 1;int expected = 2;",
			"subject.Should().BeLessOrEqualTo(expected, {0})",
			"Expect.That(subject).IsLessThanOrEqualTo(expected)");
		theoryData.AddWithBecause("int subject = 1;int expected = 2;",
			"subject.Should().BeGreaterOrEqualTo(expected, {0})",
			"Expect.That(subject).IsGreaterThanOrEqualTo(expected)");
		return theoryData;
	}

	/// <summary>
	///     <see href="https://fluentassertions.com/datetimespans/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Chronology()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime expected = DateTime.Now;",
			"subject.Should().BeAfter(expected, {0})",
			"Expect.That(subject).IsAfter(expected)");
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime expected = DateTime.Now;",
			"subject.Should().BeOnOrAfter(expected, {0})",
			"Expect.That(subject).IsOnOrAfter(expected)");
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime expected = DateTime.Now;",
			"subject.Should().BeBefore(expected, {0})",
			"Expect.That(subject).IsBefore(expected)");
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime expected = DateTime.Now;",
			"subject.Should().BeOnOrBefore(expected, {0})",
			"Expect.That(subject).IsOnOrBefore(expected)");
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime unexpected = DateTime.Now;",
			"subject.Should().NotBeAfter(unexpected, {0})",
			"Expect.That(subject).IsNotAfter(unexpected)");
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime unexpected = DateTime.Now;",
			"subject.Should().NotBeOnOrAfter(unexpected, {0})",
			"Expect.That(subject).IsNotOnOrAfter(unexpected)");
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime unexpected = DateTime.Now;",
			"subject.Should().NotBeBefore(unexpected, {0})",
			"Expect.That(subject).IsNotBefore(unexpected)");
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime unexpected = DateTime.Now;",
			"subject.Should().NotBeOnOrBefore(unexpected, {0})",
			"Expect.That(subject).IsNotOnOrBefore(unexpected)");
		theoryData.AddWithBecause("DateTime subject = DateTime.Now;DateTime[] expected = [DateTime.Now,];",
			"subject.Should().BeOneOf(expected, {0})",
			"Expect.That(subject).IsOneOf(expected)");
		return theoryData;
	}

	/// <summary>
	///     <see href="https://fluentassertions.com/strings/" />
	/// </summary>
	public static TheoryData<string, string, string, bool> Strings()
	{
		TheoryData<string, string, string, bool> theoryData = new();
		theoryData.AddWithBecause("string subject = \"foo\";",
			"subject.Should().BeNull({0})",
			"Expect.That(subject).IsNull()");
		theoryData.AddWithBecause("string subject = \"foo\";",
			"subject.Should().NotBeNull({0})",
			"Expect.That(subject).IsNotNull()");
		theoryData.AddWithBecause("string subject = \"foo\";",
			"subject.Should().BeEmpty({0})",
			"Expect.That(subject).IsEmpty()");
		theoryData.AddWithBecause("string subject = \"foo\";",
			"subject.Should().NotBeEmpty({0})",
			"Expect.That(subject).IsNotEmpty()");
		theoryData.AddWithBecause("string subject = \"foo\";",
			"subject.Should().BeNullOrEmpty({0})",
			"Expect.That(subject).IsNullOrEmpty()");
		theoryData.AddWithBecause("string subject = \"foo\";",
			"subject.Should().NotBeNullOrEmpty({0})",
			"Expect.That(subject).IsNotNullOrEmpty()");
		theoryData.AddWithBecause("string subject = \"foo\";",
			"subject.Should().BeNullOrWhiteSpace({0})",
			"Expect.That(subject).IsNullOrWhiteSpace()");
		theoryData.AddWithBecause("string subject = \"foo\";",
			"subject.Should().NotBeNullOrWhiteSpace({0})",
			"Expect.That(subject).IsNotNullOrWhiteSpace()");
		theoryData.AddWithBecause("string subject = \"foo\";string expected = \"bar\";",
			"subject.Should().Be(expected, {0})",
			"Expect.That(subject).IsEqualTo(expected)");
		theoryData.AddWithBecause("string subject = \"foo\";string unexpected = \"bar\";",
			"subject.Should().NotBe(unexpected, {0})",
			"Expect.That(subject).IsNotEqualTo(unexpected)");
		theoryData.AddWithBecause("string subject = \"foo\";string expected = \"bar\";",
			"subject.Should().Contain(expected, {0})",
			"Expect.That(subject).Contains(expected)");
		theoryData.AddWithBecause("string subject = \"foo\";string unexpected = \"bar\";",
			"subject.Should().NotContain(unexpected, {0})",
			"Expect.That(subject).DoesNotContain(unexpected)");
		theoryData.AddWithBecause("string subject = \"foo\";string expected = \"bar\";",
			"subject.Should().StartWith(expected, {0})",
			"Expect.That(subject).StartsWith(expected)");
		theoryData.AddWithBecause("string subject = \"foo\";string unexpected = \"bar\";",
			"subject.Should().NotStartWith(unexpected, {0})",
			"Expect.That(subject).DoesNotStartWith(unexpected)");
		theoryData.AddWithBecause("string subject = \"foo\";string expected = \"bar\";",
			"subject.Should().EndWith(expected, {0})",
			"Expect.That(subject).EndsWith(expected)");
		theoryData.AddWithBecause("string subject = \"foo\";string unexpected = \"bar\";",
			"subject.Should().NotEndWith(unexpected, {0})",
			"Expect.That(subject).DoesNotEndWith(unexpected)");
		theoryData.AddWithBecause("string subject = \"foo\";string expected = \"bar\";",
			"subject.Should().BeEquivalentTo(expected, o => o.IgnoringCase(), {0})",
			"Expect.That(subject).IsEqualTo(expected).IgnoringCase()");
		theoryData.AddWithBecause("string subject = \"foo\";string expected = \"bar\";",
			"subject.Should().BeEquivalentTo(expected, o => o.IgnoringLeadingWhitespace(), {0})",
			"Expect.That(subject).IsEqualTo(expected).IgnoringLeadingWhiteSpace()");
		theoryData.AddWithBecause("string subject = \"foo\";string expected = \"bar\";",
			"subject.Should().BeEquivalentTo(expected, o => o.IgnoringTrailingWhitespace(), {0})",
			"Expect.That(subject).IsEqualTo(expected).IgnoringTrailingWhiteSpace()");
		theoryData.AddWithBecause("string subject = \"foo\";string expected = \"bar\";",
			"subject.Should().BeEquivalentTo(expected, o => o.IgnoringNewlineStyle(), {0})",
			"Expect.That(subject).IsEqualTo(expected).IgnoringNewlineStyle()");
		return theoryData;
	}

	private static void AddWithBecause(this TheoryData<string, string, string, bool> theoryData,
		string arrange,
		string actual,
		string expected,
		bool isAsync = false)
	{
		if (actual.Contains("({0})"))
		{
			theoryData.Add(
				string.Format(actual, ""),
				expected,
				arrange, isAsync);
			theoryData.Add(
				string.Format(actual, "\"because foo\""),
				expected + ".Because(\"because foo\")",
				arrange, isAsync);
		}
		else if (actual.Contains(", {0})") || actual.Contains(",{0})"))
		{
			theoryData.Add(
				actual.Replace(", {0})", ")").Replace(",{0})", ")"),
				expected,
				arrange, isAsync);
			theoryData.Add(
				string.Format(actual, "\"because foo\""),
				expected + ".Because(\"because foo\")",
				arrange, isAsync);
		}
		else
		{
			theoryData.Add(
				actual,
				expected,
				arrange, isAsync);
		}
	}
}
