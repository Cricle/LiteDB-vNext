﻿namespace LiteDB;

/// <summary>
/// Represent a document (list of key-values of BsonValue) in Bson object model
/// </summary>
public class BsonDocument : BsonValue, IDictionary<string, BsonValue>
{
    private readonly Dictionary<string, BsonValue> _value;

    private int _length = -1;

    public IReadOnlyDictionary<string, BsonValue> Value => _value;

    public BsonDocument() : this(0)
    {
    }

    public BsonDocument(int capacity)
    {
        _value = new(capacity, StringComparer.OrdinalIgnoreCase);
    }

    public BsonDocument(Dictionary<string, BsonValue> value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override BsonType Type => BsonType.Document;

    public override int GetBytesCount()
    {
        var length = 4;

        foreach (var element in _value)
        {
            length += GetBytesCountElement(element.Key, element.Value);
        }

        _length = length;

        return length;
    }

    internal override int GetBytesCountCached()
    {
        if (_length >= 0) return _length;

        return this.GetBytesCount();
    }

    /// <summary>
    /// Get all document elements - Return "_id" as first of all (if exists)
    /// </summary>
    public IEnumerable<KeyValuePair<string, BsonValue>> GetElements()
    {
        if (_value.TryGetValue("_id", out var id))
        {
            yield return new ("_id", id);
        }

        foreach (var item in _value.Where(x => x.Key != "_id"))
        {
            yield return item;
        }
    }

    public override int GetHashCode() => this.Value.GetHashCode();

    #region Implement CompareTo

    public override int CompareTo(BsonValue other, Collation collation)
    {
        if (other is BsonDocument otherDocument)
        {
            var thisKeys = this.Keys.ToArray();
            var thisLength = thisKeys.Length;

            var otherKeys = otherDocument.Keys.ToArray();
            var otherLength = otherKeys.Length;

            var result = 0;
            var i = 0;
            var stop = Math.Min(thisLength, otherLength);

            for (; 0 == result && i < stop; i++)
                result = this[thisKeys[i]].CompareTo(other[thisKeys[i]], collation);

            // are different
            if (result != 0) return result;

            // test keys length to check which is bigger
            if (i == thisLength) return i == otherLength ? 0 : -1;

            return 1;
        }

        return this.CompareType(other);
    }

    #endregion

    #region IDictionary implementation

    public ICollection<string> Keys => _value.Keys;

    public ICollection<BsonValue> Values => _value.Values;

    public int Count => _value.Count;

    public bool IsReadOnly => false;

    public override BsonValue this[string key]
    {
        get => _value.GetOrDefault(key, BsonValue.Null); 
        set => _value[key] = value ?? BsonValue.Null;
    }

    public void Add(string key, BsonValue value) => _value.Add(key, value ?? BsonValue.Null);

    public bool ContainsKey(string key) => _value.ContainsKey(key);

    public bool Remove(string key) => _value.Remove(key); 

    public bool TryGetValue(string key, out BsonValue value) => _value.TryGetValue(key, out value);

    public void Add(KeyValuePair<string, BsonValue> item) => _value.Add(item.Key, item.Value ?? BsonValue.Null);

    public void Clear() => _value.Clear();

    public bool Contains(KeyValuePair<string, BsonValue> item) => _value.Contains(item);

    public void CopyTo(KeyValuePair<string, BsonValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, BsonValue>>)_value).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, BsonValue> item) => _value.Remove(item.Key);

    public IEnumerator<KeyValuePair<string, BsonValue>> GetEnumerator() => _value.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _value.GetEnumerator();

    #endregion

    #region Convert Types

    public override string ToString() => "{" + String.Join(",", this.GetElements().Select(x => x.Key + ":" + x.Value.ToString())) + "}";

    #endregion

    #region Static Helpers

    /// <summary>
    /// Get how many bytes one single element will used in BSON format
    /// </summary>
    internal static int GetBytesCountElement(string key, BsonValue value)
    {
        // check if data type is variant
        var variant = value.Type == BsonType.String || value.Type == BsonType.Binary;

        return
            Encoding.UTF8.GetByteCount(key) + // CString
            1 + // CString \0
            1 + // element type
            value.GetBytesCountCached() +
            (variant ? 4 : 0); // bytes.Length Int32
    }

    #endregion
}
