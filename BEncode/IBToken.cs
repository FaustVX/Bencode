using System.Runtime.CompilerServices;

namespace BEncode;

public interface IBTokenValue
{
    public object Value { get; set; }
    public abstract ReadOnlySpan<byte> Encode();
    public abstract string ToString();
}

public interface IBToken : IBTokenValue
{
    public static IBToken Decode(SliceableStream data, out int length)
    => data switch
    {
        [(byte)'i', ..] => BInteger.DecodeImpl(data, out length),
        [(byte)'l', ..] => BList.DecodeImpl(data, out length),
        [(byte)'d', ..] => BDictionary.DecodeImpl(data, out length),
        [>= (byte)'1' and <= (byte)'9', ..] => BString.DecodeImpl(data, out length),
        _ => OnDecodeError(data, out length),
    };

    private static IBToken OnDecodeError(SliceableStream stream, out int length, [CallerArgumentExpression(nameof(stream))]string name = null!)
    {
        using var sr = new StreamReader(stream);
        length = 0;
        var s = sr.ReadToEnd();
        System.Diagnostics.Debugger.Break();
        throw new ArgumentOutOfRangeException(name, $"{name} must start with 'i', 'l', 'd', or an ASCII-encoded integer");
    }
    protected static abstract IBToken DecodeImpl(SliceableStream data, out int length);
}
