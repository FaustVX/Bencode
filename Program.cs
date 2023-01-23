using Torrents;

var client = new Client(56881);
var token = await client.GetTorrentFile(new(@".torrent url"));
await token.Announce();
;
