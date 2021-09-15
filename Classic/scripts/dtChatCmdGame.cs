//----------------------------------DarkTiger's Chat Commands----------------------------------

// To install and use put the following  if statement at the top of chatMessageAll
// if your useing evo the override is in evoPackage.cs otherwise messages.cs

package dtChatCmd
{

function chatMessageAll(%sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
	if ( getsubstr(detag(%a2),0,1) $= "/" )
	{
        chatCmd(%sender,%a2,0);
        return;
    }

	parent::chatMessageAll(%sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(dtChatCmd))
    activatePackage(dtChatCmd);

/////////////////////////////////////////////////////////////////////////////

function chatCmd(%client, %message) //%client is sender
{
	%command = strlwr(trim(getWord(%message, 0)));// strip command trim and make it lower case

	switch$(%command)
	{
      case "/help":
         if(%client.isSuperAdmin)
		 {
            messageClient(%client, 'msgChatCmd', '\c2/summon "player name" - will summon a player to you.');
            messageClient(%client, 'msgChatCmd', '\c2/warpto "player name" - will warp you to a player.');
            messageClient(%client, 'msgChatCmd', '\c2/snowsky - changes the sky to a snow sky.');
			messageClient(%client, 'msgChatCmd', '\c2/firesky - changes the sky to a fire sky.');
            messageClient(%client, 'msgChatCmd', '\c2/rainsky - changes the sky to a rain sky.');
            messageClient(%client, 'msgChatCmd', '\c2/sandsky - changes the sky to a sand sky.');
            messageClient(%client, 'msgChatCmd', '\c2/nicesky - changes the sky to a variation of four nice skies.');
            messageClient(%client, 'msgChatCmd', '\c2/normalsky - changes the sky to a fallback sky.');
			messageClient(%client, 'msgChatCmd', '\c2/spookysky - changes the sky to a halloween sky.');
            messageClient(%client, 'msgChatCmd', '\c2/fireworks - look at some fireworks.');
            messageClient(%client, 'msgChatCmd', '\c2/disableAI  -  as it sounds.');
            messageClient(%client, 'msgChatCmd', '\c2/enableAI  -  as it sounds.');
         }
         else if(%client.isAdmin)
		 {
            messageClient(%client, 'msgChatCmd', '\c2/snowsky - changes the sky to a snow sky.');
			messageClient(%client, 'msgChatCmd', '\c2/firesky - changes the sky to a fire sky.');
            messageClient(%client, 'msgChatCmd', '\c2/rainsky - changes the sky to a rain sky.');
            messageClient(%client, 'msgChatCmd', '\c2/sandsky - changes the sky to a sand sky.');
	        messageClient(%client, 'msgChatCmd', '\c2/nicesky - changes the sky to a variation of four nice skies.');
            messageClient(%client, 'msgChatCmd', '\c2/normalsky - changes the sky to a fallback sky.');
			messageClient(%client, 'msgChatCmd', '\c2/spookysky - changes the sky to a halloween sky.');
            messageClient(%client, 'msgChatCmd', '\c2/fireworks - look at some fireworks.');
			messageClient(%client, 'msgChatCmd', '\c2/AIQ 1 or 0 - to enable tor disable ai chat.');
            messageClient(%client, 'msgChatCmd', '\c2/idInfo - get id resources.');
         }
         messageClient(%client, 'msgChatCmd', '\c2/report "message" - report a problem for server owner.');
         messageClient(%client, 'msgChatCmd', '\c2/msg "message" - leave the server owner a message.');

	   case "/summon":
         if(%client.isSuperAdmin)
		 {
			%pos = VectorAdd(%client.player.getPosition(), "0 0 10");
			%cl = clientNameAuto(getWord(%message, 1));
			%obj = %cl.player;
			%obj.setTransform(%pos SPC getWords(%obj.getTransform(), 3, 6));
			%obj.setVelocity("0 0 1");// stop them incase they are going over 9000
         }

	   case "/warpto":
         if(%client.isSuperAdmin)
		 {
			%cl = clientNameAuto(getWord(%message, 1));
			%pos = VectorAdd(%cl.player.getPosition(), "0 0 10");
			%obj = %client.player;
			%obj.setTransform(%pos SPC getWords(%obj.getTransform(), 3, 6));
         }

	   case "/fireworks":
         if(%client.isAdmin || %client.isSuperAdmin )
		 {
			fireworksSky(1);// only one sky for right now
			$CurrentSky = "fireworks";
         }

	   case "/normalsky":
         if(%client.isAdmin || %client.isSuperAdmin )
		 {
			normalSky(1);// only one sky for right now
			$CurrentSky = "normal";
         }

       case "/firesky":
         if(%client.isAdmin || %client.isSuperAdmin )
		 {
            fireSky(1);// only one sky for right now
			$CurrentSky = "fire";
         }

	   case "/rainsky":
         if(%client.isAdmin || %client.isSuperAdmin )
		 {
			rainSky(1);// only one sky for right now
			$CurrentSky = "rain";
         }

	   case "/snowsky":
         if(%client.isAdmin || %client.isSuperAdmin )
		 {
			snowSky(1);// only one sky for right now
			$CurrentSky = "snow";
         }

	   case "/sandsky":
         if(%client.isAdmin || %client.isSuperAdmin )
		 {
			sandSky(1);// only one sky for right now
			$CurrentSky = "sand";
         }
	   case "/nicesky":
         if(%client.isAdmin || %client.isSuperAdmin )
		 {
			niceSky(1);// only one sky for right now
			$CurrentSky = "nice";
		 }

	   case "/report":
		   LogMessage(%client, %message, "report");
		   messageClient(%client, 'msgChatCmd', 'Your report has been received.');

       case "/msg":
		   LogMessage(%client, %message, "message");
		   messageClient(%client, 'msgChatCmd', 'Your message has been received.');

       case "/idInfo":
			if(%client.isSuperAdmin || %client.isAdmin)
			{
				%num = new scriptObject();
				messageClient(%client, 'msgChatCmd', '\c2 Num of id left %1 / 2147483647 = %2%',%num, (%num / 2147483647) * 100);
				%num.delete();
			}

       case "/enableAI":
			if(%client.isSuperAdmin)
			{
				AISystemEnabled(true);
				messageAll('message', 'AI is now enabled.');
			}

       case "/disableAI":
			if(%client.isSuperAdmin)
			{
				AISystemEnabled(false);
				messageAll('message', 'AI is now disabled.');
			}

		case "/aichat":
			if(%client.isSuperAdmin || %client.isAdmin){
			if(!$AIDisableChat){
				$AIDisableChat = 1;
				messageAll('message', '\c2AI Chat Disable');
			}
			else{
				$AIDisableChat = 0;
				messageAll('message', '\c2AI Chat Enable');
			}
		}

	   case "/spookysky":
         if(%client.isAdmin || %client.isSuperAdmin )
		 {
			spookySky(1);// only one sky for right now
			$CurrentSky = "spookySky";
         }

	    default:
		   messageClient(%client, 'msgChatCmd', '\c2Oops, that command is not recognized. ');
	}

}

function clientNameAuto(%name)  //client name auto complate
{
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{ // the client list
      %client = ClientGroup.getObject(%i);
		%fullName = %client.nameBase;
		%fullName = strlwr(%fullName);
		%partname = strlwr(%name);
		for(%a=1; %a <= strlen(%partname); %a++){
		   if(getSubStr(%fullName,0,%a) $= getSubStr(%partname,0,%a)){
            //echo(getSubStr(%fullName,0,%a) SPC  getSubStr(%partname,0,%a));
            if(%c[%i] > %x){
                %x =%c[%i];
			      %f = %i;
            }
            %c[%i]++;
		   }
		}
	}
	//echo(ClientGroup.getObject(%f).nameBase);
			return ClientGroup.getObject(%f);
}

function LogMessage(%client, %msg, %cat) //phantoms chatlogging
{
	%filename = "logs/" @ %cat @ "/" @ formattimestring("mm-dd-yy") @ ".txt";

	if (!IsFile(%filename))
	{
		new fileobject(Clog);
		Clog.openforwrite(%filename);
		Clog.writeline(""@%client.namebase@"["@formattimestring("hh:nn a, mm - dd - yy")@"] : "@%msg@"");
		Clog.close();
		Clog.delete();
	}
	else
	{
		new fileobject(Clog);
		Clog.openforappend(%filename);
		Clog.writeline(""@%client.namebase@"["@formattimestring("hh:nn a, mm - dd - yy")@"] : "@%msg@"");
		Clog.close();
		Clog.delete();
	}
}

function removeSky(%sky)
{
	if(isObject(Sky))
		Sky.delete();
	if(isObject(Precipitation))
	{
		alxStopAll();
		Precipitation.delete();
	}
	if(isObject(Lightning))
		Lightning.delete();
	if(isObject(FireballAtmosphere))
		FireballAtmosphere.delete();
	if(isObject(PlanetSoundEmitter))
		PlanetSoundEmitter.delete();
	if(isObject(PlanetSoundEmitter))
		PlanetSoundEmitter.delete();
}

function normalSky(%sky)
{
	if($CurrentSky $= "normal")
		return;

	MessageAll('Msg', "\c2Looks like the weather is clearing up.");

	removeSky(%sky);

	new Sky(Sky)
	{
		position = "0 0 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		cloudHeightPer[0] = "0.349971";
		cloudHeightPer[1] = "0.25";
		cloudHeightPer[2] = "0.199973";
		cloudSpeed1 = "0.0001";
		cloudSpeed2 = "0.0002";
		cloudSpeed3 = "0.0003";
		visibleDistance = "425";
		useSkyTextures = "1";
		renderBottomTexture = "0";
		SkySolidColor = "0.420000 0.420000 0.420000 0.000000";
		fogDistance = "350";
		high_visibleDistance = "-1";
		high_fogDistance = "-1";
		fogColor = "0.420000 0.420000 0.420000 1.000000";
		fogVolume1 = "0 0 0";
		fogVolume2 = "0 0 0";
		fogVolume3 = "0 0 0";
		materialList = "sky_lush_blue.dml";
		windVelocity = "1 1 0";
		windEffectPrecipitation = "1";
		fogVolumeColor1 = "128.000000 128.000000 128.000000 -222768174765569861000000000000000000000.000000";
		fogVolumeColor2 = "128.000000 128.000000 128.000000 0.000000";
		fogVolumeColor3 = "128.000000 128.000000 128.000000 -170698929442160049000000000000000000000.000000";
	};

}

function fireSky(%sky)
{
	if($CurrentSky $= "fire")
		return;

	MessageAll('Msg', "\c2Is it getting hot outside?");

	removeSky(%sky);

	new Sky(Sky)
	{
		position = "-1216 -1336 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		cloudHeightPer[0] = "0.249971";
		cloudHeightPer[1] = "0.25";
		cloudHeightPer[2] = "0.05";
		cloudSpeed1 = "0.003";
		cloudSpeed2 = "0.001";
		cloudSpeed3 = "0.0008";
		visibleDistance = "630";
		useSkyTextures = "1";
		renderBottomTexture = "0";
		SkySolidColor = "0.000000 0.000000 0.000000 0.000000";
		fogDistance = "300";
		fogColor = "0.500000 0.200000 0.000000 1.000000";
		fogVolume1 = "0 0 0";
		fogVolume2 = "0 0 0";
		fogVolume3 = "0 0 0";
		materialList = "RedPlanet.dml";
		windVelocity = "1 0 0";
		windEffectPrecipitation = "0";
		fogVolumeColor1 = "128.000000 128.000000 128.000000 -1037713472.000000";
		fogVolumeColor2 = "128.000000 128.000000 128.000000 -1037713472.000000";
		fogVolumeColor3 = "128.000000 128.000000 128.000000 -1037713472.000000";
		high_visibleDistance = "-1";
		high_fogDistance = "-1";
		high_fogVolume1 = "-1 3.22439e-42 1.04486e-40";
		high_fogVolume2 = "-1 1.04845e-40 3.26643e-42";
		high_fogVolume3 = "-1 3.28324e-42 1.05581e-40";

		cloudSpeed0 = "0.000000 0.000000";
	};

	MissionCleanup.add(Sky);

	%fireball = new FireballAtmosphere(FireballAtmosphere)
	{
		position = "0 0 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		dataBlock = "fireball";
		lockCount = "1";
		homingCount = "1";
		dropRadius = 100;
		dropsPerMinute = 200;
		minDropAngle = "0";
		maxDropAngle = "50";
		startVelocity = "300";
		dropHeight = "2000";
		dropDir = "0.212 0.212 -0.953998";
	};
	%embers = new Precipitation(Precipitation)
	{
		position = "116.059 -26.7731 156.557";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		dataBlock = "Snow";
		lockCount = "0";
		homingCount = "0";
		percentage = "1";
		color1 = "0.000000 0.000000 0.000000 1.000000";
		color2 = "-1.000000 0.000000 0.000000 1.000000";
		color3 = "-1.000000 0.000000 0.000000 1.000000";
		offsetSpeed = "0";
		minVelocity = "0.02";
		maxVelocity = "0.06";
		maxNumDrops = "500";
		maxRadius = "125";
	};
	%firewind =	new AudioEmitter(PlanetSoundEmitter)
	{
		position = "289.762 209.214 173.677";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		fileName = "fx/environment/drywind.wav";
		useProfileDescription = "0";
		outsideAmbient = "1";
		volume = "1";
		isLooping = "1";
		is3D = "0";
		minDistance = "20";
		maxDistance = "1280";
		coneInsideAngle = "360";
		coneOutsideAngle = "360";
		coneOutsideVolume = "1";
		coneVector = "0 0 1";
		loopCount = "-1";
		minLoopGap = "0";
		maxLoopGap = "0";
		type = "EffectAudioType";
	};

	MissionCleanup.add(%fireball);
	MissionCleanup.add(%embers2);
	MissionCleanup.add(%firewind);

}



function rainSky(%sky)
{
	if($CurrentSky $= "rain")
		return;

	MessageAll('Msg', "\c2Looks like a storm is brewing.");

	removeSky(%sky);

	new Sky(Sky)
	{
		position = "0 0 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		cloudHeightPer[0] = "0.249971";
		cloudHeightPer[1] = "0.25";
		cloudHeightPer[2] = "0.05";
		cloudSpeed1 = "0.003";
		cloudSpeed2 = "0.001";
		cloudSpeed3 = "0.0008";
		visibleDistance = "650";
		useSkyTextures = "1";
		renderBottomTexture = "0";
		SkySolidColor = "0.520000 0.520000 0.520000 1.000000";
		fogDistance = "300";
		fogColor = "0.520000 0.520000 0.520000 1.000000";
		fogVolume1 = "0 0 0";
		fogVolume2 = "0 0 0";
		fogVolume3 = "0 0 0";
		materialList = "lush_dusk.dml";
		windVelocity = "1 0 0";
		windEffectPrecipitation = "1";
		fogVolumeColor1 = "128.000000 128.000000 128.000000 -222768174765569861000000000000000000000.000000";
		fogVolumeColor2 = "128.000000 128.000000 128.000000 0.000000";
		fogVolumeColor3 = "128.000000 128.000000 128.000000 -170698929442160049000000000000000000000.000000";
		high_visibleDistance = "-1";
		high_fogDistance = "-1";
		high_fogVolume1 = "-1 107 1.07457e-38";
		high_fogVolume2 = "-1 9.69184e-34 8.26766e-44";
		high_fogVolume3 = "-1 0 3.2509e-38";

		cloudSpeed0 = "0.000000 0.000400";
	};

	MissionCleanup.add(Sky);

	//Requires RainNoSound Datablock in weather.cs
	%rain = new Precipitation(Precipitation)
	{
		position = "-336.859 -631.623 191.648";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		dataBlock = "RainNoSound";
		lockCount = "0";
		homingCount = "0";
		percentage = "1";
		color1 = "0.600000 0.650000 0.680000 1.000000";
		color2 = "-1.000000 0.000000 0.000000 1.000000";
		color3 = "-1.000000 0.000000 0.000000 1.000000";
		offsetSpeed = "0.25";
		minVelocity = "1.25";
		maxVelocity = "4";
		maxNumDrops = "1000";
		maxRadius = "80";
	};
	%lightning = new Lightning(Lightning)
	{
		position = "-274.935 -143.111 353.049";
		rotation = "1 0 0 0";
		scale = "512 512 300";
		dataBlock = "DefaultStorm";
		lockCount = "0";
		homingCount = "0";
		strikesPerMinute = "12";
		strikeWidth = "2.5";
		chanceToHitTarget = "0.5";
		strikeRadius = "20";
		boltStartRadius = "20";
		color = "1.000000 1.000000 1.000000 1.000000";
		fadeColor = "0.100000 0.100000 1.000000 1.000000";
		useFog = "0";
	};
	%rainthunder =	new AudioEmitter(PlanetSoundEmitter)
	{
		position = "289.762 209.214 173.677";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		fileName = "fx/environment/rumblingthunder.wav";
		useProfileDescription = "0";
		outsideAmbient = "1";
		volume = "1";
		isLooping = "1";
		is3D = "0";
		minDistance = "20";
		maxDistance = "1280";
		coneInsideAngle = "360";
		coneOutsideAngle = "360";
		coneOutsideVolume = "1";
		coneVector = "0 0 1";
		loopCount = "-1";
		minLoopGap = "0";
		maxLoopGap = "0";
		type = "EffectAudioType";
	};
	%rainsound =	new AudioEmitter(PlanetSoundEmitter)
	{
		position = "-361.683 451 83.9062";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		profile = "Universal_Rain_Light_1";
		description = "AudioLooping2D";
		useProfileDescription = "1";
		outsideAmbient = "1";
		volume = "1";
		isLooping = "1";
		is3D = "1";
		minDistance = "20";
		maxDistance = "100";
		coneInsideAngle = "360";
		coneOutsideAngle = "360";
		coneOutsideVolume = "1";
		coneVector = "0 0 1";
		loopCount = "-1";
		minLoopGap = "0";
		maxLoopGap = "0";
		type = "EffectAudioType";
	};

	MissionCleanup.add(%rain);
	MissionCleanup.add(%lightning);
	MissionCleanup.add(%rainthunder);
	MissionCleanup.add(%rainsound);
}



function snowSky(%sky)
{
	if($CurrentSky $= "snow")
		return;

	MessageAll('Msg', "\c2The temperature seems to be dropping.");

	removeSky(%sky);
	removeSky(%sky);

	new Sky(Sky)
	{
		position = "0 0 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		cloudHeightPer[0] = "0.349971";
		cloudHeightPer[1] = "0.25";
		cloudHeightPer[2] = "0.199973";
		cloudSpeed1 = "0.0001";
		cloudSpeed2 = "0.0002";
		cloudSpeed3 = "0.0003";
		visibleDistance = "450";
		useSkyTextures = "1";
		SkySolidColor = "0.365000 0.390000 0.420000 0.000000";
		fogDistance = "150";
		fogColor = "0.650000 0.650000 0.700000 1.000000";
		fogVolume1 = "0 0 0";
		fogVolume2 = "0 0 0";
		fogVolume3 = "0 0 0";
		materialList = "sky_ice_blue.dml";
		windVelocity = "1 0 0";
		windEffectPrecipitation = "0";
		fogVolumeColor1 = "128.000000 128.000000 128.000000 -0.000000";
		fogVolumeColor2 = "128.000000 128.000000 128.000000 0.000000";
		fogVolumeColor3 = "128.000000 128.000000 128.000000 0.000000";

		cloudSpeed0 = "0.000000 0.000400";
	};

	MissionCleanup.add(Sky);

	%snow = new Precipitation(Precipitation)
	{
		position = "0 0 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		dataBlock = "Snow";
		percentage = "1";
		color1 = "1.000000 1.000000 1.000000 1.000000";
		color2 = "-1.000000 0.000000 0.000000 1.000000";
		color3 = "-1.000000 0.000000 0.000000 1.000000";
		offsetSpeed = "0.25";
		minVelocity = "0.25";
		maxVelocity = "1.5";
		maxNumDrops = "2000";
		maxRadius = "125";
	};
	%snowwind =	new AudioEmitter(PlanetSoundEmitter)
	{
		position = "289.762 209.214 173.677";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		fileName = "fx/environment/coldwind1.wav";
		useProfileDescription = "0";
		outsideAmbient = "1";
		volume = "1";
		isLooping = "1";
		is3D = "0";
		minDistance = "20";
		maxDistance = "1280";
		coneInsideAngle = "360";
		coneOutsideAngle = "360";
		coneOutsideVolume = "1";
		coneVector = "0 0 1";
		loopCount = "-1";
		minLoopGap = "0";
		maxLoopGap = "0";
		type = "EffectAudioType";
	};

	MissionCleanup.add(%snow);
	MissionCleanup.add(%snowwind);
}


function sandSky(%sky)
{
	if($CurrentSky $= "sand")
		return;

	MessageAll('Msg', "\c2Visibility looks like its getting low.");

	removeSky(%sky);
	removeSky(%sky);

	new Sky(Sky)
	{
		position = "-1216 -848 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		cloudHeightPer[0] = "0.349971";
		cloudHeightPer[1] = "0.25";
		cloudHeightPer[2] = "0.199973";
		cloudSpeed1 = "0.0001";
		cloudSpeed2 = "0.0002";
		cloudSpeed3 = "0.0003";
		visibleDistance = "620";
		useSkyTextures = "1";
		renderBottomTexture = "0";
		SkySolidColor = "0.390000 0.390000 0.390000 0.000000";
		fogDistance = "520";
		fogColor = "0.941100 0.858800 0.490000 1.000000";
		fogVolume1 = "700 50 100";
		fogVolume2 = "900 100 200";
		fogVolume3 = "1000 200 500";
		materialList = "Lush_l4.dml";
		windVelocity = "1 0 0";
		windEffectPrecipitation = "0";
		fogVolumeColor1 = "128.000000 128.000000 128.000000 0.000000";
		fogVolumeColor2 = "128.000000 128.000000 128.000000 -198748244414614883000000000000000000000.000000";
		fogVolumeColor3 = "128.000000 128.000000 128.000000 -222768174765569861000000000000000000000.000000";
		high_visibleDistance = "-1";
		high_fogDistance = "-1";
		high_fogVolume1 = "-1 0 0";
		high_fogVolume2 = "-1 1.83185e-39 1.8314e-39";
		high_fogVolume3 = "-1 1.83649e-39 5.46665e+36";

		cloudSpeed0 = "0.000000 0.000000";
	};

	MissionCleanup.add(Sky);

	%sandwind =	new AudioEmitter(PlanetSoundEmitter)
	{
		position = "289.762 209.214 173.677";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		fileName = "fx/environment/snowstorm2.wav";
		useProfileDescription = "0";
		outsideAmbient = "1";
		volume = "1";
		isLooping = "1";
		is3D = "0";
		minDistance = "20";
		maxDistance = "1280";
		coneInsideAngle = "360";
		coneOutsideAngle = "360";
		coneOutsideVolume = "1";
		coneVector = "0 0 1";
		loopCount = "-1";
		minLoopGap = "0";
		maxLoopGap = "0";
		type = "EffectAudioType";
	};

	MissionCleanup.add(%sandwind);
}


function niceSky(%sky)
{
	removeSky(%sky);
	removeSky(%sky);

	if($niceSkyNumber $= "" || $niceSkyNumber $= 4)
	{
		new Sky(Sky)
		{
			position = "-1216 -848 0";
			rotation = "1 0 0 0";
			scale = "1 1 1";
			cloudHeightPer[0] = "0.349971";
			cloudHeightPer[1] = "0.25";
			cloudHeightPer[2] = "0.199973";
			cloudSpeed1 = "0.0001";
			cloudSpeed2 = "0.0002";
			cloudSpeed3 = "0.0003";
			visibleDistance = "950";
			useSkyTextures = "1";
			renderBottomTexture = "0";
			SkySolidColor = "0.390000 0.390000 0.390000 0.000000";
			fogDistance = "800";
			fogColor = "0.800000 0.500000 0.300000 1.000000";
			fogVolume1 = "1700 10 320";
			fogVolume2 = "0 0 0";
			fogVolume3 = "0 0 0";
			materialList = "SOM_TR2_Armageddon.dml";
			windVelocity = "1 0 0";
			windEffectPrecipitation = "0";
			fogVolumeColor1 = "128.000000 128.000000 0.000000 0.000000";
			fogVolumeColor2 = "128.000000 128.000000 128.000000 -198748244414614883000000000000000000000.000000";
			fogVolumeColor3 = "128.000000 128.000000 128.000000 -222768174765569861000000000000000000000.000000";
			high_visibleDistance = "-1";
			high_fogDistance = "-1";
			high_fogVolume1 = "-1 -nan -nan";
			high_fogVolume2 = "-1 -nan -nan";
			high_fogVolume3 = "-1 -nan -nan";

			cloudSpeed0 = "0.000000 0.000000";
		};

		MissionCleanup.add(Sky);
		$niceSkyNumber = 1;
	}
	else if($niceSkyNumber $= 1)
	{
		new Sky(Sky)
		{
			position = "-1024 -1024 0";
			rotation = "1 0 0 0";
			scale = "1 1 1";
			cloudHeightPer[0] = "0.349971";
			cloudHeightPer[1] = "0.25";
			cloudHeightPer[2] = "0.199973";
			cloudSpeed1 = "0.0001";
			cloudSpeed2 = "0.0002";
			cloudSpeed3 = "0.0003";
			visibleDistance = "700";
			useSkyTextures = "1";
			renderBottomTexture = "0";
			SkySolidColor = "1.000000 1.000000 1.000000 1.000000";
			fogDistance = "430";
			fogColor = "0.200000 0.200000 0.200000 0.000000";
			fogVolume1 = "0 0 0";
			fogVolume2 = "0 0 0";
			fogVolume3 = "0 0 0";
			materialList = "Euro4_Bleed.dml";
			windVelocity = "1 1 1";
			windEffectPrecipitation = "0";
			fogVolumeColor1 = "2.010000 2.010000 2.010000 10000.000000";
			fogVolumeColor2 = "1.000000 1.000000 1.000000 0.742938";
			fogVolumeColor3 = "0.000000 0.000000 0.000000 1.000000";
			high_visibleDistance = "-1";
			high_fogDistance = "-1";
			high_fogVolume1 = "1 0 2.77029e-34";
			high_fogVolume2 = "1 2.77038e-34 1";
			high_fogVolume3 = "1 0 1.62236e-07";

			cloudSpeed0 = "0.900000 0.900000";
		};

		MissionCleanup.add(Sky);
		$niceSkyNumber = 2;
	}
	else if($niceSkyNumber $= 2)
	{
		new Sky(Sky)
		{
			position = "0 0 0";
			rotation = "1 0 0 0";
			scale = "1 1 1";
			cloudHeightPer[0] = "0.349971";
			cloudHeightPer[1] = "0.25";
			cloudHeightPer[2] = "0.199973";
			cloudSpeed1 = "0.0001";
			cloudSpeed2 = "0.0002";
			cloudSpeed3 = "0.0003";
			visibleDistance = "560";
			useSkyTextures = "1";
			renderBottomTexture = "0";
			SkySolidColor = "0.260000 0.410000 0.440000 1.000000";
			fogDistance = "420";
			fogColor = "0.260000 0.410000 0.440000 1.000000";
			fogVolume1 = "0 0 0";
			fogVolume2 = "0 0 0";
			fogVolume3 = "0 0 0";
			materialList = "Starfallen.dml";
			windVelocity = "1 0 0";
			windEffectPrecipitation = "0";
			fogVolumeColor1 = "128.000000 128.000000 128.000000 -36610319922801672200.000000";
			fogVolumeColor2 = "128.000000 128.000000 128.000000 9500070315656657560000000.000000";
			fogVolumeColor3 = "128.000000 128.000000 128.000000 0.000000";
			high_visibleDistance = "-1";
			high_fogDistance = "-1";
			high_fogVolume1 = "-1 -2.58511e+36 2.28656e-38";
			high_fogVolume2 = "-1 -1991.03 nan";
			high_fogVolume3 = "-1 7945.87 7.22445e-09";

			cloudSpeed0 = "0.0000003 0.0000003";
		};

		MissionCleanup.add(Sky);
		$niceSkyNumber = 3;
	}
	else if($niceSkyNumber $= 3)
	{
		new Sky(Sky)
		{
			position = "-1216 -848 0";
			rotation = "1 0 0 0";
			scale = "1 1 1";
			cloudHeightPer[0] = "0.349971";
			cloudHeightPer[1] = "0.25";
			cloudHeightPer[2] = "0.199973";
			cloudSpeed1 = "0.0001";
			cloudSpeed2 = "0.0002";
			cloudSpeed3 = "0.0003";
			visibleDistance = "820";
			useSkyTextures = "1";
			renderBottomTexture = "0";
			SkySolidColor = "0.390000 0.390000 0.390000 0.000000";
			fogDistance = "700";
			fogColor = "0.500000 0.610000 0.600000 1.000000";
			fogVolume1 = "650 110 185";
			fogVolume2 = "0 0 0";
			fogVolume3 = "0 0 0";
			materialList = "flingsky03.dml";
			windVelocity = "1 0 0";
			windEffectPrecipitation = "0";
			fogVolumeColor1 = "128.000000 128.000000 128.000000 0.000000";
			fogVolumeColor2 = "128.000000 128.000000 128.000000 -198748244414614883000000000000000000000.000000";
			fogVolumeColor3 = "128.000000 128.000000 128.000000 -222768174765569861000000000000000000000.000000";
			high_visibleDistance = "-1";
			high_fogDistance = "-1";
			high_fogVolume1 = "-1 1.93705e+31 2.37594e-15";
			high_fogVolume2 = "-1 -16964.7 -4.91925e-08";
			high_fogVolume3 = "-1 3.35544e+07 0.000931699";

			cloudSpeed0 = "0.000000 0.000000";
		};

		MissionCleanup.add(Sky);
		$niceSkyNumber = 4;
	}
}

function fireworksSky(%sky)
{
	if($CurrentSky $= "fireworks")
	{
		schedule(1500, 0, "dtCommandsReset");
		return;
	}

	removeSky(%sky);

	new Sky(Sky)
	{
		position = "0 0 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		cloudHeightPer[0] = "0.349971";
		cloudHeightPer[1] = "0.25";
		cloudHeightPer[2] = "0.199973";
		cloudSpeed1 = "0.0001";
		cloudSpeed2 = "0.0002";
		cloudSpeed3 = "0.0003";
		visibleDistance = "560";
		useSkyTextures = "1";
		renderBottomTexture = "0";
		SkySolidColor = "0.260000 0.410000 0.440000 1.000000";
		fogDistance = "420";
		fogColor = "0.260000 0.410000 0.440000 1.000000";
		fogVolume1 = "0 0 0";
		fogVolume2 = "0 0 0";
		fogVolume3 = "0 0 0";
		materialList = "Starfallen.dml";
		windVelocity = "1 0 0";
		windEffectPrecipitation = "0";
		fogVolumeColor1 = "128.000000 128.000000 128.000000 -36610319922801672200.000000";
		fogVolumeColor2 = "128.000000 128.000000 128.000000 9500070315656657560000000.000000";
		fogVolumeColor3 = "128.000000 128.000000 128.000000 0.000000";
		high_visibleDistance = "-1";
		high_fogDistance = "-1";
		high_fogVolume1 = "-1 -2.58511e+36 2.28656e-38";
		high_fogVolume2 = "-1 -1991.03 nan";
		high_fogVolume3 = "-1 7945.87 7.22445e-09";

		cloudSpeed0 = "0.0000003 0.0000003";
	};

	MissionCleanup.add(Sky);

	schedule(1500, 0, "fireworkLoop");
}

function fireworkLoop()
{
	if($CurrentSky !$= "fireworks" || !ClientGroup.getCount())
		return;

	// find a random client.
	%client = ClientGroup.getObject(getRandom(ClientGroup.getCount() - 1));

	if(isObject(%client.player) && isObject(Game))
	{
		%distance = Sky.visibleDistance;
		if(!%distance || %distance > 250)
			%distance = 250;

		%position = %client.player.position;
		// Vary by half of the visible distance.
		%neg = getRandom(0, 1) - 1;
		%x = getWord(%position, 0) + ((%distance - getRandom(%distance / 2)) * %neg);
		%neg = getRandom(0, 1) - 1; // Randomize it again.
		%y = getWord(%position, 1) + ((%distance - getRandom(%distance / 2)) * %neg);
		%z = getWord(%position, 2) + (%distance - getRandom(%distance / 2));

		%random = getRandom(1, $fireworkDatablockCount);
		explodeFirework(%x SPC %y SPC %z, %random);
	}

	$fireworkSchedule = schedule(250 + getRandom(1500), 0, "fireworkLoop");
}

function explodeFirework(%position, %id)
{
	%emitter = new ParticleEmissionDummy()
	{
		position = %position;
		rotation = "1 0 0 0";
		scale = "1 1 1";
		datablock = "defaultEmissionDummy";
		emitter = "FireworksEmitter" @ %id;
		velocity = "1";
	};

	//echo(%emitter.position);
	serverPlay3d(dtFireworksSound, %emitter.position);
	MissionCleanup.add(%emitter);
	%emitter.schedule(1250, "delete");
}

datablock AudioProfile(dtFireworksSound)
{
   filename    = "fx/weapons/mortar_explode_UW.wav";
   description = AudioBomb3d;
   preload = true;
};

function spookySky(%sky)
{
	if($CurrentSky $= "spookySky")
	{
		schedule(1500, 0, "dtCommandsReset");
		return;
	}

	removeSky(%sky);

	new Sky(Sky) {
		position = "0 0 0";
		rotation = "1 0 0 0";
		scale = "1 1 1";
		cloudHeightPer[0] = "0.349971";
		cloudHeightPer[1] = "0.25";
		cloudHeightPer[2] = "0.199973";
		cloudSpeed1 = "0.0001";
		cloudSpeed2 = "0.0002";
		cloudSpeed3 = "0.0003";
		visibleDistance = "350";
		useSkyTextures = "1";
		renderBottomTexture = "0";
		SkySolidColor = "0.000000 0.000000 0.000000 1.000000";
		fogDistance = "150";
		fogColor = "0.000000 0.000000 0.000000 1.000000";
		fogVolume1 = "0 0 0";
		fogVolume2 = "0 0 0";
		fogVolume3 = "0 0 0";
		materialList = "nef_night1.dml";
		windVelocity = "1 0 0";
		windEffectPrecipitation = "0";
		fogVolumeColor1 = "128.000000 128.000000 128.000000 -1037713472.000000";
		fogVolumeColor2 = "128.000000 128.000000 128.000000 -1037713472.000000";
		fogVolumeColor3 = "128.000000 128.000000 128.000000 -1037713472.000000";
		high_visibleDistance = "-1";
		high_fogDistance = "-1";
		high_fogVolume1 = "-1 6.94105e-41 6.95941e-41";
		high_fogVolume2 = "-1 2.01181 6.95955e-41";
		high_fogVolume3 = "-1 6.94147e-41 6.95941e-41";

		locked = "true";
		cloudSpeed0 = "0.000000 0.000000";
	};

	MissionCleanup.add(Sky);

	schedule(1500, 0, "spookyFireworkLoop");
}

function spookyFireworkLoop()
{
	if($CurrentSky !$= "spookySky" || !ClientGroup.getCount())
		return;

	// find a random client.
	%client = ClientGroup.getObject(getRandom(ClientGroup.getCount() - 1));

	if(isObject(%client.player) && isObject(Game))
	{
		%distance = Sky.visibleDistance;
		if(!%distance || %distance > 250)
			%distance = 250;

		%position = %client.player.position;
		// Vary by half of the visible distance.
		%neg = getRandom(0, 1) - 1;
		%x = getWord(%position, 0) + ((%distance - getRandom(%distance / 2)) * %neg);
		%neg = getRandom(0, 1) - 1; // Randomize it again.
		%y = getWord(%position, 1) + ((%distance - getRandom(%distance / 2)) * %neg);
		%z = getWord(%position, 2) + (%distance - getRandom(%distance / 2));

		explodeFirework(%x SPC %y SPC %z, 7); //Orange Only
		explodeFirework(%x SPC %y SPC %z, 6); //Purple Only
	}

	$fireworkSchedule = schedule(250 + getRandom(1500), 0, "spookyFireworkLoop");
}

/////////////////////////////////////////////////////////////////////////////

datablock PrecipitationData(RainNoSound)
{
   type = 0;
   //soundProfile = "Universal_Rain_Light_1";
   materialList = "raindrops.dml";
   sizeX = 0.2;
   sizeY = 0.45;

   movingBoxPer = 0.35;
   divHeightVal = 1.5;
   sizeBigBox = 1;
   topBoxSpeed = 20;
   frontBoxSpeed = 30;
   topBoxDrawPer = 0.5;
   bottomDrawHeight = 40;
   skipIfPer = -0.3;
   bottomSpeedPer = 1.0;
   frontSpeedPer = 1.5;
   frontRadiusPer = 0.5;

};

$fireworkDatablockCount = 8;

// Red fireworks
datablock ParticleData(FireworksParticle1)
{
	dragCoefficient = 0;
	gravityCoefficient = 1;
	windCoefficient = 1;

	inheritedVelFactor = 0;
	constantAcceleration = 0;

	lifetimeMS = 2000;
	lifetimeVarianceMS = 200;

	textureName = "special/crescent3";

	colors[0] = "1 0.2 0.2 1";
	colors[1] = "1 0.2 0.2 1";
	colors[2] = "1 0.2 0.2 0.75";

	sizes[0]      = 20.0;
	sizes[1]      = 40.0;
	sizes[2]      = 60.0;

	times[0]      = 0.0;
	times[1]      = 0.5;
	times[2]      = 1.0;
};

datablock ParticleEmitterData(FireworksEmitter1)
{
	ejectionPeriodMS = 2;
	periodVarianceMS = 0;

	ejectionVelocity = 70;
	velocityVariance = 20;

	ejectionOffset = 0;

	thetaMin = 0;
	thetaMax = 180;

	phiReferenceVel = 0;
	phiVariance = 360;

	overrideAdvances = false;
	orientParticles = true;

	lifeTimeMS = 250;

	particles = "FireworksParticle1";
};

// Blue fireworks
datablock ParticleData(FireworksParticle2)
{
	dragCoefficient = 0;
	gravityCoefficient = 1;
	windCoefficient = 1;

	inheritedVelFactor = 0;
	constantAcceleration = 0;

	lifetimeMS = 2000;
	lifetimeVarianceMS = 200;

	textureName = "special/crescent3";

	colors[0] = "0.2 0.2 1 1";
	colors[1] = "0.2 0.2 1 1";
	colors[2] = "0.2 0.2 1 0.75";

	sizes[0]      = 20.0;
	sizes[1]      = 40.0;
	sizes[2]      = 60.0;

	times[0]      = 0.0;
	times[1]      = 0.5;
	times[2]      = 1.0;
};

datablock ParticleEmitterData(FireworksEmitter2)
{
	ejectionPeriodMS = 2;
	periodVarianceMS = 0;

	ejectionVelocity = 70;
	velocityVariance = 20;

	ejectionOffset = 0;

	thetaMin = 0;
	thetaMax = 180;

	phiReferenceVel = 0;
	phiVariance = 360;

	overrideAdvances = false;
	orientParticles = true;

	lifeTimeMS = 250;

	particles = "FireworksParticle2";
};

// Yellow fireworks
datablock ParticleData(FireworksParticle3)
{
	dragCoefficient = 0;
	gravityCoefficient = 1;
	windCoefficient = 1;

	inheritedVelFactor = 0;
	constantAcceleration = 0;

	lifetimeMS = 2000;
	lifetimeVarianceMS = 200;

	textureName = "special/crescent3";

	colors[0] = "1 0.6 0.2 1";
	colors[1] = "1 0.6 0.2 1";
	colors[2] = "1 0.6 0.2 0.75";

	sizes[0]      = 20.0;
	sizes[1]      = 40.0;
	sizes[2]      = 60.0;

	times[0]      = 0.0;
	times[1]      = 0.5;
	times[2]      = 1.0;
};

datablock ParticleEmitterData(FireworksEmitter3)
{
	ejectionPeriodMS = 2;
	periodVarianceMS = 0;

	ejectionVelocity = 70;
	velocityVariance = 20;

	ejectionOffset = 0;

	thetaMin = 0;
	thetaMax = 180;

	phiReferenceVel = 0;
	phiVariance = 360;

	overrideAdvances = false;
	orientParticles = true;

	lifeTimeMS = 250;

	particles = "FireworksParticle3";
};

// Green fireworks
datablock ParticleData(FireworksParticle4)
{
	dragCoefficient = 0;
	gravityCoefficient = 1;
	windCoefficient = 1;

	inheritedVelFactor = 0;
	constantAcceleration = 0;

	lifetimeMS = 2000;
	lifetimeVarianceMS = 200;

	textureName = "special/crescent3";

	colors[0] = "0.2 1 0.2 1";
	colors[1] = "0.2 1 0.2 1";
	colors[2] = "0.2 1 0.2 0.75";

	sizes[0]      = 20.0;
	sizes[1]      = 40.0;
	sizes[2]      = 60.0;

	times[0]      = 0.0;
	times[1]      = 0.5;
	times[2]      = 1.0;
};

datablock ParticleEmitterData(FireworksEmitter4)
{
	ejectionPeriodMS = 2;
	periodVarianceMS = 0;

	ejectionVelocity = 70;
	velocityVariance = 20;

	ejectionOffset = 0;

	thetaMin = 0;
	thetaMax = 180;

	phiReferenceVel = 0;
	phiVariance = 360;

	overrideAdvances = false;
	orientParticles = true;

	lifeTimeMS = 250;

	particles = "FireworksParticle4";
};

// White fireworks
datablock ParticleData(FireworksParticle5)
{
	dragCoefficient = 0;
	gravityCoefficient = 1;
	windCoefficient = 1;

	inheritedVelFactor = 0;
	constantAcceleration = 0;

	lifetimeMS = 2000;
	lifetimeVarianceMS = 200;

	textureName = "special/crescent3";

	colors[0] = "0 0 0 1";
	colors[1] = "0 0 0 1";
	colors[2] = "0 0 0 0.75";

	sizes[0]      = 20.0;
	sizes[1]      = 40.0;
	sizes[2]      = 60.0;

	times[0]      = 0.0;
	times[1]      = 0.5;
	times[2]      = 1.0;
};

datablock ParticleEmitterData(FireworksEmitter5)
{
	ejectionPeriodMS = 2;
	periodVarianceMS = 0;

	ejectionVelocity = 70;
	velocityVariance = 20;

	ejectionOffset = 0;

	thetaMin = 0;
	thetaMax = 180;

	phiReferenceVel = 0;
	phiVariance = 360;

	overrideAdvances = false;
	orientParticles = true;

	lifeTimeMS = 250;

	particles = "FireworksParticle5";
};

// Purple fireworks
datablock ParticleData(FireworksParticle6)
{
	dragCoefficient = 0;
	gravityCoefficient = 1;
	windCoefficient = 1;

	inheritedVelFactor = 0;
	constantAcceleration = 0;

	lifetimeMS = 2000;
	lifetimeVarianceMS = 200;

	textureName = "special/crescent3";

	colors[0] = "1 0.2 1 1";
	colors[1] = "1 0.2 1 1";
	colors[2] = "1 0.2 1 0.75";

	sizes[0]      = 20.0;
	sizes[1]      = 40.0;
	sizes[2]      = 60.0;

	times[0]      = 0.0;
	times[1]      = 0.5;
	times[2]      = 1.0;
};

datablock ParticleEmitterData(FireworksEmitter6)
{
	ejectionPeriodMS = 2;
	periodVarianceMS = 0;

	ejectionVelocity = 70;
	velocityVariance = 20;

	ejectionOffset = 0;

	thetaMin = 0;
	thetaMax = 180;

	phiReferenceVel = 0;
	phiVariance = 360;

	overrideAdvances = false;
	orientParticles = true;

	lifeTimeMS = 250;

	particles = "FireworksParticle6";
};

// Orange fireworks
datablock ParticleData(FireworksParticle7)
{
	dragCoefficient = 0;
	gravityCoefficient = 1;
	windCoefficient = 1;

	inheritedVelFactor = 0;
	constantAcceleration = 0;

	lifetimeMS = 2000;
	lifetimeVarianceMS = 200;

	textureName = "special/crescent3";

	colors[0] = "1 0.4 0.2 1";
	colors[1] = "1 0.4 0.2 1";
	colors[2] = "1 0.4 0.2 0.75";

	sizes[0]      = 20.0;
	sizes[1]      = 40.0;
	sizes[2]      = 60.0;

	times[0]      = 0.0;
	times[1]      = 0.5;
	times[2]      = 1.0;
};

datablock ParticleEmitterData(FireworksEmitter7)
{
	ejectionPeriodMS = 2;
	periodVarianceMS = 0;

	ejectionVelocity = 70;
	velocityVariance = 20;

	ejectionOffset = 0;

	thetaMin = 0;
	thetaMax = 180;

	phiReferenceVel = 0;
	phiVariance = 360;

	overrideAdvances = false;
	orientParticles = true;

	lifeTimeMS = 250;

	particles = "FireworksParticle7";
};

// Aqua fireworks
datablock ParticleData(FireworksParticle8)
{
	dragCoefficient = 0;
	gravityCoefficient = 1;
	windCoefficient = 1;

	inheritedVelFactor = 0;
	constantAcceleration = 0;

	lifetimeMS = 2000;
	lifetimeVarianceMS = 200;

	textureName = "special/crescent3";

	colors[0] = "0.2 1 1 1";
	colors[1] = "0.2 1 1 1";
	colors[2] = "0.2 1 1 0.75";

	sizes[0]      = 20.0;
	sizes[1]      = 40.0;
	sizes[2]      = 60.0;

	times[0]      = 0.0;
	times[1]      = 0.5;
	times[2]      = 1.0;
};

datablock ParticleEmitterData(FireworksEmitter8)
{
	ejectionPeriodMS = 2;
	periodVarianceMS = 0;

	ejectionVelocity = 70;
	velocityVariance = 20;

	ejectionOffset = 0;

	thetaMin = 0;
	thetaMax = 180;

	phiReferenceVel = 0;
	phiVariance = 360;

	overrideAdvances = false;
	orientParticles = true;

	lifeTimeMS = 250;

	particles = "FireworksParticle8";
};

// Reset
function dtCommandsReset()
{
	$CurrentSky = "";
}

// Mainly so Fireworks Loop wont extend into the next map
package dtCommandsReset
{

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);

	//Reset CurrentSky
	dtCommandsReset();
}

// Mainly so client wont crash when testing with fireworks enabled
function DestroyServer()
{
	Parent::DestroyServer();

	//Reset CurrentSky
	dtCommandsReset();
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(dtCommandsReset))
    activatePackage(dtCommandsReset);
