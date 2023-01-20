var data = System.Text.Encoding.UTF8.GetBytes("3:bar");
var token = Torrents.IBToken.Decode(new(data), out var length);
var encode = token.Encode();
var ok = length == data.Length;
var same = encode.SequenceEqual(data);
;

token = new Torrents.BString("Hello");
token = new Torrents.BList()
{
    Value =
    {
        token,
        token,
    },
};
token = new Torrents.BDictionary()
{
    Value =
    {
        ["Hello"] = token,
    },
};
;
