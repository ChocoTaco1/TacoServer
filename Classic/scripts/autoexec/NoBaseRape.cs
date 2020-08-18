package NoBaseRape
{

// From Evolution MOD
// Modified for our needs
function StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
   //echo( %targetObject.getDataBlock().getClassName() );
   //echo( %targetObject.getDataBlock().getName() );

   %targetname = %targetObject.getDataBlock().getName();    

   //Used on some maps to make invs invincible
   if( %targetObject.invincible && %targetname $= "StationInventory" )
		return;

   if(!$Host::TournamentMode && $Host::NoBaseRapeEnabled && $Host::NoBaseRapePlayerCount > $TotalTeamPlayerCount)
   {
		if( %targetname $= "GeneratorLarge" || %targetname $= "StationInventory" || %targetname $= "SolarPanel" ) 
		{
			//Notify only if asset is on other team
			if( %targetObject.team !$= %sourceObject.team )
				NBRAssetMessage(%sourceObject);
			return;
		}
   }
   
   parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType);  
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(NoBaseRape))
    activatePackage(NoBaseRape);


// Message when a player hits a protected enemy asset
function NBRAssetMessage(%sourceObject)
{
	if($CurrentMissionType $= "CTF" && $Host::EnableNoBaseRapeNotify )
	{
		%timeDif = getSimTime() - %sourceObject.NBRTime;
		if(%timeDif > 10000 || !%sourceObject.NBRTime)
		{
			messageClient(%sourceObject.client, 'MsgNoBaseRapeNotify', '\c2No Base Rape is enabled until %1 players.', $Host::NoBaseRapePlayerCount );
			%sourceObject.NBRTime = getSimTime();
		}
	}
}
