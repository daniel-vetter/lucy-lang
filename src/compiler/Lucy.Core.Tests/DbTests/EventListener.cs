using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.Tests.DbTests
{
    internal class EventListener : List<IDbEvent>
    {
        public EventListener(Db db)
        {
            db.AddEventHandler(x => Add(x));
        }
    }
}
