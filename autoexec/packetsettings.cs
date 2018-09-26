$pref::Net::PacketRateToClient = "32"; //determines how many packets per second sent to each client
$pref::Net::PacketRateToServer = "32"; //may determine how many packets are allowed from each client
$pref::Net::PacketSize = "350"; //size of each packet sent to each client, maximum.has no effect on size of packets client send to the server


setlogmode(0);
// leave this set to zero unless you are coding and need a log it will make a huge file...!!!

$logechoenabled=0;
//set to 1 you can now see game details in console.  Thanks to tubaguy.

SetPerfCounterEnable(0);
//server stutter fix