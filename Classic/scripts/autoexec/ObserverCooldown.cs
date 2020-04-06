// Observer Cooldown Script
//
// Made as a preventative measure to ensure no rapid observer/spawning
// Limits how fast a client can spawn and go to observer
//

package ObserverTimeout
{

function serverCmdClientMakeObserver( %client )
{
    //10 second cooldown on becoming an observer
    if( !%client.MakeObserverTimeout || %client.isAdmin )
    {
		if ( isObject( Game ) && Game.kickClient != %client )
			Game.forceObserver( %client, "playerChoose" );
		
		%client.MakeObserverTimeout = true;
		%client.ObserverProtectStart = getSimTime();
		schedule(10000, 0, "ResetMakeObserverTimeout", %client );
    }
	//5 second cooldown on the notification
	else if( !%client.ObserverCooldownMsgPlayed )
	{
		%wait = mFloor((10000 - (getSimTime() - %client.ObserverProtectStart)) / 1000);
		messageClient(%client, 'MsgObserverCooldown', '\c3Observer Cooldown:\cr Please wait another %1 seconds.', %wait );
		//messageClient(%client, 'MsgObserverCooldown', '\c2Observer is on cooldown.' );
		
		%client.ObserverCooldownMsgPlayed = true;
		schedule(2000, 0, "ResetObserverCooldownMsgPlayed", %client );
	}
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(ObserverTimeout))
    activatePackage(ObserverTimeout);

//Allow client to become observer again
function ResetMakeObserverTimeout( %client )
{
	%client.MakeObserverTimeout = false;
}

//Allow a notification again
function ResetObserverCooldownMsgPlayed( %client )
{
	%client.ObserverCooldownMsgPlayed = false;
}