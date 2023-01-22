using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Web;

namespace Torrents;

public sealed class Client
{
    private static string _peerID = GetPeerID();

    public static async Task<BDictionary> GetTorrentFile(Uri uri)
    {
        var task = await new HttpClient().GetAsync(uri);
        var data = task.Content.ReadAsStream();
        var token = IBToken.Decode(new(data), out var length);
        if (length == data.Length)
            return (BDictionary)token;
        throw new Exception();
    }

    public static async Task<IPEndPoint[]> Announce(BDictionary torrent)
    {
        var announce = ((BString)torrent.Value["announce"]).Value;
        var info = (BDictionary)torrent.Value["info"];
        var byteLength = ((BInteger)info.Value["length"]).Value;
        var infoHash = SHA1.HashData(info.Encode());
        var announceKeyed = $"{announce}?info_hash={HttpUtility.UrlEncode(infoHash)}&peer_id={_peerID}&port=56881&uploaded=0&downloaded=0&left={byteLength.ToString()}&compact=0";
        var get = await new HttpClient()
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", "MyTorent 1.0" },
                { "Connection", "close" },
            }
        }.GetAsync(announceKeyed);
        get.EnsureSuccessStatusCode();
        var response = (BDictionary)IBToken.Decode(new(get.Content.ReadAsStream()), out var length);
        return ((BString)response.Value["peers"]).RawValue.Chunk(6)
            .Select(static peer => (ip: peer.AsMemory(..4), port: peer.AsMemory(^2)))
            .Select(static peer => new System.Net.IPEndPoint(new System.Net.IPAddress(peer.ip.Span), (peer.port.Span[0] << 8) | peer.port.Span[1]))
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
