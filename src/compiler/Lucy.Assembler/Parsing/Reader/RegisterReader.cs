using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class RegisterReader
    {
        public static bool TryReadRegister(this AsmReader reader, [NotNullWhen(true)] out Register? register)
        {
            using var t = reader.BeginTransaction();

            if (!reader.TryReadIdentifier(out var identifier))
            {
                register = null;
                return false;
            }

            var match = Register.All.FirstOrDefault(x => x.Name.Equals(identifier, StringComparison.InvariantCultureIgnoreCase));
            if (match == null)
            {
                register = null;
                return false;
            }

            t.Commit();
            register = match;
            return true;
        }
    }
}
