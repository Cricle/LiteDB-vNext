﻿namespace LiteDB;

/// <summary>
/// All supported BsonTypes in sort order
/// </summary>
public enum BsonType : byte
{
    MinValue = 0,

    Null = 1,

    Int32 = 2,
    Int64 = 3,
    Double = 4,
    Decimal = 5,

    String = 6,

    Document = 7,
    Array = 8,

    Binary = 9,
    ObjectId = 10,
    Guid = 11,
    DateTime = 12,
    Boolean = 13,
    // 13 reserved for (true|false)

    MaxValue = 31
}
