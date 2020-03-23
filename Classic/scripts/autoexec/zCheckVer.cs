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


// Added some things so it can be toggled in game. -ChocoTaco
// Toggle Tourney Net Client
// $Host::EnableNetTourneyClient = 1; 

// Original
function checkVer_showBanner(%client)
{
	// customize me
	commandToClient(%client, 'CenterPrint', "<font:Sui Generis:22><color:3cb4b4>Version Check Failed!\n<font:Univers:16><color:3cb4b4>You need the latest TribesNext patch and TourneyNetClient2 to play.\n Download it from playt2.com and drop it into your GameData/Base folder.", 10, 3);
}

package checkver
{
	function serverCmdClientJoinTeam(%client, %team)
	{
		if (!%client.t2csri_sentComCertDone)
		{
			checkVer_showBanner(%client);
			return;
		}
		Parent::serverCmdClientJoinTeam(%client, %team);
	}
	function serverCmdClientJoinGame(%client)
	{
		if (!%client.t2csri_sentComCertDone)
		{
			checkVer_showBanner(%client);
			return;
		}
		Parent::serverCmdClientJoinGame(%client);
	}
	function serverCmdClientPickedTeam(%client, %option)
	{
		Parent::serverCmdClientPickedTeam(%client, %option); //Put first
		
		if($Host::EnableNetTourneyClient) //Added
		{		
			if (!%client.t2csri_sentComCertDone)
			{
				if($Host::TournamentMode && %client.team !$= 0) //Added
				{	
					serverCmdClientMakeObserver( %client );
					messageAll('', '\cr%1 has failed the Tribesnext version check.', %client.name);
				}	
				checkVer_showBanner(%client);
				return;
			}
		}
	}
	function serverCmdClientTeamChange(%client, %option)
	{
		if (!%client.t2csri_sentComCertDone)
		{
			checkVer_showBanner(%client);
			return;
		}
		Parent::serverCmdClientTeamChange(%client, %option);
	}
	function Observer::onTrigger(%data, %obj, %trigger, %state)
	{
		%client = %obj.getControllingClient();
		if (!%client.t2csri_sentComCertDone)
		{
			checkVer_showBanner(%client);
			return;
		}
		Parent::onTrigger(%data, %obj, %trigger, %state);
	}	
};


package StartCheckVer
{

function loadMissionStage2()
{		
	//Activate NetTourneyClient package if enabled.
	if($Host::EnableNetTourneyClient && !isActivePackage(checkver)) //Added
		activatePackage(checkver);
   
    parent::loadMissionStage2();
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(StartCheckVer))
    activatePackage(StartCheckVer);



// Throw offenders to observer when enabled
function CheckVerObserver(%client)
{
	if($Host::EnableNetTourneyClient && !$Host::TournamentMode)
	{
		for(%i = 0; %i < ClientGroup.getCount(); %i++)	   
		{
			%client = ClientGroup.getObject(%i);
			
			//Check ver
			if(!%client.isAIControlled() && !%client.t2csri_sentComCertDone) //No bots
			{
				messageClient(%client, 'MsgClientCheckObserver', '\c2Tribesnext version check has failed.');
				serverCmdClientMakeObserver( %client );
			}
		}
	}
}

// List Names of players without NTC
function CheckVerList(%client)
{
	for(%i = 0; %i < ClientGroup.getCount(); %i++)	   
	{
		%client = ClientGroup.getObject(%i);
			
		//Check ver
		if(!%client.isAIControlled() && !%client.t2csri_sentComCertDone) //No bots
			echo(%client.nameBase);
	}
}

//Added -ChocoTaco
//Evo Code
//
//In defaultgame.ovl DefaultGame::sendGameVoteMenu(%game, %client, %key) 
//		 
//		//Toggle Tournament Net Client
//		if(%client.isAdmin && $Host::EnableNetTourneyClient)
//			messageClient( %client, 'MsgVoteItem', "", %key, 'ToggleTourneyNetClient', 'Disable Tournament Net Client', "Disable Tournament Net Client" );
//		else if(%client.isAdmin)
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
//			   
//			   if(isActivePackage(checkver))
//					deactivatePackage(checkver);
//			   
//			   messageClient( %client, '', "Tournament Net Client checking has been disabled.~wfx/powered/vehicle_screen_on.wav" );
//			   adminLog(%client, " has disabled Net Tourney Client checking.");
//			}
//            else
//            {
//               $Host::EnableNetTourneyClient = 1;
//			   
//			   if(!isActivePackage(checkver))
//					activatePackage(checkver);
//			   
//			   //Boot Offenders into Obs
//			   CheckVerObserver(%client);
//			   
//			   messageClient( %client, '', "Tournament Net Client checking has been enabled.~wfx/powered/vehicle_screen_on.wav" );
//			   ResetClientChangedTeams();
//			   adminLog(%client, " has enabled Net Tourney Client checking.");
//            }
//         }