// Memory Patches

//Nope...Try Agian fix - telnet
//From Krash
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

//Disable UE box on crash
//Used if a clean crash is desired
//memPatch("7dc7fc","90");

//Show Linux Icon in server list
//memPatch("5C9628","80CB05");

//Bahke MPB stability fix
memPatch("614120","9090");