using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Web;
using Torrents;

// new Thread(TCP)
// {
//     Name = "TCP",
// }.Start();

var task = await new HttpClient().GetAsync(@".torrent url");
var data = task.Content.ReadAsStream();
var token = (BDictionary)IBToken.Decode(new(data), out var length);
var peerID = GetPeerID();
if (length == data.Length)
{
    var announce = ((BString)token.Value["announce"]).Value;
    var info = (BDictionary)token.Value["info"];
    var byteLength = ((BInteger)info.Value["length"]).Value;
    var infoHash = SHA1.HashData(info.Encode());
    var announceKeyed = $"{announce}?info_hash={HttpUtility.UrlEncode(infoHash)}&peer_id={peerID}&port=56881&uploaded=0&downloaded=0&left={byteLength.ToString()}&compact=0";
    var get = await new HttpClient()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "MyTorent 1.0" },
            { "Connection", "close" },
        }
    }.GetAsync(announceKeyed);
    get.EnsureSuccessStatusCode();
    var response = (BDictionary)IBToken.Decode(new(get.Content.ReadAsStream()), out length);
    var peers = ((BString)response.Value["peers"]).RawValue.Chunk(6)
        .Select(static peer => new System.Net.IPEndPoint(new System.Net.IPAddress(peer.AsSpan(0, 4)), (peer[4] << 8) | peer[2]))
        .ToArray();
}

static string GetPeerID()
{
    var peerId = new byte[10];
    Random.Shared.NextBytes(peerId);
    peerId = System.Text.Encoding.UTF8.GetBytes(new BigInteger(peerId).ToString("x20"));
    "-VX1000-"u8.CopyTo(peerId);
    return System.Text.Encoding.UTF8.GetString(peerId);
}

static void TCP()
{
    TcpListener server = null!;
    try
    {
        server = new TcpListener(System.Net.IPAddress.Any, 18000);

        // Start listening for client requests.
        server.Start();

        // Buffer for reading data
        var bytes = new Byte[256];

        // Enter the listening loop.
        while (true)
        {
            Console.Write("Waiting for a connection... ");

            // Perform a blocking call to accept requests.
            using var client = server.AcceptTcpClient();
            Console.WriteLine($"Connected from {client.Client.RemoteEndPoint}");
            using var stream = client.GetStream();

            // Loop to receive all the data sent by the client.
            while (stream.Read(bytes, 0, bytes.Length) is var i and not 0)
            {
                var data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                Console.Write("Received: {0}", data);
            }

            stream.Write("hello"u8);
        }
    }
    catch (SocketException e)
    {
        Console.WriteLine("SocketException: {0}", e);
    }
    finally
    {
        server?.Stop();
    }

    Console.WriteLine("\nHit enter to continue...");
    Console.Read();
}
