//Enable or Disable
//$Host::EnableNoBaseRapeNotify = 1;
//
//Notifys the user if NoBase rape is on or off.
function NBRStatusNotify( %game, %client, %respawn )
{	
	if( $CurrentMissionType $= "CTF" && $Host::EnableNoBaseRapeNotify )
	{
		//echo ("%client " @ %client);
		//echo ("$TeamBalanceClient " @ $TeamBalanceClient);
		
		//On
		if( !$Host::TournamentMode && $Host::EvoNoBaseRapeEnabled && $Host::EvoNoBaseRapeClassicPlayerCount > $TotalTeamPlayerCount && $NoBaseRapeNotifyCount ) 
		{
			messageAll('MsgNoBaseRapeNotify', 'No Base Rape is \c1Enabled.~wfx/misc/nexus_cap.wav');
			$NoBaseRapeNotifyCount = false;
		}
		//Off
		else if( !$NoBaseRapeNotifyCount ) 
		{
			messageAll('MsgNoBaseRapeNotify', 'No Base Rape is \c1Disabled.~wfx/misc/diagnostic_on.wav');
			$NoBaseRapeNotifyCount = true;
		}
	}
}

//This function is at DefaultGame::gameOver(%game) CTFGame.cs
//Resets the client NotifyCount when the mission ends
function ResetNBRNotify()
{
	$NoBaseRapeNotifyCount = -1;
}

//This function is at StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
//In the staticshape.ovl in evoClassic.vl2
//Plays a sound when a player hits a protected asset
function NBRAssetSound( %game, %sourceObject )
{
	messageClient(%sourceObject.client, 'MsgNoBaseRapeNotify', '~wfx/misc/diagnostic_beep.wav');
}


