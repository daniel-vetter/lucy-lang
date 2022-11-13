using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Lucy.Common;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using Lucy.Core.TestApp;
using System.Security.Cryptography;

//var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

var ws = new Workspace();
var changeReader = new TestCaseReader(ws, "./SampleApp");
//var sdb = new SemanticDatabase(ws, "./graphOutput");
var sdb = new SemanticDatabase(ws);
while (changeReader.NextVersion())
{
    var mainFile = ws.GetCodeFile("/main.lucy");
    var firstStatement = mainFile.SyntaxTree.StatementList.Statements[0];
    //sdb.Query(new GetAllEntryPoints());
    Dumper.Dump(sdb.Query(new GetAllEntryPoints()));

    //Console.WriteLine(JsonConvert.SerializeObject(s), Formatting.Indented)); 
    //Console.WriteLine(JsonConvert.SerializeObject(sdb.Query(new GetAllEntryPoints()), Formatting.Indented));
    //Console.WriteLine(JsonConvert.SerializeObject(sdb.Query(new GetScopeTree("/main.lucy")), Formatting.Indented));

    Console.WriteLine(GC.GetTotalMemory(true) / 1024.0 / 1024.0);
}

public class Md5VsSha256
{
    [Benchmark]
    public void Run()
    {
        var db = new Db();
        db.RegisterHandler(new Handler());
        for (int i = 0; i < 1000; i++)
        {
            db.SetInput(new BaseInput(), new BaseOuput(i));
            var result = db.Query(new SampleQuery(1000));
        }
    }

    public record SampleQuery(int Counter) : IQuery<SampleResult>;
    public record SampleResult(int Result);

    public class Handler : QueryHandler<SampleQuery, SampleResult>
    {
        public override SampleResult Handle(IDb db, SampleQuery query)
        {
            if (query.Counter == 0)
            {
                db.Query(new BaseInput());
                return new SampleResult(0);
            }
            else
                return db.Query(new SampleQuery(query.Counter - 1));
        }
    }

    public record BaseInput() : IQuery<BaseOuput>;
    public record BaseOuput(int Counter);
}

