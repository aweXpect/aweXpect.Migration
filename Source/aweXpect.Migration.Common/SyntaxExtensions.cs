using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace aweXpect.Migration.Common;

internal static class SyntaxExtensions
{
	public static IEnumerable<TOutput> GetAllAncestorSyntaxesOfType<TOutput>(this SyntaxNode input) 
		where TOutput : SyntaxNode
	{
		var parent = input.Parent;
        
		while (parent != null)
		{
			if (parent is TOutput output)
			{
				yield return output;
			}
            
			parent = parent.Parent;
		}
	}
    
	public static TOutput? GetAncestorSyntaxOfType<TOutput>(this SyntaxNode input) 
		where TOutput : SyntaxNode
	{
		var parent = input.Parent;
        
		while (parent != null && parent is not TOutput)
		{
			parent = parent.Parent;
		}

		return parent as TOutput;
	}
        
	public static IEnumerable<IOperation> GetAncestorOperations(this IOperation operation) 
	{
		var parent = operation.Parent;
        
		while (parent != null)
		{
			yield return parent;
			parent = parent.Parent;
		}
	}
}
