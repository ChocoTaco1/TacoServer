// Debrief Loading Screen Script
//
// Use Debrief screen to show more information while loading.
// Modified to work with Classic
// Originally made for DarkMod
// Edited dramatically by ChocoTaco
//
// LoadScreenLines 5 and 6 are only used in Debrief mode.
// When LoadingScreenUseDebrief is off it defaults to the server's default loadscreen.
// MOTD Strings can be left " " in ServerPrefs to not use.


// PhantomPackage
// Phantom139
//
// Global Variables
// LOADSCREEN
//
// $version="V0.31";
// $dtLoadingScreen::DevTag = "RK4000"; //places the developer name on the screen
// $dtLoadingScreen::CoDevTag = "DarkTiger"; //places co-devs on the screen
// $dtLoadingScreen::ThankYous = "dtLoadingScreen forum members and active players.";

// Enable Debrief Loading Screen
// $Host::LoadingScreenUseDebrief = 0;

// Colors
// $Host::LoadScreenColor1 = "05edad"; //Light Teal
// $Host::LoadScreenColor2 = "29DEE7"; //Bright Blue Teal
// $Host::LoadScreenColor3 = "33CCCC"; //Dark Teal

// Lines
// $Host::LoadScreenLine1 = "Join Discord:";
// $Host::LoadScreenLine1_Msg = "https://discord.me/tribes2";
// $Host::LoadScreenLine2 = "Game Modes:";
// $Host::LoadScreenLine2_Msg = "LakRabbit, Capture the Flag, DeathMatch, (Light Only) Capture the Flag";	
// $Host::LoadScreenLine3 = "Required Mappacks:";
// $Host::LoadScreenLine3_Msg = "S5, S8, TWL, TWL2";
// $Host::LoadScreenLine4 = "Server Provided by:";
// $Host::LoadScreenLine4_Msg = "Ravin";	
// $Host::LoadScreenLine5 = "Server Hosted by:";
// $Host::LoadScreenLine5_Msg = "Branzone";
// $Host::LoadScreenLine6 = "Server Github:";
// $Host::LoadScreenLine6_Msg = "https://github.com/ChocoTaco1/TacoServer";

// MOTD or EVENTS Messages
// $Host::LoadScreenMOTD1 = "Blaster is here to stay!";
// $Host::LoadScreenMOTD2 = "Come play Arena on Wednesday Nights!";
// $Host::LoadScreenMOTD3 = "Lak crowd early evenings after work during the week.";
// $Host::LoadScreenMOTD4 = "Big CTF games Fridays, Saturdays, and Sundays!";

// First Screen loading time (Map Screen)
// If this is set too low the second screen wont show at all
$dtLoadingScreen::FirstScreen = 6000;
// Second Screen Delay
$dtLoadingScreen::Delay = 0;

// Include map and game rules on the debrief screen as well
// Useful if youre looking to replace both screens
$dtLoadingScreen::ShowFullScreen = 0;
// Enable/Disable Images
$dtLoadingScreen::ShowImages = 0;



// Color safetynet
// If a $Host::LoadScreenColor is "" ServerPrefs will delete and replace with serverDefaults
if( $Host::LoadScreenColor1 $= " " ) $Host::LoadScreenColor1 = "05edad";
if( $Host::LoadScreenColor2 $= " " ) $Host::LoadScreenColor2 = "29DEE7";
if( $Host::LoadScreenColor3 $= " " ) $Host::LoadScreenColor3 = "33CCCC";

// So ServerDefaults wont replace a "" value when meant to be blank
for(%x = 1; %x <= 4; %x++) 
{
	if( $Host::LoadScreenMOTD[%x] $= "")
	{
		$Host::LoadScreenMOTD[%x] = " ";
	}
}


// Keep it in a package to be neat and organized!
package LoadScreenPackage
{
	function sendLoadInfoToClient( %client ) 
	{
		//error( "** SENDING LOAD INFO TO CLIENT " @ %client @ "! **" );
		%singlePlayer = $CurrentMissionType $= "SinglePlayer";
		messageClient( %client, 'MsgLoadInfo', "", $CurrentMission, $MissionDisplayName, $MissionTypeDisplayName );

		// Send map quote:
		for ( %line = 0; %line < $LoadQuoteLineCount; %line++ )
		{
			if($LoadQuoteLine[%line] !$= "")
				messageClient( %client, 'MsgLoadQuoteLine', "", $LoadQuoteLine[%line] );
		}

		// Send map objectives:
		if ( %singlePlayer )
		{
			switch ( $pref::TrainingDifficulty )
			{
				case 2:  %diff = "Medium";
				case 3:  %diff = "Hard";
				default: %diff = "Easy";
			}
			messageClient( %client, 'MsgLoadObjectiveLine', "", "<spush><font:" @ $ShellLabelFont @ ":" @ $ShellMediumFontSize @ ">DIFFICULTY: <spop>" @ %diff );
		}

		for ( %line = 0; %line < $LoadObjLineCount; %line++ )
		{
			if ( $LoadObjLine[%line] !$= "" )
				messageClient( %client, 'MsgLoadObjectiveLine', "", $LoadObjLine[%line], !%singlePlayer );
		}

		// Send rules of engagement:
		if ( !%singlePlayer )
			messageClient( %client, 'MsgLoadRulesLine', "", "<spush><font:Univers Condensed:18>RULES OF ENGAGEMENT:<spop>", false );

		for ( %line = 0; %line < $LoadRuleLineCount; %line++ )
		{
			if ( $LoadRuleLine[%line] !$= "" )
				messageClient( %client, 'MsgLoadRulesLine', "", $LoadRuleLine[%line], !%singlePlayer );
		}

		messageClient( %client, 'MsgLoadInfoDone' );

		// ----------------------------------------------------------------------------------------------
		// z0dd - ZOD, 5/12/02. Send the mod info screen if this isn't the second showing of mission info
		if(!%second)
			schedule($dtLoadingScreen::FirstScreen, 0, "ALTsendModInfoToClient", %client);
		// ----------------------------------------------------------------------------------------------
	}
};


// Prevent package from being activated if it is already
if (!isActivePackage(LoadScreenPackage) && $Host::LoadingScreenUseDebrief)
    activatePackage(LoadScreenPackage);



// Dont even try to override sendModInfoToClient since evo has it
// Just make our own
function ALTsendModInfoToClient(%client)
{  
	// Wont allow Debrief on consecutive map loads
	if(%client.loaded)
	{
		schedule($dtLoadingScreen::FirstScreen, 0, "NORMALsendModInfoToClient", %client);
		return;
	}

	// Sound
	// As the background hum will stop on the debrief page
	// Breaks the abrupt stop
	// LoadingScreen sounds are limited to 5 secs or you'll receive an error 
	%snd = '~wgui/inventory_hum.wav';
	messageClient(%client, 'MsgLoadQuoteLine', %snd, "");

	%line1 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine1 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine1_Msg;
	%line2 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine2 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine2_Msg;
	%line3 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine3 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine3_Msg;
	%line4 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine4 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine4_Msg;
	%line5 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine5 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine5_Msg;
	%line6 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine6 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine6_Msg;	

	if($Host::TimeLimit $= "999" || $Host::TimeLimit $= "unlimited") %timeloadingvar = "Unlimited"; else %timeloadingvar = $Host::TimeLimit;
	
	if($Host::KickObserverTimeout $= 0) %obskickvar = "Off"; else %obskickvar = ($Host::KickObserverTimeout / 60)  @ " Minutes";

	%time = "<color:" @ $Host::LoadScreenColor1 @ ">Time limit: <color:" @ $Host::LoadScreenColor2 @ ">" @ %timeloadingvar;
	%max = "<color:" @ $Host::LoadScreenColor1 @ ">Max players: <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::MaxPlayers;
	%net = "<color:" @ $Host::LoadScreenColor1 @ ">Packets Rate / Size: <color:" @ $Host::LoadScreenColor2 @ ">" @ $pref::Net::PacketRateToClient @ " / " @ $pref::Net::PacketSize;
	%smurf = "<color:" @ $Host::LoadScreenColor1 @ ">Refuse smurfs: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::NoSmurfs ? "On" : "Off");
	%obskick = "<color:" @ $Host::LoadScreenColor1 @ ">Obs Kick Time: <color:" @ $Host::LoadScreenColor2 @ ">" @ %obskickvar;

	//%random = "<color:" @ $Host::LoadScreenColor1 @ ">Random teams: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($RandomTeams ? "On" : "Off");
	//%fair = 	"<color:" @ $Host::LoadScreenColor1 @ ">Fair teams: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::ClassicFairTeams ? "On" : "Off");
	//%rape = 	"<color:" @ $Host::LoadScreenColor1 @ ">No Base Rape: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::EvoNoBaseRapeEnabled ? "On" : "Off");
	//%td = 	"<color:" @ $Host::LoadScreenColor1 @ ">Team damage: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::TeamDamageOn ? "On" : "Off");
	//%crc = 	"<color:" @ $Host::LoadScreenColor1 @ ">CRC checking: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::CRCTextures ? "On" : "Off");
	//%pure = 	"<color:" @ $Host::LoadScreenColor1 @ ">Pure server: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::PureServer ? "On" : "Off");

	if($Host::NoBaseRapeEnabled)
		%rapeppl = "<color:" @ $Host::LoadScreenColor1 @ ">Min No Base Rape: <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::NoBaseRapePlayerCount;

	%turrets = "<color:" @ $Host::LoadScreenColor1 @ ">Min Turrets: <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::EnableTurretPlayerCount;

	if($Host::ClassicEvoStats && $Host::ClassicStatsType > 0)
		%stats = "<color:" @ $Host::LoadScreenColor1 @ ">Stats based on: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::ClassicStatsType == 1 ? "Kills" : "Damage");


	%currentmis = "<color:" @ $Host::LoadScreenColor1 @ ">Current mission: <color:" @ $Host::LoadScreenColor2 @ ">" @  $MissionDisplayName @ " (" @ $MissionTypeDisplayName @ ")";


	$dmlP = 0;
	
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";

	// Images
	// Desired pics much exist in the texticons folder on the client in some capacity
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	if($dtLoadingScreen::ShowImages)
	{
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		%randompics = getRandom(1,4);
		switch$(%randompics)
		{
			case 1:
				$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<Just:CENTER><bitmap:twb/twb_lakedebris_01><Just:RIGHT><bitmap:twb/twb_waterdemise_03><Just:LEFT><bitmap:twb/twb_action_05>";
			case 2:
				$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<Just:CENTER><bitmap:twb/twb_blowngen_01><Just:RIGHT><bitmap:twb/twb_action_03><Just:LEFT><bitmap:twb/twb_starwolf_shrike>";
			case 3:				
				$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<Just:CENTER><bitmap:twb/twb_TRIBES2><Just:RIGHT><bitmap:twb/twb_Harbingers><Just:LEFT><bitmap:twb/twb_action_10>";
			case 4:				
				$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<Just:CENTER><bitmap:twb/twb_inferno_02><Just:RIGHT><bitmap:twb/twb_action_04><Just:LEFT><bitmap:twb/twb_action_06>";
		}
		//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<Just:CENTER><bitmap:Cred_logo5.png><bitmap:twb/twb_action_04><bitmap:twb/twb_action_06><Just:LEFT>";
		
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
	}

	// Full screen things
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	if($dtLoadingScreen::ShowFullScreen)
	{
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<color:" @ $Host::LoadScreenColor2 @ "><lmargin:12><Font:Univers Condensed Bold:28>" @ $MissionDisplayName @ ":<font:Univers italic:16>";
		for ( %line = 0; %line < $LoadQuoteLineCount; %line++ )
		{
			if($LoadQuoteLine[%line] !$= "")
				$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<color:dcdcdc><lmargin:24>" @ StripMLControlChars($LoadQuoteLine[%line]);
		}

		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
		for ( %line = 0; %line < $LoadObjLineCount; %line++ )
		{
			if($LoadObjLine[%line] !$= "")
				$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<bitmap:bullet_2><Font:univers:18><lmargin:24><color:" @ $Host::LoadScreenColor2 @ ">" @ $LoadObjLine[%line];
		}

		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";

		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:12><Font:Univers Condensed Bold:28><color:" @ $Host::LoadScreenColor2 @ ">" @ $MissionTypeDisplayName @ ":";
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers condensed:18><color:" @ $Host::LoadScreenColor2 @ ">RULES OF ENGAGEMENT:";
		for ( %line = 0; %line < $LoadRuleLineCount; %line++ )
			$dtLoadingScreen::LoadScreenMessage[$dmlP++] =  "<bitmap:bullet_2><lmargin:24><Font:univers:18><color:" @ $Host::LoadScreenColor2 @ ">" @ StripMLControlChars($LoadRuleLine[%line]);
		$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:12><color:" @ $Host::LoadScreenColor2 @ "><Font:Univers Condensed Bold:28>Info:";
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %line1;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %line2;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %line3;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:32><Font:univers:18><color:" @ $Host::LoadScreenColor2 @ ">Please use /report or /msg, to report bugs, glitches, problems, suggestions, or just leave a message.";
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";



	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:12><Font:Univers Condensed Bold:28><color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::GameName @ ":";
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %line4;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %line5;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %line6;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %net;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %time;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %max;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %smurf;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %rapeppl;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %turrets;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %obskick;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %stats;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<lmargin:24><Font:univers:18><bitmap:bullet_2>" @ %currentmis;
	$dtLoadingScreen::LoadScreenMessage[$dmlP++] = " ";

	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = %rape;
	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = %random;
	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = %fair;
	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = %pure;
	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = %crc;

	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<Font:Arial:15>*" @ $Host::GameName;
	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<Font:Arial:15>" @ $Host::Info;

	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = "<Font:univers:18><lmargin:12><color:" @ $Host::LoadScreenColor2 @ "><Font:Univers Condensed Bold:28>Map Info:<lmargin:24><Font:univers:18>";    
	//$dtLoadingScreen::LoadScreenMessage[$dmlP++] = %currentmis;

	schedule($dtLoadingScreen::Delay, 0, "sendLoadscreen", %client);
}

// Send debrief screen
function sendLoadscreen(%client)
{
	%client.loaded = 1;
	messageClient( %client, 'MsgGameOver', "");
	messageClient( %client, 'MsgClearDebrief', "" );

	messageClient(%client, 'MsgDebriefResult', "", "<font:Sui Generis:22><Just:CENTER><color:" @ $Host::LoadScreenColor2 @ ">CLASSIC");
	messageClient(%client, 'MsgDebriefResult', "", "<font:Sui Generis:12>");
	messageClient(%client, 'MsgDebriefResult', "", "<font:verdana bold:16><color:" @ $Host::LoadScreenColor3 @ ">Version: <color:" @ $Host::LoadScreenColor2 @ ">" @ $classicVersion);
	messageClient(%client, 'MsgDebriefResult', "", "<font:verdana bold:16><color:" @ $Host::LoadScreenColor3 @ ">Developers: <color:" @ $Host::LoadScreenColor2 @ ">z0dd <color:" @ $Host::LoadScreenColor3 @ ">and <color:" @ $Host::LoadScreenColor2 @ ">ZOD");

	//%ServerMissionType = "<font:univers:21>" @ $MissionDisplayName @ "" @ "\n" @ $MissionTypeDisplayName @ "";
	//messageClient(%client, 'MsgDebriefAddLine', "", %ServerMissionType);

	//%Thanks = "\n<Font:Arial:21>Thanks: "@$dtLoadingScreen::ThankYous@" "@
	//"\n";
	//messageClient(%client, 'MsgDebriefAddLine', "", %Thanks);

	for ( %a = 1; %a <= $dmlP; %a++ )
	{
		%msgTag = $dtLoadingScreen::LoadScreenMessage[%a];
		messageClient(%client, 'MsgDebriefAddLine', "", %msgTag);
	} 

	%MOTDHeader = "<lmargin:12><Font:Univers Condensed Bold:28><color:" @ $Host::LoadScreenColor2 @ ">Events:";
	%MOTDMsg1 = "<lmargin:24><Font:univers:18><bitmap:bullet_2><color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenMOTD1;
	%MOTDMsg2 = "<lmargin:24><Font:univers:18><bitmap:bullet_2><color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenMOTD2;
	%MOTDMsg3 = "<lmargin:24><Font:univers:18><bitmap:bullet_2><color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenMOTD3;
	%MOTDMsg4 = "<lmargin:24><Font:univers:18><bitmap:bullet_2><color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenMOTD4;
	
	//MOTD Loop
	//Leave line " " in ServerPrefs to not show a line
	for(%x = 1; %x <= 4; %x++) 
	{
		if($Host::LoadScreenMOTD[%x] !$= " " && $Host::LoadScreenMOTD[%x] !$= "")
		{
			if(%x $= 1)
			{
				messageClient(%client, 'MsgDebriefAddLine', "", %MOTDHeader);
				messageClient(%client, 'MsgDebriefAddLine', "", %MOTDMsg[%x]);
				%header = 1; //No other lines without the header
			}
			else if(%header)
				messageClient(%client, 'MsgDebriefAddLine', "", %MOTDMsg[%x]);
		}
	}
	
    // Normal Screen Always in the Background
	// If client hits continue during debrief screen
	sendLoadInfoToClient(%client);
}

// Show normal second screen during following map loads
function NORMALsendModInfoToClient(%client)
{   
	%line1 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine1 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine1_Msg;
	%line2 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine2 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine2_Msg;
	%line3 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine3 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine3_Msg;
	%line4 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine4 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine4_Msg;
	%line5 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine5 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine5_Msg;
	%line6 = "<color:" @ $Host::LoadScreenColor1 @ ">" @ $Host::LoadScreenLine6 @ " <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::LoadScreenLine6_Msg;

	if($Host::TimeLimit $= "999" || $Host::TimeLimit $= "unlimited") %timeloadingvar = "Unlimited"; else %timeloadingvar = $Host::TimeLimit;

	%time = "<color:" @ $Host::LoadScreenColor1 @ ">Time limit: <color:" @ $Host::LoadScreenColor2 @ ">" @ %timeloadingvar;
	%max = "<color:" @ $Host::LoadScreenColor1 @ ">Max players: <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::MaxPlayers;
	%net = "<color:" @ $Host::LoadScreenColor1 @ ">Packets Rate / Size: <color:" @ $Host::LoadScreenColor2 @ ">" @ $pref::Net::PacketRateToClient @ " / " @ $pref::Net::PacketSize;
	%smurf = "<color:" @ $Host::LoadScreenColor1 @ ">Refuse smurfs: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::NoSmurfs ? "On" : "Off");
   
	//%random = "<color:" @ $Host::LoadScreenColor1 @ ">Random teams: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($RandomTeams ? "On" : "Off");
	//%fair = 	"<color:" @ $Host::LoadScreenColor1 @ ">Fair teams: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::ClassicFairTeams ? "On" : "Off");
	//%rape = 	"<color:" @ $Host::LoadScreenColor1 @ ">No Base Rape: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::EvoNoBaseRapeEnabled ? "On" : "Off");
	//%td = 	"<color:" @ $Host::LoadScreenColor1 @ ">Team damage: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::TeamDamageOn ? "On" : "Off");
	//%crc = 	"<color:" @ $Host::LoadScreenColor1 @ ">CRC checking: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::CRCTextures ? "On" : "Off");
	//%pure = 	"<color:" @ $Host::LoadScreenColor1 @ ">Pure server: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::PureServer ? "On" : "Off");


	if($Host::NoBaseRapeEnabled)
		%rapeppl = "<color:" @ $Host::LoadScreenColor1 @ ">Min No Base Rape: <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::NoBaseRapePlayerCount;

	%turrets = "<color:" @ $Host::LoadScreenColor1 @ ">Min Turrets: <color:" @ $Host::LoadScreenColor2 @ ">" @ $Host::EnableTurretPlayerCount;

	if($Host::ClassicEvoStats && $Host::ClassicStatsType > 0)
		%stats = "<color:" @ $Host::LoadScreenColor1 @ ">Stats based on: <color:" @ $Host::LoadScreenColor2 @ ">" @ ($Host::ClassicStatsType == 1 ? "Kills" : "Damage");

	//if($Evo::ETMMode && $ETMmode::CurrentMap <= $ETMmode::Counter)
	//{
	//	%nmis = "<color:" @ $Host::LoadScreenColor1 @ ">Next mission: <color:" @ $Host::LoadScreenColor2 @ ">" @ $ETMmode::MapDisplayName[$ETMmode::CurrentMap];
	//}
	//else
	//{
		//%nmis = "<color:" @ $Host::LoadScreenColor1 @ ">Next mission: <color:" @ $Host::LoadScreenColor2 @ ">" @ findNextCycleMission();
		//if ( $Host::ClassicRandomMissions )
		//{
		//%nmis = %nmis SPC "(Random)";
		//}
		//if($Host::EvoTourneySameMap && $Host::TournamentMode)
		//{
		//%nmis = "<color:" @ $Host::LoadScreenColor1 @ ">Next mission: <color:" @ $Host::LoadScreenColor2 @ ">" @ $CurrentMission @ " (Same)";
		//}
	//}

	%currentmis = "<color:" @ $Host::LoadScreenColor1 @ ">Current mission: <color:" @ $Host::LoadScreenColor2 @ ">" @  $MissionDisplayName @ " (" @ $MissionTypeDisplayName @ ")";

	// classic doesn't use a variable to print the version, it needs to be edited manually
	%modName = "";
	//%ModLine[0] = "<color:ffb734>Classic Developers: <color:29DEE7><a:PLAYER\tz0dd>z0dd</a> and <a:PLAYER\t-ZOD->ZOD</a>";
	%ModLine[0] = "<spush><font:sui generis:22><color:" @ $Host::LoadScreenColor2 @ "><just:center>CLASSIC<spop>";
	%ModLine[1] = "";
	%ModLine[1] = "<spush><font:verdana bold:16><color:" @ $Host::LoadScreenColor3 @ ">Version: <color:" @ $Host::LoadScreenColor2 @ ">" @ $classicVersion @ "<spop>";
	%ModLine[3] = "";
	%ModLine[4] = "<spush><font:verdana bold:16><color:" @ $Host::LoadScreenColor3 @ ">Developers: <color:" @ $Host::LoadScreenColor2 @ "><a:PLAYER\tz0dd>z0dd</a> <color:" @ $Host::LoadScreenColor3 @ ">and <color:" @ $Host::LoadScreenColor2 @ "><a:PLAYER\t-ZOD->ZOD</a><spop>";
	%ModLine[5] = "<just:left><font:univers:18>";

	%ModCnt = 6;

	%SpecialCnt = 4;
	%SpecialTextLine[0] = %line1;
	%SpecialTextLine[1] = %line2;
	%SpecialTextLine[2] = %line3;
	%SpecialTextLine[3] = %line4;

	%ServerCnt = 8;
	%ServerTextLine[0] = %time;
	%ServerTextLine[1] = %max;
	%ServerTextLine[2] = %net;
	%ServerTextLine[3] = %smurf;
	%ServerTextLine[4] = %rapeppl;
	%ServerTextLine[5] = %turrets;
	%ServerTextLine[6] = %stats;
	%ServerTextLine[7] = %currentmis;

	//%serverTextLine[2] = %td;
	//%serverTextLine[3] = %crc;
	//%ServerTextLine[4] = %pure;
	//%ServerTextLine[5] = %fair;
	//%ServerTextLine[6] = %random;
	//%ServerTextLine[7] = %rape;


	%singlePlayer = $CurrentMissionType $= "SinglePlayer";
	//messageClient(%client, 'MsgLoadInfo', "", $CurrentMission, %modName, $Host::GameName);
	messageClient(%client, 'MsgLoadInfo', "", $CurrentMission);

	// Send mod details (non bulleted list, small text):
	for(%line = 0; %line < %ModCnt; %line++)
		if(%ModLine[%line] !$= "")
			messageClient(%client, 'MsgLoadQuoteLine', "", %ModLine[%line]);

	// Send mod special settings (bulleted list, large text):
	for(%line = 0; %line < %SpecialCnt; %line++)
		if(%SpecialTextLine[%line] !$= "")
			messageClient(%client, 'MsgLoadObjectiveLine', "", %SpecialTextLine[%line], !%singlePlayer);

	// Send server info:
	//if(!%singlePlayer)
	//	messageClient(%client, 'MsgLoadRulesLine', "", "<color:29DEE7>" @ $Host::Info, false);

	for(%line = 0; %line < %ServerCnt; %line++)
	if (%ServerTextLine[%line] !$= "")
		messageClient(%client, 'MsgLoadRulesLine', "", %ServerTextLine[%line], !%singlePlayer);

	messageClient(%client, 'MsgLoadInfoDone');
}