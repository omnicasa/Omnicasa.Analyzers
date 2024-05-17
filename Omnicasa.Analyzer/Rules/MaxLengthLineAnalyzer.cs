using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Omnicasa.Analyzers.Rules
{
    /// <summary>
    /// Omnicasa max length rule.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning disable RS1036
    public class MaxLengthLineAnalyzer : DiagnosticAnalyzer
#pragma warning restore RS1036
    {
        /// <summary>DiagnosticId.</summary>
        public const string DiagnosticId = "MOBILEOMNI_RULE_0001";

        private const string Title = "Line length exceeds maximum limit";

        private const string MessageFormat = "Line length is {0} characters, which exceeds the maximum allowed length of {1} characters";

        private const string Description = "Ensure lines do not exceed the maximum length.";

        private const string Category = "Formatting";

        private const int MaxLineLength = 110;

        private static readonly DiagnosticDescriptor rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(rule);
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var text = context.Tree.GetText(context.CancellationToken);
            foreach (var line in text.Lines)
            {
                if (line.Span.Length > MaxLineLength)
                {
                    var location = Location.Create(context.Tree, new TextSpan(line.Start, line.Span.Length));
                    var diagnostic = Diagnostic.Create(rule, location, line.Span.Length, MaxLineLength);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
