//Amount of players on a team to enable turrets
//$Host::EnableTurretPlayerCount = 10;
//
//Disable MortarTurret
//$Host::EnableMortarTurret = 0;
//
//Disable = 0
//Enable  = 1

package antiTurret 
{

function TurretData::selectTarget(%this, %turret)
{
	
	if( !$Host::TournamentMode && $TotalTeamPlayerCount < $Host::EnableTurretPlayerCount )
	{
		%turret.clearTarget();
	}
	else if( $Host::EnableMortarTurret )
	{
		Parent::selectTarget(%this, %turret);
	}
	//No possibility of mortar turret working if map already has it and its banned.
	else if( %turret.initialBarrel !$= "MortarBarrelLarge" )
	{
		Parent::selectTarget(%this, %turret);
    }
	
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(antiTurret))
    activatePackage(antiTurret);