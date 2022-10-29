using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;

var ws = await Workspace.CreateFromPath("SampleApp");
using var sa = new SemanticAnalyzer(ws, "./graphOutput");

var firstDoc = (CodeFile)ws.Documents.First().Value;
var firstStatement = firstDoc.SyntaxTree.StatementList.Statements[0];
var secondStatement = firstDoc.SyntaxTree.StatementList.Statements[1];

Console.WriteLine(sa.GetNodeById(firstStatement.NodeId));
Console.WriteLine(sa.GetNodeById(secondStatement.NodeId));