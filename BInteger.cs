using System.Diagnostics;
using System.Text;

namespace Torrents;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class BInteger : IBToken
{
    public long Value { get; set; }
    object IBTokenValue.Value
    {
        get => Value;
        set => Value = (long)value;
    }

    public BInteger(long value)
    {
        Value = value;
    }

    public static IBToken DecodeImpl(SliceableStream data, out int length)
    {
        if (data[0] != (byte)'i')
            throw new ArgumentException("Message sould start with 'i'", nameof(data));
        (var value, length) = Parse(0, data[1..]);
        length++;
        return new BInteger(value);

        static (long value, int length) Parse(long value, SliceableStream data)
        => data switch
        {
            [var i and >= (byte)'0' and <= (byte)'9', (byte)'e', ..] => (value * 10 + (i - (byte)'0'), 2),
            [var i and >= (byte)'0' and <= (byte)'9', .. var tail] => Parse(value * 10 + (i - (byte)'0'), tail) switch { (var v, var l) => (v, l + 1) },
            [(byte)'e', ..] => throw new ArgumentException($"{nameof(data)} can't be empty", nameof(data)),
            _ => throw new ArgumentException(nameof(data)),
        };
    }

    public ReadOnlySpan<byte> Encode()
    => Encoding.UTF8.GetBytes($"i{Value}e");

    private string GetDebuggerDisplay()
    => ToString();

    public override string ToString()
    => Value.ToString();
}
