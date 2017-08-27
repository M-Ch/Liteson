using System;
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
			var resultNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Liteson"));

			void MergeFile(string filePath)
			{
				var catalog = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
				var ns = catalog.GetRoot().ChildNodes().First(i => i.Kind() == SyntaxKind.NamespaceDeclaration);

				foreach (var node in ns.ChildNodes().OfType<BaseTypeDeclarationSyntax>())
				{
					var modified = RewriteAccess(node);
					usings.AddRange(catalog.GetRoot().ChildNodes().OfType<UsingDirectiveSyntax>());
					if(modified.Modifiers.Any(i => i.IsKind(SyntaxKind.InternalKeyword)))
						resultNs = resultNs.AddMembers(modified);
					else
						jsonConvert = jsonConvert.AddMembers(modified);
				}
			}

			foreach(var file in Directory.EnumerateFiles(litesonDir, "*.cs").Where(i => Path.GetFileNameWithoutExtension(i) != "JsonConvert"))
				MergeFile(file);

			var filteredUsings = usings.DistinctBy(i => i.ToFullString()).OrderByDescending(i => i.ToFullString()).ToList();
			var newNs = resultNs.WithMembers(new[] { RewriteAccess(jsonConvert) }.Concat(resultNs.Members).ToSyntaxList());
			var result = SyntaxFactory.CompilationUnit(
				SyntaxFactory.List<ExternAliasDirectiveSyntax>(),
				SyntaxFactory.List(filteredUsings),
				SyntaxFactory.List<AttributeListSyntax>(),
				SyntaxFactory.List<MemberDeclarationSyntax>(new[] {newNs}));

			var formatted = Formatter.Format(result, new AdhocWorkspace());
			var text = formatted.ToFullString();
			var license = File.ReadAllLines(Path.Combine(slnDir, "License.txt"));
			license[0] = "/*" + license[0];
			license[license.Length - 1] = license.Last() + "*/";

			var fullText = string.Join(Environment.NewLine, license) + Environment.NewLine + text.Replace("\t", "    ");
			File.WriteAllText(Path.Combine(slnDir, "Liteson.merged.cs"), fullText);
		}

		private static BaseTypeDeclarationSyntax RewriteAccess(BaseTypeDeclarationSyntax declaration)
		{
			switch (declaration)
			{
				case ClassDeclarationSyntax classSyntax: return classSyntax.WithModifiers(classSyntax.Modifiers.Select(RewriteModifier));
				case EnumDeclarationSyntax enumSyntax: return enumSyntax.WithModifiers(enumSyntax.Modifiers.Select(RewriteModifier));
				case StructDeclarationSyntax structSyntax: return structSyntax.WithModifiers(structSyntax.Modifiers.Select(RewriteModifier));
				case InterfaceDeclarationSyntax interfaceSyntax: return interfaceSyntax.WithModifiers(interfaceSyntax.Modifiers.Select(RewriteModifier));
				default: throw new Exception($"Not supported top level syntax {declaration.GetType().Name}");
			}
		}


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
