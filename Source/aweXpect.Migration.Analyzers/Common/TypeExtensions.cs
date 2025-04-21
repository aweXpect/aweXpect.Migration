using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace aweXpect.Migration.Analyzers.Common;

internal static class TypeExtensions
{
	public static string GetFullNameWithoutGenericArity(this Type type)
	{
		string? name = type.FullName ?? type.Name;

		int index = name.IndexOf('`');

		return index == -1 ? name : name.Substring(0, index);
	}

	public static IEnumerable<ITypeSymbol> GetSelfAndBaseTypes(this ITypeSymbol namedTypeSymbol)
	{
		ITypeSymbol? type = namedTypeSymbol;

		while (type != null && type.SpecialType != SpecialType.System_Object)
		{
			yield return type;
			type = type.BaseType;
		}
	}

	public static string GloballyQualified(this ISymbol typeSymbol) =>
		typeSymbol.ToDisplayString(DisplayFormats.FullyQualifiedGenericWithGlobalPrefix);

	public static string GloballyQualifiedNonGeneric(this ISymbol typeSymbol) =>
		typeSymbol.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix);

	public static bool IsOrInherits(this ITypeSymbol namedTypeSymbol, ITypeSymbol typeSymbol) => namedTypeSymbol
		.GetSelfAndBaseTypes()
		.Any(x => SymbolEqualityComparer.Default.Equals(x, typeSymbol));
}
