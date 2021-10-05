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
	if(!$Host::TournamentMode && $TotalTeamPlayerCount < $Host::EnableTurretPlayerCount)
		%turret.clearTarget();
	else
	{
		if($Host::EnableMortarTurret) //All turret types can fire
			parent::selectTarget(%this, %turret);
		else if(%turret.initialBarrel !$= "MortarBarrelLarge") //Only non-MortarTurret types can fire
			parent::selectTarget(%this, %turret);
    }
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(AntiTurret))
    activatePackage(AntiTurret);


$InvBanList[CTF, "MortarBarrelPack"] = !$Host::EnableMortarTurret;
$InvBanList[CnH, "MortarBarrelPack"] = !$Host::EnableMortarTurret;
$InvBanList[Siege, "MortarBarrelPack"] = !$Host::EnableMortarTurret;

// To be run in-game thru console to update Mortar Turret status
function ToggleMortarTurret()
{
	if($Host::EnableMortarTurret $= 0)
		$Host::EnableMortarTurret = 1;
	else
		$Host::EnableMortarTurret = 0;
	
	$InvBanList[CTF, "MortarBarrelPack"] = !$Host::EnableMortarTurret;
	$InvBanList[CnH, "MortarBarrelPack"] = !$Host::EnableMortarTurret;
	$InvBanList[Siege, "MortarBarrelPack"] = !$Host::EnableMortarTurret;
}