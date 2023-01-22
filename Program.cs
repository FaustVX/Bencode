using Torrents;

var token = await Client.GetTorrentFile(new(@".torrent url"));
var peers = await Client.Announce(token);
;
