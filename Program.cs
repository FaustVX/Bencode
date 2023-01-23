using Torrents;

var client = new Client(56881);
var singleFile = await client.GetTorrentFile(new(@"http://ftp.crifo.org/debian-cd/current/amd64/bt-cd/debian-11.6.0-amd64-netinst.iso.torrent"));
await singleFile.Announce();
var multipleFiles = await client.GetTorrentFile(new(@"https://archive.org/download/opensuse-11.3_release/opensuse-11.3_release_archive.torrent"));
await multipleFiles.Announce();
;
