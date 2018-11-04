//Disable MortarTurret
//$Host::EnableMortarTurret = 0;
//
//Disable = 0
//Enable  = 1
//
// ban mortar turret from inventory in main gametypes
if( !$Host::EnableMortarTurret ) {
	
	$InvBanList[CTF, "MortarBarrelPack"] = 1;
	$InvBanList[CnH, "MortarBarrelPack"] = 1;
	$InvBanList[Siege, "MortarBarrelPack"] = 1;
	
}

package noMortarTurret {

// if a mortar turret somehow makes it into the game, keep it from working
function TurretData::selectTarget(%this, %turret) {
	
    if( %turret.initialBarrel !$= "MortarBarrelLarge" ) {
        Parent::selectTarget(%this, %turret);
    }
	
}

};
// Prevent package from being activated if it is already
if (!isActivePackage(noMortarTurret))
    activatePackage(noMortarTurret);