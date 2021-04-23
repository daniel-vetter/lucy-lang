using Disassembler.Assembling;
using Disassembler.Assembling.Operations;
using Disassembler.ContainerFormats.PE;
using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Expressions.Unary;
using Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.Core.Compiler
{
    public class WinCompiler
    {
        public static async Task Compile(WorkspaceProcessor processedWorkspace, string outFile)
        {
            var b = new PortableExecutableBuilder();
            AddImportTable(processedWorkspace, b);
            AddStringData(processedWorkspace, b);
            AddCode(processedWorkspace, b);
            await b.Write(outFile);
        }

        private static void AddCode(WorkspaceProcessor processedWorkspace, PortableExecutableBuilder b)
        {
            var doc = processedWorkspace.Documents.Single();
            var rootNode = doc;
            var asm = new Assembler(OperandSize.S32);


            void Traverse(SyntaxNode node)
            {
                if (node is FunctionCallExpressionSyntaxNode fc)
                {
                    for (int i = fc.ArgumentList.Count - 1; i >= 0; i--)
                    {
                        var arg = fc.ArgumentList[i];
                        Traverse(arg);
                        asm.Add(new Push(Register.EAX));
                    }

                    var functionInfo = fc.GetFunctionInfo();
                    if (functionInfo == null)
                        throw new Exception("No function info found.");

                    if (functionInfo.Extern == null)
                        throw new Exception("Only extern functions are currently supported.");

                    asm.Add(new Call(new Memory(OperandSize.S32, 0, new AddressImport(new ImportAddressTableEntry(functionInfo.Extern.LibraryName, functionInfo.Extern.FunctionName), AddressType.AbsoluteVirtualAddress))));
                    return;
                }

                if (node is StringConstantExpressionSyntaxNode sc)
                {
                    asm.Add(new Mov(Register.EAX, new Immediate(OperandSize.S32, 0, new AddressImport(sc.GetRequiredAnnotation<DataIndex>(), AddressType.AbsoluteVirtualAddress))));
                }

                if (node is NumberConstantExpressionSyntaxNode nc)
                {
                    asm.Add(new Mov(Register.EAX, new Immediate(OperandSize.S32, (uint)nc.Value)));
                }

                foreach (var child in node.GetChildNodes())
                    Traverse(child.Node);
            }

            if (doc.SyntaxTree == null)
                throw new Exception("No syntax tree found.");

            Traverse(doc.SyntaxTree);

            var processResult = asm.Process();
            if (processResult.Issues.Any())
            {
                throw new Exception(string.Join(Environment.NewLine, processResult.Issues.Select(x => x.Severity + " " + x.Message)));
            }

            Console.WriteLine(asm.ToString());
            b.AddSection(new CodeSection(processResult.Data));
        }

        private static void AddStringData(WorkspaceProcessor processedWorkspace, PortableExecutableBuilder b)
        {
            var dataSection = new DataSection();

            void Traverse(SyntaxNode node)
            {
                if (node is StringConstantExpressionSyntaxNode stringConstant)
                {
                    var index = new DataIndex(dataSection.EntryCount);
                    dataSection.Add(Encoding.UTF8.GetBytes(stringConstant.Value).Concat(new byte[] { 0x00 }).ToArray(), new AddressExport(index));
                    stringConstant.SetAnnotation(index);
                }

                foreach (var childNode in node.GetChildNodes())
                    Traverse(childNode.Node);
            }

            foreach (var parsedDocument in processedWorkspace.Documents)
                Traverse(parsedDocument.SyntaxTree ?? throw new Exception("No syntax tree found."));

            b.AddSection(dataSection);
        }

        private static void AddImportTable(WorkspaceProcessor processedWorkspace, PortableExecutableBuilder b)
        {
            List<ImportTableEntry> list = new();

            foreach (var parsedDocument in processedWorkspace.Documents)
            {
                foreach (var statement in parsedDocument.SyntaxTree?.StatementList.Statements ?? new List<Parser.Nodes.Statements.StatementSyntaxNode>())
                {
                    if (statement is not FunctionDeclarationStatementSyntaxNode functionDeclaration)
                        continue;

                    var functionInfo = functionDeclaration.GetFunctionInfo();
                    if (functionInfo != null)
                    {
                        if (functionInfo.Extern == null)
                            continue;

                        list.Add(new ImportTableEntry(functionInfo.Extern.LibraryName, functionInfo.Extern.FunctionName));
                    }
                }
            }

            var importTable = new ImportTableSection();
            foreach (var entry in list.Distinct())
                importTable.Add(entry);
            b.AddSection(importTable);
        }

        private record DataIndex(int index)
        {
            public override string ToString() => $"r_data{index}";
        }
    }
}
