﻿namespace LiteDB;

/// <summary>
/// Represent an abstract minimal document information
/// </summary>
public abstract class BsonValue : IComparable<BsonValue>, IEquatable<BsonValue>
{
    #region Static object creator helper

    public static readonly BsonValue MinValue = new BsonMinValue();

    public static readonly BsonValue Null = new BsonNull();

    public static readonly BsonValue MaxValue = new BsonMaxValue();

    #endregion

    /// <summary>
    /// BsonType
    /// </summary>
    public abstract BsonType Type { get; }

    /// <summary>
    /// Get how much this Bson object will use in disk space (formated as Bson format)
    /// </summary>
    public abstract int GetBytesCount();

    /// <summary>
    /// Get how many bytes one single element will used in BSON format
    /// </summary>
    protected int GetBytesCountElement(string key, BsonValue value)
    {
        // check if data type is variant
        var variant = value.Type == BsonType.String || value.Type == BsonType.Binary || value.Type == BsonType.Guid;

        return
            1 + // element type
            Encoding.UTF8.GetByteCount(key) + // CString
            1 + // CString \0
            value.GetBytesCount() +
            (variant ? 5 : 0); // bytes.Length + 0x??
    }

    #region Implement IComparable

    public int CompareTo(BsonValue other) => this.CompareTo(other, Collation.Binary);

    public abstract int CompareTo(BsonValue other, Collation collation);

    protected int CompareType(BsonValue other)
    {
        var result = this.Type.CompareTo(other.Type);

        return result < 0 ? -1 : result > 0 ? +1 : 0;
    }

    #endregion

    public bool Equals(BsonValue other) => other is not null && this.CompareTo(other) == 0;

    public override bool Equals(object other) => this.Equals((BsonValue)other);

    public abstract override int GetHashCode();

    #region Convert types

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual BsonArray AsArray => (this as BsonArray) ?? throw new InvalidCastException($"BsonValue must be an Array value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual BsonDocument AsDocument => (this as BsonDocument) ?? throw new InvalidCastException($"BsonValue must be a Document value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual byte[] AsBinary => (this as BsonBinary)?.Value ?? throw new InvalidCastException($"BsonValue must be a Binary value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual bool AsBoolean => (this as BsonBoolean)?.Value ?? throw new InvalidCastException($"BsonValue must be a Boolean value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual string AsString => (this as BsonString)?.Value ?? throw new InvalidCastException($"BsonValue must be a String value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual int AsInt32 => (this as BsonInt32)?.Value ?? throw new InvalidCastException($"BsonValue must be an Int32 value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual long AsInt64 => (this as BsonInt64)?.Value ?? throw new InvalidCastException($"BsonValue must be an Int64 value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual double AsDouble => (this as BsonDouble)?.Value ?? throw new InvalidCastException($"BsonValue must be a Double value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual decimal AsDecimal => (this as BsonDecimal)?.Value ?? throw new InvalidCastException($"BsonValue must be a Decimal value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual DateTime AsDateTime => (this as BsonDateTime)?.Value ?? throw new InvalidCastException($"BsonValue must be a DateTime value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual ObjectId AsObjectId => (this as BsonObjectId)?.Value ?? throw new InvalidCastException($"BsonValue must be a ObjectId value. Current value type: {this.Type}");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual Guid AsGuid => (this as BsonGuid)?.Value ?? throw new InvalidCastException($"BsonValue must be a Guid value. Current value type: {this.Type}");

    #endregion

    #region Explicit Operators

    public static bool operator ==(BsonValue left, BsonValue right) => left.Equals(right);

    public static bool operator !=(BsonValue left, BsonValue right) => !left.Equals(right);

    public static BsonValue operator +(BsonValue left, BsonValue right)
    {
        // if both sides are string, concat
        if (left.IsString && right.IsString)
        {
            return left.AsString + right.AsString;
        }
        // if any sides are string, concat casting both to string
        else if (left.IsString || right.IsString)
        {
            return left.ToString() + right.ToString();
        }
        // if any side are DateTime and another is number, add days in date
        else if (left.IsDateTime && right.IsNumber)
        {
            return left.AsDateTime.AddDays(right.ToDouble());
        }
        else if (left.IsNumber && right.IsDateTime)
        {
            return right.AsDateTime.AddDays(left.ToDouble());
        }
        // if both sides are number, add as math
        else if (left.IsNumber && right.IsNumber)
        {
            if (left.IsDecimal || right.IsDecimal)
            {
                return left.ToDecimal() + right.ToDecimal();
            }
            else if (left.IsDouble || right.IsDouble)
            {
                return left.ToDouble() + right.ToDouble();
            }
            else if (left.IsInt64 || right.IsInt64)
            {
                return left.ToInt64() + right.ToInt64();
            }
            else if (left.IsInt32 || right.IsInt32)
            {
                return left.ToInt32() + right.ToInt32();
            }
        }

        return BsonValue.Null;
    }

    public static BsonValue operator -(BsonValue left, BsonValue right)
    {
        if (left.IsDateTime && right.IsNumber)
        {
            return left.AsDateTime.AddDays(-right.ToDouble());
        }
        else if (left.IsNumber && right.IsDateTime)
        {
            return right.AsDateTime.AddDays(-left.ToDouble());
        }
        else if (left.IsNumber && right.IsNumber)
        {
            if (left.IsDecimal || right.IsDecimal)
            {
                return left.ToDecimal() - right.ToDecimal();
            }
            else if (left.IsDouble || right.IsDouble)
            {
                return left.ToDouble() - right.ToDouble();
            }
            else if (left.IsInt64 || right.IsInt64)
            {
                return left.ToInt64() - right.ToInt64();
            }
            else if (left.IsInt32 || right.IsInt32)
            {
                return left.ToInt32() - right.ToInt32();
            }
        }

        return BsonValue.Null;
    }

    public static BsonValue operator *(BsonValue left, BsonValue right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            if (left.IsDecimal || right.IsDecimal)
            {
                return left.ToDecimal() * right.ToDecimal();
            }
            else if (left.IsDouble || right.IsDouble)
            {
                return left.ToDouble() * right.ToDouble();
            }
            else if (left.IsInt64 || right.IsInt64)
            {
                return left.ToInt64() * right.ToInt64();
            }
            else if (left.IsInt32 || right.IsInt32)
            {
                return left.ToInt32() * right.ToInt32();
            }
        }

        return BsonValue.Null;
    }

    public static BsonValue operator /(BsonValue left, BsonValue right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            if (left.IsDecimal || right.IsDecimal)
            {
                return left.ToDecimal() / right.ToDecimal();
            }
            else if (left.IsDouble || right.IsDouble)
            {
                return left.ToDouble() / right.ToDouble();
            }
            else if (left.IsInt64 || right.IsInt64)
            {
                return left.ToInt64() / right.ToInt64();
            }
            else if (left.IsInt32 || right.IsInt32)
            {
                return left.ToInt32() / right.ToInt32();
            }
        }

        return BsonValue.Null;
    }

    public static BsonValue operator %(BsonValue left, BsonValue right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            if (left.IsDecimal || right.IsDecimal)
            {
                return left.ToDecimal() % right.ToDecimal();
            }
            else if (left.IsDouble || right.IsDouble)
            {
                return left.ToDouble() % right.ToDouble();
            }
            else if (left.IsInt64 || right.IsInt64)
            {
                return left.ToInt64() % right.ToInt64();
            }
            else if (left.IsInt32 || right.IsInt32)
            {
                return left.ToInt32() % right.ToInt32();
            }
        }

        return BsonValue.Null;
    }

    public static bool operator <(BsonValue left, BsonValue right)
    {
        if (left is null && right is null) return false;
        if (left is null) return true;
        if (right is null) return false;

        return left.CompareTo(right) < 0;
    }

    public static bool operator >(BsonValue left, BsonValue right) => !(left < right);

    public static bool operator <=(BsonValue left, BsonValue right)
    {
        if (left is null && right is null) return false;
        if (left is null) return true;
        if (right is null) return false;

        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(BsonValue left, BsonValue right) => !(left < right);

    #endregion

    #region Index "this" property

    /// <summary>
    /// Get/Set a field for document. Fields are case sensitive - Works only when value are document
    /// </summary>
    public virtual BsonValue this[string name]
    {
        get => throw new InvalidCastException($"BsonValue must be a Document. Current type: {this.Type}");
        set => throw new InvalidCastException($"BsonValue must be a Document. Current type: {this.Type}");
    }

    /// <summary>
    /// Get/Set value in array position. Works only when value are array
    /// </summary>
    public virtual BsonValue this[int index]
    {
        get => throw new InvalidCastException($"BsonValue must be an Array. Current type: {this.Type}");
        set => throw new InvalidCastException($"BsonValue must be an Array. Current type: {this.Type}");
    }

    #endregion

    #region Implicit Ctor

    // Int32
    public static implicit operator int(BsonValue value) => value.AsInt32;

    public static implicit operator BsonValue(int value) => new BsonInt32(value);

    // Int64
    public static implicit operator long(BsonValue value) => value.AsInt64;

    public static implicit operator BsonValue(long value) => new BsonInt64(value);

    // Double
    public static implicit operator double(BsonValue value) => value.AsDouble;

    public static implicit operator BsonValue(double value) => new BsonDouble(value);

    // Decimal
    public static implicit operator decimal(BsonValue value) => value.AsDecimal;

    public static implicit operator BsonValue(decimal value) => new BsonDecimal(value);

    // String
    public static implicit operator string(BsonValue value) => value.AsString;

    public static implicit operator BsonValue(string value) => value is null ? BsonValue.Null : value.Length == 0 ? BsonString.Emtpy : new BsonString(value);

    // Binary
    public static implicit operator byte[](BsonValue value) => value.AsBinary;

    public static implicit operator BsonValue(byte[] value) => new BsonBinary(value);

    // Guid
    public static implicit operator Guid(BsonValue value) => value.AsGuid;

    public static implicit operator BsonValue(Guid value) => new BsonGuid(value);

    // Boolean
    public static implicit operator bool(BsonValue value) => value.AsBoolean;

    public static implicit operator BsonValue(bool value) => value ? BsonBoolean.True : BsonBoolean.False;

    // ObjectId
    public static implicit operator ObjectId(BsonValue value) => value.AsObjectId;

    public static implicit operator BsonValue(ObjectId value) => new BsonObjectId(value);

    // DateTime
    public static implicit operator DateTime(BsonValue value) => value.AsDateTime;

    public static implicit operator BsonValue(DateTime value) => new BsonDateTime(value);

    #endregion

    #region IsTypes

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsNull => this.Type == BsonType.Null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsArray => this.Type == BsonType.Array;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDocument => this.Type == BsonType.Document;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsInt32 => this.Type == BsonType.Int32;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsInt64 => this.Type == BsonType.Int64;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDouble => this.Type == BsonType.Double;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDecimal => this.Type == BsonType.Decimal;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsNumber => this.IsInt32 || this.IsInt64 || this.IsDouble || this.IsDecimal;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsBinary => this.Type == BsonType.Binary;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsBoolean => this.Type == BsonType.Boolean;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsString => this.Type == BsonType.String;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsObjectId => this.Type == BsonType.ObjectId;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsGuid => this.Type == BsonType.Guid;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDateTime => this.Type == BsonType.DateTime;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsMinValue => this.Type == BsonType.MinValue;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsMaxValue => this.Type == BsonType.MaxValue;

    #endregion

    #region Convert Types

    public virtual bool ToBoolean() => true;

    public virtual int ToInt32() => throw new NotSupportedException($"{this.Type} does not support ToInt32.");

    public virtual long ToInt64() => throw new NotSupportedException($"{this.Type} does not support ToInt32.");

    public virtual double ToDouble() => throw new NotSupportedException($"{this.Type} does not support ToDouble.");

    public virtual decimal ToDecimal() => throw new NotSupportedException($"{this.Type} does not support ToDecimal.");

    #endregion

}
