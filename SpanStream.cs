namespace Torrents;

public class SpanStream : Stream
{
    public SpanStream(Memory<byte> span)
    => _memory = span;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _memory.Length;

    public override long Position { get; set; }

    private readonly Memory<byte> _memory;

    public ReadOnlySpan<byte> Span => _memory[(int)Position..].Span;

    public override void Flush()
    { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var destination = buffer.AsMemory(offset, count);
        _memory.Slice((int)Position, count).CopyTo(destination);
        var read = (int)Math.Min(destination.Length, Length - Position);
        Position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    => Position = origin switch
    {
        SeekOrigin.Begin => offset,
        SeekOrigin.Current => Position + offset,
        SeekOrigin.End => Length - offset,
    };

    public override void SetLength(long value)
    => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        var destination = buffer.AsMemory(offset, count);
        destination.CopyTo(_memory.Slice((int)Position, count));
        var read = (int)Math.Min(destination.Length, Length - Position);
        Position += read;
    }
}
