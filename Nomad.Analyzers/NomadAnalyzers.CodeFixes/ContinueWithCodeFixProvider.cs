using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NomadAnalyzers
{
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ContinueWithCodeFixProvider)), Shared]
  public class ContinueWithCodeFixProvider : CodeFixProvider
  {
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ContinueWithAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      // Find the type declaration identified by the diagnostic.
      var memberAccess = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().First();

      //Register a code action that will invoke the fix.
      context.RegisterCodeFix(
          CodeAction.Create(
            TaskRunAnalyzer.Title.ToString(),
              c => UseTaskUtils(context.Document, memberAccess, c)),
          diagnostic);
    }

    private async Task<Document> UseTaskUtils(Document document,
      MemberAccessExpressionSyntax memberAccessExpression,
      CancellationToken cancellationToken)
    {
      var method = memberAccessExpression.GetLastToken();
      var runSuppressFlowMethod = SyntaxFactory.Identifier(method.ValueText + "SuppressFlow");
      var newMemberAccessExpression = memberAccessExpression.ReplaceToken(method, runSuppressFlowMethod);

      // Add an annotation to format the new local declaration.
      var formatted = newMemberAccessExpression.WithAdditionalAnnotations(Formatter.Annotation);

      // Replace the old local declaration with the new local declaration.
      var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      var newRoot = oldRoot.ReplaceNode(memberAccessExpression, formatted);

      // Return document with transformed tree.
      return document.WithSyntaxRoot(newRoot);
    }
  }
}
