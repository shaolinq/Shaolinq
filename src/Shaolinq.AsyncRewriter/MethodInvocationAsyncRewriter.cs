using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Shaolinq.AsyncRewriter
{
	internal class MethodInvocationAsyncRewriter : MethodInvocationInspector
	{
		public MethodInvocationAsyncRewriter(IAsyncRewriterLogger log, CompilationLookup extensionMethodLookup, SemanticModel semanticModel, HashSet<ITypeSymbol> excludeTypes, ITypeSymbol cancellationTokenSymbol)
			: base(log, extensionMethodLookup, semanticModel, excludeTypes, cancellationTokenSymbol)
		{
		}

		protected override ExpressionSyntax InspectExpression(InvocationExpressionSyntax node, int cancellationTokenPos, IMethodSymbol candidate, bool explicitExtensionMethodCall)
		{
			InvocationExpressionSyntax rewrittenInvocation;

			if (node.Expression is IdentifierNameSyntax)
			{
				var identifierName = (IdentifierNameSyntax)node.Expression;

				rewrittenInvocation = node.WithExpression(identifierName.WithIdentifier(SyntaxFactory.Identifier(identifierName.Identifier.Text + "Async")));
			}
			else if (node.Expression is MemberAccessExpressionSyntax)
			{
				var memberAccessExp = (MemberAccessExpressionSyntax)node.Expression;
				var nestedInvocation = memberAccessExp.Expression as InvocationExpressionSyntax;

				if (nestedInvocation != null)
				{
					memberAccessExp = memberAccessExp.WithExpression(nestedInvocation);
				}

				if (explicitExtensionMethodCall)
				{
					rewrittenInvocation = node.WithExpression
					(
						memberAccessExp
							.WithExpression(SyntaxFactory.IdentifierName(candidate.ContainingType.ToMinimalDisplayString(this.semanticModel, node.SpanStart)))
							.WithName(memberAccessExp.Name.WithIdentifier(SyntaxFactory.Identifier(memberAccessExp.Name.Identifier.Text + "Async")))
					);

					rewrittenInvocation = rewrittenInvocation.WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>().Add(SyntaxFactory.Argument(memberAccessExp.Expression.WithoutTrivia())).AddRange(node.ArgumentList.Arguments)));
				}
				else
				{
					rewrittenInvocation = node.WithExpression(memberAccessExp.WithName(memberAccessExp.Name.WithIdentifier(SyntaxFactory.Identifier(memberAccessExp.Name.Identifier.Text + "Async"))));
				}
			}
			else if (node.Expression is GenericNameSyntax)
			{
				var genericNameExp = (GenericNameSyntax)node.Expression;

				rewrittenInvocation = node.WithExpression(genericNameExp.WithIdentifier(SyntaxFactory.Identifier(genericNameExp.Identifier.Text + "Async")));
			}
			else if (node.Expression is MemberBindingExpressionSyntax && node.Parent is ConditionalAccessExpressionSyntax)
			{
				var memberBindingExpression = (MemberBindingExpressionSyntax)node.Expression;

				rewrittenInvocation = node.WithExpression(memberBindingExpression.WithName(memberBindingExpression.Name.WithIdentifier(SyntaxFactory.Identifier(memberBindingExpression.Name.Identifier.Text + "Async"))));
			}
			else
			{
				throw new InvalidOperationException($"Cannot process node of type: ({node.Expression.GetType().Name})");
			}

			if (cancellationTokenPos != -1)
			{
				var cancellationTokenArg = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"));

				if (explicitExtensionMethodCall)
				{
					cancellationTokenPos++;
				}
				
				if (cancellationTokenPos == rewrittenInvocation.ArgumentList.Arguments.Count)
				{
					rewrittenInvocation = rewrittenInvocation.WithArgumentList(rewrittenInvocation.ArgumentList.AddArguments(cancellationTokenArg));
				}
				else
				{
					rewrittenInvocation = rewrittenInvocation.WithArgumentList(SyntaxFactory.ArgumentList(rewrittenInvocation.ArgumentList.Arguments.Insert(cancellationTokenPos, cancellationTokenArg)));
				}
			}

			var methodInvocation = SyntaxFactory.InvocationExpression
			(
				SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, rewrittenInvocation, SyntaxFactory.IdentifierName("ConfigureAwait")),
				SyntaxFactory.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(SyntaxFactory.Argument(SyntaxFactory.ParseExpression("false"))))
			);

			var rewritten = (ExpressionSyntax)SyntaxFactory.AwaitExpression(methodInvocation);

			if (!(node.Parent == null || node.Parent is StatementSyntax))
			{
				rewritten = SyntaxFactory.ParenthesizedExpression(rewritten);
			}

			return rewritten;
		}

		public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
		{
			var expression = this.Visit(node.Expression);

			if (expression is IfStatementSyntax)
			{
				return expression;
			}
			else
			{
				var semicolonToken = this.VisitToken(node.SemicolonToken);

				return node.Update((ExpressionSyntax)expression, semicolonToken);
			}
		}

		public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
		{
			var result = base.VisitConditionalAccessExpression(node);
			var conditionalAccessResult = result as ConditionalAccessExpressionSyntax;

			if (conditionalAccessResult == node || conditionalAccessResult == null)
			{
				return node;
			}

			if (((conditionalAccessResult.WhenNotNull as ParenthesizedExpressionSyntax)?.Expression ?? conditionalAccessResult.WhenNotNull as AwaitExpressionSyntax)?.Kind() == SyntaxKind.AwaitExpression)
			{
				var awaitExpression = (AwaitExpressionSyntax)(conditionalAccessResult.WhenNotNull as ParenthesizedExpressionSyntax)?.Expression ?? conditionalAccessResult.WhenNotNull as AwaitExpressionSyntax;
				var awaitExpressionExpression = awaitExpression?.Expression;

				if (awaitExpressionExpression == null)
				{
					return result;
				}

				var stack = new Stack<ExpressionSyntax>();
				var syntax = awaitExpressionExpression;
				
				while (true)
				{
					if (syntax is MemberAccessExpressionSyntax)
					{
						stack.Push(syntax);
						syntax = ((MemberAccessExpressionSyntax)syntax).Expression;
					}
					else if (syntax is InvocationExpressionSyntax)
					{
						stack.Push(syntax);
						syntax = ((InvocationExpressionSyntax)syntax).Expression;
					}
					else if (syntax is MemberBindingExpressionSyntax)
					{
						var name = ((MemberBindingExpressionSyntax)syntax).Name;

						dynamic current = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, conditionalAccessResult.Expression, name);

						while (stack.Count > 0)
						{
							dynamic next = stack.Pop();

							current = next.WithExpression(current);
						}

						syntax = current;

						break;
					}
					else
					{
						throw new InvalidOperationException("Unsupported expression " + syntax);
					}
				}

				if (node.Parent == null || node.Parent is StatementSyntax)
				{
					return SyntaxFactory.IfStatement(SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, conditionalAccessResult.Expression, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)), SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(SyntaxFactory.AwaitExpression(syntax))));
				}
				else
				{
					return SyntaxFactory.ConditionalExpression(SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, conditionalAccessResult.Expression, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)), SyntaxFactory.AwaitExpression(syntax), SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
				}
			}

			return result;
		}
	}
}