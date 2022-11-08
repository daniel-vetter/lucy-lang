using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;
using Lucy.Core.TestApp;

var ws = new Workspace();
var changeReader = new TestCaseReader(ws, "./SampleApp");
var sdb = new SemanticDatabase(ws, "./graphOutput");
while (changeReader.NextVersion())
{
    var mainFile = ws.GetCodeFile("/main.lucy");
    var firstStatement = mainFile.SyntaxTree.StatementList.Statements[0];
    //sdb.Query(new GetAllEntryPoints());
    Dumper.Dump(sdb.Query(new GetDublicateDeclarations("/main.lucy")));

    //Console.WriteLine(JsonConvert.SerializeObject(s), Formatting.Indented)); 
    //Console.WriteLine(JsonConvert.SerializeObject(sdb.Query(new GetAllEntryPoints()), Formatting.Indented));
    //Console.WriteLine(JsonConvert.SerializeObject(sdb.Query(new GetScopeTree("/main.lucy")), Formatting.Indented));
}
