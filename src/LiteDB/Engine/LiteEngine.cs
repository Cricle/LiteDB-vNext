﻿namespace LiteDB.Engine;

/// <summary>
/// A public class that take care of all engine data structure access - it´s basic implementation of a NoSql database
/// Its isolated from complete solution - works on low level only (no linq, no poco... just BSON objects)
/// [ThreadSafe]
/// </summary>
public partial class LiteEngine //: ILiteEngine
{
    private bool _disposed = false;

    private readonly EngineServices _services;

    #region Ctor

    /// <summary>
    /// Initialize LiteEngine using connection memory database
    /// </summary>
    public LiteEngine()
        : this(new EngineSettings { DataStream = new MemoryStream() })
    {
    }

    /// <summary>
    /// Initialize LiteEngine using connection string using key=value; parser
    /// </summary>
    public LiteEngine(string filename)
        : this (new EngineSettings { Filename = filename })
    {
    }

    /// <summary>
    /// Initialize LiteEngine using initial engine settings
    /// </summary>
    public LiteEngine(EngineSettings settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        _services = new EngineServices(settings);
    }

    #endregion

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~LiteEngine()
    {
        this.Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _services.Dispose();
        }

        _disposed = true;
    }
}
