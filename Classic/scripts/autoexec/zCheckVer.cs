// TribesNext Minimum Version Enforcement
// Written by Thyth
// 2014-08-18

// Updated on 2014-08-31 after testing/feedback from Heat Killer.

// This script prevents clients from joining a non-observer team if they are not running
// TribesNext RC2a or newer, with the tournamentNetClient.vl2 installed. An early form of
// anticheat was added to the RC2 patch that kills HM2. This script allows detecting of
// a new enough version by the interaction with the TribesNext community/browser system.
// Support for clan tags (and account renaming) was added along with the HM2 killer in RC2,
// but no client side code to talk to the browser server was in yet. Now that the browser
// system backend is complete, all clients can install the tournamentNetClient to the
// browser, and users running RC2 (with HM2 killer) can be detected.

// The variable on the client object:
// %client.t2csri_sentComCertDone
// Will be 1 if they are running RC2+ with tournamentNetClient.vl2

// Admins can override this restriction when forcing players to join a team.


//Added -ChocoTaco
//Toggle Touney Net Client
//$Host::EnableNetTourneyClient = 1; 
$CheckVerObserverRunOnce = false;
$CheckVerObserverTrys = 0;

//Added -ChocoTaco
//Run in GetCounts.cs
//Coming from other modes, checks all %clients and put them into observer with a Version check fail
function CheckVerObserver(%client)
{
	if($CurrentMissionType $= "CTF" && $Host::EnableNetTourneyClient && !$CheckVerObserverRunOnce && !$Host::TournamentMode)
	{
		if (!%client.t2csri_sentComCertDone)
		{
			messageClient(%client, 'MsgClientCheckObserver', '\c2Tribesnext version check has failed.');
			serverCmdClientMakeObserver( %client );
		}
		
		$CheckVerObserverTrys++; if($CheckVerObserverTrys $= $AllPlayerCount) $CheckVerObserverRunOnce = true;
		//echo($CheckVerObserverTrys);
	}
}

//Added -ChocoTaco
//Run in PUGpasscheck.cs. Reset when the server leaves ctf and when its reenabled thru admin options.
function CheckVerObserverReset()
{
	$CheckVerObserverRunOnce = false;
	$CheckVerObserverTrys = 0;
}


//Original
function checkVer_showBanner(%client)
{
	// customize me
	commandToClient(%client, 'CenterPrint', "<font:Sui Generis:22><color:3cb4b4>Version Check Failed!\n<font:Univers:16><color:3cb4b4>You need the latest TribesNext patch and TourneyNetClient2 to play.\n Download it from t2discord.tk and drop it into your GameData/Base folder.", 10, 3);
}

package checkver
{
	function serverCmdClientJoinTeam(%client, %team)
	{
		if($CurrentMissionType $= "CTF" && $Host::EnableNetTourneyClient) //Added -ChocoTaco
		{
			if (!%client.t2csri_sentComCertDone)
			{
				checkVer_showBanner(%client);
				return;
			}
		}
		Parent::serverCmdClientJoinTeam(%client, %team);
	}
	function serverCmdClientJoinGame(%client)
	{
		if($CurrentMissionType $= "CTF" && $Host::EnableNetTourneyClient) //Added -ChocoTaco
		{	
			if (!%client.t2csri_sentComCertDone)
			{
				checkVer_showBanner(%client);
				return;
			}
		}
		Parent::serverCmdClientJoinGame(%client);
	}
	function serverCmdClientPickedTeam(%client, %option)
	{
		Parent::serverCmdClientPickedTeam(%client, %option); //Put first -ChocoTaco
		
		if($CurrentMissionType $= "CTF" && $Host::EnableNetTourneyClient) //Added -ChocoTaco
		{		
			if (!%client.t2csri_sentComCertDone)
			{
				if($Host::TournamentMode && %client.team !$= 0) //Added -ChocoTaco
					serverCmdClientMakeObserver( %client ); 	
				checkVer_showBanner(%client);
				return;
			}
		}
	}
	function serverCmdClientTeamChange(%client, %option)
	{
		if($CurrentMissionType $= "CTF" && $Host::EnableNetTourneyClient) //Added -ChocoTaco
		{		
			if (!%client.t2csri_sentComCertDone)
			{
				checkVer_showBanner(%client);
				return;
			}
		}
		Parent::serverCmdClientTeamChange(%client, %option);
	}
	function Observer::onTrigger(%data, %obj, %trigger, %state)
	{	
		if($CurrentMissionType $= "CTF" && $Host::EnableNetTourneyClient) //Added -ChocoTaco
		{	
			%client = %obj.getControllingClient();
			if (!%client.t2csri_sentComCertDone)
			{			
				checkVer_showBanner(%client);
				return;
			}
		}
		Parent::onTrigger(%data, %obj, %trigger, %state);
	}	
};

activatePackage(checkver);



//Added -ChocoTaco
//Evo options
//
//In defaultgame.ovl DefaultGame::sendGameVoteMenu(%game, %client, %key) 
//		 
//		//Toggle Tournament Net Client
//		if(%client.isAdmin && $Host::EnableNetTourneyClient && $CurrentMissionType $= "CTF")
//			messageClient( %client, 'MsgVoteItem', "", %key, 'ToggleTourneyNetClient', 'Disable Tournament Net Client', "Disable Tournament Net Client" );
//		else if(%client.isAdmin && $CurrentMissionType $= "CTF")
//			messageClient( %client, 'MsgVoteItem', "", %key, 'ToggleTourneyNetClient', 'Enable Tournament Net Client', "Enable Tournament Net Client" );
//
//
//In admin.ovl serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %playerVote)
//
//	  case "ToggleTourneyNetClient":
//         if (%client.isAdmin)
//         {
//            if($Host::EnableNetTourneyClient)
//			{
//               $Host::EnableNetTourneyClient = 0;
//			   messageClient( %client, '', "Tournament Net Client checking has been disabled.~wfx/powered/vehicle_screen_on.wav" );
//			}
//            else
//            {
//               $Host::EnableNetTourneyClient = 1;
//			   messageClient( %client, '', "Tournament Net Client checking has been enabled.~wfx/powered/vehicle_screen_on.wav" );
//			   CheckVerObserverReset();
//			   ResetClientChangedTeams();
//            }
//         }
		
