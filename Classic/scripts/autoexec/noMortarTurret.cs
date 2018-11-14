//Disable MortarTurret
//$Host::EnableMortarTurret = 0;
//
//Disable = 0
//Enable  = 1


// ban mortar turret from inventory in main gametypes
if( !$Host::EnableMortarTurret )
{
	
	$InvBanList[CTF, "MortarBarrelPack"] = 1;
	$InvBanList[CnH, "MortarBarrelPack"] = 1;
	$InvBanList[Siege, "MortarBarrelPack"] = 1;
	
}

//Initial mortar turret barrel code moved to antiTurret.cs
//to avoid double override.