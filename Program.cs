using Torrents;

var data = System.Text.Encoding.UTF8.GetBytes("d3:bar4:spam3:fooi42ee");
var token = IBToken.Decode(new(new ReadOnlySpanStream(data)), out var length);
var encode = token.Encode();
var ok = length == data.Length;
var same = encode.SequenceEqual(data);
;

token = new BString("Hello");
token = new BList()
{
    Value =
    {
        token,
        token,
    },
};
token = new BDictionary()
{
    Value =
    {
        ["Hello"] = token,
    },
};
;
