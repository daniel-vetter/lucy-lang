using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.Tests.DbTests
{
    internal class EventListener : List<IDbEvent>
    {
        public EventListener(Db db)
        {
            db.AddEventHandler((db, e) => Add(e));
        }
    }
}
