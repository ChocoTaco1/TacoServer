// Made as a preventitive measure to ensure no rapid observer/spawning
// 
//

package ObserverTimeout
{

function serverCmdClientMakeObserver( %client )
{
   //10 second cooldown on becoming an observer
   if( !%client.MakeObserverTimeout )
   {
		if ( isObject( Game ) && Game.kickClient != %client )
			Game.forceObserver( %client, "playerChoose" );
		
		%client.MakeObserverTimeout = true;
		schedule(10000, 0, "ResetMakeObserverTimeout", %client );
   }
	//5 second cooldown on the notification
	else if( !%client.ObserverCooldownMsgPlayed )
	{
		messageClient(%client, 'MsgObserverCooldown', '\c2Observer is on cooldown.' );
		
		%client.ObserverCooldownMsgPlayed = true;
		schedule(5000, 0, "ResetObserverCooldownMsgPlayed", %client );
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