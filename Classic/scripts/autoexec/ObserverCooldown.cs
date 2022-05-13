// Observer Cooldown Script
//
// Made as a preventative measure to ensure no rapid observer/spawning
// Limits how fast a client can spawn and go to observer
//

package ObserverTimeout
{

function serverCmdClientMakeObserver(%client)
{
	//10 second cooldown on becoming an observer
	%timeDif  = getSimTime() - %client.observerTimeout;
	%timeDif1 = getSimTime() - %client.observerMsg;
	if(%timeDif > 10000 || !%client.observerTimeout || %client.isAdmin)
	{
		%client.observerProtectStart = getSimTime();
		%client.observerTimeout = getSimTime();

		if (isObject(Game) && Game.kickClient != %client)
			Game.forceObserver(%client, "playerChoose");
		if($Host::TournamentMode) //Added to clear FIRE centerPrint
			ClearCenterPrint(%client);
    }
	//1 second cooldown on message
	else if((%timeDif1 > 1000 || !%client.observerMsg))
	{
		%wait = mFloor((10000 - (getSimTime() - %client.observerProtectStart)) / 1000);
		messageClient(%client, 'MsgObserverCooldown', '\c3Observer Cooldown:\cr Please wait another %1 seconds.', %wait );

		%client.observerMsg = getSimTime();
	}
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(ObserverTimeout))
    activatePackage(ObserverTimeout);