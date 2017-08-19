using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Liteson.Merger
{
	class Program
	{
		static void Main()
		{
			//should be run from default output directory
			var slnDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
			var litesonDir = Path.Combine(slnDir, "Liteson");
			var main = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(litesonDir, "JsonConvert.cs")));
			var jsonConvert = main.FindClass("JsonConvert");
			var unit = (CompilationUnitSyntax)main.GetRoot();
			var usings = unit.ChildNodes().OfType<UsingDirectiveSyntax>().ToList();

			void MergeFile(string filePath)
			{
				var catalog = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
				var ns = catalog.GetRoot().ChildNodes().First(i => i.Kind() == SyntaxKind.NamespaceDeclaration);

				foreach (var node in ns.ChildNodes().OfType<ClassDeclarationSyntax>())
				{
					var modified = RewriteAccess(node);
					usings.AddRange(catalog.GetRoot().ChildNodes().OfType<UsingDirectiveSyntax>());
					jsonConvert = jsonConvert.AddMembers(modified);
				}
			}

			foreach(var file in Directory.EnumerateFiles(litesonDir, "*.cs").Where(i => Path.GetFileNameWithoutExtension(i) != "JsonConvert"))
				MergeFile(file);

			var filteredUsings = usings.DistinctBy(i => i.ToFullString()).OrderByDescending(i => i.ToFullString()).ToList();
			var newNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Liteson")).AddMembers(RewriteAccess(jsonConvert));
			var result = SyntaxFactory.CompilationUnit(
				SyntaxFactory.List<ExternAliasDirectiveSyntax>(),
				SyntaxFactory.List(filteredUsings),
				SyntaxFactory.List<AttributeListSyntax>(),
				SyntaxFactory.List<MemberDeclarationSyntax>(new[] {newNs}));

			var formatted = Formatter.Format(result, new AdhocWorkspace());
			var text = formatted.ToFullString();
			File.WriteAllText(Path.Combine(slnDir, "Liteson.merged.cs"), text);
		}

		private static ClassDeclarationSyntax RewriteAccess(ClassDeclarationSyntax classDeclaration) 
			=> classDeclaration.WithModifiers(classDeclaration.Modifiers.Select(RewriteModifier));

		private static SyntaxToken RewriteModifier(SyntaxToken token)
		{
			if (token.IsKind(SyntaxKind.PublicKeyword))
				return token.WithKind(SyntaxKind.InternalKeyword);

			return token.IsKind(SyntaxKind.InternalKeyword)
				? token.WithKind(SyntaxKind.PrivateKeyword)
				: token;
		}
	}
}
