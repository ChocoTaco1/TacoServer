//------------------------------------------------------------------------------
function setUpFavPrefs()
{
   if($pref::FavCurrentSelect $= "")
      $pref::FavCurrentSelect = 0;   
   for(%i = 0; %i < 10; %i++)
   {
      if($pref::FavNames[%i] $= "")
         $pref::FavNames[%i] = "Favorite " @ %i + 1;
      if($pref::Favorite[%i] $= "")
         $pref::Favorite[%i] = "armor\tLight Armor";
   }
   if($pref::FavCurrentList $= "")
      $pref::FavCurrentList = 0;
}

$FavCurrent = 0;
setUpFavPrefs();

$InvArmor[0] = "Scout";
$InvArmor[1] = "Assault";
$InvArmor[2] = "Juggernaut";

$NameToInv["Scout"]  = "Light";
$NameToInv["Assault"] = "Medium";
$NameToInv["Juggernaut"]  = "Heavy";


$InvWeapon[0] = "Blaster";
$InvWeapon[1] = "Plasma Rifle";
$InvWeapon[2] = "Chaingun";
$InvWeapon[3] = "Spinfusor";
$InvWeapon[4] = "Grenade Launcher";
$InvWeapon[5] = "Laser Rifle";
$InvWeapon[6] = "ELF Projector";
$InvWeapon[7] = "Fusion Mortar";
$InvWeapon[8] = "Missile Launcher";
$InvWeapon[9] = "Shocklance";
//$InvWeapon[10] = "Targeting Laser";

// -------------------------------------
// z0dd - ZOD, 9/12/02. TR2 need
$InvWeapon[10] = "TR2 Spinfusor";
$InvWeapon[11] = "TR2 Grenade Launcher";
$InvWeapon[12] = "TR2 Chaingun";
$InvWeapon[13] = "TR2 Shocklance";
$InvWeapon[14] = "TR2 Mortar";
// -------------------------------------

$NameToInv["Blaster"] = "Blaster";
$NameToInv["Plasma Rifle"] = "Plasma";
$NameToInv["Chaingun"] = "Chaingun";
$NameToInv["Spinfusor"] = "Disc";
$NameToInv["Grenade Launcher"] = "GrenadeLauncher";
$NameToInv["Laser Rifle"] = "SniperRifle";
$NameToInv["ELF Projector"] = "ELFGun";
$NameToInv["Fusion Mortar"] = "Mortar";
$NameToInv["Missile Launcher"] = "MissileLauncher";
$NameToInv["Shocklance"] = "ShockLance";
//$NameToInv["Targeting Laser"] = "TargetingLaser";

// -------------------------------------------------------
// z0dd - ZOD, 9/12/02. TR2 need
$NameToInv["TR2 Spinfusor"] = "TR2Disc";
$NameToInv["TR2 Grenade Launcher"] = "TR2GrenadeLauncher";
$NameToInv["TR2 Chaingun"] = "TR2Chaingun";
$NameToInv["TR2 Energy Pack"] = "TR2EnergyPack";
$NameToInv["TR2 Shocklance"] = "TR2Shocklance";
$NameToInv["TR2 Mortar"] = "TR2Mortar";
// -------------------------------------------------------


$InvPack[0] = "Energy Pack";
$InvPack[1] = "Repair Pack";
$InvPack[2] = "Shield Pack";
$InvPack[3] = "Cloak Pack";
$InvPack[4] = "Sensor Jammer Pack";
$InvPack[5] = "Ammunition Pack";
$InvPack[6] = "Satchel Charge";
$InvPack[7] = "Motion Sensor Pack";
$InvPack[8] = "Pulse Sensor Pack";
$InvPack[9] = "Inventory Station";
$InvPack[10] = "Landspike Turret";
$InvPack[11] = "Spider Clamp Turret";
$InvPack[12] = "ELF Turret Barrel";
$InvPack[13] = "Mortar Turret Barrel";
$InvPack[14] = "Plasma Turret Barrel";
$InvPack[15] = "AA Turret Barrel";
$InvPack[16] = "Missile Turret Barrel";
$InvPack[17] = "TR2 Energy Pack"; // z0dd - ZOD, 9/12/02. TR2 need

// non-team mission pack choices (DM, Hunters, Rabbit)

// z0dd - ZOD, 7/16/03. These are not used for anything, remove, save some bytes of ram
//$NTInvPack[0] = "Energy Pack";
//$NTInvPack[1] = "Repair Pack";
//$NTInvPack[2] = "Shield Pack";
//$NTInvPack[3] = "Cloak Pack";
//$NTInvPack[4] = "Sensor Jammer Pack";
//$NTInvPack[5] = "Ammunition Pack";
//$NTInvPack[6] = "Satchel Charge";
//$NTInvPack[7] = "Motion Sensor Pack";
//$NTInvPack[8] = "Pulse Sensor Pack";
//$NTInvPack[9] = "Inventory Station";
//$NTInvPack[10] = "TR2 Energy Pack"; // z0dd - ZOD, 9/12/02. TR2 need

$NameToInv["Energy Pack"] = "EnergyPack";
$NameToInv["Repair Pack"] = "RepairPack";
$NameToInv["Shield Pack"] = "ShieldPack";
$NameToInv["Cloak Pack"] = "CloakingPack";
$NameToInv["Sensor Jammer Pack"] = "SensorJammerPack";
$NameToInv["Ammunition Pack"] = "AmmoPack";
$NameToInv["Satchel Charge"] = "SatchelCharge";
$NameToInv["Motion Sensor Pack"] = "MotionSensorDeployable";
$NameToInv["Pulse Sensor Pack"] = "PulseSensorDeployable";
$NameToInv["Inventory Station"] = "InventoryDeployable";
$NameToInv["Landspike Turret"] = "TurretOutdoorDeployable";
$NameToInv["Spider Clamp Turret"] = "TurretIndoorDeployable";
$NameToInv["ELF Turret Barrel"] = "ELFBarrelPack";
$NameToInv["Mortar Turret Barrel"] = "MortarBarrelPack";
$NameToInv["Plasma Turret Barrel"] = "PlasmaBarrelPack";
$NameToInv["AA Turret Barrel"] = "AABarrelPack";
$NameToInv["Missile Turret Barrel"] = "MissileBarrelPack";

$InvGrenade[0] = "Grenade";
$InvGrenade[1] = "Whiteout Grenade";
$InvGrenade[2] = "Concussion Grenade";
$InvGrenade[3] = "Flare Grenade";
$InvGrenade[4] = "Deployable Camera";
$InvGrenade[5] = "TR2Grenade"; // z0dd - ZOD, 9/12/02. TR2 need

$NameToInv["Grenade"] = "Grenade";
$NameToInv["Whiteout Grenade"] = "FlashGrenade";
$NameToInv["Concussion Grenade"] = "ConcussionGrenade";
$NameToInv["Flare Grenade"] = "FlareGrenade";
$NameToInv["Deployable Camera"] = "CameraGrenade";
$NameToInv["TR2Grenade"] = "TR2Grenade"; // z0dd - ZOD, 9/12/02. TR2 need


$InvMine[0] = "Mine";

$NameToInv["Mine"] = "Mine";

//$InvBanList[DeployInv, "ElfBarrelPack"] = 1;
//$InvBanList[DeployInv, "MortarBarrelPack"] = 1;
//$InvBanList[DeployInv, "PlasmaBarrelPack"] = 1;
//$InvBanList[DeployInv, "AABarrelPack"] = 1;
//$InvBanList[DeployInv, "MissileBarrelPack"] = 1;
$InvBanList[DeployInv, "InventoryDeployable"] = 1;

//------------------------------------------------------------------------------
function InventoryScreen::loadHud( %this, %tag )
{
   $Hud[%tag] = InventoryScreen;
   $Hud[%tag].childGui = INV_Root;
   $Hud[%tag].parent = INV_Root;
}

//------------------------------------------------------------------------------
function InventoryScreen::setupHud( %this, %tag )
{
   %favListStart = $pref::FavCurrentList * 10;
   %this.selId = $pref::FavCurrentSelect - %favListStart + 1;

   // Add the list menu:
   $Hud[%tag].staticData[0, 0] = new ShellPopupMenu(INV_ListMenu) 
   {
      profile = "ShellPopupProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "16 313";
      extent = "170 36";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      maxPopupHeight = "220";
      text = "";
   };
   
   // Add favorite tabs:  
   for( %i = 0; %i < 10; %i++ )
   {
      %yOffset = ( %i * 30 ) + 10;
      $Hud[%tag].staticData[0, %i + 1] = new ShellTabButton() {
         profile = "ShellTabProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "4 " @ %yOffset;
         extent = "206 38";
         minExtent = "8 8";
         visible = "1";
         setFirstResponder = "0";
         modal = "1";
         helpTag = "0";
         command = "InventoryScreen.onTabSelect(" @ %favListStart + %i @ ");";
         text = strupr( $pref::FavNames[%favListStart + %i] );
      };
      $Hud[%tag].staticData[0, %i + 1].setValue( ( %favListStart + %i ) == $pref::FavCurrentSelect );
            
      $Hud[%tag].parent.add( $Hud[%tag].staticData[0, %i + 1] );
   }
 
   %text = "Favorites " @ %favListStart + 1 SPC "-" SPC %favListStart + 10;
   $Hud[%tag].staticData[0, 0].onSelect( $pref::FavCurrentList, %text, true );
 
   $Hud[%tag].parent.add( $Hud[%tag].staticData[0, 0] );

   // Add the SAVE button:
   $Hud[%tag].staticData[1, 0] = new ShellBitmapButton() 
   {
      profile = "ShellButtonProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "409 295";
      extent = "75 38";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      command = "saveFavorite();";
      text = "SAVE";
   };      
   
   // Add the name edit control:
   $Hud[%tag].staticData[1, 1] = new ShellTextEditCtrl() 
   {
      profile = "NewTextEditProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "217 295";
      extent = "196 38";
      minExtent = "8 8";
      visible = "1";
      altCommand = "saveFavorite()";
      setFirstResponder = "1";
      modal = "1";
      helpTag = "0";
      historySize = "0";
      maxLength = "16";
   };
   
   $Hud[%tag].staticData[1, 1].setValue( $pref::FavNames[$pref::FavCurrentSelect] );
   
   $Hud[%tag].parent.add( $Hud[%tag].staticData[1, 0] );
   $Hud[%tag].parent.add( $Hud[%tag].staticData[1, 1] );
}

//------------------------------------------------------------------------------
function InventoryScreen::addLine( %this, %tag, %lineNum, %type, %count )
{
   $Hud[%tag].count = %count;

   // Add label:
   %yOffset = ( %lineNum * 30 ) + 28;
   $Hud[%tag].data[%lineNum, 0] = new GuiTextCtrl() 
   {
      profile = "ShellTextRightProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "228 " @ %yOffset;
      extent = "80 22";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      text = "";
   };

   // Add drop menu:
   $Hud[%tag].data[%lineNum, 1] = new ShellPopupMenu(INV_Menu) 
   {
      profile = "ShellPopupProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "305 " @ %yOffset - 9;
      extent = "180 36";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      maxPopupHeight = "200";
      text = "";
      type = %type;
   };

   return 2;
}
   
//------------------------------------------------------------------------------
function InventoryScreen::updateHud( %this, %client, %tag )
{
   %noSniperRifle = true;
   %armor = getArmorDatablock( %client, $NameToInv[%client.favorites[0]] );
   if ( %client.lastArmor !$= %armor )
   {
      %client.lastArmor = %armor;
      for ( %x = 0; %x < %client.lastNumFavs; %x++ )
         messageClient( %client, 'RemoveLineHud', "", 'inventoryScreen', %x );
      %setLastNum = true;
   }

   %cmt = $CurrentMissionType;
//Create - ARMOR - List
   %armorList = %client.favorites[0];
   for ( %y = 0; $InvArmor[%y] !$= ""; %y++ )
      if ( $InvArmor[%y] !$= %client.favorites[0] )  
         %armorList = %armorList TAB $InvArmor[%y];

//Create - WEAPON - List
   for ( %y = 0; $InvWeapon[%y] !$= ""; %y++ )
   {
      %notFound = true;
      for ( %i = 0; %i < getFieldCount( %client.weaponIndex ); %i++ )
      {
         %WInv = $NameToInv[$InvWeapon[%y]];
         if ( ( $InvWeapon[%y] $= %client.favorites[getField( %client.weaponIndex,%i )] ) || !%armor.max[%WInv] )  
         {
            %notFound = false;
            break;
         }
         else if ( "SniperRifle" $= $NameToInv[%client.favorites[getField( %client.weaponIndex,%i )]] )
         {
            %noSniperRifle = false;
            %packList = "noSelect\tEnergy Pack\tEnergy Pack must be used when \tLaser Rifle is selected!";     
            %client.favorites[getField(%client.packIndex,0)] = "Energy Pack";
         }   
      }

      if ( !($InvBanList[%cmt, %WInv]) )
      {
         if ( %notFound && %weaponList $= "" )
            %weaponList = $InvWeapon[%y];
         else if ( %notFound )
            %weaponList = %weaponList TAB $InvWeapon[%y];
      }
   }

//Create - PACK - List
   if ( %noSniperRifle )
   {
      if ( getFieldCount( %client.packIndex ) )
         %packList = %client.favorites[getField( %client.packIndex, 0 )];
      else
      {
         %packList = "EMPTY";
         %client.numFavs++;
      }
      for ( %y = 0; $InvPack[%y] !$= ""; %y++ )
      {
         %PInv = $NameToInv[$InvPack[%y]];
         if ( ( $InvPack[%y] !$= %client.favorites[getField( %client.packIndex, 0 )]) && 
         %armor.max[%PInv] && !($InvBanList[%cmt, %PInv]))  
            %packList = %packList TAB $Invpack[%y];
      }
   }   
//Create - GRENADE - List
   for ( %y = 0; $InvGrenade[%y] !$= ""; %y++ )
   {
      %notFound = true;
      for(%i = 0; %i < getFieldCount( %client.grenadeIndex ); %i++)
      {
         %GInv = $NameToInv[$InvGrenade[%y]];
         if ( ( $InvGrenade[%y] $= %client.favorites[getField( %client.grenadeIndex, %i )] ) || !%armor.max[%GInv] )  
         {
            %notFound = false;
            break;
         }
      }
      if ( !($InvBanList[%cmt, %GInv]) )
      { 
         if ( %notFound && %grenadeList $= "" )
            %grenadeList = $InvGrenade[%y];
         else if ( %notFound )
            %grenadeList = %grenadeList TAB $InvGrenade[%y];
      }
   }

//Create - MINE - List
   for ( %y = 0; $InvMine[%y] !$= "" ; %y++ )
   {
      %notFound = true;
      // -----------------------------------------------------------------------------------------------------
      // z0dd - ZOD, 4/24/02. This was broken, Fixed.
      for(%i = 0; %i < getFieldCount( %client.mineIndex ); %i++)
      {
         %MInv = $NameToInv[$InvMine[%y]];
         if ( ( $InvMine[%y] $= %client.favorites[getField( %client.mineIndex, %i )] ) || !%armor.max[%MInv] )  
         {
            %notFound = false;
            break;
         }
      }
      // -----------------------------------------------------------------------------------------------------
      if ( !($InvBanList[%cmt, %MInv]) )
      {
         if ( %notFound && %mineList $= "" )
            %mineList = $InvMine[%y];
         else if ( %notFound )
            %mineList = %mineList TAB $InvMine[%y];
      }
   }
   %client.numFavsCount++;
   messageClient( %client, 'SetLineHud', "", %tag, 0, "Armor:", %armorList, armor, %client.numFavsCount );
   %lineCount = 1;

   for ( %x = 0; %x < %armor.maxWeapons; %x++ )
   {
      %client.numFavsCount++;
      if ( %x < getFieldCount( %client.weaponIndex ) )
      {
         %list = %client.favorites[getField( %client.weaponIndex,%x )];
         if ( %list $= Invalid )
         {
            %client.favorites[%client.numFavs] = "INVALID";
            %client.weaponIndex = %client.weaponIndex TAB %client.numFavs;
         }   
      }
      else
      {
         %list = "EMPTY";
         %client.favorites[%client.numFavs] = "EMPTY";
         %client.weaponIndex = %client.weaponIndex TAB %client.numFavs;
         %client.numFavs++;
      }
      if ( %list $= empty )
         %list = %list TAB %weaponList;
      else
         %list = %list TAB %weaponList TAB "EMPTY";
      messageClient( %client, 'SetLineHud', "", %tag, %x + %lineCount, "Weapon Slot " @ %x + 1 @ ": ", %list , weapon, %client.numFavsCount );
   }
   %lineCount = %lineCount + %armor.maxWeapons;
   
   %client.numFavsCount++;
   if ( getField( %packList, 0 ) !$= empty && %noSniperRifle )
      %packList = %packList TAB "EMPTY";
   %packText = %packList;
   %packOverFlow = "";
   if ( strlen( %packList ) > 255 )
   {
      %packText = getSubStr( %packList, 0, 255 );
      %packOverFlow = getSubStr( %packList, 255, 512 );
   }
   messageClient( %client, 'SetLineHud', "", %tag, %lineCount, "Pack:", %packText, pack, %client.numFavsCount, %packOverFlow );
   %lineCount++;
   
   for( %x = 0; %x < %armor.maxGrenades; %x++ )
   {
      %client.numFavsCount++;
      if ( %x < getFieldCount( %client.grenadeIndex ) )
      {
         %list = %client.favorites[getField( %client.grenadeIndex, %x )];
         if (%list $= Invalid)
         {
            %client.favorites[%client.numFavs] = "INVALID";
            %client.grenadeIndex = %client.grenadeIndex TAB %client.numFavs;
         }
      }
      else
      {
         %list = "EMPTY";
         %client.favorites[%client.numFavs] = "EMPTY";
         %client.grenadeIndex = %client.grenadeIndex TAB %client.numFavs;
         %client.numFavs++;
      }
      
      if ( %list $= empty )
         %list = %list TAB %grenadeList;
      else
         %list = %list TAB %grenadeList TAB "EMPTY";

      messageClient( %client, 'SetLineHud', "", %tag, %x + %lineCount, "Grenade:", %list, grenade, %client.numFavsCount );
   }
   %lineCount = %lineCount + %armor.maxGrenades;
   
   for ( %x = 0; %x < %armor.maxMines; %x++ )
   {
      %client.numFavsCount++;
      if ( %x < getFieldCount( %client.mineIndex ) )
      {
         %list = %client.favorites[getField( %client.mineIndex, %x )];
         if ( %list $= Invalid )
         {
            %client.favorites[%client.numFavs] = "INVALID";
            %client.mineIndex = %client.mineIndex TAB %client.numFavs;
         }
      }
      else
      {
         %list = "EMPTY";
         %client.favorites[%client.numFavs] = "EMPTY";
         %client.mineIndex = %client.mineIndex TAB %client.numFavs;
         %client.numFavs++;
      }
      
      if ( %list !$= Invalid )
      {
         if ( %list $= empty )
            %list = %list TAB %mineList;
         else if ( %mineList !$= "" )
            %list = %list TAB %mineList TAB "EMPTY";
         else 
            %list = %list TAB "EMPTY";
      }
         
      messageClient( %client, 'SetLineHud', "", %tag, %x + %lineCount, "Mine:", %list, mine, %client.numFavsCount );
   }

   if ( %setLastNum )
      %client.lastNumFavs = %client.numFavs;
}

//------------------------------------------------------------------------------
function buyFavorites(%client)
{
   if(isObject(Game)) // z0dd - ZOD, 8/9/03. No armors in Spawn CTF.
   {
      if(Game.class $= SCtFGame)
      {
         buyDeployableFavorites(%client);
         return;
      }
   }
   // z0dd - ZOD, 5/27/03. Check to see if we reached the cap on armors, if so, buy ammo and go away mad.
   if(%client.favorites[0] !$= "Scout" && !$Host::TournamentMode && $LimitArmors)
   {
      if($TeamArmorCount[%client.team, $NameToInv[%client.favorites[0]]] >= $TeamArmorMax)
      {
         messageClient(%client, 'MsgTeamDepObjCount', '\c2Your team has reached the maximum (%2) allotment of %1 armors', %client.favorites[0], $TeamArmorMax);
         getAmmoStationLovin(%client);
         return;
      }
   }

   // z0dd - ZOD, 5/27/03. Increase the teams armor count and let the player know whats left etc.
   if(!$Host::TournamentMode && $LimitArmors)
   {
      $TeamArmorCount[%client.team, %client.armor]--;
      $TeamArmorCount[%client.team, $NameToInv[%client.favorites[0]]]++;
      if(%client.favorites[0] !$= "Scout")
         messageClient(%client, 'MsgTeamDepObjCount', '\c2Your team has %1 of %2 %3 armors in use', $TeamArmorCount[%client.team, $NameToInv[%client.favorites[0]]], $TeamArmorMax, %client.favorites[0]);
   }

   // don't forget -- for many functions, anything done here also needs to be done
   // below in buyDeployableFavorites !!!
   %client.player.clearInventory();
   %client.setWeaponsHudClearAll();
   %cmt = $CurrentMissionType;

   %curArmor = %client.player.getDatablock();
   %curDmgPct = getDamagePercent(%curArmor.maxDamage, %client.player.getDamageLevel());

   // armor
   %client.armor = $NameToInv[%client.favorites[0]];
   %client.player.setArmor( %client.armor );
   %newArmor = %client.player.getDataBlock();

   %client.player.setDamageLevel(%curDmgPct * %newArmor.maxDamage);
   %weaponCount = 0;

   // weapons
   for(%i = 0; %i < getFieldCount( %client.weaponIndex ); %i++)
   {
      %inv = $NameToInv[%client.favorites[getField( %client.weaponIndex, %i )]];
      
      if( %inv !$= "" )
      {   
         %weaponCount++;
         %client.player.setInventory( %inv, 1 );
      }
      
      // ----------------------------------------------------
      // z0dd - ZOD, 4/24/02. Code optimization.
      if ( %inv.image.ammo !$= "" )
         %client.player.setInventory( %inv.image.ammo, 999 );
      // ----------------------------------------------------
   }
   %client.player.weaponCount = %weaponCount;

   // pack
   %pCh = $NameToInv[%client.favorites[%client.packIndex]];
   if ( %pCh $= "" )
      %client.clearBackpackIcon();
   else
      %client.player.setInventory( %pCh, 1 );

   // if this pack is a deployable that has a team limit, warn the purchaser
	// if it's a deployable turret, the limit depends on the number of players (deployables.cs)
	if(%pCh $= "TurretIndoorDeployable" || %pCh $= "TurretOutdoorDeployable")
		%maxDep = countTurretsAllowed(%pCh);
	else
	   %maxDep = $TeamDeployableMax[%pCh];

   if(%maxDep !$= "")
   {
      %depSoFar = $TeamDeployedCount[%client.player.team, %pCh];
      %packName = %client.favorites[%client.packIndex];

      if(Game.numTeams > 1)
         %msTxt = "Your team has "@%depSoFar@" of "@%maxDep SPC %packName@"s deployed.";
      else
         %msTxt = "You have deployed "@%depSoFar@" of "@%maxDep SPC %packName@"s.";

      messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
   }

   // grenades
   for ( %i = 0; %i < getFieldCount( %client.grenadeIndex ); %i++ )
   {
      if ( !($InvBanList[%cmt, $NameToInv[%client.favorites[getField( %client.grenadeIndex, %i )]]]) )
        %client.player.setInventory( $NameToInv[%client.favorites[getField( %client.grenadeIndex,%i )]], 30 );
   }

   %client.player.lastGrenade = $NameToInv[%client.favorites[getField( %client.grenadeIndex,%i )]];

   // if player is buying cameras, show how many are already deployed
   if(%client.favorites[%client.grenadeIndex] $= "Deployable Camera")
   {
      %maxDep = $TeamDeployableMax[DeployedCamera];
      %depSoFar = $TeamDeployedCount[%client.player.team, DeployedCamera];
      if(Game.numTeams > 1)
         %msTxt = "Your team has "@%depSoFar@" of "@%maxDep@" Deployable Cameras placed.";
      else
         %msTxt = "You have placed "@%depSoFar@" of "@%maxDep@" Deployable Cameras.";
      messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
   }

   // mines
   // -----------------------------------------------------------------------------------------------------
   // z0dd - ZOD, 4/24/02. Old code did not check to see if mines are banned, fixed.
   for ( %i = 0; %i < getFieldCount( %client.mineIndex ); %i++ )
   {
      if ( !($InvBanList[%cmt, $NameToInv[%client.favorites[getField( %client.mineIndex, %i )]]]) )
        %client.player.setInventory( $NameToInv[%client.favorites[getField( %client.mineIndex,%i )]], 30 );
   }
   // -----------------------------------------------------------------------------------------------------
   // miscellaneous stuff -- Repair Kit, Beacons, Targeting Laser
   if ( !($InvBanList[%cmt, RepairKit]) )
      %client.player.setInventory( RepairKit, 1 );
   if ( !($InvBanList[%cmt, Beacon]) )
      %client.player.setInventory( Beacon, 20 ); // z0dd - ZOD, 4/24/02. 400 was a bit much, changed to 20
   if ( !($InvBanList[%cmt, TargetingLaser]) )
      %client.player.setInventory( TargetingLaser, 1 );

   // ammo pack pass -- hack! hack!
   if( %pCh $= "AmmoPack" )
      invAmmoPackPass(%client);
}

//------------------------------------------------------------------------------
function buyDeployableFavorites(%client)
{
   %player = %client.player;
	%prevPack = %player.getMountedImage($BackpackSlot);
   %player.clearInventory();
   %client.setWeaponsHudClearAll();
   %cmt = $CurrentMissionType;

   // players cannot buy armor from deployable inventory stations
	%weapCount = 0;
   for ( %i = 0; %i < getFieldCount( %client.weaponIndex ); %i++ )
   {
      %inv = $NameToInv[%client.favorites[getField( %client.weaponIndex, %i )]];
      if ( !($InvBanList[DeployInv, %inv]) )
      {
         %player.setInventory( %inv, 1 );
			// increment weapon count if current armor can hold this weapon
         if(%player.getDatablock().max[%inv] > 0)      
				%weapCount++;
      // ---------------------------------------------
      // z0dd - ZOD, 4/24/02. Code streamlining.
      if ( %inv.image.ammo !$= "" )
         %player.setInventory( %inv.image.ammo, 999 );
      // ---------------------------------------------
	if(%weapCount >= %player.getDatablock().maxWeapons)
		break;
      }
   }
   %player.weaponCount = %weapCount;
   // give player the grenades and mines they chose, beacons, and a repair kit
   for ( %i = 0; %i < getFieldCount( %client.grenadeIndex ); %i++)
   {   
      %GInv = $NameToInv[%client.favorites[getField( %client.grenadeIndex, %i )]];
      %client.player.lastGrenade = %GInv;
      if ( !($InvBanList[DeployInv, %GInv]) )
         %player.setInventory( %GInv, 30 );
   }

   // if player is buying cameras, show how many are already deployed
   if(%client.favorites[%client.grenadeIndex] $= "Deployable Camera")
   {
      %maxDep = $TeamDeployableMax[DeployedCamera];
      %depSoFar = $TeamDeployedCount[%client.player.team, DeployedCamera];
      if(Game.numTeams > 1)
         %msTxt = "Your team has "@%depSoFar@" of "@%maxDep@" Deployable Cameras placed.";
      else
         %msTxt = "You have placed "@%depSoFar@" of "@%maxDep@" Deployable Cameras.";
      messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
   }

   for ( %i = 0; %i < getFieldCount( %client.mineIndex ); %i++ )
   {
      %MInv = $NameToInv[%client.favorites[getField( %client.mineIndex, %i )]];
      if ( !($InvBanList[DeployInv, %MInv]) )
         %player.setInventory( %MInv, 30 );
   }
   if ( !($InvBanList[DeployInv, Beacon]) && !($InvBanList[%cmt, Beacon]) )
      %player.setInventory( Beacon, 20 ); // z0dd - ZOD, 4/24/02. 400 was a bit much, changed to 20.
   if ( !($InvBanList[DeployInv, RepairKit]) && !($InvBanList[%cmt, RepairKit]) )
      %player.setInventory( RepairKit, 1 );
   if ( !($InvBanList[DeployInv, TargetingLaser]) && !($InvBanList[%cmt, TargetingLaser]) )
      %player.setInventory( TargetingLaser, 1 );

   // players cannot buy deployable station packs from a deployable inventory station
   %packChoice = $NameToInv[%client.favorites[%client.packIndex]];
   if ( !($InvBanList[DeployInv, %packChoice]) )
      %player.setInventory( %packChoice, 1 );

   // if this pack is a deployable that has a team limit, warn the purchaser
	// if it's a deployable turret, the limit depends on the number of players (deployables.cs)
	if(%packChoice $= "TurretIndoorDeployable" || %packChoice $= "TurretOutdoorDeployable")
		%maxDep = countTurretsAllowed(%packChoice);
	else
	   %maxDep = $TeamDeployableMax[%packChoice];
   if((%maxDep !$= "") && (%packChoice !$= "InventoryDeployable"))
   {
      %depSoFar = $TeamDeployedCount[%client.player.team, %packChoice];
      %packName = %client.favorites[%client.packIndex];

      if(Game.numTeams > 1)
         %msTxt = "Your team has "@%depSoFar@" of "@%maxDep SPC %packName@"s deployed.";
      else
         %msTxt = "You have deployed "@%depSoFar@" of "@%maxDep SPC %packName@"s.";

      messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
   }

   if(%prevPack > 0)
   {
      // if player had a "forbidden" pack (such as a deployable inventory station)
      // BEFORE visiting a deployed inventory station AND still has that pack chosen
      // as a favorite, give it back
      if((%packChoice $= %prevPack.item) && ($InvBanList[DeployInv, %packChoice]))
         %player.setInventory( %prevPack.item, 1 );
   }

   if(%packChoice $= "AmmoPack")
      invAmmoPackPass(%client);
}

//-------------------------------------------------------------------------------------
function getAmmoStationLovin(%client)
{
   // z0dd - ZOD, 4/24/02. This function was quite a mess, needed rewrite
   %cmt = $CurrentMissionType;

   // weapons
   for(%i = 0; %i < %client.player.weaponSlotCount; %i++)
   {
      %weapon = %client.player.weaponSlot[%i];
      if ( %weapon.image.ammo !$= "" )
         %client.player.setInventory( %weapon.image.ammo, 999 );
   }

   // grenades
   for(%i = 0; $InvGrenade[%i] !$= ""; %i++) // z0dd - ZOD, 5/27/03. Clear them all in one pass
      %player.setInventory($NameToInv[$InvGrenade[%i]], 0);

   for ( %i = 0; %i < getFieldCount( %client.grenadeIndex ); %i++ )
   {
      %client.player.lastGrenade = $NameToInv[%client.favorites[getField( %client.grenadeIndex, %i )]];
   }
   %grenType = %client.player.lastGrenade;
   if(%grenType $= "")
   {
      %grenType = Grenade;
   } 
   if ( !($InvBanList[%cmt, %grenType]) )
      %client.player.setInventory( %grenType, 30 );

   if(%grenType $= "Deployable Camera")
   {
      %maxDep = $TeamDeployableMax[DeployedCamera];
      %depSoFar = $TeamDeployedCount[%client.player.team, DeployedCamera];
      if(Game.numTeams > 1)
         %msTxt = "Your team has "@%depSoFar@" of "@%maxDep@" Deployable Cameras placed.";
      else
         %msTxt = "You have placed "@%depSoFar@" of "@%maxDep@" Deployable Cameras.";
      messageClient(%client, 'MsgTeamDepObjCount', %msTxt);
   }

   // Mines
   for(%i = 0; $InvMine[%i] !$= ""; %i++) // z0dd - ZOD, 5/27/03. Clear them all in one pass
      %player.setInventory($NameToInv[$InvMine[%i]], 0);

   for ( %i = 0; %i < getFieldCount( %client.mineIndex ); %i++ )
   {
      %client.player.lastMine = $NameToInv[%client.favorites[getField( %client.mineIndex, %i )]];
   }
   %mineType = %client.player.lastMine;
   if(%mineType $= "")
   {
      %mineType = Mine;
   }
   if ( !($InvBanList[%cmt, %mineType]) )
      %client.player.setInventory( %mineType, 30 );

   // miscellaneous stuff -- Repair Kit, Beacons, Targeting Laser
   if ( !($InvBanList[%cmt, RepairKit]) )
      %client.player.setInventory( RepairKit, 1 );

   if ( !($InvBanList[%cmt, Beacon]) )
      %client.player.setInventory( Beacon, 20 );

   if ( !($InvBanList[%cmt, TargetingLaser]) )
      %client.player.setInventory( TargetingLaser, 1 );

   if( %client.player.getMountedImage($BackpackSlot) $= "AmmoPack" )
      invAmmoPackPass(%client);
}

function invAmmoPackPass(%client)
{
   // "normal" ammo stuff (everything but mines and grenades)
   for ( %idx = 0; %idx < $numAmmoItems; %idx++ ) 
   {
      %ammo = $AmmoItem[%idx];
      %client.player.incInventory(%ammo, AmmoPack.max[%ammo]);
   }
   //our good friends, the grenade family *SIGH*
   // first find out what type of grenade the player has selected
   %grenFav = %client.favorites[getField(%client.grenadeIndex, 0)];
   if((%grenFav !$= "EMPTY") && (%grenFav !$= "INVALID"))
      %client.player.incInventory($NameToInv[%grenFav], AmmoPack.max[$NameToInv[%grenFav]]);
   // now the same check for mines
   %mineFav = %client.favorites[getField(%client.mineIndex, 0)];
   if((%mineFav !$= "EMPTY") && (%mineFav !$= "INVALID") && !($InvBanList[%cmt, Mine]))
      %client.player.incInventory($NameToInv[%mineFav], AmmoPack.max[$NameToInv[%mineFav]]);
}

//------------------------------------------------------------------------------
function loadFavorite( %index, %echo )
{
   $pref::FavCurrentSelect = %index;
   %list = mFloor( %index / 10 );

   if ( isObject( $Hud['inventoryScreen'] ) )
   {
      // Deselect the old tab:
      if ( InventoryScreen.selId !$= "" )
         $Hud['inventoryScreen'].staticData[0, InventoryScreen.selId].setValue( false );

      // Make sure we are looking at the same list:
      if ( $pref::FavCurrentList != %list )
      {
         %favListStart = %list * 10;
         %text = "Favorites " @ %favListStart + 1 SPC "-" SPC %favListStart + 10;
         $Hud['inventoryScreen'].staticData[0, 0].onSelect( %list, %text, true );
      }

      // Select the new tab:
      %tab = $pref::FavCurrentSelect - ( $pref::FavCurrentList * 10 ) + 1;
      InventoryScreen.selId = %tab;
      $Hud['inventoryScreen'].staticData[0, %tab].setValue( true );

      // Update the Edit Name field:
      $Hud['inventoryScreen'].staticData[1, 1].setValue( $pref::FavNames[%index] );
   }

   if ( %echo )
      addMessageHudLine( "Inventory set \"" @ $pref::FavNames[%index] @ "\" selected." );

   commandToServer( 'setClientFav', $pref::Favorite[%index] );   
}

//------------------------------------------------------------------------------
function saveFavorite()
{
   if ( $pref::FavCurrentSelect !$= "" )
   {
      %favName = $Hud['inventoryScreen'].staticData[1, 1].getValue();
      $pref::FavNames[$pref::FavCurrentSelect] = %favName;
      $Hud['inventoryScreen'].staticData[0, $pref::FavCurrentSelect - ($pref::FavCurrentList * 10) + 1].setText( strupr( %favName ) );
      //$Hud[%tag].staticData[1, 1].setValue( %favName );
      %favList = $Hud['inventoryScreen'].data[0, 1].type TAB $Hud['inventoryScreen'].data[0, 1].getValue();
      for ( %i = 1; %i < $Hud['inventoryScreen'].count; %i++ )
      {
         %name = $Hud['inventoryScreen'].data[%i, 1].getValue();
         if ( %name $= invalid )
            %name = "EMPTY";
         %favList = %favList TAB $Hud['inventoryScreen'].data[%i, 1].type TAB %name;
      }
      $pref::Favorite[$pref::FavCurrentSelect] = %favList;
      echo("exporting pref::* to ClientPrefs.cs");
      export("$pref::*", "prefs/ClientPrefs.cs", False);
   }
//   else
//      addMessageHudLine("Must First Select A Favorite Button.");
}

//------------------------------------------------------------------------------
function addQuickPackFavorite( %pack, %item )
{
	// this has been such a success it has been changed to handle grenades 
	// and other equipment as well as packs so everything seems to be called 'pack' 
	// including the function itself. The default IS pack

	if(%item $= "")
		%item = "Pack";
	%packFailMsg = "You cannot use that equipment with your selected loadout.";
	if ( !isObject($Hud['inventoryScreen'].staticData[1, 1]) || $Hud['inventoryScreen'].staticData[1, 1].getValue() $= ""  ) 
	{
		//if the player hasnt brought up the inv screen we use his current fav
		%currentFav = $pref::Favorite[$pref::FavCurrentSelect];
		//echo(%currentFav);

		for ( %i = 0; %i < getFieldCount( %currentFav ); %i++ ) 
		{
			%type = getField( %currentFav, %i );
			%equipment = getField( %currentFav, %i++ );
			
			%invalidPack = checkPackValidity(%pack, %equipment, %item );
			if(%invalidPack)
			{
				addMessageHudLine( %packFailMsg );
				return;
			
			}
		// Success--------------------------------------------------
			if ( %type $= %item )
				%favList = %favList @ %type TAB %pack @ "\t";
			else 
				%favList = %favList  @ %type TAB %equipment @ "\t";  
		}
		//echo(%favList);
	}
	else 
	{
		//otherwise we go with whats on the invScreen (even if its asleep)
		%armor =  $Hud['inventoryScreen'].data[0, 1].getValue();
		
		// check pack validity with armor
		%invalidPack = checkPackValidity(%pack, %armor, %item );
		if(%invalidPack)
		{
			addMessageHudLine( %packFailMsg );
			return;
		
		}
	   %favList = $Hud['inventoryScreen'].data[0, 1].type TAB %armor;
		for ( %i = 1; %i < $Hud['inventoryScreen'].count; %i++ ) 
		{
			//echo( $Hud['inventoryScreen'].Data[%i, 1].type);
			%type = $Hud['inventoryScreen'].data[%i, 1].type;
			%equipment = $Hud['inventoryScreen'].data[%i, 1].getValue();

			if(%type $= %item)
				%equipment = %pack;
			
		// Special Cases again------------------------------------------------
			%invalidPack = checkPackValidity(%pack, %equipment, %item );
			if(%invalidPack)
			{
				addMessageHudLine( %packFailMsg );
				return;
			
			}

			%favList = %favList TAB %type TAB %equipment;
		}
		//echo(%favList);
	}
	commandToServer( 'setClientFav', %favList );

	//we message the player real nice like
	addMessageHudLine( "Inventory updated to " @ %pack @ "." );
}

function checkPackValidity(%pack, %equipment, %item)
{
	//echo("validityChecking:" SPC %pack SPC %equipment);
	
	// this is mostly for ease of mod makers
	// this is the base restrictions stuff
	// for your mod just overwrite this function and 
	// change the restrictions and onlyUses

	// you must have #1 to use #2
	//%restrict[#1, #2] = true;
	
	%restrict["Scout", "Inventory Station"] = true;
	%restrict["Scout", "Landspike Turret"] = true;
	%restrict["Scout", "Spider Clamp Turret"] = true;
	%restrict["Scout", "ELF Turret Barrel"] = true;
	%restrict["Scout", "Mortar Turret Barrel"] = true;
	%restrict["Scout", "AA Turret Barrel"] = true;
	%restrict["Scout", "Plasma Turret Barrel"] = true;
	%restrict["Scout", "Missile Turret Barrel"] = true;
	%restrict["Assault", "Cloak Pack"] = true;
	%restrict["Juggernaut", "Cloak Pack"] = true;

	// you can only use #1 if you have a #2 of type #3
	//%require[#1] = #2 TAB #3;

	%require["Laser Rifle"] = "Pack" TAB "Energy Pack";
	

 	if(%restrict[%equipment, %pack] )
 		return true;
	
	else if(%require[%equipment] !$="" )
	{
		if(%item $= getField(%require[%equipment], 0) )
		{
			if(%pack !$= getField(%require[%equipment], 1) )
				return true;
		}
	}
}


//------------------------------------------------------------------------------
function setDefaultInventory(%client)
{
   commandToClient(%client,'InitLoadClientFavorites');
}

//------------------------------------------------------------------------------
function checkInventory( %client, %text )
{
   %armor = getArmorDatablock( %client, $NameToInv[getField( %text, 1 )] );
   %list = getField( %text, 0 ) TAB getField( %text, 1 );
   %cmt = $CurrentMissionType;
   for( %i = 3; %i < getFieldCount( %text ); %i = %i + 2 )
   {
      %inv = $NameToInv[getField(%text,%i)];
      if ( (( %armor.max[%inv] && !($InvBanList[%cmt, %inv]) ) || 
          getField( %text, %i ) $= Empty || getField( %text, %i ) $= Invalid) 
          && (($InvTotalCount[getField( %text, %i - 1 )] - $BanCount[getField( %text, %i - 1 )]) > 0))
         %list = %list TAB getField( %text, %i - 1 ) TAB getField( %text, %i );
      else if( $InvBanList[%cmt, %inv] || %inv $= empty || %inv $= "")
         %list = %list TAB getField( %text, %i - 1 ) TAB "INVALID";         
   }
   return %list;
}

//------------------------------------------------------------------------------
function getArmorDatablock(%client, %size)
{
   if ( %client.race $= "Bioderm" )
      %armor = %size @ "Male" @ %client.race @ Armor;
   else
      %armor = %size @ %client.sex @ %client.race @ Armor;
   return %armor;
}

//------------------------------------------------------------------------------
function InventoryScreen::onWake(%this)
{
   if ( $HudHandle[inventoryScreen] !$= "" )
      alxStop( $HudHandle[inventoryScreen] );
   alxPlay(HudInventoryActivateSound, 0, 0, 0);
   $HudHandle[inventoryScreen] = alxPlay(HudInventoryHumSound, 0, 0, 0);

   if ( isObject( hudMap ) )
   {
      hudMap.pop();
      hudMap.delete();
   }
   new ActionMap( hudMap );
   hudMap.blockBind( moveMap, toggleScoreScreen );
   hudMap.blockBind( moveMap, toggleCommanderMap );
   hudMap.bindCmd( keyboard, escape, "", "InventoryScreen.onDone();" );
   hudMap.push();
}

//------------------------------------------------------------------------------
function InventoryScreen::onSleep()
{
   hudMap.pop();
   hudMap.delete();   
   alxStop($HudHandle[inventoryScreen]);
   alxPlay(HudInventoryDeactivateSound, 0, 0, 0);
   $HudHandle[inventoryScreen] = "";
}

//------------------------------------------------------------------------------
function InventoryScreen::onDone( %this )
{
   toggleCursorHuds( 'inventoryScreen' );
}

//------------------------------------------------------------------------------
function InventoryScreen::onTabSelect( %this, %favId )
{
   loadFavorite( %favId, 0 );
}

function createInvBanCount()
{
   $BanCount["Armor"] = 0;
   $BanCount["Weapon"] = 0;
   $BanCount["Pack"] = 0;
   $BanCount["Grenade"] = 0;
   $BanCount["Mine"] = 0;

   for(%i = 0; $InvArmor[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvArmor[%i]]])
         $BanCount["Armor"]++;
   $InvTotalCount["Armor"] = %i;
   
   for(%i = 0; $InvWeapon[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvWeapon[%i]]])
         $BanCount["Weapon"]++;
   $InvTotalCount["Weapon"] = %i;

   for(%i = 0; $InvPack[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvPack[%i]]])
         $BanCount["Pack"]++;
   $InvTotalCount["Pack"] = %i;

   for(%i = 0; $InvGrenade[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvGrenade[%i]]])
         $BanCount["Grenade"]++;
   $InvTotalCount["Grenade"] = %i;

   for(%i = 0; $InvMine[%i] !$= ""; %i++)
      if($InvBanList[$CurrentMissionType, $NameToInv[$InvMine[%i]]])
         $BanCount["Mine"]++;
   $InvTotalCount["Mine"] = %i;
}

// z0dd - ZOD, 5/17/03. New functions, limit armor types you can purchase
function countArmorAllowed()
{
   // This function is called from DefaultGame::assignClientTeam
   // and loadMissionStage2, so its not run constantly.

   for(%j = 1; %j < Game.numTeams; %j++)
      %teamPlayerCount[%j] = 0;

   %numClients = ClientGroup.getCount();
   for(%i = 0; %i < %numClients; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl.team > 0)
         %teamPlayerCount[%cl.team]++;
   }
   // the bigger team determines the number of armors allowed
   %maxPlayers = %teamPlayerCount[1] > %teamPlayerCount[2] ? %teamPlayerCount[1] : %teamPlayerCount[2];
   if(%maxPlayers >= 16)
      %teamArmorMax = mFloor(%maxPlayers * 0.38);
   else
      %teamArmorMax = 6;

   return $TeamArmorMax = %teamArmorMax;
}

function resetArmorMaxes()
{
   for(%i = 0; %i <= Game.numTeams; %i++)
   {
      $TeamArmorCount[%i, Light] = 0;
      $TeamArmorCount[%i, Medium] = 0;
      $TeamArmorCount[%i, Heavy] = 0;
   }
}
