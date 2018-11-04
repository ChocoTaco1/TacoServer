//Enable or Disable
//$Host::EnableNoBaseRapeNotify = 1;
//
//This function is at DefaultGame::spawnPlayer( %game, %client, %respawn ) defaultGame.cs
//Notifys the user if NoBase rape is on or off. Has a Counter so it is only run once and doesnt spam the client. It is triggered at spawn.
function PlayerNotify::AtSpawn( %game, %client, %respawn )
{	
	if( $CurrentMissionType $= "CTF" && $Host::EnableNoBaseRapeNotify )
	{
		//echo ("%client " @ %client);
		//echo ("$TeamBalanceClient " @ $TeamBalanceClient);
	
		//Is NoBaseRape On or off
		if( !$Host::TournamentMode && $Host::EvoNoBaseRapeEnabled && $Host::EvoNoBaseRapeClassicPlayerCount > $TotalTeamPlayerCount ) {
			//If on, has the client gotten the notification already
			if($NoBaseRapeNotifyCount !$= 0) {
				messageAll('MsgNoBaseRapeNotify', 'No Base Rape is \c1Enabled.~wfx/misc/nexus_cap.wav');
				$NoBaseRapeNotifyCount = 0;
			}
		}
		else 
			//NoBaseRape is off		
			//Has the client gotten the notification already
			if($NoBaseRapeNotifyCount !$= 1) {
				messageAll('MsgNoBaseRapeNotify', 'No Base Rape is \c1Disabled.~wfx/misc/diagnostic_on.wav');
				$NoBaseRapeNotifyCount = 1;
			}
	}	
}

//This function is at StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
//In the evopackage.cs or evoClassic.vl2
//Plays a sound when a player hits a protected asset
function PlayerNotifyEnabled::OnDamage( %game, %sourceObject )
{
	messageClient(%sourceObject.client, 'MsgNoBaseRapeNotify', '~wfx/misc/diagnostic_beep.wav');
}

//This function is at DefaultGame::gameOver(%game) CTFGame.cs
//Resets the client NotifyCount when the mission ends
function ResetNotify::MissionEnd( %game, %client )
{
	$NoBaseRapeNotifyCount = -1;
}


