//Vote to toggle chaingun.

//Defaults are set at server start.
function VoteChaingunStart()
{
	if($Host::EnableChaingun)
	{
		if(isActivePackage(DisableChaingun))
			deactivatePackage(DisableChaingun);
		
		$InvBanList[CTF, "Chaingun"] = 0;
		$InvBanList[SCtF, "Chaingun"] = 0;
	}
	else
	{
		if(!isActivePackage(DisableChaingun))
			activatePackage(DisableChaingun);
		
		$InvBanList[CTF, "Chaingun"] = 1;
		$InvBanList[SCtF, "Chaingun"] = 1;
	}
}

$VoteMessage["VoteChaingun"] = "Turn";

//Vote stuff
//In defaultgame.ovl evo
//
//
//function CTFGame::sendGameVoteMenu( %game, %client, %key )
//{
//	//From CFTGame.cs
//	DefaultGame::sendGameVoteMenu(%game, %client, %key);
//	
//	parent::sendGameVoteMenu( %game, %client, %key );
//	
//	%isAdmin = ( %client.isAdmin || %client.isSuperAdmin );
//
//	if( %game.scheduleVote $= "" )
//	{
//		if(%isAdmin)
//		{
//			if(!$Host::EnableChaingun)
//				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChaingun', 'Enable Chaingun.', 'Enable Chaingun' );
//			else
//				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChaingun', 'Disable Chaingun.', 'Disable Chaingun' );
//		}
//		else if (%client.ForceVote > 0)
//		{
//			if(!$Host::EnableChaingun)
//				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChaingun', 'Enable Chaingun.', 'Vote to enable Chaingun' );
//			else
//				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChaingun', 'Disable Chaingun.', 'Vote to disable Chaingun' );
//		}
//		else
//		{
//			if(!$Host::EnableChaingun)
//				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChaingun', 'Enable Chaingun.', 'Vote to enable Chaingun' );
//			else
//				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteChaingun', 'Disable Chaingun.', 'Vote to disable Chaingun' );
//		}      
//	}
//}
//
//
//function CTFGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4)
//{	
//	parent::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4);
//	
//	switch$ (%typeName)
//	{
//	case "VoteChaingun":
//		%game.VoteChaingun(%admin, %arg1, %arg2, %arg3, %arg4);
//	}
//}
//
//
//In Admin.ovl evo
//
//
//      case "VoteChaingun":
//         if( %isAdmin && !%client.ForceVote )
//         {
//            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
//			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
//         }
//         else
//         {
//            if(Game.scheduleVote !$= "")
//            {
//               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
//               return;
//            }
//			%actionMsg = ($Host::EnableChaingun ? "disable chaingun" : "enable chaingun");
//            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
//            {
//               %cl = ClientGroup.getObject(%idx);
//               if(!%cl.isAIControlled())
//               {
//                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
//                  %clientsVoting++;
//               }
//            }
//            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
//			$VoteSoundInProgress = true;
//			schedule(10000, 0, "VoteSound", %game);
//         }
//


function DefaultGame::VoteChaingun(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
	if(%admin)
	{
		if($Host::EnableChaingun)
		{
			messageAll('MsgAdminForce', '\c2The Admin has disabled chaingun.');

			$InvBanList[CTF, "Chaingun"] = 1;
			$InvBanList[SCtF, "Chaingun"] = 1;
			
			if(!isActivePackage(DisableChaingun))
				activatePackage(DisableChaingun);

			$Host::EnableChaingun = 0;
			
			ResetVOall(%game);
		}
		else
		{
			messageAll('MsgAdminForce', '\c2The Admin has enabled chaingun.');

			$InvBanList[CTF, "Chaingun"] = 0;
			$InvBanList[SCtF, "Chaingun"] = 0;

			if(isActivePackage(DisableChaingun))
				deactivatePackage(DisableChaingun);
				
			$Host::EnableChaingun = 1;
			
			ResetVOall(%game);
		}
	}
	else 
	{
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / ClientGroup.getCount()) > ($Host::VotePasspercent / 100))
		{
			if($Host::EnableChaingun)
			{
				messageAll('MsgVotePassed', '\c2Chaingun has been disabled by vote: %1 percent.', mFloor(%game.totalVotesFor/ClientGroup.getCount() * 100));

				$InvBanList[CTF, "Chaingun"] = 1;
				$InvBanList[SCtF, "Chaingun"] = 1;
				
				if (!isActivePackage(DisableChaingun))
					activatePackage(DisableChaingun);

				$Host::EnableChaingun = 0;
				
				ResetVOall(%game);
			}
			else
			{
				messageAll('MsgVotePassed', '\c2Chaingun has been enabled by vote: %1 percent.', mFloor(%game.totalVotesFor/ClientGroup.getCount() * 100));

				$InvBanList[CTF, "Chaingun"] = 0;
				$InvBanList[SCtF, "Chaingun"] = 0;
				
				if(isActivePackage(DisableChaingun))			
					deactivatePackage(DisableChaingun);

				$Host::EnableChaingun = 1;
				
				ResetVOall(%game);
			}
		}
		else
			messageAll('MsgVoteFailed', '\c2Vote to change chaingun status did not pass: %1 percent.', mFloor(%game.totalVotesFor/ClientGroup.getCount() * 100));
			ResetVOall(%game);
	}
}

package VoteChaingun
{

//So player wont spawn with chaingun
function CTFGame::playerSpawned(%game, %player)
{
	//call the default stuff first...
	DefaultGame::playerSpawned(%game, %player);
	
	if(!$Host::EnableChaingun)
	{
		%player.clearInventory();
		%player.setInventory(Disc,1);
		%player.setInventory(Blaster,1);
		%player.setInventory(DiscAmmo,15);
		%player.setInventory(Grenade,5);
		%player.setInventory(RepairKit,1);
		%player.use("Disc");
	}
}

function SCtFGame::playerSpawned(%game, %player)
{
	//call the default stuff first...
	DefaultGame::playerSpawned(%game, %player);
	
	if(!$Host::EnableChaingun)
	{
		%player.clearInventory();
		%player.setInventory(Disc,1);
		%player.setInventory(Blaster,1);
		%player.setInventory(DiscAmmo,15);
		%player.setInventory(Grenade,5);
		%player.setInventory(RepairKit,1);
		%player.use("Disc");
	}
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(VoteChaingun))
    activatePackage(VoteChaingun);

//So if the player is able to get a cg, he cant use it
package DisableChaingun
{

function ChaingunImage::onFire(%data,%obj,%slot)
{
	//Nothing
}

};

