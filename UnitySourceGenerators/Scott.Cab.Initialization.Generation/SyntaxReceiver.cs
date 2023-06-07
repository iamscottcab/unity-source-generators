using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Scott.Cab.Initialization.Generation
{
    public class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Nodes { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not ClassDeclarationSyntax classDeclaration) return;

            Nodes.Add(classDeclaration);
        }
    }
}
