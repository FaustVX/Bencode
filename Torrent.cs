using System.Net;
using System.Security.Cryptography;
using BEncode;

namespace Torrents;

public sealed class Torrent
{
    private readonly Client _client;

    public BDictionary Datas { get; }
    public ReadOnlyMemory<IPEndPoint> Peers { get; private set; }
    public ReadOnlyMemory<File> Pathes { get; }
    public long TotalLength { get; }

    public Torrent(BDictionary dictionary, Client client)
    {
        Datas = dictionary;
        _client = client;
        var info = (BDictionary)Datas.Value["info"];
        if (info.Value.TryGetValue("length", out BInteger length))
        {
            Pathes = new File[]
            {
                new()
                {
                    Name = ((BString)info.Value["name"]).Value,
                    Length = length.Value,
                },
            };
            TotalLength = length.Value;
        }
        else if (info.Value.TryGetValue("files", out BList files))
        {
            var pathes = files.Value.Cast<BDictionary>().Select(static file => new File()
            {
                Length = ((BInteger)file.Value["length"]).Value,
                Name = Path.Combine(((BList)file.Value["path"]).Value.Cast<BString>().Select(static path => path.Value).ToArray()),
            }).ToArray();
            Pathes = pathes;
            TotalLength = pathes.Sum(static file => file.Length);
        }
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
