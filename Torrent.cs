using System.Net;
using System.Security.Cryptography;
using BEncode;

namespace Torrents;

public sealed class Torrent
{
    private readonly Client _client;

    public BDictionary Datas { get; }
    public ReadOnlyMemory<IPEndPoint> Peers { get; private set; }

    public Torrent(BDictionary dictionary, Client client)
    {
        Datas = dictionary;
        _client = client;
    }

    public async Task Announce()
    {
        Peers = await _client.Announce(this);
    }

    public static Torrent Create(BEncode.BDictionary dictionary, Client client)
    {
        var info = (BDictionary)dictionary.Value["info"];
        var sha1 = SHA1.HashData(info.Encode());
        var storage = new BDictionary()
        {
            Value =
            {
                ["sha1"] = new BString(sha1),
                ["sha1_url"] = new BString(System.Web.HttpUtility.UrlEncode(sha1)),
            },
        };
        dictionary.Value[nameof(storage)] = storage;
        return new(dictionary, client);
    }
}
