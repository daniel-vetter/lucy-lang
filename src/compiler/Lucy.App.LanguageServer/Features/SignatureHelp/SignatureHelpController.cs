using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.SemanticAnalysis;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Infrastructure.RpcServer;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

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
        public RpcSignatureHelp SignatureHelp(RpcSignatureHelpParams request)
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

            if (document.SemanticModel == null)
                return new RpcSignatureHelp();

            if (request.Position == null)
                return new RpcSignatureHelp();

            var functionCall = TreeAnalyzer.FindDeepest<FunctionCallExpressionSyntaxNode>(document.SyntaxTree, request.Position.Line, request.Position.Character);
            if (functionCall == null)
                return new RpcSignatureHelp();

            var scope = document.SemanticModel.GetScope(functionCall);
            var matchingFunctions = scope.GetAllMatchingSymbols(functionCall.FunctionName.Token.Text);

            var result = new List<RpcSignatureInformation>();


            foreach (var matchingFunction in matchingFunctions.OfType<FunctionInfo>())
            {
                var sigInfo = new RpcSignatureInformation();

                var labelSb = new StringBuilder();
                labelSb.Append(matchingFunction.Name);
                labelSb.Append("(");

                for (int i = 0; i < matchingFunction.Parameter.Length; i++)
                {
                    FunctionParameterInfo? parameter = matchingFunction.Parameter[i];
                    sigInfo.Parameters.Add(new RpcParameterInformation
                    {
                        Label = new[] { labelSb.Length, labelSb.Length + parameter.Name.Length }
                    });

                    labelSb.Append(parameter.Name);
                    if (i != matchingFunction.Parameter.Length - 1)
                        labelSb.Append(", ");
                }

                labelSb.Append(")");
                sigInfo.Label = labelSb.ToString();
                result.Add(sigInfo);
            }

            return new RpcSignatureHelp
            {
                Signatures = result.ToArray(),
                ActiveParameter = GetActiveParameterIndex(functionCall, request.Position)
            };
        }

        private static int GetActiveParameterIndex(FunctionCallExpressionSyntaxNode functionCall, RpcPosition position)
        {
            for (int i = 0; i < functionCall.ArgumentList.Count; i++)
            {
                if (functionCall.ArgumentList[i].GetRange().Contains(position.Line, position.Character, functionCall.ArgumentList[i].Seperator != null))
                {
                    return i;
                }
            }

            return functionCall.ArgumentList.Count;
        }
    }
}
