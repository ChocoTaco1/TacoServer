// ***************************************************************************************************
// Items with a one in front of them are banned items. Items with a zero in front of them are allowed.
//
// Change the one's and zero's to your liking. Its pretty straight foward, each armor size gets it's
// own banlist.
//
// You can switch armor classes by issuing a commandToServer('ArmorDefaults', "Light");
// ***************************************************************************************************

function setArmorDefaults(%armor)
{
   switch$ ( %armor )
   {
      case "Light": // Set your servers Light armor bans
         // Packs
         $InvBanList[SCtF, "TurretOutdoorDeployable"] = 1;
         $InvBanList[SCtF, "TurretIndoorDeployable"] = 1;
         $InvBanList[SCtF, "ElfBarrelPack"] = 1;
         $InvBanList[SCtF, "MortarBarrelPack"] = 1;
         $InvBanList[SCtF, "PlasmaBarrelPack"] = 1;
         $InvBanList[SCtF, "AABarrelPack"] = 1;
         $InvBanList[SCtF, "AmmoPack"] = 1;
         $InvBanList[SCtF, "CloakingPack"] = 1;
         $InvBanList[SCtF, "MotionSensorDeployable"] = 1;
         $InvBanList[SCtF, "PulseSensorDeployable"] = 1;
         $InvBanList[SCtF, "TurretOutdoorDeployable"] = 1;
         $InvBanList[SCtF, "TurretIndoorDeployable"] = 1;
         $InvBanList[SCtF, "EnergyPack"] = 0;
         $InvBanList[SCtF, "RepairPack"] = 1;
         $InvBanList[SCtF, "SatchelCharge"] = 1;
         $InvBanList[SCtF, "SensorJammerPack"] = 1;
         $InvBanList[SCtF, "ShieldPack"] = 1;
		 $InvBanList[SCtF, "TargetingLaser"] = 0;
         // Weapons
         $InvBanList[SCtF, "Blaster"] = 0;
         $InvBanList[SCtF, "Disc"] = 0;
         $InvBanList[SCtF, "ELFGun"] = 1;
         $InvBanList[SCtF, "GrenadeLauncher"] = 0;
         $InvBanList[SCtF, "MissileBarrelPack"] = 1;
         $InvBanList[SCtF, "MissileLauncher"] = 1;
         $InvBanList[SCtF, "Mortar"] = 1;
         $InvBanList[SCtF, "SniperRifle"] = 1;
         // Misc
         $InvBanList[SCtF, "Mine"] = 0;
         $InvBanList[SCtF, "ConcussionGrenade"] = 0;
         $InvBanList[SCtF, "CameraGrenade"] = 1;
         $InvBanList[SCtF, "FlareGrenade"] = 1;
         $InvBanList[SCtF, "FlashGrenade"] = 1;
         $InvBanList[SCtF, "Grenade"] = 0;
		 //Pro Mode
         $InvBanList[SCtF, "ShockLance"] = $Host::SCtFProMode;
         $InvBanList[SCtF, "Chaingun"] = $Host::SCtFProMode;
         $InvBanList[SCtF, "Plasma"] = $Host::SCtFProMode;
		 
      case "Medium": // Set your servers Medium armor bans
         // Packs
         $InvBanList[SCtF, "TurretOutdoorDeployable"] = 1;
         $InvBanList[SCtF, "TurretIndoorDeployable"] = 1;
         $InvBanList[SCtF, "ElfBarrelPack"] = 1;
         $InvBanList[SCtF, "MortarBarrelPack"] = 1;
         $InvBanList[SCtF, "PlasmaBarrelPack"] = 1;
         $InvBanList[SCtF, "AABarrelPack"] = 1;
         $InvBanList[SCtF, "AmmoPack"] = 1;
         $InvBanList[SCtF, "CloakingPack"] = 1;
         $InvBanList[SCtF, "MotionSensorDeployable"] = 1;
         $InvBanList[SCtF, "PulseSensorDeployable"] = 1;
         $InvBanList[SCtF, "TurretOutdoorDeployable"] = 1;
         $InvBanList[SCtF, "TurretIndoorDeployable"] = 1;
         $InvBanList[SCtF, "EnergyPack"] = 0;
         $InvBanList[SCtF, "RepairPack"] = 0;
         $InvBanList[SCtF, "SatchelCharge"] = 1;
         $InvBanList[SCtF, "SensorJammerPack"] = 1;
         $InvBanList[SCtF, "ShieldPack"] = 1;
         // Weapons
         $InvBanList[SCtF, "Blaster"] = 1;
         $InvBanList[SCtF, "Chaingun"] = 0;
         $InvBanList[SCtF, "Disc"] = 0;
         $InvBanList[SCtF, "ELFGun"] = 0;
         $InvBanList[SCtF, "GrenadeLauncher"] = 0;
         $InvBanList[SCtF, "MissileBarrelPack"] = 1;
         $InvBanList[SCtF, "MissileLauncher"] = 1;
         $InvBanList[SCtF, "Mortar"] = 1;
         $InvBanList[SCtF, "Plasma"] = 0;
         $InvBanList[SCtF, "SniperRifle"] = 1;
         $InvBanList[SCtF, "ShockLance"] = 1;
         // Misc
         $InvBanList[SCtF, "Mine"] = 0;
         $InvBanList[SCtF, "ConcussionGrenade"] = 0;
         $InvBanList[SCtF, "CameraGrenade"] = 1;
         $InvBanList[SCtF, "FlareGrenade"] = 1;
         $InvBanList[SCtF, "FlashGrenade"] = 1;
         $InvBanList[SCtF, "Grenade"] = 0;

      case "Heavy": // Set your servers Heavy armor bans
         // Packs
         $InvBanList[SCtF, "TurretOutdoorDeployable"] = 1;
         $InvBanList[SCtF, "TurretIndoorDeployable"] = 1;
         $InvBanList[SCtF, "ElfBarrelPack"] = 1;
         $InvBanList[SCtF, "MortarBarrelPack"] = 1;
         $InvBanList[SCtF, "PlasmaBarrelPack"] = 1;
         $InvBanList[SCtF, "AABarrelPack"] = 1;
         $InvBanList[SCtF, "AmmoPack"] = 1;
         $InvBanList[SCtF, "CloakingPack"] = 1;
         $InvBanList[SCtF, "MotionSensorDeployable"] = 1;
         $InvBanList[SCtF, "PulseSensorDeployable"] = 1;
         $InvBanList[SCtF, "TurretOutdoorDeployable"] = 1;
         $InvBanList[SCtF, "TurretIndoorDeployable"] = 1;
         $InvBanList[SCtF, "EnergyPack"] = 0;
         $InvBanList[SCtF, "RepairPack"] = 0;
         $InvBanList[SCtF, "SatchelCharge"] = 1;
         $InvBanList[SCtF, "SensorJammerPack"] = 1;
         $InvBanList[SCtF, "ShieldPack"] = 0;
         // Weapons
         $InvBanList[SCtF, "Blaster"] = 1;
         $InvBanList[SCtF, "Chaingun"] = 0;
         $InvBanList[SCtF, "Disc"] = 0;
         $InvBanList[SCtF, "ELFGun"] = 1;
         $InvBanList[SCtF, "GrenadeLauncher"] = 0;
         $InvBanList[SCtF, "MissileBarrelPack"] = 1;
         $InvBanList[SCtF, "MissileLauncher"] = 1;
         $InvBanList[SCtF, "Mortar"] = 0;
         $InvBanList[SCtF, "Plasma"] = 0;
         $InvBanList[SCtF, "SniperRifle"] = 1;
         $InvBanList[SCtF, "ShockLance"] = 1;
         // Misc
         $InvBanList[SCtF, "Mine"] = 0;
         $InvBanList[SCtF, "ConcussionGrenade"] = 1;
         $InvBanList[SCtF, "CameraGrenade"] = 1;
         $InvBanList[SCtF, "FlareGrenade"] = 1;
         $InvBanList[SCtF, "FlashGrenade"] = 1;
         $InvBanList[SCtF, "Grenade"] = 0;
   }
}
