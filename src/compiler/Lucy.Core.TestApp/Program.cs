using System.Collections.Immutable;using System.Diagnostics;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;


//Profiler.Attach();
var total = Stopwatch.StartNew();
var sw = Stopwatch.StartNew();
var ws = await Workspace.CreateFromPath("C:\\lucy-sample-project");

Console.WriteLine("Project loaded: " + sw.Elapsed.TotalMilliseconds);

sw.Restart();
var sa = new SemanticDatabase(ws);
Console.WriteLine("Semantic buildup: " + sw.Elapsed.TotalMilliseconds);

sw.Restart();
var errors = sa.GetAllErrors();
Console.WriteLine("GetAllErrors found " + errors.Count + " in " + sw.Elapsed.TotalMilliseconds);

Console.WriteLine("TOTAL: " + total.Elapsed.TotalMilliseconds);

var data = sa.GetLastQueryExecutionLog().Calculations
    .GroupBy(x => x.Query.GetType())
    .Select(x => new
    {
        QueryName = x.Key.Name, 
        Count = x.Count(),
        Time = x.Select(y => y.ExlusiveHandlerExecutionTime).Sum(y => y.TotalMilliseconds)
    })
    .ToArray();

Console.WriteLine("Calc total time: " + data.Sum(x => x.Time));


foreach (var calcs in data.OrderByDescending(x => x.Time))
{
    Console.WriteLine(calcs.QueryName + " - " + calcs.Count + " - " + calcs.Time);
}




//Profiler.ExportAndShow();

/*
foreach (var error in errors)
    Console.WriteLine(error.ToString());
*/

/*
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

*/