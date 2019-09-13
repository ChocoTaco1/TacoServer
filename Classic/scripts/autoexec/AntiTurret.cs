// AntiTurret Script
//
// Turrets are disabled until threshold is reached
//
// Amount of players on a team to enable turrets
// $Host::EnableTurretPlayerCount = 10;
//
// Disable MortarTurret
// $Host::EnableMortarTurret = 0;
//
// Disable = 0
// Enable  = 1
//

package AntiTurret
{

function TurretData::selectTarget(%this, %turret)
{
	if( !$Host::TournamentMode && $TotalTeamPlayerCount < $Host::EnableTurretPlayerCount )
	{
		%turret.clearTarget();
	}
	else
	{
		//All turret types can fire
		if( $Host::EnableMortarTurret )
		{
			parent::selectTarget(%this, %turret);
		}
		//Only non-MortarTurret types can fire
		else if( %turret.initialBarrel !$= "MortarBarrelLarge" )
		{
			parent::selectTarget(%this, %turret);
		}
    }
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(AntiTurret))
    activatePackage(AntiTurret);

$InvBanList[CTF, "MortarBarrelPack"] = !$Host::EnableMortarTurret;
$InvBanList[CnH, "MortarBarrelPack"] = !$Host::EnableMortarTurret;
$InvBanList[Siege, "MortarBarrelPack"] = !$Host::EnableMortarTurret;