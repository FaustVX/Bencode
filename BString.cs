using System.Text;

namespace Torrents;

public sealed class BString : IBToken
{
    public byte[] RawValue { get; set; } = default!;
    public string Value
    {
        get => Encoding.UTF8.GetString(RawValue);
        set => RawValue = Encoding.UTF8.GetBytes(value);
    }

    public BString(string value)
    {
        Value = value;
    }

    public BString(ReadOnlySpan<byte> value)
    {
        RawValue = value.ToArray();
    }

    public BString(byte[] value)
    {
        RawValue = value;
    }

    object IBTokenValue.Value
    {
        get => Value;
        set => Value = (string)value;
    }

    public static IBToken DecodeImpl(SliceableStream data, out int length)
    {
        (var size, var offset) = ParseLength(0, data);
        length = size + offset;
        var buffer = new byte[size];
        data.Slice(offset, size).Read(buffer);
        return new BString(buffer);

        static (int value, int length) ParseLength(int value, SliceableStream data)
        => data switch
        {
            [var i and >= (byte)'0' and <= (byte)'9', (byte)':', ..] => (value * 10 + (i - (byte)'0'), 2),
            [var i and >= (byte)'0' and <= (byte)'9', .. var tail] => ParseLength(value * 10 + (i - (byte)'0'), tail) switch { (var v, var l) => (v, l + 1) },
            [(byte)':', ..] => throw new ArgumentException($"{nameof(data)} can't be empty", nameof(data)),
            _ => throw new ArgumentException(nameof(data)),
        };
    }

    public ReadOnlySpan<byte> Encode()
    {
        var header = Encoding.UTF8.GetBytes($"{RawValue.Length}:");
        var result = new List<byte>(header.Length + RawValue.Length);
        result.AddRange(header);
        result.AddRange(RawValue);
        return System.Runtime.InteropServices.CollectionsMarshal.AsSpan(result);
    }

    public override string ToString()
    => Value;
}
