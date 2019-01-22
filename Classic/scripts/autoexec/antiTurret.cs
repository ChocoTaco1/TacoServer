// Amount of players on a team to enable turrets
// $Host::EnableTurretPlayerCount = 10;
//

package antiTurret 
{

function TurretData::selectTarget(%this, %turret)
{
	if( !$Host::TournamentMode && $TotalTeamPlayerCount < $Host::EnableTurretPlayerCount )
	{
		%turret.clearTarget();
	}
	else
	{
		parent::selectTarget(%this, %turret);
    }
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(antiTurret))
    activatePackage(antiTurret);