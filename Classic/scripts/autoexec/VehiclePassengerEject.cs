if (! $EjectDone) 
{
	schedule( 30000, 0, "exec", "scripts/autoexec/VehiclePassengerEject.cs" );
	$EjectDone = true;
}
	

function Player::use( %this,%data )
{
   // If player is in a station then he can't use any items
   if(%this.station !$= "")
      return false;

   // Convert the word "Backpack" to whatever is in the backpack slot.
   if ( %data $= "Backpack" ) 
   {
      if ( %this.inStation )
         return false;

      if ( %this.isPilot() )
      {
		%vehicle = %this.getObjectMount();
		EjectAllPassengers(%vehicle, %this);
		// messageClient( %this.client, 'MsgCantUsePack', '\c2You can\'t use your pack while piloting.~wfx/misc/misc.error.wav' );
		// return( false );
      }
      else if ( %this.isWeaponOperator() )
      {
         messageClient( %this.client, 'MsgCantUsePack', '\c2You can\'t use your pack while in a weaponry position.~wfx/misc/misc.error.wav' );
         return( false );
      }
      
      %image = %this.getMountedImage( $BackpackSlot );
      if ( %image )
         %data = %image.item;
   }

   // Can't use some items when piloting or your a weapon operator
   if ( %this.isPilot() || %this.isWeaponOperator() ) 
      if ( %data.getName() !$= "RepairKit" )
         return false;
   
   return ShapeBase::use( %this, %data );
}


function EjectAllPassengers(%obj, %player)
{
    // if there are passengers, kick them out
    // %this.deleteAllMounted(%obj);
    for(%i = 0; %i < %obj.getDatablock().numMountPoints; %i++)
		if (%obj.getMountNodeObject(%i)) 
		{
			%passenger = %obj.getMountNodeObject(%i);
			
			if (%passenger != %player) 
			{
				%passenger.getDataBlock().doDismount(%passenger, true);
				%xVel = 250.0 - (getRandom() * 500.0);
				%yVel = 250.0 - (getRandom() * 500.0);
				%zVel = (getRandom() * 100.0) + 250.0;
				%flingVel = %xVel @ " " @ %yVel @ " " @ %zVel;
				%passenger.applyImpulse(%passenger.getTransform(), %fllingvel);
			}
		}
}
