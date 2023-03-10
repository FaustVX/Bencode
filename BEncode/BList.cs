namespace BEncode;

public sealed class BList : IBToken
{
    public List<IBTokenValue> Value { get; set; } = new();
    object IBTokenValue.Value
    {
        get => Value;
        set => Value = value switch
        {
            List<IBTokenValue> l => l,
            IList<IBTokenValue> il => new(il),
        };
    }

    public static IBToken DecodeImpl(SliceableStream data, out int length)
    {
        if (data[0] != (byte)'l')
            throw new ArgumentException("Message should start with 'l'", nameof(data));
        var list = new BList();
        length = 1;
        while (data[length] != (byte)'e')
        {
            list.Value.Add(IBToken.Decode(data[length..], out var l));
            length += l;
        }
        length++;
        return list;
    }

    public ReadOnlySpan<byte> Encode()
    {
        var values = Value.Select(static token => token.Encode().ToArray()).ToArray();
        var result = new List<byte>(values.Sum(static a => a.Length) + 2);
        result.Add((byte)'l');
        foreach (var value in values)
            result.AddRange(value);
        result.Add((byte)'e');
        return System.Runtime.InteropServices.CollectionsMarshal.AsSpan(result);
    }

    public override string ToString()
    => Value.ToString()!;
}
