using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.TestApp;
using Newtonsoft.Json;

var ws = new Workspace();
var changeReader = new FileChangeReader(ws, "./SampleApp");
var sdb = new SemanticDatabase(ws, "./graphOutput");
while (changeReader.NextVersion())
{
    var mainFile = ws.GetCodeFile("/main.lucy");
    var firstStatement = mainFile.SyntaxTree.StatementList.Statements[0];
    
    Console.WriteLine(JsonConvert.SerializeObject(sdb.Query(new GetAllEntryPoints()), Formatting.Indented));

    
}
