using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Liteson.Merger
{
	public static class RoslynExtensions
	{
		public static ClassDeclarationSyntax FindClass(this SyntaxTree tree, string className)
		{
			var ns = tree.GetRoot().ChildNodes().First(i => i.IsKind(SyntaxKind.NamespaceDeclaration));
			return ns.ChildNodes().OfType<ClassDeclarationSyntax>().First(i => i.Identifier.ValueText == className);
		}

		public static SyntaxToken WithKind(this SyntaxToken token, SyntaxKind kind) => SyntaxFactory.Token(token.LeadingTrivia, kind, token.TrailingTrivia);

		public static SyntaxTokenList ToTokenList(this IEnumerable<SyntaxToken> tokens) => SyntaxFactory.TokenList(tokens);
		public static ClassDeclarationSyntax WithModifiers(this ClassDeclarationSyntax classSyntax, IEnumerable<SyntaxToken> tokens) => classSyntax.WithModifiers(tokens.ToTokenList());
	}
}