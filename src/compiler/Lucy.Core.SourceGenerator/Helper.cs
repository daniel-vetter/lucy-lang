using System;
using System.Collections.Generic;
using System.Text;

namespace Lucy.Core.SourceGenerator
{
    public static class Helper
    {
        public static string Then(this bool condition, string message)
        {
            return condition ? message : "";
        }
    }
}
