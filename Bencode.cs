namespace Torrents;

public interface IBTokenValue
{
    public object Value { get; set; }
    public abstract ReadOnlySpan<byte> Encode();
    public abstract string ToString();
}

public interface IBToken : IBTokenValue
{
    public static IBToken Decode(ReadOnlySpanStream data, out int length)
    => data switch
    {
        [(byte)'i', ..] => BInteger.DecodeImpl(data, out length),
        [(byte)'l', ..] => BList.DecodeImpl(data, out length),
        [(byte)'d', ..] => BDictionary.DecodeImpl(data, out length),
        [>= (byte)'1' and <= (byte)'9', ..] => BString.DecodeImpl(data, out length),
        _ => throw new ArgumentOutOfRangeException(nameof(data), $"{nameof(data)} must start with 'i', 'l', 'd', or an ASCII-encoded integer"),
    };
    protected static abstract IBToken DecodeImpl(ReadOnlySpanStream data, out int length);
}
