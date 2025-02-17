﻿namespace LiteDB.Engine;

/// <summary>
/// Manage multiple open readonly Stream instances from same source (file). 
/// Support single writer instance
/// Close all Stream on dispose
/// [ThreadSafe]
/// </summary>
internal class StreamPool : IDisposable
{
    private readonly ConcurrentBag<Stream> _pool = new ();
    private readonly Stream _writer;
    private readonly IStreamFactory _factory;

    public StreamPool(IStreamFactory factory, bool readOnly)
    {
        _factory = factory;

        _writer = readOnly ? null : _factory.GetStream(false);
    }

    /// <summary>
    /// Get single Stream writer instance
    /// </summary>
    public Stream Writer => _writer;

    /// <summary>
    /// Rent a Stream reader instance
    /// </summary>
    public Stream Rent()
    {
        if (!_pool.TryTake(out var stream))
        {
            stream = _factory.GetStream(true);
        }

        return stream;
    }

    /// <summary>
    /// After use, return Stream reader instance
    /// </summary>
    public void Return(Stream stream)
    {
        _pool.Add(stream);
    }

    /// <summary>
    /// Close all Stream instances (readers/writer)
    /// </summary>
    public void Dispose()
    {
        // dipose stream only implement on factory
        if (_factory.CloseOnDispose == false) return;

        // dispose all reader stream
        foreach (var stream in _pool.ToArray())
        {
            stream.Dispose();
        }

        // do writer dispose (wait async writer thread)
        if (_writer != null)
        {
            _writer.Dispose();
        }
    }
}
