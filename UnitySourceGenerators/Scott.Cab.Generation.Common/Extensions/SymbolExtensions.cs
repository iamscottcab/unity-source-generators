using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scott.Cab.Generation.Common
{
    public static class SymbolExtensions
    {
        /// <summary>
        /// Returns whether the given symbol has the supplied attribute applied directly to it.
        /// </summary>
        public static bool HasAttribute(this INamedTypeSymbol symbol, INamedTypeSymbol targetAttribute)
        {
            if (symbol == null) return false;

            return symbol.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, targetAttribute));
        }

        /// <summary>
        /// Returns whether the given symbol has the supplied attribute applied directly to it, or in any of its base classes.
        /// </summary>
        public static bool HasAttributeRecursive(this INamedTypeSymbol symbol, INamedTypeSymbol targetAttribute, out INamedTypeSymbol matchedSymbol)
        {
            matchedSymbol = symbol;
            
            if (symbol == null) return false;

            if (symbol.HasAttribute(targetAttribute)) return true;

            return symbol.BaseType.HasAttributeRecursive(targetAttribute, out matchedSymbol);
        }

        /// <summary>
        /// Returns all fields of the given symbol.
        /// </summary>
        public static IReadOnlyCollection<IFieldSymbol> GetFields(this INamespaceOrTypeSymbol symbol)
        {
            if (symbol == null) return Array.Empty<IFieldSymbol>();

            List<IFieldSymbol> fields = new();

            foreach (var member in symbol.GetMembers())
            {
                if (member is not IFieldSymbol fieldSymbol) continue;

                fields.Add(fieldSymbol);
            }

            return fields;
        }

        /// <summary>
        /// Returns a string for use in a source generated template as follows:
        /// <accessibility> partial class <name><generic, type, arguments>
        /// </summary>
        public static string GetTemplateClassDefinition(this INamedTypeSymbol symbol)
        {
            var genericTypeArguments = "";

            if (symbol.IsGenericType)
            {
                genericTypeArguments = $"<{string.Join(",", symbol.TypeArguments.Select(x => x.Name))}>";
            }

            return $"{symbol.DeclaredAccessibility.ToString().ToLower()} partial class {symbol.Name}{genericTypeArguments}";
        }
    }
}
