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

// This function is at StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
// In the staticshape.ovl in evoClassic.vl2
// Plays a sound when a player hits a protected asset
function NBRAssetSound( %game, %sourceObject )
{
	%client = %sourceObject;
	
	if( !%client.NBRAssetSoundMsgPlayed && $CurrentMissionType $= "CTF" && $Host::EnableNoBaseRapeNotify && !$Host::TournamentMode && $Host::EvoNoBaseRapeEnabled )
	{
		messageClient(%sourceObject.client, 'MsgNoBaseRapeNotify', '\c2No Base Rape is enabled until %1 players.', $Host::EvoNoBaseRapeClassicPlayerCount );
	
		%client.NBRAssetSoundMsgPlayed = true;
		schedule(10000, 0, "ResetNBRAssetSound", %client );
	}
}

// Cool down between messages
function ResetNBRAssetSound( %client )
{
	%client.NBRAssetSoundMsgPlayed = false;
}

// Reset every map change
package ResetNoBaseRapeNotify
{

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset NoBaseRapeNotify
	$NoBaseRapeNotifyCount = -1;
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(ResetNoBaseRapeNotify))
    activatePackage(ResetNoBaseRapeNotify);


