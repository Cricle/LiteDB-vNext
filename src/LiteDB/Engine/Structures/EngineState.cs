﻿namespace LiteDB.Engine;

/// <summary>
/// All engine state
/// </summary>
public enum EngineState
{
    /// <summary>
    /// Initial state - all services are disposed no touch on file
    /// </summary>
    Close,

    //Opening

    /// <summary>
    /// Database are created and initialized ok. Any recovery already made.
    /// Ready to use
    /// </summary>
    Open,
    //Locked

    /// <summary>
    /// Database are in shutdown mode - called by user or Critical exception.
    /// No one can use until back to initial state: Closed
    /// </summary>
    Shutdown

}
