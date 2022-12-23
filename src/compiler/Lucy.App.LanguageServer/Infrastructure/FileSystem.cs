using Lucy.Common.ServiceDiscovery;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Infrastructure;

// TODO: Should this class be removed?

public interface IFileSystem
{
    Task<byte[]> ReadAllBytes(SystemPath path);
    Task WriteAllBytes(SystemPath path, byte[] data);
    void CreateDirectory(SystemPath path);
    Task<SystemPath[]> GetFilesInDirectory(SystemPath path);
    Task<string> ReadAllText(SystemPath file);
    Task WriteAllText(SystemPath file, string content);
}

public sealed class SystemPath : IEquatable<SystemPath?>
{
    private readonly string? _host;
    private readonly string[] _elements;
    private static readonly bool _runningOnWindows;
    private readonly string _equalString;

    static SystemPath()
    {
        _runningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    private SystemPath(string? host, string[] elements)
    {
        _host = host;
        _elements = elements;

        _equalString = _runningOnWindows
            ? ToString().ToLowerInvariant()
            : ToString();
    }

    public SystemPath(string path)
    {
        if (path.StartsWith("file://", StringComparison.InvariantCultureIgnoreCase))
        {
            var parts = path.Substring(7).Split('/', '\\');
            _host = parts[0] == "" ? null : parts[0];
            _elements = parts.Skip(1).Select(x => Uri.UnescapeDataString(x)).ToArray();
        }
        else
        {
            if (_runningOnWindows)
            {
                _host = null;
                _elements = path.Split('/', '\\');
            }
            else
            {
                _host = null;
                if (!path.StartsWith("/"))
                    throw new Exception("Invalid path: " + path);
                _elements = path.Substring(1).Split('/', '\\');
            }
        }

        _equalString = _runningOnWindows 
            ? ToString().ToLowerInvariant() 
            : ToString();
    }

    public Uri ToUri()
    {
        return new Uri($"file://{_host}/" + string.Join("/", _elements.Select(x => Uri.EscapeDataString(x))));
    }

    internal SystemPath Append(string path)
    {
        if (path.StartsWith("/"))
            path = path.Substring(1);

        var elements = path.Split('/');
        return new SystemPath(_host, _elements.Concat(elements).ToArray());
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (_runningOnWindows)
        {
            if (_host != null)
            {
                sb.Append("\\\\");
                sb.Append(_host);
            }
            for (int i = 0; i < _elements.Length; i++)
            {
                sb.Append(_elements[i]);
                if (i != _elements.Length - 1)
                    sb.Append("\\");
            }
        }
        else
        {
            sb.Append("/");
            for (int i = 0; i < _elements.Length; i++)
            {
                sb.Append(_elements[i]);
                if (i != _elements.Length - 1)
                    sb.Append("/");
            }
        }
        return sb.ToString();
    }

    public override int GetHashCode() => _equalString.GetHashCode();
    public override bool Equals(object? obj) => Equals(obj as SystemPath);
    public bool Equals(SystemPath? other) => !ReferenceEquals(null, other) && other._equalString == _equalString;
    public static bool operator ==(SystemPath? left, SystemPath? right)
    {
        if (left is null && right is null) return true;
        if (left is not null && right is null) return false;
        if (left is null && right is not null) return false;
        if (left is not null && right is not null) return left._equalString == right._equalString;
        throw new NotSupportedException();
    }

    public static bool operator !=(SystemPath? left, SystemPath? right)
    {
        return !(left == right);
    }
}

[Service(Lifetime.Singleton)]
public class FileSystem : IFileSystem
{
    public async Task<byte[]> ReadAllBytes(SystemPath path)
    {
        return await File.ReadAllBytesAsync(path.ToString());
    }

    public async Task WriteAllBytes(SystemPath path, byte[] data)
    {
        await File.WriteAllBytesAsync(path.ToString(), data);
    }

    public Task<SystemPath[]> GetFilesInDirectory(SystemPath path)
    {
        return Task.FromResult(Directory.GetFiles(path.ToString(), "*.*", SearchOption.AllDirectories).Select(x => new SystemPath(x)).ToArray());
    }

    public async Task<string> ReadAllText(SystemPath file)
    {
        return await File.ReadAllTextAsync(file.ToString());
    }

    public async Task WriteAllText(SystemPath file, string content)
    {
        await File.WriteAllTextAsync(file.ToString(), content);
    }

    public void CreateDirectory(SystemPath path)
    {
        if (!Directory.Exists(path.ToString()))
            Directory.CreateDirectory(path.ToString());
    }
}