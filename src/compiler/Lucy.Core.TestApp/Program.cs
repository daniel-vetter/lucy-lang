using BenchmarkDotNet.Attributes;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.TestApp;

var ws = new Workspace();
var changeReader = new TestCaseReader(ws, "./SampleApp");

var sdb = new SemanticDatabase(ws, "./graphOutput");
//var sdb = new SemanticDatabase(ws);

while (changeReader.NextVersion())
{
    Dumper.Dump(new { Result = sdb.GetAllErrors() });
    Console.WriteLine(GC.GetTotalMemory(true) / 1024.0 / 1024.0);
}

namespace Lucy.Core.TestApp
{
    public class Md5VsSha256
    {
        [Benchmark]
        public void Run()
        {
            var db = new Db();
            db.RegisterHandler(new Handler());
            for (int i = 0; i < 1000; i++)
            {
                db.SetInput(new BaseInput(), new BaseOutput(i));
                db.Query(new SampleQuery(1000));
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

        public record BaseInput : IQuery<BaseOutput>;
        public record BaseOutput(int Counter);
    }
}

