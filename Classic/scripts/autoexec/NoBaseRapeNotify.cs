//Enable or Disable
//$Host::EnableNoBaseRapeNotify = 1;
//

//Notifys the user if NoBase rape is on or off.
function NBRStatusNotify( %game )
{	
	if( $CurrentMissionType $= "CTF" && $Host::EnableNoBaseRapeNotify && !$Host::TournamentMode && $Host::EvoNoBaseRapeEnabled )
	{
		//echo ("%client " @ %client);
		//echo ("$TeamBalanceClient " @ $TeamBalanceClient);
		
		//On
		if( $Host::EvoNoBaseRapeClassicPlayerCount > $TotalTeamPlayerCount ) 
		{
			if( $NoBaseRapeNotifyCount !$= 0 )
			{
				messageAll('MsgNoBaseRapeNotify', '\c1No Base Rape: \c0Enabled.');
				$NoBaseRapeNotifyCount = 0;
			}
		}
		//Off
		else if( $NoBaseRapeNotifyCount !$= 1 )
		{
			messageAll('MsgNoBaseRapeNotify', '\c1No Base Rape: \c0Disabled.~wfx/misc/diagnostic_on.wav');
			$NoBaseRapeNotifyCount = 1;
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
	%client = %sourceObject;
	
	if( !%client.NBRAssetSoundMsgPlayed )
	{
		messageClient(%sourceObject.client, 'MsgNoBaseRapeNotify', '\c2No Base Rape is enabled until %1 players.', $Host::EvoNoBaseRapeClassicPlayerCount );
	
		%client.NBRAssetSoundMsgPlayed = true;
		schedule(10000, 0, "ResetNBRAssetSound", %client );
	}
}

//Cool down between messeges
function ResetNBRAssetSound( %client )
{
	%client.NBRAssetSoundMsgPlayed = false;
}


