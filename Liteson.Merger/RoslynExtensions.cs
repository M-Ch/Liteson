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
		public static SyntaxList<T> ToSyntaxList<T>(this IEnumerable<T> tokens) where T : SyntaxNode => SyntaxFactory.List(tokens);
		public static ClassDeclarationSyntax WithModifiers(this ClassDeclarationSyntax classSyntax, IEnumerable<SyntaxToken> tokens) => classSyntax.WithModifiers(tokens.ToTokenList());
		public static EnumDeclarationSyntax WithModifiers(this EnumDeclarationSyntax enumSyntax, IEnumerable<SyntaxToken> tokens) => enumSyntax.WithModifiers(tokens.ToTokenList());
		public static StructDeclarationSyntax WithModifiers(this StructDeclarationSyntax structSyntax, IEnumerable<SyntaxToken> tokens) => structSyntax.WithModifiers(tokens.ToTokenList());
		public static InterfaceDeclarationSyntax WithModifiers(this InterfaceDeclarationSyntax interfaceSyntax, IEnumerable<SyntaxToken> tokens) => interfaceSyntax.WithModifiers(tokens.ToTokenList());
	}
}