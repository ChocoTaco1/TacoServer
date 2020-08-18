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
	if( $Host::EnableNoBaseRapeNotify && $Host::NoBaseRapeEnabled )
	{	
		//On
		if( $Host::NoBaseRapePlayerCount > $TotalTeamPlayerCount )
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


