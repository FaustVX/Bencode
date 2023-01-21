namespace Torrents;

public class SliceableStream : Stream
{
    public SliceableStream(Stream stream)
    : this(stream, 0, (int)stream.Length)
    { }

    public SliceableStream(Stream stream, Range range)
    : this(stream, range.Start.GetOffset((int)stream.Length), GetRangeLength((int)stream.Length, range))
    { }

    private static int GetRangeLength(int length, Range range)
    => range.End.GetOffset(length) - range.Start.GetOffset(length);

    public SliceableStream(Stream stream, int start, int length)
    {
        if (stream is not { CanSeek: true, CanRead: true })
            throw new Exception();
        (_offset, Parent) = (stream is SliceableStream { _offset: var offset, Parent : var parent })
            ? (offset + start, parent)
            : (start, stream);
        Parent.Seek(_offset, SeekOrigin.Begin);
        Length = Math.Min(length, Parent.Length - _offset);
        _offsetFromEnd = Parent.Length - (Length + _offset);
    }

    public override bool CanRead => Parent.CanRead;

    public override bool CanSeek => Parent.CanSeek;

    public override bool CanWrite => Parent.CanWrite;

    public override long Length { get; }

    public override long Position
    {
        get => Parent.Position - _offset;
        set => Parent.Position = value + _offset;
    }
    public Stream Parent { get; }
    private readonly long _offset;
    private readonly long _offsetFromEnd;

    public int Count => (int)Length;

    public byte this[int pos]
    {
        get
        {
            var current = Parent.Position;
            Parent.Seek(pos + _offset, SeekOrigin.Begin);
            var data = (byte)Parent.ReadByte();
            Parent.Seek(current, SeekOrigin.Begin);
            return data;
        }
    }

    public SliceableStream this[Range range] => new(this, range);

    public SliceableStream Slice(int start, int length)
    => new(this, start, length);

    public override void Flush()
    => Parent.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    => Parent.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin)
    => origin switch
    {
        SeekOrigin.Begin => Parent.Seek(offset + _offset, origin),
        SeekOrigin.Current => Parent.Seek(offset, origin),
        SeekOrigin.End => Parent.Seek(offset + _offsetFromEnd, origin),
    };

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    => Parent.Write(buffer, offset, count);
}
