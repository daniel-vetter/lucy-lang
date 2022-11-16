namespace Lucy.Core.SourceGenerator.Generators
{
    internal class Logger
    {
        private readonly string _name;
        private readonly bool _enabled;

        public Logger(string name, bool enabled)
        {
            _name = name;
            _enabled = enabled;

            if (enabled)
            {
                if (!Directory.Exists("C:\\generator-log"))
                    Directory.CreateDirectory("C:\\generator-log");
            }

        }

        public bool IsEnabled => _enabled;

        public void Write(string message)
        {
            if (!_enabled)
                return;

            message = DateTimeOffset.UtcNow + " " + message;
            File.AppendAllLines(Path.Combine("C:\\generator-log", _name + ".log"), new[] { message });
        }
    }
}
