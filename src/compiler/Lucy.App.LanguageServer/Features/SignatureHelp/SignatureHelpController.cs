using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Helper;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.SemanticAnalysis;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Infrastructure.RpcServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Features.SignatureHelp
{
    [Service]
    public class SignatureHelpController
    {
        private readonly CurrentWorkspace _currentWorkspace;

        public SignatureHelpController(CurrentWorkspace currentWorkspace)
        {
            _currentWorkspace = currentWorkspace;
        }

        [JsonRpcFunction("textDocument/signatureHelp")]
        public async Task<RpcSignatureHelp> SignatureHelp(RpcSignatureHelpParams request)
        {
            if (request.TextDocument == null)
                return new RpcSignatureHelp();

            if (_currentWorkspace.Workspace == null)
                return new RpcSignatureHelp();

            var document = _currentWorkspace.Workspace.Get(_currentWorkspace.ToWorkspacePath(request.TextDocument.Uri));
            if (document == null)
                return new RpcSignatureHelp();

            if (document.SyntaxTree == null)
                return new RpcSignatureHelp();

            if (request.Position == null)
                return new RpcSignatureHelp();

            
            var functionCall = FindFunctionCall(document.SyntaxTree, request.Position);
            if (functionCall == null)
                return new RpcSignatureHelp();

            var scope = functionCall.GetScope();
            var matchingFunctions = scope.GetAllMatchingSymbols(functionCall.FunctionName.Token.Text);

            var result = new List<RpcSignatureInformation>();

            foreach(var matchingFunction in matchingFunctions)
            {
                var sigInfo = new RpcSignatureInformation();
                sigInfo.Label = matchingFunction.Name;
                result.Add(sigInfo);
            }

            return new RpcSignatureHelp
            {
                Signatures = result.ToArray()
            };
        }

        private FunctionCallExpressionSyntaxNode? FindFunctionCall(DocumentSyntaxNode rootNode, RpcPosition position)
        {
            List<SyntaxTreeNode> stack = new();
            stack.Add(rootNode);

            void Walk(SyntaxTreeNode node)
            {
                foreach (var child in node.GetChildNodes())
                {
                    var range = child.GetRange();
                    if (range.Contains(position.Line, position.Character))
                    {
                        stack.Add(child);
                        Walk(child);
                    }
                }
            }
            Walk(rootNode);

            stack.Reverse();

            return (FunctionCallExpressionSyntaxNode?)stack.FirstOrDefault(x => x is FunctionCallExpressionSyntaxNode);
        }
    }
}
