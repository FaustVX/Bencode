namespace Torrents;

public sealed class BDictionary : IBToken
{
    public Dictionary<string, IBTokenValue> Value { get; set; } = new();
    object IBTokenValue.Value
    {
        get => Value;
        set => Value = (Dictionary<string, IBTokenValue>)value;
    }

    public static IBToken DecodeImpl(ReadOnlySpan<byte> data, out int length)
    {
        if (data[0] != (byte)'d')
            throw new ArgumentException("Message sould start with 'd'", nameof(data));
        var list = new BDictionary();
        length = 1;
        while (data[length] != (byte)'e')
        {
            list.Value.Add(((BString)BString.DecodeImpl(data[length..], out var l1)).Value, IBToken.Decode(data[(length + l1)..], out var l2));
            length += l1 + l2;
        }
        length++;
        return list;
    }

    public ReadOnlySpan<byte> Encode()
    {
        var keys = Value.Keys.Select(static k => new BString(k).Encode().ToArray()).ToArray();
        var values = Value.Values.Select(static token => token.Encode().ToArray()).ToArray();
        var result = new List<byte>(keys.Sum(static a => a.Length) + values.Sum(static a => a.Length) + 2);
        result.Add((byte)'d');
        for (int i = 0; i < values.Length; i++)
        {
            result.AddRange(keys[i]);
            result.AddRange(values[i]);
        }

        result.Add((byte)'e');
        return System.Runtime.InteropServices.CollectionsMarshal.AsSpan(result);
    }

    public override string ToString()
    => Value.ToString()!;
}
