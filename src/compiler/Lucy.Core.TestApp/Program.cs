using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.TestApp;

var ws = new Workspace();
var changeReader = new FileChangeReader(ws, "./SampleApp");
using var sa = new SemanticDatabase(ws, "./graphOutput");

while (changeReader.NextVersion())
{
    var mainFile = ws.GetCodeFile("/main.lucy");
    var firstStatement = mainFile.SyntaxTree.StatementList.Statements[0];
    
    Console.WriteLine(sa.Query(new GetNodeById(firstStatement.NodeId)));
}
