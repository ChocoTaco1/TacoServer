// Memory Patches

// Remote Code Execution Patch by Bahke
memPatch("A3C300","A370C3A300E8D609A0FF8B46205053E98103A0FF");
memPatch("A3C330","C70570C3A30000000000E8A109A0FF8B462085C0E96D03A0FF");
memPatch("A3C400","E80BFB9FFF6089C38B1570C3A300B8FF00000029D039C37D0661E92509A0FFA380C3A30061A180C3A300E91509A0FF");
memPatch("A3C430","E8DBFA9FFF6089C38B1570C3A300B8FF00000029D039C37D0661E9A009A0FFA380C3A30061A180C3A300E99009A0FF");
memPatch("43C68B","E970FC5F00");
memPatch("43C6AC","E97FFC5F00");
memPatch("43CD3F","E9BCF65F00");
memPatch("43CDEA","E941F65F00");

//Nope...Try Agian fix - telnet
memPatch("756076","6169");

// Thanks Turkeh
// TraversalRoot Console spam fix
function suppressTraversalRootPatch()
{
   if($tvpatched)
      return;

   warn("Patching traversal root error...");
   memPatch("56AD8A", "90909090909090909090909090909090909090909090");
   memPatch("56D114", "90909090909090909090909090909090909090909090");
   $tvpatched = 1;
}

if (!$CmdArmor::Patched)
{
	$CmdArmor::Patched = true;
	//memPatch("6FC746", "66B8000090906683FE017408ACAA84C075FA89D05F5EC3");
	memPatch("6FC746", "83FE017408ACAA84C075FA89D05F5EC3");
	//Removed register size override (cmp si, 1 ->  cmp esi, 1) and got rid of
	//weird NASM garbage code at the beginning. Had a mov ax, 0 which did nothing
	//and wasn't necessary anyways because of xor eax, eax in the original. It also
	//generated several NOPs after that for no reason.
}

function serverCmd(%client)
{
	// Stick your own administrative action code here
	messageAll('msgAll',"\c3" @ %client.namebase SPC "is attempting to crash the server!");

	messageClient(%client, 'onClientBanned', "");
	messageAllExcept( %client, -1, 'MsgClientDrop', "", %client.name, %client );
   
	// kill and delete this client
	if( isObject(%client.player) )
		%client.player.scriptKill(0);
   
	if ( isObject( %client ) )
	{
		%client.setDisconnectReason("You have been banned for attempting to crash the server.");
		%client.schedule(700, "delete");
	}
   
	BanList::add(%client.guid, %client.getAddress(), $Host::BanTime);
}