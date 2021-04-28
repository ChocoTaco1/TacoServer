// Eolk - People like evo's stats a lot... so that's what we'll give them...
$weap_message[1] = "Blaster master";
$weap_message[2] = "Plasma roaster";
$weap_message[3] = "Chainwh0re";
$weap_message[4] = "Disc-O-maniac";
$weap_message[5] = "Grenade puppy";
$weap_message[6] = "Laser turret";
$weap_message[8] = "Mortar maniac";
$weap_message[9] = "Missile lamer";
$weap_message[10] = "Shocklance bee";
$weap_message[11] = "Mine mayhem";
$weap_message[13] = "Road killer";
// Extra Stats
$weap_message[31] = "Demoman";
$weap_message[21] = "Clamp Farmer";
$weap_message[22] = "Spike Farmer";
$weap_message[26] = "Shrike Gunner";
$weap_message[27] = "Tailgunner";
$weap_message[28] = "Bombardier";
$weap_message[29] = "Tank Gunner (chain)";
$weap_message[30] = "Tank Gunner (mortar)";
$weap_message[31] = "Satchel Punk";
$weap_message[50] = "Combo King (mine+disc)";

// Handlers
package EvoStatHandles
{

function Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC)
{
   Parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
   // call the function
   if(!$Host::TournamentMode && $Host::ClassicEvoStats)
      handleDamageStat(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
}

function DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation)
{
   Parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   // call the function
   if(!$Host::TournamentMode && $Host::ClassicEvoStats)
      handleKillStat(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
}

function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
{
   if(isObject(%targetObject)) // Console spam fix.
   {
      // call the function
      if(!$Host::TournamentMode && $Host::ClassicEvoStats)
         handleMAStat(%data, %projectile, %targetObject, %modifier, %position, %normal);
   }
   Parent::onCollision( %data, %projectile, %targetObject, %modifier, %position, %normal );
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(EvoStatHandles))
	activatePackage(EvoStatHandles);

// handleDamageStat(%targetObject, %sourceObject, %position, %amount, %damageType)
// Info: Calcs: Damage and SnipeShot detection.
function handleDamageStat(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC)
{	
	// Reject damage that is not player initiated.
	if(%damageType == 7 || %damageType == 12 || %damageType == 14 || %damageType == 24 || %damageType == 25 || %damageType == 33 || %damageType == 35 || %damageType == 36 || %damageType == 98 || %damageType == 99)
		return;

	// failsafe
	if(!isObject(%sourceObject) || %sourceObject $= "" || !isObject(%targetObject) || %targetObject $= "")
		return;

	// don't count damage done to vehicles
	if(%targetObject.isMounted())
		return;

	// Vehicle Impacts.
	if(%damageType == 13){ // run down by vehicle
		if(!(%attacker = %sourceObject.getControllingClient()) > 0){
			return;
		}
	}
	// Turrets.
	else if(%sourceObject.getClassName() $= "Turret" || %sourceObject.getClassName() $= "VehicleTurret" || %sourceObject.getClassName() $= "FlyingVehicle" || %sourceObject.getClassName() $= "HoverVehicle"){
		// Controlled
		%attacker = %sourceObject.getControllingClient(); //is turret being controlled?
		if(%attacker == 0){ // Not controlled.
			// Owned
			if(isObject(%sourceObject.owner)){
				%attacker = %sourceObject.owner;
			}
			// Automated & no-owner.
			else{
				return;
			}
		}
	} else { // Pretty much anything else.
		%attacker = %sourceObject.client;
	}

	%victim = %targetObject.client;

	// failsafe
	if(%attacker $= "" || %victim $= "" || %attacker $= %victim)
		return;

	// check if it's a tk
	if(Game.numTeams > 1 && %attacker.team $= %victim.team)
		return;
	
	// store the damage
	if($Host::ClassicStatsType == 2)
	{
		// Teratos: Going to add mine+disc as a category...
		// Teratos: Tracking only gets the second batch of damage, but we add 85% of that to make up for the first batch of damage (just a dumb estimate so damage numbers look realistic in post-game).
		if(%victim.mineDisc) {
			//$stats::weapon_damage[%attacker, 50] += %amount + (%amount * (%damageType == 11 ? 0.87 : 1.41)); // Mine accounts for more than disc.
			$stats::weapon_damage[%attacker, 50] += %amount + $stats::last_minedisc[%attacker];

			if($stats::weapon_damage[%attacker, 50] > $stats::weap_table[50])
			{
				$stats::weap_table[50]        = $stats::weapon_damage[%attacker, 50];
				$stats::client_weap_table[50] = getTaggedString(%attacker.name);
			}
		}
		$stats::last_minedisc[%attacker] = %amount; // Track the last amount of damage so we can add it to the mine+disc.
		// Teratos: END Mine+Disc Support.
		
		$stats::weapon_damage[%attacker, %damageType] += %amount;

		if($stats::weapon_damage[%attacker, %damageType] > $stats::weap_table[%damageType])
		{
			$stats::weap_table[%damageType]        = $stats::weapon_damage[%attacker, %damageType];
			$stats::client_weap_table[%damageType] = getTaggedString(%attacker.name);
		}
	}

	// is it a laser damage?
	if(%damageType == 6)
	{
		// i will consider only shots that have been fired with 60% of total energy
		if(%sourceObject.getEnergyLevel() / %sourceObject.getDataBlock().maxEnergy < 0.6)
			return;

		%distance = mFloor(VectorDist(%position, %sourceObject.getWorldBoxCenter()));

		// max distance for sniper (this is the only fix i could find)
		// if(%distance > 1000)
		// %distance = 1000;

		// is it an headshot?
		if(%victim.headshot)
		{
		  %attacker.hs++;

		  if( ( %attacker.showMA $= "" ) || ( %attacker.showMA == 1 ) )
		    bottomPrint(%attacker, "HEADSHOT (" @ %attacker.hs @ ")! Distance is " @ %distance @ " meters.", 3);
			logEcho(%attacker.nameBase @" (pl "@%attacker.player@"/cl "@%attacker@") headshot ("@%distance@")");
		  
		  if(%attacker.hs > $stats::snipe_counter)
          {
            $stats::snipe_counter = %attacker.hs;
            $stats::snipe_client  = getTaggedString(%attacker.name);
          }
          if(%distance > $stats::snipe_maxdistance)
          {
            $stats::snipe_maxdistance       = %distance;
            $stats::snipe_maxdistanceclient = getTaggedString(%attacker.name);
          }
		}
		else // no
		{
			if(%attacker.showMA $= "" || %attacker.showMA)
				bottomPrint(%attacker, "HIT! Distance is " @ %distance @ " meters.", 3);

			if(%distance > $stats::snipe_maxdistance)
         {
            $stats::snipe_maxdistance       = %distance;
            $stats::snipe_maxdistanceclient = getTaggedString(%attacker.name);
         }
		}

		// this callback will allow players to autoscreenshot the shot
		messageClient(%attacker, 'MsgSnipeShot', "", %distance);
	}
}

// handleKillStat(%clVictim, %clKiller, %damageType, %implement)
// Info: Calcs: Kills, TeamKills, FC kills
function handleKillStat(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation)
{ 
  if(%damageType == 13) // is a roadkill
    %clKiller = %implement.getControllingClient();

  
  if ( !isObject( %clVictim ) || !isObject( %clKiller ) )
    return;

 	// failsafe
	if(%clKiller $= "" || %clVictim $= "" || %clKiller $= %clVictim)
		return;

	// is it a tk?
   if(%game.numTeams > 1 && %clKiller.team $= %clVictim.team)
   {
      $stats::tk[%clKiller]++;

      if($stats::tk[%clKiller] > $stats::tk_counter)
      {
		 $stats::tk_counter = $stats::tk[%clKiller];
         $stats::tk_client  = getTaggedString(%clKiller.name);
      }
   }
   else // no
   {
   	if($Host::ClassicStatsType == 1)
   	{
	      $stats::weapon_kills[%clKiller, %damageType]++;

	      if($stats::weapon_kills[%clKiller, %damageType] > $stats::weap_table[%damageType])
	      {
	         $stats::weap_table[%damageType]        = $stats::weapon_kills[%clKiller, %damageType];
	         $stats::client_weap_table[%damageType] = getTaggedString(%clKiller.name);
	      }
	   }

		// was the victim a fc?
      if(%clVictim.plyrDiedHoldingFlag)
      {
      	$stats::fckiller[%clKiller]++;

      	if($stats::fckiller[%clKiller] > $stats::fckiller_counter)
	      {
	         $stats::fckiller_counter = $stats::fckiller[%clKiller];
	         $stats::fckiller_client = getTaggedString(%clKiller.name);
	      }
	   }
   }
}

// handleMAStat(%projectile, %targetObject, %position)
// Info: MA detection
function handleMAStat(%data, %projectile, %targetObject, %modifier, %position, %normal)
{
	// failsafe
	if(!isObject(%targetObject) || %targetObject $= "")
		return;

	// failsafe
	if(!isObject(%projectile.sourceObject) || %projectile.sourceObject $= "")
		return;

	%victim = %targetObject.client;
	%killer = %projectile.sourceObject.client;

	//	// Altair's method
	//	%distance = mFloor(VectorDist(%position, %projectile.sourceObject.getWorldBoxCenter()));

	//	// Evolution Method
	%distance = mFloor(VectorDist(%position, %projectile.initialPosition));

	// failsafe
	if(%victim $= "" || %killer $= "")
		return;

	%projectileType = %data.getName() !$= "TR2DiscProjectile" ? %data.getName() : "DiscProjectile";

	// only disc, plasma, or blaster
	if(%projectileType !$= "DiscProjectile" && %projectileType !$= "PlasmaBolt" && %projectileType !$= "EnergyBolt")
		return;

	// is it a tk?
	if(Game.numTeams > 1 && %killer.team $= %victim.team)
		return;

	// Eolk - changes to MA code
	%position = %targetObject.getPosition();
	%raycast = containerRaycast(%position, vectorAdd(%position, "0 0 -10"), $TypeMasks::ForceFieldObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::StaticObjectType | $TypeMasks::TerrainObjectType, %targetObject);
	if(!isObject(firstWord(%raycast))) // We've got something...
	{
		switch$(%projectileType)
		{
			case DiscProjectile:
				%killer.midairs++;

				if( ( %killer.showMA $= "" ) || ( %killer.showMA == 1 ) )   
					bottomPrint(%killer, "Midair Disk (" @ %killer.midairs @ ")! Distance is " @ %distance @ " meters.", 3);

				// this callback will allow players to autoscreenshot the MA
				messageClient(%killer, 'MsgMidAir', "", %distance);
				logEcho(%killer.nameBase @" (pl "@%killer.player@"/cl "@%killer@") hit a midair disc shot ("@%distance@")");

				if(%killer.midairs > $stats::ma_counter)
				{
					$stats::ma_counter = %killer.midairs;
					$stats::ma_client  = getTaggedString(%killer.name);
				}

				if(%distance > $stats::ma_maxdistance)
				{
					$stats::ma_maxdistance       = %distance;
					$stats::ma_maxdistanceclient = getTaggedString(%killer.name);
				}
			case PlasmaBolt:
				%killer.PlaMA++;

				if ( ( %killer.showMA $= "" ) || ( %killer.showMA == 1 ) )   
					bottomPrint(%killer, "Midair Plasma (" @ %killer.PlaMA @ ")! Distance is " @ %distance @ " meters.", 3);

				// this callback will allow players to autoscreenshot the MA
				messageClient(%killer, 'MsgPlasmaMidAir', "", %distance);
				logEcho(%killer.nameBase @" (pl "@%killer.player@"/cl "@%killer@") hit a midair plasma shot ("@%distance@")");

				if(%killer.PlaMA > $stats::PlaMA_counter)
				{
					$stats::PlaMA_counter = %killer.PlaMA;
					$stats::PlaMA_client  = getTaggedString(%killer.name);
				}

				if(%distance > $stats::PlaMA_maxdistance)
				{
					$stats::PlaMA_maxdistance       = %distance;
					$stats::PlaMA_maxdistanceclient = getTaggedString(%killer.name);
				}
			case EnergyBolt:
				%killer.blaMA++;

				if( ( %killer.showMA $= "" ) || ( %killer.showMA == 1 ) )   
					bottomPrint(%killer, "Midair Blaster (" @ %killer.blaMA @ ")! Distance is " @ %distance @ " meters.", 3);

				// this callback will allow players to autoscreenshot the MA
				messageClient(%killer, 'MsgBlasterMidAir', "", %distance);
				logEcho(%killer.nameBase @" (pl "@%killer.player@"/cl "@%killer@") hit a midair blaster shot ("@%distance@")");

				if(%killer.blaMA > $stats::BlaMA_counter)
				{
					$stats::BlaMA_counter = %killer.BlaMA;
					$stats::BlaMA_client  = getTaggedString(%killer.name);
				}

				if(%distance > $stats::BlaMA_maxdistance)
				{
					$stats::BlaMA_maxdistance       = %distance;
					$stats::BlaMA_maxdistanceclient = getTaggedString(%killer.name);
				}
		}
	}
}

// sendEvoDebriefing(%client)
// Info: Send Evo stats to the debriefing page
function sendEvoDebriefing(%client)
{
	// Eolk - Remove redundant checks
	messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00>%1 - %2<spop>', $MissionDisplayName, $MissionTypeDisplayName);
      
	if($stats::MaxGrabSpeed || $stats::grabs_counter || $stats::fckiller_counter || $stats::caps_counter || $stats::fastestCap)
	{
		messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>FLAG STATS<spop>');
		if($stats::fastestCap)
			messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> Fastest Cap</clip><lmargin%%:30><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2</clip><spop>', $stats::fastcap_client, $stats::fastcap_time);
		if($stats::MaxGrabSpeed)
			messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> Flaming Ass</clip><lmargin%%:30><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2 Kph!</clip><spop>', $stats::Grabber, $stats::MaxGrabSpeed);
		if($stats::caps_counter)
			messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> Cap Mastah</clip><lmargin%%:30><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2</clip><spop>', $stats::caps_client, $stats::caps_counter);
		if($stats::grabs_counter)
			messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> Grabz0r</clip><lmargin%%:30><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2</clip><spop>', $stats::grabs_client, $stats::grabs_counter);
		if($stats::fckiller_counter)
			messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> FC killer</clip><lmargin%%:30><clip%%:60> %1</clip><lmargin%%:60><clip%%:40> %2</clip><spop>', $stats::fckiller_client, $stats::fckiller_counter);
	}
      
	if($stats::BlaMA_counter || $stats::ma_counter || $stats::PlaMA_counter)
	{
		messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>MID AIR<lmargin%%:30>CHAMPION<lmargin%%:60>DISTANCE<spop>');

		if($stats::ma_counter)
			messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> Disk</clip><lmargin%%:30><clip%%:60> %1 (%2)</clip><lmargin%%:60><clip%%:60> %3 (%4 mt)</clip><spop>', $stats::ma_client, $stats::ma_counter, $stats::ma_maxdistanceclient, $stats::ma_maxdistance);
		if($stats::PlaMA_counter)
			messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> Plasma</clip><lmargin%%:30><clip%%:60> %1 (%2)</clip><lmargin%%:60><clip%%:60> %3 (%4 mt)</clip><spop>', $stats::PlaMA_client, $stats::PlaMA_counter, $stats::PlaMA_maxdistanceclient, $stats::PlaMA_maxdistance);
		if($stats::BlaMA_counter)
		    messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> Blaster</clip><lmargin%%:30><clip%%:60> %1 (%2)</clip><lmargin%%:60><clip%%:60> %3 (%4 mt)</clip><spop>', $stats::BlaMA_client, $stats::BlaMA_counter, $stats::BlaMA_maxdistanceclient, $stats::BlaMA_maxdistance);
	}
      
	if($stats::snipe_counter)
		messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>Headhunter<lmargin%%:30> %1 (%2)!<spop>', $stats::snipe_client, $stats::snipe_counter);
      
	if($stats::snipe_maxdistance)
	{
		%x = $stats::snipe_counter ? "" : "\n";
		messageClient(%client, 'MsgDebriefAddLine', "", '%3<lmargin:0><spush><color:00dc00><font:univers condensed:18>Longest Snipeshot is %1 meters by %2<spop>', $stats::snipe_maxdistance, $stats::snipe_maxdistanceclient, %x);
	}
      
	for(%damageType = 1; %damageType < 51; %damageType++)
	{
		if(%damageType == 7 || %damageType == 12 || (%damageType > 13 && %damageType < 21) || %damageType == 23  || %damageType == 24 || %damageType == 25) {
			continue;
		}
		if(%damageType > 31 && %damageType < 50) {
			continue;
		}

		if($stats::weap_table[%damageType] > 0)
		{
			if($Host::ClassicStatsType == 2)
			{
				if(!%message)
				{
					messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>TYPE<lmargin%%:30>PLAYER<lmargin%%:60>TOTAL DAMAGE<spop>');
					%message = 1;
				}
				messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> %1</clip><lmargin%%:30><clip%%:60> %2</clip><lmargin%%:60><clip%%:40> %3</clip><spop>', $weap_message[%damageType], $stats::client_weap_table[%damageType], mFormatFloat($stats::weap_table[%damageType], "%.2f"));
			}
			else if($Host::ClassicStatsType == 1)
			{
				if(!%message)
				{
					messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>TYPE<lmargin%%:30>PLAYER<lmargin%%:60>KILLS<spop>');
					%message = 1;
				}
				messageClient(%client, 'MsgDebriefAddLine', "", '<lmargin:0><spush><color:00dc00><clip%%:40><font:univers condensed:18> %1</clip><lmargin%%:30><clip%%:60> %2</clip><lmargin%%:60><clip%%:40> %3</clip><spop>', $weap_message[%damageType], $stats::client_weap_table[%damageType], $stats::weap_table[%damageType]);
			}
		}
	}
      
	if($stats::tk_counter)
		messageClient(%client, 'MsgDebriefAddLine', "", '\n<lmargin:0><spush><color:00dc00><font:univers condensed:18>And the best teamkiller award goes to... %1 (%2)!<spop>', $stats::tk_client, $stats::tk_counter);
}
