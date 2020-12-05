using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace NomadAnalyzers
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class NewTaskAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "NewTask";

    public static readonly LocalizableString Title =
      new LocalizableResourceString(nameof(Resources.NewTaskAnalyzerTitle), Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableString Description =
      new LocalizableResourceString(nameof(Resources.NewTaskAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule =
      new DiagnosticDescriptor(DiagnosticId, Title, Description, Category, DiagnosticSeverity.Error, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
      var memberAccess = (ObjectCreationExpressionSyntax)context.Node;

      var taskClass = memberAccess.ChildNodes().FirstOrDefault();

      if (taskClass?.GetFirstToken().ValueText != "Task")
        return;

      context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }
  }
}
