using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace NomadAnalyzers
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ContinueWithAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "ContinueWith";

    public static readonly LocalizableString Title =
      new LocalizableResourceString(nameof(Resources.ContinueWithAnalyzerTitle), Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableString Description =
      new LocalizableResourceString(nameof(Resources.ContinueWithAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule =
      new DiagnosticDescriptor(DiagnosticId, Title, Description, Category, DiagnosticSeverity.Error, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
      var memberAccess = (MemberAccessExpressionSyntax)context.Node;

      var method = memberAccess.GetLastToken();

      if (method.ValueText != "ContinueWith")
        return;

      context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }
  }
}
