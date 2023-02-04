using Lucy.Assembler.TestCaseGenerator.Services;
using System.Threading.Tasks;

namespace Lucy.Assembler.TestCaseGenerator
{
    class Program
    {
        static async Task Main()
        {
            var operation = "push";
            var operandCount = 2;
            var bits = 32;
            var writeSqliteDb = false;

            await Orchestrator.Run(operation, operandCount, bits, writeSqliteDb);
        }
    }
}
