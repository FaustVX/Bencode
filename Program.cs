var data = "3:bar"u8;
var token = Torrents.IBToken.Decode(data, out var length);
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
