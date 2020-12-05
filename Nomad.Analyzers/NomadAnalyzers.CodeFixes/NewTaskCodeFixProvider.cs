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
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NewTaskCodeFixProvider)), Shared]
  public class NewTaskCodeFixProvider : CodeFixProvider
  {
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NewTaskAnalyzer.DiagnosticId);

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
      var objectCreation = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ObjectCreationExpressionSyntax>().First();

      // Register a code action that will invoke the fix.
      context.RegisterCodeFix(
          CodeAction.Create(
            TaskRunAnalyzer.Title.ToString(),
              c => UseTaskUtils(context.Document, objectCreation, c)),
          diagnostic);
    }

    private async Task<Document> UseTaskUtils(Document document,
      ObjectCreationExpressionSyntax objectCreation,
      CancellationToken cancellationToken)
    {
      var argumentList = objectCreation.ArgumentList;

      var className = SyntaxFactory.IdentifierName("TaskUtils");
      var dot = SyntaxFactory.Token(SyntaxKind.DotToken);
      var methodName = SyntaxFactory.IdentifierName("CreateSuppressFlow");

      var memberAccess = SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.IdentifierName("TaskUtils"),
          SyntaxFactory.IdentifierName("CreateSuppressFlow"))
        .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken));
      //SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,)

      var invocationExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList);

      // Add an annotation to format the new local declaration.
      var formatted = invocationExpression.WithAdditionalAnnotations(Formatter.Annotation);

      // Replace the old local declaration with the new local declaration.
      var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      var newRoot = oldRoot.ReplaceNode(objectCreation, formatted);

      // Return document with transformed tree.
      return document.WithSyntaxRoot(newRoot);
    }
  }
}
