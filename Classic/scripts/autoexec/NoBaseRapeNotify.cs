// No Base Rape Notify Script
//
// Notifys clients if NoBase rape is on or off.
//
// Enable or Disable
// $Host::EnableNoBaseRapeNotify = 1;
//

// Called in GetTeamCounts.cs
function NBRStatusNotify( %game )
{	
	if( $CurrentMissionType $= "CTF" && $Host::EnableNoBaseRapeNotify && !$Host::TournamentMode && $Host::EvoNoBaseRapeEnabled )
	{	
		//On
		if( $Host::EvoNoBaseRapeClassicPlayerCount > $TotalTeamPlayerCount )
		{
			if( $NBRStatus !$= "PLAYEDON" )
				$NBRStatus = "ON";
		}
		//Off
		else
		{
			if( $NBRStatus !$= "PLAYEDOFF" )
				$NBRStatus = "OFF";
		}
		
		switch$($NBRStatus)
		{
			case ON:
				messageAll('MsgNoBaseRapeNotify', '\c1No Base Rape: \c0Enabled.');
				$NBRStatus = "PLAYEDON";
			case OFF:
				messageAll('MsgNoBaseRapeNotify', '\c1No Base Rape: \c0Disabled.~wfx/misc/diagnostic_on.wav');
				$NBRStatus = "PLAYEDOFF";
			case PLAYEDON:				
				//Do Nothing
			case PLAYEDOFF:				
				//Do Nothing
		}
	}
}

// Reset gameover
package ResetNBRNotify
{

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset NoBaseRapeNotify
	$NBRStatus = "IDLE";
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(ResetNBRNotify))
    activatePackage(ResetNBRNotify);


// This function is at StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType) in the staticshape.ovl in evoClassic.vl2
// Plays a sound when a player hits a protected enemy asset
function NBRAssetSound( %game, %sourceObject )
{
	//Wont play again until the schedule is done
	if(!isEventPending(%sourceObject.NBRAssetSoundSchedule) && $CurrentMissionType $= "CTF" && $Host::EnableNoBaseRapeNotify && !$Host::TournamentMode && $Host::EvoNoBaseRapeEnabled )
	{
		messageClient(%sourceObject.client, 'MsgNoBaseRapeNotify', '\c2No Base Rape is enabled until %1 players.', $Host::EvoNoBaseRapeClassicPlayerCount );
		%sourceObject.NBRAssetSoundSchedule = schedule(10000, 0, "ResetNBRAssetSound", %sourceObject );
	}
}
// Reset
function ResetNBRAssetSound( %sourceObject )
{
	if(isEventPending(%sourceObject.NBRAssetSoundSchedule)) 
		cancel(%sourceObject.NBRAssetSoundSchedule);
}


