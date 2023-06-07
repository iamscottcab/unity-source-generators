using Microsoft.CodeAnalysis;

namespace Scott.Cab.Generation.Common
{
    public static class SyntaxNodeExtensions
    {
        /// <summary>
        /// Returns a <see cref="ISymbol"/> based on the current <see cref="Compilation"/>.
        /// </summary>
        public static ISymbol GetSymbol(this SyntaxNode node, Compilation compilation)
        {
            if (node == null || compilation == null) return null;

            var semanticModel = compilation.GetSemanticModel(node.SyntaxTree);
            return semanticModel.GetDeclaredSymbol(node);
        }

        /// <summary>
        /// Returns a <see cref="ISymbol"/> based on the current <see cref="Compilation"/> casting it to the specified generic constraint.
        /// </summary>
        public static T GetSymbol<T>(this SyntaxNode node, Compilation compilation) where T : ISymbol
        {
            return (T)node.GetSymbol(compilation);
        }
    }
}
