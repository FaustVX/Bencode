using System.Net;
using System.Numerics;
using BEncode;

namespace Torrents;

public sealed class Client
{
    public ushort Port { get; }

    public Client(ushort port)
    {
        Port = port;
        _urlQueryPort = string.Format(_urlQuery, Port.ToString());
    }

    private static readonly string _peerID = GetPeerID();
    private static readonly string _urlQuery = $"&peer_id={_peerID}&port={{0}}&compact=0";
    private readonly string _urlQueryPort = default!;

    public IReadOnlyList<Torrent> Torrents => _torrents;
    private readonly List<Torrent> _torrents = new();

    public async Task<Torrent> GetTorrentFile(Uri uri)
    {
        var task = await new HttpClient().GetAsync(uri);
        var data = task.Content.ReadAsStream();
        var token = (BDictionary)IBToken.Decode(new(data), out var length);
        if (length != data.Length)
            throw new Exception();

        var torrent = Torrent.Create(token, this);
        _torrents.Add(torrent);
        return torrent;
    }

    public async Task<IPEndPoint[]> Announce(Torrent torrent)
    {
        var announce = ((BString)torrent.Datas.Value["announce"]).Value;
        var info = (BDictionary)torrent.Datas.Value["info"];
        var storage = (BDictionary)torrent.Datas.Value["storage"];

        var infoHash = ((BString)storage.Value["sha1_url"]).Value;
        var byteLength = torrent.TotalLength;
        announce = $"{announce}?info_hash={infoHash}&uploaded=0&downloaded=0&left={byteLength.ToString()}{_urlQueryPort}";

        var get = await new HttpClient()
        {
            DefaultRequestHeaders =
                {
                    { "User-Agent", "MyTorent 1.0" },
                    { "Connection", "close" },
                }
        }.GetAsync(announce);

#if !DEBUG
            get.EnsureSuccessStatusCode();
#endif

        var response = (BDictionary)IBToken.Decode(new(get.Content.ReadAsStream()), out var length);
        if (length != get.Content.Headers.ContentLength)
            throw new Exception();

        var peers = response.Value["peers"] switch
        {
            BString s => s.RawValue.Chunk(6)
                .Select(static peer => (ip: peer.AsMemory(..4), port: (peer[^2] << 8) | peer[^1])),
            BDictionary d => d.Value.Values.Cast<BDictionary>().Select(peer => (ip: ((BString)peer.Value["ip"]).RawValue.AsMemory(), port: (int)((BInteger)peer.Value["port"]).Value))
        };
        return peers
            .Select(static peer => new System.Net.IPEndPoint(new System.Net.IPAddress(peer.ip.Span), peer.port))
            .ToArray();
    }

    private static string GetPeerID()
    {
        var peerId = new byte[10];
        Random.Shared.NextBytes(peerId);
        peerId = System.Text.Encoding.UTF8.GetBytes(new BigInteger(peerId).ToString("x20"));
        "-VX1000-"u8.CopyTo(peerId);
        return System.Text.Encoding.UTF8.GetString(peerId);
    }
}
