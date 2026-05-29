using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Omnicasa.Analyzers.Rules
{
    /// <summary>
    /// CSharp analyzer to ensure classes are sealed by default.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DesignedInheritanceAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <seealso cref="DesignedInheritanceAnalyzer"/> analyzer.
        /// </summary>
        public const string AmbiguousDiagnosticId = "MOBILE_RULE_0002";

        /// <summary>
        /// The ID for diagnostics produced by the <seealso cref="DesignedInheritanceAnalyzer"/> analyzer.
        /// </summary>
        public const string RedundantDiagnosticId = "MOBILE_RULE_0003";

        private static readonly DiagnosticDescriptor AmbiguousDescriptor = new (
            AmbiguousDiagnosticId,
            "Type should be explicitly designed to support inheritance",
            "The {0} type should be explicitly designed to support inheritance",
            "MOBILE",
            DiagnosticSeverity.Info,
            true,
            helpLinkUri: "https://github.com/energyworldnet/Ewn.Analyzers/blob/main/docs/EWN0002.md");

        private static readonly DiagnosticDescriptor RedundantDescriptor = new (
            RedundantDiagnosticId,
            "Inheritability of type is already explicitly defined",
            "Inheritability of the {0} type is already explicitly defined",
            "MOBILEOMNI",
            DiagnosticSeverity.Warning,
            true,
            helpLinkUri: "https://github.com/energyworldnet/Ewn.Analyzers/blob/main/docs/EWN0003.md");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(AmbiguousDescriptor, RedundantDescriptor);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                AnalyzeTypeSyntax, SyntaxKind.ClassDeclaration, SyntaxKind.RecordDeclaration);
        }

        private static bool IsDesignedInheritanceAttribute(AttributeSyntax attributeSyntax) =>
            attributeSyntax.Name.ToString() == DesignedInheritanceConstants.AttributeName;

        private static void AnalyzeTypeSyntax(SyntaxNodeAnalysisContext context)
        {
            var typeSyntax = (TypeDeclarationSyntax)context.Node;
            var hasInheritabilityModifier =
                typeSyntax.Modifiers.Any(SyntaxKind.SealedKeyword) ||
                typeSyntax.Modifiers.Any(SyntaxKind.AbstractKeyword) ||
                typeSyntax.Modifiers.Any(SyntaxKind.StaticKeyword);
            var designedInheritanceAttributes = typeSyntax.AttributeLists
                .SelectMany(al => al.Attributes.Where(IsDesignedInheritanceAttribute))
                .ToImmutableList();

            if (hasInheritabilityModifier ^ designedInheritanceAttributes.Count > 0)
            {
                return;
            }

            if (hasInheritabilityModifier)
            {
                foreach (var designedInheritanceAttribute in designedInheritanceAttributes)
                {
                    var diagnostic = Diagnostic.Create(
                        RedundantDescriptor,
                        designedInheritanceAttribute.GetLocation(),
                        typeSyntax.Identifier.Text);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else
            {
                var diagnostic = Diagnostic.Create(
                    AmbiguousDescriptor,
                    typeSyntax.Identifier.GetLocation(),
                    typeSyntax.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
