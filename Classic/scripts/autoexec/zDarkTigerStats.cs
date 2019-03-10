// /////////////////////////////////////////////////////////////////////////////
//Score Hud Stats System, Gather data across x number of games to do math/stats 
//This also has the added benefit of restoreing scores after leaving 
//Script BY: DarkTiger
//Prerequisites - Classic 1.5.2 - Evolution Admin Mod  - (zAdvancedStatsLogless.vl2 - for mine disc support)
// Version 1.0
//CTF and LakRabbit, more game modes can be added but the script could use a refactoring pass to condense and clean it up a fair bit befor doing so
///////////////////////////////////////////////////////////////////////////////

//-----------Settings------------

$dtStats::viewSelf = 0; //Only self client can see his own stats, any stat, unless admin
//number of games to gather a running average, i would not make this too big of a number as its alot of data load/save
//If you want a larger number of games  make sure slowMode is on  
$dtStats::MaxNumOfGames = 10;
//set to 1 for the averaging to skip over zeros for example 0 0 1 2 0 4 0  it would only add 1 2 4 and divide by 3 
$dtStats::skipZeros = 1; 
$dtStats::Enable = 1; //a way to disable the stats system with out haveing to remove it
// Set to 1 for it to collect stats only on full games, the first game is ignored becuase its the game the player joined in at unless they meet the percentage requirement
//With it off it records all even after the player has left it will save
$dtStats::fullGames["CTF"] = 1;
//if they are here for 75% of the game, count it as a full game, this percentage is calc from time and score limit
$dtStats::fgPercentage["CTF"] = 25; 
//0 score based, 1 time based, 2 the closer  one to finishing the game, 3 mix avg
$dtStats::fgPercentageType["CTF"] = 2; 

$dtStats::fullGames["LAK"] = 1;
$dtStats::fgPercentage["LAK"] = 25; 
$dtStats::fgPercentageType["LAK"] = 2;   

$dtStats::returnToMenuTimer = (30*1000)*1;// 1 min after not making an action reset
//Set to 1 when your makeing changes to the menu so you can see them  update live note the refresh rate is like 2-4 secs
//just make your edit and exec("scripts/autoexec/stats.cs"); to re exec it and it should apply 
$dtStats::enableRefresh = 0; 
// This is as it sounds other wise it ill save when client leaves/ gameover
$dtStats::saveBetweenGames = 1;
// best to just leave this on in a later version its just going to be the only mode 
$dtStats::enableSlowMode = 1;// best to keep on 
$dtStats::slowLoadTime = 200;//not as big of an issue as its loads only when player joins but nessary if you want a lot of games recorded
$dtStats::slowSaveTime = 100;// 100 x 10 games  will take 1000 aka 1 sec to save * 16 players = 16 secs 

//debug
//$pref::NoClearConsole = 1;

// colors used
//00dcd4 Darker blue
//0befe7 Lighter blue
//00dc00 Green
//0099FF Blue
//FF9A00 Orange
//05edad Teal
//FF0000 Red
//dcdcdc White
//02d404 T2 Green

// kd ratio acc in acc dmg ratio dmg local maper weapon score per min  game duration   total time plaied  longest shot time used weapon?
//---------------------------------
//  Torque Markup Language - TML
//  Reference Tags
//---------------------------------

//<font:name:size>Sets the current font to the indicated name and size. Example: <font:Arial Bold:20>
//<tag:ID>Set a tag to which we can scroll a GuiScrollContentCtrl (parent control of the guiMLTextCtrl)
//<color:RRGGBBAA>Sets text color. Example: <color:dcdcdc> will display red text.
//<linkcolor:RRGGBBAA>Sets the color of a hyperlink.
//<linkcolorHL:RRGGBBAA>Sets the color of a hyperlink that is being clicked.
//<shadow:x:y>Add a shadow to the text, displaced by (x, y).
//<shadowcolor:RRGGBBAA>Sets the color of the text shadow.
//<bitmap:filePath>Displays the bitmap image of the given file. Note this is hard coded in t2 to only look in texticons in textures
//<spush>Saves the current text formatting so that temporary changes to formatting can be made. Used with <spop>.
//<spop>Restores the previously saved text formatting. Used with <spush>.
//<sbreak>Produces line breaks, similarly to <br>. However, while <br> keeps the current flow (for example, when flowing around the image), <sbreak> moves the cursor position to a new line in a more global manner (forces our text to stop flowing around the image, so text is drawn at a new line under the image).
//<just:left>Left justify the text.
//<just:right>Right justify the text.
//<just:center>Center the text.
//<a:URL>content</a>Inserts a hyperlink into the text. This can also be used to call a function class::onURL
//<lmargin:width>Sets the left margin.
//<lmargin%:width>Sets the left margin as a percentage of the full width.
//<rmargin:width>Sets the right margin.
//<clip:width>content</clip>Produces the content, but clipped to the given width.
//<div:bool>Use the profile's fillColorHL to draw a background for the text.
//<tab:##[,##[,##]]>Sets tab stops at the given locations.
//<br>Forced line break.

// Just a note on the package and the functions its moding.
// The functions with in the package are mostly just my code additions and the parent order if there are other packages shouldent really matter for this
// The true overwrites that may be of issue if others exists are RadiusExplosion and  SniperRifleImage::onFire
if(!$dtStats::Enable){return;} // abort exec
if(!isObject(statsGroup)){new SimGroup(statsGroup);}
package dtStats{
   function CTFGame::clientMissionDropReady(%game, %client){ // called when client has finished loading
      //error(" CTFGame::clientMissionDropReady package test");
      parent::clientMissionDropReady(%game, %client);
      %foundOld =0;
      if(!%client.isAIControlled() && !isObject(%client.dtStats)){
         for (%i = 0; %i < statsGroup.getCount(); %i++){ // check to see if my old data is still there
            %dtStats = statsGroup.getObject(%i);
            if(%dtStats.guid == %client.guid){
                if(game.getGamePct() < $dtStats::fgPercentage["CTF"] && $dtStats::fullGames["CTF"]){
                   %client.dtStats.dtGameCounter = 0;// reset to 0 so this game does count this game 
                }
               //error(%dtStats.guid SPC %client.guid);
               %client.dtStats = %dtStats;
               %dtStats.client = %client;
               %dtStats.guid = %client.guid;// this should be teh same  prob nto needed
               %dtStats.name = %client.nameBase;
               %dtStats.markForDelete = 0;
               %foundOld =1;
               resCTFStats(%client); // restore stats;
                messageClient(%client, 'MsgClient', "Welcome back your score has been restored.");
               break;
            }
         }
         if(!%foundOld){
            %dtStats = new scriptObject(); // object used stats storage
            statsGroup.add(%dtStats);
            %client.dtStats = %dtStats;
            %dtStats.client =%client;
            %dtStats.guid = %client.guid;
            %dtStats.name =%client.nameBase;
            %dtStats.markForDelete = 0;
            loadCTFStats(%client.dtStats);
            loadLakStats(%client.dtStats);
            %client.dtStats.dtGameCounter = 0;// mark player as just joined after the first game over  they will record stats
       
             if(Game.getGamePct() > $dtStats::fgPercentage["CTF"] && $dtStats::fullGames["CTF"]){// they will be here long enough to count as a full game 
                 %client.dtStats.dtGameCounter++;
             }
         }
      }
   }

   function LakRabbitGame::clientMissionDropReady(%game, %client){ // called when client has finished loading 
      parent::clientMissionDropReady(%game, %client);
      %foundOld =0;
      if(!%client.isAIControlled() && !isObject(%client.dtStats)){
         for (%i = 0; %i < statsGroup.getCount(); %i++){ // check to see if my old data is still there
            %dtStats = statsGroup.getObject(%i);
            if(%dtStats.guid == %client.guid){
               if(game.getGamePct() < $dtStats::fgPercentage["LAK"] && $dtStats::fullGames["LAK"]){
                   %client.dtStats.dtGameCounter = 0;// reset to 0 so it dosent count this game 
                }
               %client.dtStats = %dtStats;
               %dtStats.client = %client;
               %dtStats.guid = %client.guid;// this should be teh same  prob nto needed
               %dtStats.name = %client.nameBase;
               %dtStats.markForDelete = 0;
               %foundOld =1;
               resLakStats(%client); // restore stats;
               messageClient(%client, 'MsgClient', "Welcome back your score has been restored.");
               break;
            }
         }
         if(!%foundOld){
            %dtStats = new scriptObject(); // object used stats storage
            statsGroup.add(%dtStats);
            %client.dtStats = %dtStats;
            %dtStats.client =%client;
            %dtStats.guid = %client.guid;
            %dtStats.name =%client.nameBase;
            %dtStats.markForDelete = 0;
            %dtStats.leftPCT =%game.getGamePct();
            loadCTFStats(%client.dtStats);
            loadLakStats(%client.dtStats);
            %client.dtStats.dtGameCounter = 0;// mark player as just joined after the first game over  they will record stats
             if(Game.getGamePct() > $dtStats::fgPercentage["LAK"] && $dtStats::fullGames["LAK"]){// they will be here long enough to count as a full game 
                 %client.dtStats.dtGameCounter++;
             }
         }
      }
   }
   function CTFGame::onClientLeaveGame(%game, %client){
      //error("DefaultGame::onClientLeaveGame package test");
      if(!%client.isAiControlled()){
         %client.dtStats.markForDelete = 1;
         bakCTFStats(%client);//back up there current game in case they lost connection
         %client.dtStats.leftPCT = %game.getGamePct();
         saveCTFStats(%client.dtStats);
      }
      parent::onClientLeaveGame(%game, %client);
   }
   
   function CTFGame::timeLimitReached(%game){
      //error("CTFGame::timeLimitReached package test");
      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(!%client.isAiControlled()){
            if($dtStats::fullGames["CTF"]){
               if( %client.dtStats.dtGameCounter > 0){ //we throw out the first game as we joined it in progress
                  incCTFStats(%client); // setup for next game
               }
            }
            %client.dtStats.dtGameCounter++;
         }
      }
      parent::timeLimitReached(%game);
      
   }
   function CTFGame::scoreLimitReached(%game){
    //  error("CTFGame::scoreLimitReached package test");
      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(!%client.isAiControlled()){
            if($dtStats::fullGames["CTF"]){ // same as time limit reached
               if( %client.dtStats.dtGameCounter > 0){
                  incCTFStats(%client);
               }
            }
            %client.dtStats.dtGameCounter++; // next game should be a full game
         }
      }
      parent::scoreLimitReached(%game);
   }
   function CTFGame::gameOver( %game ){
      //error("CTF::gameOver");
      %timeNext =0;
      for (%i = 0; %i < statsGroup.getCount(); %i++){// see if we have any old clients data
         %dtStats = statsGroup.getObject(%i);
         if(%dtStats.markForDelete){ // find any that left during the match and
            if($dtStats::fullGames["CTF"]){
               if((100 - %dtStats.leftPCT) > $dtStats::fgPercentage["CTF"]){ // if they where here for most of it and left at the end save it
                  incBakCTFStats(%dtStats);// dump the backup into are stats and save
                  saveCTFStats(%dtStats);
                  %dtStats.delete();// finish it off
               }
               else{
                  saveCTFStats(%dtStats);
                  %dtStats.delete();// finish it off
               }
            }
            else{
               incBakCTFStats(%dtStats);// dump the backup into are stats and save
               saveCTFStats(%dtStats);
               %dtStats.delete();// finish it off
            }
         }
      }
      for (%z = 0; %z < ClientGroup.getCount(); %z++){
         %client = ClientGroup.getObject(%z);
         %client.viewMenu = 0; // reset hud
         %client.viewClient = 0;
         %client.viewStats = 0;
         if(!%client.isAiControlled()){
            
            if(!$dtStats::fullGames["CTF"]){ // if we dont care about full games  setup next gamea and copy over stats
               incCTFStats(%client);
            }
            if($dtStats::saveBetweenGames){// as it says
                if($dtStats::enableSlowMode){
                  %time += %timeNext; // this will chain them 
                  %timeNext = $dtStats::slowSaveTime * %client.dtStats.ctfGameCount;      
                  schedule(%time ,0,"saveCTFStats",%client.dtStats); //
                }
                else{
                   saveCTFStats(%client.dtStats);
                }
            }
             initWepStats(%client);
         }
      }
      parent::gameOver(%game);
   }
   function CTFGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      //error("CTFGame::processGameLink");
     // error("game link" SPC %arg1 SPC %arg2 SPC %arg3 SPC %arg4 SPC %arg5);
      //the default behavior when clicking on a game link is to start observing that client
      if(%arg1 $= "Stats"){
         %client.viewStats = 1;// lock out score hud from updateing untill they are done
         %client.viewMenu = %arg2;
         %client.viewClient = %arg3;
         %client.GlArg4 = %arg4;
         statsMenu(%client, %game.class);
         if(%arg2 !$= "Reset"){
            return;
         }
         else{
            messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );
            %client.viewStats = 0;
            Game.updateScoreHud(%client, 'scoreScreen');
         }
      }
      
      %targetClient = %arg1;
      if ((%client.team == 0) && isObject(%targetClient) && (%targetClient.team != 0))
      {
         %prevObsClient = %client.observeClient;
         
         // update the observer list for this client
         observerFollowUpdate( %client, %targetClient, %prevObsClient !$= "" );
         
         serverCmdObserveClient(%client, %targetClient);
         displayObserverHud(%client, %targetClient);
         
         if (%targetClient != %prevObsClient)
         {
            messageClient(%targetClient, 'Observer', '\c1%1 is now observing you.', %client.name);
            messageClient(%prevObsClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
         }
      }
   }
	function CTFGame::updateScoreHud(%game, %client, %tag){ 
	   // error("CTFGame::updateScoreHud");
	   if(%client.viewStats && $dtStats::enableRefresh){
		  //echo("view stats");
		  statsMenu(%client, %game.class);
		  return;
	   }
	   else if(%client.viewStats && !$dtStats::enableRefresh){
		  return;
	   }
	   
	   %ShowScores = ( $Host::TournamentMode || $Host::ShowIngamePlayerScores );
	   
	   if(Game.numTeams > 1)
	   {
		  // Send header:
		  messageClient(%client, 'SetScoreHudHeader', "", '<tab:15,315>\t%1<rmargin:260><just:right>%2<rmargin:560><just:left>\t%3<just:right>%4', %game.getTeamName(1), $TeamScore[1], %game.getTeamName(2), $TeamScore[2]);
		  
		  if ( !$TeamRank[1, count] )
		  {
			 $TeamRank[1, count] = 0;
		  }
		  
		  if ( !$TeamRank[2, count] )
		  {
			 $TeamRank[2, count] = 0;
		  }
		  
		  if ( $Host::EvoAveragePings )
		  {
			 for ( %count = 0; %count <= Game.numteams; %count++ )
			 {
				%Ping[%count] = 0;
				%PingSq[%count] = 0;
				%PingCount[%count] = 0;
			 }
			 
			 for ( %ClientCount = ClientGroup.getCount() -1 ; %ClientCount >= 0;
				%ClientCount-- )
			 {
				%ThisClient = ClientGroup.getObject( %ClientCount );
				%Team = %ThisClient.team;
				
				%PingVal = %ThisClient.getPing();
				
				%Ping[%Team] += %PingVal;
				%PingSq[%Team] += ( %PingVal * %PingVal );
				%PingCount[%Team] ++;
			 }
			 
			 for ( %count = 0; %count <= %game.numteams; %count++ )
			 {
				if ( %PingCount[%count] )
				{
				   %Ping[%count]   /= %PingCount[%count];
				   %PingSq[%count] /= %PingCount[%count];
				   
				   %PingSq[%count] = msqrt( %PingSq[%count] - ( %Ping[%count] * %Ping[%count] ) );
				   
				   %Ping[%count]   = mfloor( %Ping[%count] );
				   %PingSq[%count] = mfloor( %PingSq[%count] );
				   
				   %PingString[%count] = "<spush><font:Arial:14>P<font:Arial:12>ING: " @ %Ping[%count] @ " +/- " @ %PingSq[%count] @ "ms   <spop>";
				}
			 }
		  }
		  messageClient( %client, 'SetScoreHudSubheader', "",
		  '<tab:25,325>\tPLAYERS (%1)<rmargin:260><just:right>%4%3<rmargin:560><just:left>\tPLAYERS (%2)<just:right>%5%3', $TeamRank[1, count], $TeamRank[2, count], (%ShowScores?'SCORE':''),%PingString[1],%PingString[2]);
		  
		  %index = 0;
		  while(true)
		  {
			 
			 if(%index >= $TeamRank[1, count]+2 && %index >= $TeamRank[2, count]+2)
				break;
			 
			 //get the team1 client info
			 %team1Client = "";
			 %team1ClientScore = "";
			 %col1Style = "";
			 if(%index < $TeamRank[1, count])
			 {
				%team1Client = $TeamRank[1, %index];
				
				if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores && %team1Client.score >= 0)
				   %team1ClientScore = 0;
				else
				   %team1ClientScore = %team1Client.score $= "" ? 0 : %team1Client.score;
				
				%col1Style = %team1Client == %client ? "<color:dcdcdc>" : "";
				
				if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores)
				   %team1playersTotalScore = 0;
				else
				   %team1playersTotalScore += %team1Client.score;
			 }
			 else if(%index == $teamRank[1, count] && $teamRank[1, count] != 0 && %game.class $= "CTFGame")
			 {
				%team1ClientScore = "--------------";
			 }
			 else if(%index == $teamRank[1, count]+1 && $teamRank[1, count] != 0 && %game.class $= "CTFGame")
			 {
				if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores)
				   %team1ClientScore = 0;
				else
				   %team1ClientScore = %team1playersTotalScore != 0 ? %team1playersTotalScore : 0;
			 }
			 
			 //get the team2 client info
			 %team2Client = "";
			 %team2ClientScore = "";
			 %col2Style = "";
			 if(%index < $TeamRank[2, count])
			 {
				%team2Client = $TeamRank[2, %index];
				
				if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores && %team2Client.score >= 0)
				   %team2ClientScore = 0;
				else
				   %team2ClientScore = %team2Client.score $= "" ? 0 : %team2Client.score;
				
				%col2Style = %team2Client == %client ? "<color:dcdcdc>" : "";
				
				if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores)
				   %team2playersTotalScore = 0;
				else
				   %team2playersTotalScore += %team2Client.score;
			 }
			 else if(%index == $teamRank[2, count] && $teamRank[2, count] != 0 && %game.class $= "CTFGame")
			 {
				%team2ClientScore = "--------------";
			 }
			 else if(%index == $teamRank[2, count]+1 && $teamRank[2, count] != 0 && %game.class $= "CTFGame")
			 {
				if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores)
				   %team2ClientScore = 0;
				else
				   %team2ClientScore = %team2playersTotalScore != 0 ? %team2playersTotalScore : 0;
			 }
			 
			 if (!%ShowScores)
			 {
				%team1ClientScore = '';
				%team2ClientScore = '';
			 }
			 
			 if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
				if(%client.team != 0){ //if the client is not an observer, send the message
				   if(%team1Client.name !$= "" && %team2Client.name !$= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name !$= "" && %team2Client.name $= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>  %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name $= "" && %team2Client.name !$= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>%3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
				   
				}
				else{ //else for observers, create an anchor around the player name so they can be observed
				   //messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   
				   if(%team1Client.name !$= "" && %team2Client.name !$= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name !$= "" && %team2Client.name $= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name $= "" && %team2Client.name !$= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
				   
				}
			 }
			 else{
				if(%client.team != 0){
				   if(%team1Client.name $= %client.name && %team2Client.name !$= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name !$=""  && %team2Client.name $= %client.name)
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name $= %client.name && %team2Client.name $= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name $= "" && %team2Client.name $= %client.name)
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>%3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
				   
				}
				else{ //else for observers, create an anchor around the player name so they can be observed
				   //messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   if(%team1Client.name $= %client.name && %team2Client.name !$= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name !$= "" && %team2Client.name $= %client.name)
					  mssageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name $= %client.name && %team2Client.name $= "")
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else if(%team1Client.name $= "" && %team2Client.name $= %client.name)
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
				   else
					  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
				   
				}
				
			 }
			 
			 %index++;
		  }
	   }
	   else
	   {
		  //tricky stuff here...  use two columns if we have more than 15 clients...
		  %numClients = $TeamRank[0, count];
		  if(%numClients > $ScoreHudMaxVisible)
			 %numColumns = 2;
		  
		  // Clear header:
		  messageClient(%client, 'SetScoreHudHeader', "", "");
		  
		  // Send header:
		  if(%numColumns == 2)
			 messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,315>\tPLAYER<rmargin:270><just:right>%1<rmargin:570><just:left>\tPLAYER<just:right>%1', (%ShowScores?'SCORE':''));
		  else
			 messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15>\tPLAYER<rmargin:270><just:right>%1', (%ShowScores?'SCORE':''));
		  
		  %countMax = %numClients;
		  if(%countMax > ( 2 * $ScoreHudMaxVisible ))
		  {
			 if(%countMax & 1)
				%countMax++;
			 %countMax = %countMax / 2;
		  }
		  else if(%countMax > $ScoreHudMaxVisible)
			 %countMax = $ScoreHudMaxVisible;
		  
		  for(%index = 0; %index < %countMax; %index++)
		  {
			 //get the client info
			 %col1Client = $TeamRank[0, %index];
			 %col1ClientScore = %col1Client.score $= "" ? 0 : %col1Client.score;
			 %col1Style = %col1Client == %client ? "<color:dcdcdc>" : "";
			 
			 //see if we have two columns
			 if(%numColumns == 2)
			 {
				%col2Client = "";
				%col2ClientScore = "";
				%col2Style = "";
				
				//get the column 2 client info
				%col2Index = %index + %countMax;
				if(%col2Index < %numClients)
				{
				   %col2Client = $TeamRank[0, %col2Index];
				   %col2ClientScore = %col2Client.score $= "" ? 0 : %col2Client.score;
				   %col2Style = %col2Client == %client ? "<color:dcdcdc>" : "";
				}
			 }
			 
			 if ( !%ShowScores )
			 {
				%col1ClientScore = "";
				%col2ClientScore = "";
			 }
			 
			 //if the client is not an observer, send the message
			 if(%client.team != 0)
			 {
				if(%numColumns == 2)
				   messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:25,325>\t<spush>%5<clip:195>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:195>%3</clip><just:right>%4', %col1Client.name, %col1ClientScore, %col2Client.name, %col2ClientScore, %col1Style, %col2Style);
				else
				   messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:25>\t%3<clip:195>%1</clip><rmargin:260><just:right>%2', %col1Client.name, %col1ClientScore, %col1Style);
			 }
			 //else for observers, create an anchor around the player name so they can be observed
			 else
			 {
				if(%numColumns == 2)
				   messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:25,325>\t<spush>%5<clip:195><a:gamelink\t%7>%1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:195><a:gamelink\t%8>%3</a></clip><just:right>%4', %col1Client.name, %col1ClientScore, %col2Client.name, %col2ClientScore, %col1Style, %col2Style, %col1Client, %col2Client);
				else
				   messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:25>\t%3<clip:195><a:gamelink\t%4>%1</a></clip><rmargin:260><just:right>%2', %col1Client.name, %col1ClientScore, %col1Style, %col1Client);
			 }
		  }
		  
	   }
	   
	   // Tack on the list of observers:
	   %observerCount = 0;
	   for(%i = 0; %i < ClientGroup.getCount(); %i++)
	   {
		  %cl = ClientGroup.getObject(%i);
		  if(%cl.team == 0)
			 %observerCount++;
	   }
	   
	   if(%observerCount > 0)
	   {
		  messageClient(%client, 'SetLineHud', "", %tag, %index, "");
		  %index++;
		  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
		  %index++;
		  for(%i = 0; %i < ClientGroup.getCount(); %i++)
		  {
			 %cl = ClientGroup.getObject(%i);
			 //if this is an observer
			 if(%cl.team == 0)
			 {
				%obsTime = getSimTime() - %cl.observerStartTime;
				%obsTimeStr = %game.formatTime(%obsTime, false);
				if(%client.isAdmin ||%client.isSuperAdmin || !$dtStats::viewSelf){
				   messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%client);
				   
				}
				else if(%cl == %client){
				   messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%client);
				}
				else{
				   messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150>%1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr);
				}
				%index++;
			 }
		  }
	   }
	   
	   //clear the rest of Hud so we don't get old lines hanging around...
	   messageClient(%client, 'ClearHud', "", %tag, %index);
	}   
   function CTFGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      //error("CTFGame::onClientKilled");
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %damageLocation);
   }
   
   function LakRabbitGame::onClientLeaveGame(%game, %client){
     // error("DefaultGame::onClientLeaveGame package test");
      if(!%client.isAiControlled()){
         %client.dtStats.markForDelete =1;
         bakLakStats(%client);//back up there current game in case they lost connection
         saveLakStats(%client.dtStats); // save what they have done so far
         %client.dtStats.leftPCT = %game.getGamePct();
      }
      parent::onClientLeaveGame(%game, %client);
   }
   
   function LakRabbitGame::timeLimitReached(%game){
      //error("LakRabbitGame::timeLimitReached package test");
      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(!%client.isAiControlled()){
            if($dtStats::fullGames["LAK"]){
               if( %client.dtStats.dtGameCounter > 0){
                  incLakStats(%client);
               }
            }
            %client.dtStats.dtGameCounter++;// next game should be a full game
         }
      }
      parent::timeLimitReached(%game);
      
   }
   function LakRabbitGame::scoreLimitReached(%game){
      //error("LakRabbitGame::scoreLimitReached package test");
      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(!%client.isAiControlled()){
            if($dtStats::fullGames["LAK"]){
               if( %client.dtStats.dtGameCounter > 0){
                  incLakStats(%client);
               }
            }
            %client.dtStats.dtGameCounter++; // next game should be a full game
         }
      }
      parent::scoreLimitReached(%game);
   }
   function LakRabbitGame::gameOver( %game ){
     // error("LakRabbitGame::gameOver");
      %timeNext =0;
      for (%i = 0; %i < statsGroup.getCount(); %i++){// see if we have any old clients
         %dtStats = statsGroup.getObject(%i);
         if(%dtStats.markForDelete){ // find any that left during the match and
            if($dtStats::fullGames["LAK"]){
               if((100 - %dtStats.leftPCT) > $dtStats::fgPercentage["LAK"]){
                  incBakLakStats(%dtStats);// dump the backup into are stats and save
                  saveLakStats(%dtStats);
                  %dtStats.delete();// finish it off
               }
               else{
                  saveLakStats(%dtStats);
                  %dtStats.delete();// finish it off
               }
            }
            else{
               incBakLakStats(%dtStats);// dump the backup into are stats and save
               saveLakStats(%dtStats);
               %dtStats.delete();// finish it off
            }
         }
      }
      for (%z = 0; %z < ClientGroup.getCount(); %z++){
         %client = ClientGroup.getObject(%z);
         %client.viewMenu = 0; // reset hud
         %client.viewClient = 0;
         %client.viewStats = 0;
         if(!%client.isAiControlled()){
            if(!$dtStats::fullGames["LAK"]){
               incLakStats(%client);
            }
            if($dtStats::saveBetweenGames){
               if($dtStats::enableSlowMode){
                  %time += %timeNext; // this will chain them 
                      %timeNext = $dtStats::slowSaveTime * %client.dtStats.lakGameCount;
                     // error(%time SPC "time");
                  schedule(%time ,0,"saveLakStats",%client.dtStats); //
                }
                else{
                   saveLakStats(%client.dtStats);
                }
            }
             initWepStats(%client.dtStats);
         }
      }
      parent::gameOver(%game);
   }
   
   function LakRabbitGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      //error("LakGame::processGameLink");
      //echo("game link" SPC %arg1 SPC %arg2 SPC %arg3 SPC %arg4 SPC %arg5);
      //the default behavior when clicking on a game link is to start observing that client
      if(%arg1 $= "Stats"){
         %client.viewStats = 1;// lock out score hud from updateing untill they are done
         %client.viewMenu = %arg2;
         //echo(%arg3);
         %client.viewClient = getCNameToCID(%arg3);
         %client.GlArg4 = %arg4;
         statsMenu(%client, %game.class);
         if(%arg2 !$= "Reset"){
            return;
         }
         else{
            messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );
            %client.viewStats = 0;
            Game.updateScoreHud(%client, 'scoreScreen');
         }
      }
      
      %targetClient = %arg1;
      if ((%client.team == 0) && isObject(%targetClient) && (%targetClient.team != 0))
      {
         %prevObsClient = %client.observeClient;
         
         // update the observer list for this client
         observerFollowUpdate( %client, %targetClient, %prevObsClient !$= "" );
         
         serverCmdObserveClient(%client, %targetClient);
         displayObserverHud(%client, %targetClient);
         
         if (%targetClient != %prevObsClient)
         {
            messageClient(%targetClient, 'Observer', '\c1%1 is now observing you.', %client.name);
            messageClient(%prevObsClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
         }
      }
   }
	function LakRabbitGame::updateScoreHud(%game, %client, %tag){
	   // error("LakRabbitGame::updateScoreHud");
	   if(%client.viewStats && $dtStats::enableRefresh){
		  //echo("view stats");
		  statsMenu(%client, %game.class);
		  return;
	   }
	   else if(%client.viewStats && !$dtStats::enableRefresh){
		  return;
	   }
	   
	   //tricky stuff here...  use two columns if we have more than 15 clients...
	   %numClients = $TeamRank[0, count];
	   if ( %numClients > $ScoreHudMaxVisible )
		  %numColumns = 2;
	   
	   // Clear the header:
	   messageClient( %client, 'SetScoreHudHeader', "", "" );
	   
	   // Send subheader:
	   if (%numColumns == 2)
		  messageClient(%client, 'SetScoreHudSubheader', "", '<tab:5,155,225,305,455,525>\tPLAYER\tSCORE\tTIME\tPLAYER\tSCORE\tTIME');
	   else
		  messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,235,335>\tPLAYER\tSCORE\tTIME');
	   
	   //recalc the score for whoever is holding the flag
	   if (isObject($AIRabbitFlag.carrier))
		  %game.recalcScore($AIRabbitFlag.carrier.client);
	   
	   %countMax = %numClients;
	   if ( %countMax > ( 2 * $ScoreHudMaxVisible ) )
	   {
		  if ( %countMax & 1 )
			 %countMax++;
		  %countMax = %countMax / 2;
	   }
	   else if ( %countMax > $ScoreHudMaxVisible )
		  %countMax = $ScoreHudMaxVisible;
	   
	   for (%index = 0; %index < %countMax; %index++)
	   {
		  //get the client info
		  %col1Client = $TeamRank[0, %index];
		  %col1ClientScore = %col1Client.score $= "" ? 0 : %col1Client.score;
		  %col1Style = "";
		  
		  if (isObject(%col1Client.player.holdingFlag))
		  {
			 %col1ClientTimeMS = %col1Client.flagTimeMS + getSimTime() - %col1Client.startTime;
			 %col1Style = "<color:00dc00>";
		  }
		  else
		  {
			 %col1ClientTimeMS = %col1Client.flagTimeMS;
			 if ( %col1Client == %client )
				%col1Style = "<color:dcdcdc>";
		  }
		  
		  if (%col1ClientTimeMS <= 0)
			 %col1ClientTime = "";
		  else
		  {
			 %minutes = mFloor(%col1ClientTimeMS / (60 * 1000));
			 if (%minutes <= 0)
				%minutes = "0";
			 %seconds = mFloor(%col1ClientTimeMS / 1000) % 60;
			 if (%seconds < 10)
				%seconds = "0" @ %seconds;
			 
			 %col1ClientTime = %minutes @ ":" @ %seconds;
		  }
		  
		  //see if we have two columns
		  if (%numColumns == 2)
		  {
			 %col2Client = "";
			 %col2ClientScore = "";
			 %col2ClientTime = "";
			 %col2Style = "";
			 
			 //get the column 2 client info
			 %col2Index = %index + %countMax;
			 if (%col2Index < %numClients)
			 {
				%col2Client = $TeamRank[0, %col2Index];
				%col2ClientScore = %col2Client.score $= "" ? 0 : %col2Client.score;
				
				if (isObject(%col2Client.player.holdingFlag))
				{
				   %col2ClientTimeMS = %col2Client.flagTimeMS + getSimTime() - %col2Client.startTime;
				   %col2Style = "<color:00dc00>";
				}
				else
				{
				   %col2ClientTimeMS = %col2Client.flagTimeMS;
				   if ( %col2Client == %client )
					  %col2Style = "<color:dcdcdc>";
				}
				
				if (%col2ClientTimeMS <= 0)
				   %col2ClientTime = "";
				else
				{
				   %minutes = mFloor(%col2ClientTimeMS / (60 * 1000));
				   if (%minutes <= 0)
					  %minutes = "0";
				   %seconds = mFloor(%col2ClientTimeMS / 1000) % 60;
				   if (%seconds < 10)
					  %seconds = "0" @ %seconds;
				   
				   %col2ClientTime = %minutes @ ":" @ %seconds;
				}
			 }
		  }
		  
		  //if the client is not an observer, send the message
		  if (%client.team != 0)
		  {
			 if ( %numColumns == 2 ){
				if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
				   if(%col1Client.name !$= "" && %col2Client.name !$= "")
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   else if(%col1Client.name !$= "" && %col2Client.name $= "")
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   else if(%col1Client.name $= "" && %col2Client.name !$= "")
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   else
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   
				}
				else{
				   if(%col1Client.name $= %client.name && %col2Client.name !$= "")//<a:gamelink\tStats\tView\t%4>+</a>
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)//<a:gamelink\tStats\tView\t%4>+</a>
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   else if(%col1Client.name $= %client.name && %col2Client.name $= "")
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   else
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
				   
				}
			 }
			 else{
				if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
				   if(%col1Client.name !$= "")
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style);
				   else
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style );
				}
				else{
				   if(%col1Client.name $= %client.name)
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style);
				   else
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
				   %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style );
				}
			 }
		  }
		  //else for observers, create an anchor around the player name so they can be observed
		  else
		  {
			 if ( %numColumns == 2 )
			 {
				if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
				   //this is really crappy, but I need to save 1 tag - can only pass in up to %9, %10 doesn't work...
				   if (%col2Style $= "<color:00dc00>")//<a:gamelink\tStats\tView\t%1>+</a>
				   {
					  if(%col1Client.name !$= "" && %col2Client.name !$= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name !$= "" && %col2Client.name $= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= "" && %col2Client.name !$= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  
				   }
				   else if (%col2Style $= "<color:dcdcdc>")
				   {
					  if(%col1Client.name !$= "" && %col2Client.name !$= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name !$= "" && %col2Client.name $= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= "" && %col2Client.name !$= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
				   }
				   else
				   {
					  if(%col1Client.name !$= "" && %col2Client.name !$= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name !$= "" && %col2Client.name $= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= "" && %col2Client.name !$= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
				   }
				}
				else{/////////////////////////////////////////////////////////////////////
				   if (%col2Style $= "<color:00dc00>")//<a:gamelink\tStats\tView\t%1>+</a><a:gamelink\tStats\tView\t%4>+</a>
				   {
					  if(%col1Client.name $= %client.name && %col2Client.name !$= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= %client.name && %col2Client.name $= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  
				   }
				   else if (%col2Style $= "<color:dcdcdc>")//<a:gamelink\tStats\tView\t%4>+</a>
				   {
					  if(%col1Client.name $= %client.name && %col2Client.name !$= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= %client.name && %col2Client.name $= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
				   }
				   else
				   {
					  if(%col1Client.name $= %client.name && %col2Client.name !$= "")//<a:gamelink\tStats\tView\t%4>+</a>
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)//<a:gamelink\tStats\tView\t%4>+</a>
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= %client.name && %col2Client.name $= "")
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
					  else
						 messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
					  %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
					  %col1Style, %col1Client, %col2Client );
				   }
				   
				}
			 }
			 else{
				if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
				   if(%col1Client.name !$= ""){
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
				   }
				   else{
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
				   }
				}
				else{
				   if(%col1Client.name $= %client.name){
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
				   }
				   else{
					  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
					  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
				   }
				}
			 }
		  }
	   }
	   
	   // Tack on the list of observers:
	   %observerCount = 0;
	   for (%i = 0; %i < ClientGroup.getCount(); %i++)
	   {
		  %cl = ClientGroup.getObject(%i);
		  if (%cl.team == 0)
			 %observerCount++;
	   }
	   
	   if (%observerCount > 0)
	   {
		  messageClient( %client, 'SetLineHud', "", %tag, %index, "");
		  %index++;
		  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
		  %index++;
		  for (%i = 0; %i < ClientGroup.getCount(); %i++)
		  {
			 %cl = ClientGroup.getObject(%i);
			 //if this is an observer
			 if (%cl.team == 0)
			 {
				%obsTime = getSimTime() - %cl.observerStartTime;
				%obsTimeStr = %game.formatTime(%obsTime, false);//<a:gamelink\tStats\tView\t%3>+</a>
				if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
				   messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2',
				   %cl.name, %obsTimeStr,%cl.nameBase );
				}
				else if(%client.name $= %cl.name){
				   messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2',
				   %cl.name, %obsTimeStr,%cl.nameBase );
				}
				else{
				   messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150> %1</clip><rmargin:260><just:right>%2',
				   %cl.name, %obsTimeStr,%cl.nameBase );
				}
				
				%index++;
			 }
		  }
	   }
	   
	   //clear the rest of Hud so we don't get old lines hanging around...
	   messageClient( %client, 'ClearHud', "", %tag, %index );
	}
   function LakRabbitGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      //error("CTFGame::onClientKilled");
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %damageLocation);
   }
   function RadiusExplosion(%explosionSource, %position, %radius, %damage, %impulse, %sourceObject, %damageType)
   {
      // error("RadiusExplosion");
      InitContainerRadiusSearch(%position, %radius, $TypeMasks::PlayerObjectType      |
      $TypeMasks::VehicleObjectType     |
      $TypeMasks::StaticShapeObjectType |
      $TypeMasks::TurretObjectType      |
      $TypeMasks::ItemObjectType);
      
      %numTargets = 0;
      while ((%targetObject = containerSearchNext()) != 0)
      {
         %dist = containerSearchCurrRadDamageDist();
         
         if (%dist > %radius)
            continue;
         
         // z0dd - ZOD, 5/18/03. Changed to stop Force Field console spam
         // if (%targetObject.isMounted())
         if (!(%targetObject.getType() & $TypeMasks::ForceFieldObjectType) && %targetObject.isMounted())
         {
            %mount = %targetObject.getObjectMount();
            %found = -1;
            for (%i = 0; %i < %mount.getDataBlock().numMountPoints; %i++)
            {
               if (%mount.getMountNodeObject(%i) == %targetObject)
               {
                  %found = %i;
                  break;
               }
            }
            if (%found != -1)
            {
               if (%mount.getDataBlock().isProtectedMountPoint[%found])
               {
                  continue;
               }
            }
         }
         %targets[%numTargets]     = %targetObject;
         %targetDists[%numTargets] = %dist;
         %numTargets++;
      }
      
      for (%i = 0; %i < %numTargets; %i++)
      {
         %targetObject = %targets[%i];
         %dist = %targetDists[%i];
         if(isObject(%targetObject)) // z0dd - ZOD, 5/18/03 Console spam fix.
         {
            %coverage = calcExplosionCoverage(%position, %targetObject,
            ($TypeMasks::InteriorObjectType |
            $TypeMasks::TerrainObjectType |
            $TypeMasks::ForceFieldObjectType |
            $TypeMasks::VehicleObjectType));
            if (%coverage == 0)
               continue;
            
            //if ( $splashTest )
            %amount = (1.0 - ((%dist / %radius) * 0.88)) * %coverage * %damage;
            //else
            //%amount = (1.0 - (%dist / %radius)) * %coverage * %damage;
            
            //error( "damage: " @ %amount @ " at distance: " @ %dist @ " radius: " @ %radius @ " maxDamage: " @ %damage );
            
            %data = %targetObject.getDataBlock();
            %className = %data.className;
            
            if (%impulse && %data.shouldApplyImpulse(%targetObject))
            {
               %p = %targetObject.getWorldBoxCenter();
               %momVec = VectorSub(%p, %position);
               %momVec = VectorNormalize(%momVec);
               
               //------------------------------------------------------------------------------
               // z0dd - ZOD, 7/08/02. More kick when player damages self with disc or mortar.
               // Stronger DJs and mortar jumps without impacting others (mainly HoFs)
               if(%sourceObject == %targetObject)
               {
                  if (%damageType == $DamageType::Disc)
                  {
                     %impulse = 4475;
                  }
                  else if (%damageType == $DamageType::Mortar)
                  {
                     %impulse = 5750;
                  }
               }
               //------------------------------------------------------------------------------
               
               %impulseVec = VectorScale(%momVec, %impulse * (1.0 - (%dist / %radius)));
               %doImpulse = true;
            }
            else if( %className $= FlyingVehicleData || %className $= HoverVehicleData ) // Removed WheeledVehicleData. z0dd - ZOD, 4/24/02. Do not allow impulse applied to MPB, conc MPB bug fix.
            {
               %p = %targetObject.getWorldBoxCenter();
               %momVec = VectorSub(%p, %position);
               %momVec = VectorNormalize(%momVec);
               %impulseVec = VectorScale(%momVec, %impulse * (1.0 - (%dist / %radius)));
               
               if( getWord( %momVec, 2 ) < -0.5 )
                  %momVec = "0 0 1";
               
               // Add obj's velocity into the momentum vector
               %velocity = %targetObject.getVelocity();
               //%momVec = VectorNormalize( vectorAdd( %momVec, %velocity) );
               %doImpulse = true;
            }
            else
            {
               %momVec = "0 0 1";
               %doImpulse = false;
            }
            
            if(%amount > 0){
               %data.damageObject(%targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %explosionSource.theClient, %explosionSource);
               clientIndirectDmgStats(Game.getId(),%data,%sourceObject,%targetObject, %damageType,%amount);
            }
            else if( %explosionSource.getDataBlock().getName() $= "ConcussionGrenadeThrown" && %data.getClassName() $= "PlayerData" )
            {
               %data.applyConcussion( %dist, %radius, %sourceObject, %targetObject );
               
               if(!$teamDamage && %sourceObject != %targetObject && %sourceObject.client.team == %targetObject.client.team)
               {
                  messageClient(%targetObject.client, 'msgTeamConcussionGrenade', '\c1You were hit by %1\'s concussion grenade.', getTaggedString(%sourceObject.client.name));
               }
            }
            //-------------------------------------------------------------------------------
            // z0dd - ZOD, 4/16/02. Tone done the how much bomber & HPC flip out when damaged
            if( %doImpulse )
            {
               %vehName = %targetObject.getDataBlock().getName();
               if ((%vehName $= "BomberFlyer") || (%vehName $= "HAPCFlyer"))
               {
                  %bomberimp = VectorScale(%impulseVec, 0.6);
                  %impulseVec = %bomberimp;
               }
               %targetObject.applyImpulse(%position, %impulseVec);
            }
            //if( %doImpulse )
            //   %targetObject.applyImpulse(%position, %impulseVec);
            //-------------------------------------------------------------------------------
         }
      }
   }
   function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal){
      //  error("ProjectileData::onCollision");
      parent::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal);
      clientDirectDmgStats(Game.getId(),%data,%projectile, %targetObject);
   }
   function ShapeBaseImageData::onFire(%data, %obj, %slot){
      // error("ShapeBaseImageData::onFire");
      %p = parent::onFire(%data, %obj, %slot);
      if(isObject(%p)){
         clientShotsFired(Game.getId(),%data.projectile, %p);
      }
      return %p;
   }
   function SniperRifleImage::onFire(%data,%obj,%slot){
       //error("SniperRifleImage::onFire");
      if(Game.class $= "LakRabbitGame"){
       return;  
      }
      if(!%obj.hasEnergyPack || %obj.getEnergyLevel() < %this.minEnergy) // z0dd - ZOD, 5/22/03. Check for energy too.
      {
         // siddown Junior, you can't use it
         serverPlay3D(SniperRifleDryFireSound, %obj.getTransform());
         return;
      }
      %pct = %obj.getEnergyLevel() / %obj.getDataBlock().maxEnergy;
      %p = new (%data.projectileType)() {
         dataBlock        = %data.projectile;
         initialDirection = %obj.getMuzzleVector(%slot);
         initialPosition  = %obj.getMuzzlePoint(%slot);
         sourceObject     = %obj;
         damageFactor     = %pct * %pct;
         sourceSlot       = %slot;
      };
      clientShotsFired(Game.getId(),%data.projectile, %p);
      %p.setEnergyPercentage(%pct);
      
      %obj.lastProjectile = %p;
      MissionCleanup.add(%p);
      serverPlay3D(SniperRifleFireSound, %obj.getTransform());
      
      // AI hook
      if(%obj.client)
         %obj.client.projectile = %p;
      
      %obj.setEnergyLevel(0);
      if($Host::ClassicLoadSniperChanges)
         %obj.decInventory(%data.ammo, 1);
   }
   function ShockLanceImage::onFire(%this, %obj, %slot){
     // error("ShockLanceImage::onFire");
      %p = parent::onFire(%this, %obj, %slot);
      if(isObject(%p)){
         clientShotsFired(Game.getId(),%data.projectile, %p);
      }
      return %p;
   }
   
};
if($dtStats::Enable){
   activatePackage(dtStats);
}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// all new functions below
function CTFGame::getGamePct(%game)
{
                 
        %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
             %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;   
             
   %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
   if(%scoreLimit $= "")
      %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;
      
   if($TeamScore[1] > $TeamScore[2])
      %scorePct =  ($TeamScore[1] / %scoreLimit) * 100;
   else
      %scorePct =  ($TeamScore[2] / %scoreLimit) * 100;
 
   switch$($dtStats::fgPercentageType["CTF"]){  
      case 0:
         return 100 -  %scorePct;
      case 1: 
         return 100 - %timePct;
      case 2:   
         if(%scorePct > %timePct)
           return 100 -  %scorePct;
         else
            return 100 - %timePct;
      case 3:
        %mixPct =  ((100 -  %scorePct) + (100 - %timePct)) / 2;
         return %mixPct;
      default:
        if(%scorePct > %timePct)
           return 100 -  %scorePct;
         else
            return 100 - %timePct;
   }
}

function LakRabbitGame::getGamePct(%game)
{
      %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
      %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;   
             
   %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
   if(%scoreLimit $= "")
      %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;
      
    for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(%lScore < %client.score){
            %lScore = %client.score;
         }
    }
   %scorePct =  (%lScore / %scoreLimit) * 100;
     
 
   switch$($dtStats::fgPercentageType["LAK"]){  
      case 0:
         return 100 -  %scorePct;
      case 1: 
         return 100 - %timePct;
      case 2:   
         if(%scorePct > %timePct)
           return 100 -  %scorePct;
         else
            return 100 - %timePct;
      case 3:
        %mixPct =  ((100 -  %scorePct) + (100 - %timePct)) / 2;
         return %mixPct;
      default:
        if(%scorePct > %timePct)
           return 100 -  %scorePct;
         else
            return 100 - %timePct;
   }
    
}
function getCNameToCID(%name){
   if(isObject(%name) && %name.getClassName() $= "GameConnection"){
      return %name;
   }
   else{
   for (%i = 0; %i < ClientGroup.getCount(); %i++){
      %client = ClientGroup.getObject(%i);
      if(%client.nameBase $= %name){
         return %client;
      }
   }
   }
}
function loadCTFStats(%dtStats){// called when client joins server.cs onConnect
   if($dtStats::Enable  == 0){return;}
   loadCTFTotalStats(%dtStats);
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/CTF/" @ %dtStats.guid @ "/" @ 1 @ ".cs";
   }
   else{
      return;
   }
   if(!isFile(%filename)){  initWepStats(%dtStats.client); return;}// new player
   %file = new FileObject();
   %file.OpenForRead(%filename);
   while( !%file.isEOF() ){
      %line = %file.readline();
      %line = strreplace(%line,"%t","\t");
      %var = trim(getField(%line,0));
      %val = trim(getField(%line,1));
      if(%var $= "ctfGameCount"){
          if(%val > $dtStats::MaxNumOfGames){
             %dtStats.ctfGameCount = $dtStats::MaxNumOfGames;
          }
          else{
            %dtStats.ctfGameCount = %val;
          }
      }
      else if(%var $= "ctfStatsOverWrite"){
         %dtStats.ctfStatsOverWrite = %val;
      }
      else{
            %dtStats.ctfStats[%var,1] =  %val;
      }
   }
   %file.close();
   if($dtStats::enableSlowMode){
      schedule($dtStats::slowLoadTime,0,"loadCTFSlow",%dtStats,%file,2);
   }
   else{
      if(%dtStats.ctfGameCount > 1){// load the rest
         for(%i = 2; %i<= %dtStats.ctfGameCount; %i++){
            if(%dtStats.guid !$= ""){
               %filename = "serverStats/CTF/" @ %dtStats.guid @ "/" @ %i @ ".cs";
            }
            if(isFile(%filename)){
               %file.OpenForRead(%filename);
               while( !%file.isEOF() ){
                  %line = %file.readline();
                  %line = strreplace(%line,"%t","\t");
                  %var = trim(getField(%line,0));
                  %val = trim(getField(%line,1));
                  if(%var $= "ctfGameCount" ){
                     if(%val > $dtStats::MaxNumOfGames){
                        %dtStats.ctfGameCount = $dtStats::MaxNumOfGames;
                     }
                     else{
                        %dtStats.ctfGameCount = %val;
                     }
                  }
                  else if(%var $= "ctfStatsOverWrite"){
                     %dtStats.ctfStatsOverWrite = %val;
                  }
                  else{
                     %dtStats.ctfStats[%var,%i] =  %val;
                  }
               }
            }
         }
      }
      
      %file.delete();
      initWepStats(%dtStats.client);
   }
}
function loadCTFSlow(%dtStats,%file,%i){
   if(%dtStats.ctfGameCount > 1){// load the rest
      if(%i <= %dtStats.ctfGameCount){
        // error("slow Load" SPC %i);
         if(%dtStats.guid !$= ""){
            %filename = "serverStats/CTF/" @ %dtStats.guid @ "/" @ %i @ ".cs";
         }
         if(isFile(%filename)){
            %file.OpenForRead(%filename);
            while( !%file.isEOF() ){
               %line = %file.readline();
               %line = strreplace(%line,"%t","\t");
               %var = trim(getField(%line,0));
               %val = trim(getField(%line,1));
               if(%var $= "ctfGameCount"){
                 if(%val > $dtStats::MaxNumOfGames){
                     %dtStats.ctfGameCount = $dtStats::MaxNumOfGames;
                  }
                  else{
                     %dtStats.ctfGameCount = %val;
                  }
               }
               else if(%var $= "ctfStatsOverWrite"){
                  %dtStats.ctfStatsOverWrite = %val;
               }
               else{
                  %dtStats.ctfStats[%var,%i] =  %val;
               }
            }
         }
         schedule($dtStats::slowLoadTime,0,"loadCTFSlow",%dtStats,%file,%i++);
      }
      else{
         %file.delete();
         initWepStats(%dtStats.client);
      }
   }
   
}
function loadLakStats(%dtStats){// called when client joins server.cs onConnect
   if($dtStats::Enable  == 0){return;}
   loadLakTotalStats(%dtStats);
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/Lak/" @ %dtStats.guid @ "/" @ 1 @ ".cs";
   }
   else{
      return;
   }
   if(!isFile(%filename)){  initWepStats(%dtStats.client); return;}// new player
   %file = new FileObject();
   %file.OpenForRead(%filename);
   while( !%file.isEOF() ){
      %line = %file.readline();
      %line = strreplace(%line,"%t","\t");
      %var = trim(getField(%line,0));
      %val = trim(getField(%line,1));
      if(%var $= "lakGameCount"){
         if(%val > $dtStats::MaxNumOfGames){
            %dtStats.lakGameCount = $dtStats::MaxNumOfGames;
         }
         else{
               %dtStats.lakGameCount = %val;
         }
      }
      else if(%var $= "lakStatsOverWrite"){
         %dtStats.lakStatsOverWrite = %val;
      }
      else{
         %dtStats.lakStats[%var,1] =  %val;
      }
   }
   %file.close();
   if($dtStats::enableSlowMode){
      schedule($dtStats::slowLoadTime,0,"loadLakSlow",%dtStats,%file,2);
   }
   else{
      if(%dtStats.lakGameCount > 1){// load the rest
         for(%i = 2; %i<= %dtStats.lakGameCount; %i++){
            if(%dtStats.guid !$= ""){
               %filename = "serverStats/Lak/" @ %dtStats.guid @ "/" @ %i @ ".cs";
            }
            if(isFile(%filename)){
               %file.OpenForRead(%filename);
               while( !%file.isEOF() ){
                  %line = %file.readline();
                  %line = strreplace(%line,"%t","\t");
                  %var = trim(getField(%line,0));
                  %val = trim(getField(%line,1));
                  if(%var $= "lakGameCount"){
                    if(%val > $dtStats::MaxNumOfGames){
                        %dtStats.lakGameCount = $dtStats::MaxNumOfGames;
                     }
                     else{
                        %dtStats.lakGameCount = %val;
                     }
                  }
                  else if(%var $= "lakStatsOverWrite"){
                     %dtStats.lakStatsOverWrite = %val;
                  }
                  else{
                     %dtStats.lakStats[%var,%i] =  %val;
                  }
               }
            }
         }
      }
      
      %file.delete();
      initWepStats(%dtStats.client);
   }
}
function loadLakSlow(%dtStats,%file,%i){
   if(%dtStats.lakGameCount > 1){// load the rest
      if( %i  <= %dtStats.lakGameCount){
         if(%dtStats.guid !$= ""){
            %filename = "serverStats/Lak/" @ %dtStats.guid @ "/" @ %i @ ".cs";
         }
         if(isFile(%filename)){
            %file.OpenForRead(%filename);
            while( !%file.isEOF() ){
               %line = %file.readline();
               %line = strreplace(%line,"%t","\t");
               %var = trim(getField(%line,0));
               %val = trim(getField(%line,1));
               if(%var $= "lakGameCount"){
                 if(%val > $dtStats::MaxNumOfGames){
                     %dtStats.lakGameCount = $dtStats::MaxNumOfGames;
                  }
                  else{
                     %dtStats.lakGameCount = %val;
                  }
               }
               else if(%var $= "lakStatsOverWrite"){
                  %dtStats.lakStatsOverWrite = %val;
               }
               else{
                  %dtStats.lakStats[%var,%i] =  %val;
               }
            }
         }
         schedule($dtStats::slowLoadTime,0,"loadLakSlow",%dtStats,%file,%i++);
         
      }
      else{
         %file.delete();
         initWepStats(%dtStats.client);
      }
   }
   
}
function saveCTFStats(%dtStats){ // called when client leaves server.cs onDrop
   if($dtStats::Enable  == 0){return;}
   saveCTFTotalStats(%dtStats);
    if(%dtStats.ctfStatsOverWrite $= ""){
         %dtStats.ctfStatsOverWrite = 0;
      }
   if($dtStats::enableSlowMode){
      saveCTFStatsSlow(%dtStats,1);
   }
   else{
      %file = new FileObject();
      for(%c = 1; %c <= %dtStats.ctfGameCount; %c++){
         if(%dtStats.guid !$= ""){
            %filename = "serverStats/CTF/" @ %dtStats.guid @ "/" @ %c @ ".cs";
            
            %file.OpenForWrite(%filename);
            
            %file.writeLine("ctfGameCount" @ "%t" @ %dtStats.ctfGameCount);
            %file.writeLine("ctfStatsOverWrite" @ "%t" @ %dtStats.ctfStatsOverWrite);
            %file.writeLine("timeStamp" @ "%t" @ %dtStats.ctfStats["timeStamp",%c]);
            
            %file.writeLine("kills" @ "%t" @ %dtStats.ctfStats["kills",%c]);
            %file.writeLine("deaths" @ "%t" @ %dtStats.ctfStats["deaths", %c]);
            %file.writeLine("suicides" @ "%t" @ %dtStats.ctfStats["suicides",%c]);
            %file.writeLine("teamKills" @ "%t" @ %dtStats.ctfStats["teamKills",%c]);
            %file.writeLine("flagCaps" @ "%t" @ %dtStats.ctfStats["flagCaps",%c]);
            %file.writeLine("flagGrabs" @ "%t" @ %dtStats.ctfStats["flagGrabs",%c]);
            %file.writeLine("carrierKills" @ "%t" @ %dtStats.ctfStats["carrierKills",%c]);
            %file.writeLine("flagReturns" @ "%t" @ %dtStats.ctfStats["flagReturns",%c]);
            %file.writeLine("score" @ "%t" @ %dtStats.ctfStats["score",%c]);
            %file.writeLine("scoreMidAir" @ "%t" @ %dtStats.ctfStats["scoreMidAir",%c]);
            %file.writeLine("scoreHeadshot" @ "%t" @ %dtStats.ctfStats["scoreHeadshot",%c]);
            %file.writeLine("minePlusDisc" @ "%t" @ %dtStats.ctfStats["minePlusDisc",%c]);
            
            %file.writeLine("scoreRearshot" @ "%t" @ %dtStats.ctfStats["scoreRearshot",%c]);
            %file.writeLine("escortAssists" @ "%t" @ %dtStats.ctfStats["escortAssists",%c]);
            %file.writeLine("defenseScore" @ "%t" @ %dtStats.ctfStats["defenseScore",%c]);
            %file.writeLine("offenseScore" @ "%t" @ %dtStats.ctfStats["offenseScore",%c]);
            %file.writeLine("flagDefends" @ "%t" @ %dtStats.ctfStats["flagDefends",%c]);
            
            %file.writeLine("cgKills" @ "%t" @ %dtStats.ctfStats["cgKills",%c]);
            %file.writeLine("cgDeaths" @ "%t" @ %dtStats.ctfStats["cgDeaths",%c]);
            %file.writeLine("discKills" @ "%t" @ %dtStats.ctfStats["discKills",%c]);
            %file.writeLine("discDeaths" @ "%t" @ %dtStats.ctfStats["discDeaths",%c]);
            %file.writeLine("grenadeKills" @ "%t" @ %dtStats.ctfStats["grenadeKills",%c]);
            %file.writeLine("grenadeDeaths" @ "%t" @ %dtStats.ctfStats["grenadeDeaths",%c]);
            %file.writeLine("Headshot" @ "%t" @ %dtStats.ctfStats["laserKills",%c]);
            %file.writeLine("laserDeaths" @ "%t" @ %dtStats.ctfStats["laserDeaths",%c]);
            %file.writeLine("mortarKills" @ "%t" @ %dtStats.ctfStats["mortarKills",%c]);
            %file.writeLine("mortarDeaths" @ "%t" @ %dtStats.ctfStats["mortarDeaths",%c]);
            %file.writeLine("missileKills" @ "%t" @ %dtStats.ctfStats["missileKills",%c]);
            %file.writeLine("missileDeaths" @ "%t" @ %dtStats.ctfStats["missileDeaths",%c]);
            %file.writeLine("shockLanceKills" @ "%t" @ %dtStats.ctfStats["shockLanceKills",%c]);
            %file.writeLine("shockLanceDeaths" @ "%t" @ %dtStats.ctfStats["shockLanceDeaths",%c]);
            %file.writeLine("plasmaKills" @ "%t" @ %dtStats.ctfStats["plasmaKills",%c]);
            %file.writeLine("plasmaDeaths" @ "%t" @ %dtStats.ctfStats["plasmaDeaths",%c]);
            %file.writeLine("blasterKills" @ "%t" @ %dtStats.ctfStats["blasterKills",%c]);
            %file.writeLine("blasterDeaths" @ "%t" @ %dtStats.ctfStats["blasterDeaths",%c]);
            %file.writeLine("elfKills" @ "%t" @ %dtStats.ctfStats["elfKills",%c]);
            %file.writeLine("elfDeaths" @ "%t" @ %dtStats.ctfStats["elfDeaths",%c]);
            %file.writeLine("mineKills" @ "%t" @ %dtStats.ctfStats["mineKills",%c]);
            %file.writeLine("mineDeaths" @ "%t" @ %dtStats.ctfStats["mineDeaths",%c]);
            %file.writeLine("explosionKills" @ "%t" @ %dtStats.ctfStats["explosionKills",%c]);
            %file.writeLine("explosionDeaths" @ "%t" @ %dtStats.ctfStats["explosionDeaths",%c]);
            %file.writeLine("impactKills" @ "%t" @ %dtStats.ctfStats["impactKills",%c]);
            %file.writeLine("impactDeaths" @ "%t" @ %dtStats.ctfStats["impactDeaths",%c]);
            %file.writeLine("groundKills" @ "%t" @ %dtStats.ctfStats["groundKills",%c]);
            %file.writeLine("groundDeaths" @ "%t" @ %dtStats.ctfStats["groundDeaths",%c]);
            %file.writeLine("turretKills" @ "%t" @ %dtStats.ctfStats["turretKills",%c]);
            %file.writeLine("turretDeaths" @ "%t" @ %dtStats.ctfStats["turretDeaths",%c]);
            %file.writeLine("plasmaTurretKills" @ "%t" @ %dtStats.ctfStats["plasmaTurretKills",%c]);
            %file.writeLine("plasmaTurretDeaths" @ "%t" @ %dtStats.ctfStats["plasmaTurretDeaths",%c]);
            %file.writeLine("aaTurretKills" @ "%t" @ %dtStats.ctfStats["aaTurretKills",%c]);
            %file.writeLine("aaTurretDeaths" @ "%t" @ %dtStats.ctfStats["aaTurretDeaths",%c]);
            %file.writeLine("elfTurretKills" @ "%t" @ %dtStats.ctfStats["elfTurretKills",%c]);
            %file.writeLine("elfTurretDeaths" @ "%t" @ %dtStats.ctfStats["elfTurretDeaths",%c]);
            %file.writeLine("mortarTurretKills" @ "%t" @ %dtStats.ctfStats["mortarTurretKills",%c]);
            %file.writeLine("mortarTurretDeaths" @ "%t" @ %dtStats.ctfStats["mortarTurretDeaths",%c]);
            %file.writeLine("missileTurretKills" @ "%t" @ %dtStats.ctfStats["missileTurretKills",%c]);
            %file.writeLine("missileTurretDeaths" @ "%t" @ %dtStats.ctfStats["missileTurretDeaths",%c]);
            %file.writeLine("indoorDepTurretKills" @ "%t" @ %dtStats.ctfStats["indoorDepTurretKills",%c]);
            %file.writeLine("indoorDepTurretDeaths" @ "%t" @ %dtStats.ctfStats["indoorDepTurretDeaths",%c]);
            %file.writeLine("outdoorDepTurretKills" @ "%t" @ %dtStats.ctfStats["outdoorDepTurretKills",%c]);
            %file.writeLine("outdoorDepTurretDeaths" @ "%t" @ %dtStats.ctfStats["outdoorDepTurretDeaths",%c]);
            %file.writeLine("sentryTurretKills" @ "%t" @ %dtStats.ctfStats["sentryTurretKills",%c]);
            %file.writeLine("sentryTurretDeaths" @ "%t" @ %dtStats.ctfStats["sentryTurretDeaths",%c]);
            %file.writeLine("outOfBoundKills" @ "%t" @ %dtStats.ctfStats["outOfBoundKills",%c]);
            %file.writeLine("outOfBoundDeaths" @ "%t" @ %dtStats.ctfStats["outOfBoundDeaths",%c]);
            %file.writeLine("lavaKills" @ "%t" @ %dtStats.ctfStats["lavaKills",%c]);
            %file.writeLine("lavaDeaths" @ "%t" @ %dtStats.ctfStats["lavaDeaths",%c]);
            %file.writeLine("shrikeBlasterKills" @ "%t" @ %dtStats.ctfStats["shrikeBlasterKills",%c]);
            %file.writeLine("shrikeBlasterDeaths" @ "%t" @ %dtStats.ctfStats["shrikeBlasterDeaths",%c]);
            %file.writeLine("bellyTurretKills" @ "%t" @ %dtStats.ctfStats["bellyTurretKills",%c]);
            %file.writeLine("bellyTurretDeaths" @ "%t" @ %dtStats.ctfStats["bellyTurretDeaths",%c]);
            %file.writeLine("bomberBombsKills" @ "%t" @ %dtStats.ctfStats["bomberBombsKills",%c]);
            %file.writeLine("bomberBombsDeaths" @ "%t" @ %dtStats.ctfStats["bomberBombsDeaths",%c]);
            %file.writeLine("tankChaingunKills" @ "%t" @ %dtStats.ctfStats["tankChaingunKills",%c]);
            %file.writeLine("tankChaingunDeaths" @ "%t" @ %dtStats.ctfStats["tankChaingunDeaths",%c]);
            %file.writeLine("tankMortarKills" @ "%t" @ %dtStats.ctfStats["tankMortarKills",%c]);
            %file.writeLine("tankMortarDeaths" @ "%t" @ %dtStats.ctfStats["tankMortarDeaths",%c]);
            %file.writeLine("satchelChargeKills" @ "%t" @ %dtStats.ctfStats["satchelChargeKills",%c]);
            %file.writeLine("satchelChargeDeaths" @ "%t" @ %dtStats.ctfStats["satchelChargeDeaths",%c]);
            %file.writeLine("mpbMissileKills" @ "%t" @ %dtStats.ctfStats["mpbMissileKills",%c]);
            %file.writeLine("mpbMissileDeaths" @ "%t" @ %dtStats.ctfStats["mpbMissileDeaths",%c]);
            %file.writeLine("lightningKills" @ "%t" @ %dtStats.ctfStats["lightningKills",%c]);
            %file.writeLine("lightningDeaths" @ "%t" @ %dtStats.ctfStats["lightningDeaths",%c]);
            %file.writeLine("vehicleSpawnKills" @ "%t" @ %dtStats.ctfStats["vehicleSpawnKills",%c]);
            %file.writeLine("vehicleSpawnDeaths" @ "%t" @ %dtStats.ctfStats["vehicleSpawnDeaths",%c]);
            %file.writeLine("forceFieldPowerUpKills" @ "%t" @ %dtStats.ctfStats["forceFieldPowerUpKills",%c]);
            %file.writeLine("forceFieldPowerUpDeaths" @ "%t" @ %dtStats.ctfStats["forceFieldPowerUpDeaths",%c]);
            %file.writeLine("crashKills" @ "%t" @ %dtStats.ctfStats["crashKills",%c]);
            %file.writeLine("crashDeaths" @ "%t" @ %dtStats.ctfStats["crashDeaths",%c]);
            %file.writeLine("waterKills" @ "%t" @ %dtStats.ctfStats["waterKills",%c]);
            %file.writeLine("waterDeaths" @ "%t" @ %dtStats.ctfStats["waterDeaths",%c]);
            %file.writeLine("nexusCampingKills" @ "%t" @ %dtStats.ctfStats["nexusCampingKills",%c]);
            %file.writeLine("nexusCampingDeaths" @ "%t" @ %dtStats.ctfStats["nexusCampingDeaths",%c]);
            %file.writeLine("unknownKill" @ "%t" @ %dtStats.ctfStats["unknownKill",%c]);
            %file.writeLine("unknownDeaths" @ "%t" @ %dtStats.ctfStats["unknownDeaths",%c]);
            
            %file.writeLine("cgDmg" @ "%t" @ %dtStats.ctfStats["cgDmg",%c]);
            %file.writeLine("cgDirectHits" @ "%t" @ %dtStats.ctfStats["cgDirectHits",%c]);
            %file.writeLine("cgDmgTaken" @ "%t" @ %dtStats.ctfStats["cgDmgTaken",%c]);
            %file.writeLine("discDmg" @ "%t" @ %dtStats.ctfStats["discDmg",%c]);
            %file.writeLine("discDirectHits" @ "%t" @ %dtStats.ctfStats["discDirectHits",%c]);
            %file.writeLine("discDmgTaken" @ "%t" @ %dtStats.ctfStats["discDmgTaken",%c]);
            %file.writeLine("grenadeDmg" @ "%t" @ %dtStats.ctfStats["grenadeDmg",%c]);
            %file.writeLine("grenadeDirectHits" @ "%t" @ %dtStats.ctfStats["grenadeDirectHits",%c]);
            %file.writeLine("grenadeDmgTaken" @ "%t" @ %dtStats.ctfStats["grenadeDmgTaken",%c]);
            %file.writeLine("laserDmg" @ "%t" @ %dtStats.ctfStats["laserDmg",%c]);
            %file.writeLine("laserDirectHits" @ "%t" @ %dtStats.ctfStats["laserDirectHits",%c]);
            %file.writeLine("laserDmgTaken" @ "%t" @ %dtStats.ctfStats["laserDmgTaken",%c]);
            %file.writeLine("mortarDmg" @ "%t" @ %dtStats.ctfStats["mortarDmg",%c]);
            %file.writeLine("mortarDirectHits" @ "%t" @ %dtStats.ctfStats["mortarDirectHits",%c]);
            %file.writeLine("mortarDmgTaken" @ "%t" @ %dtStats.ctfStats["mortarDmgTaken",%c]);
            %file.writeLine("missileDmg" @ "%t" @ %dtStats.ctfStats["missileDmg",%c]);
            %file.writeLine("missileDirectHits" @ "%t" @ %dtStats.ctfStats["missileDirectHits",%c]);
            %file.writeLine("missileDmgTaken" @ "%t" @ %dtStats.ctfStats["missileDmgTaken",%c]);
            %file.writeLine("shockLanceDmg" @ "%t" @ %dtStats.ctfStats["shockLanceDmg",%c]);
            %file.writeLine("shockLanceDirectHits" @ "%t" @ %dtStats.ctfStats["shockLanceDirectHits",%c]);
            %file.writeLine("shockLanceDmgTaken" @ "%t" @ %dtStats.ctfStats["shockLanceDmgTaken",%c]);
            %file.writeLine("plasmaDmg" @ "%t" @ %dtStats.ctfStats["plasmaDmg",%c]);
            %file.writeLine("plasmaDirectHits" @ "%t" @ %dtStats.ctfStats["plasmaDirectHits",%c]);
            %file.writeLine("plasmaDmgTaken" @ "%t" @ %dtStats.ctfStats["plasmaDmgTaken",%c]);
            %file.writeLine("blasterDmg" @ "%t" @ %dtStats.ctfStats["blasterDmg",%c]);
            %file.writeLine("blasterDirectHits" @ "%t" @ %dtStats.ctfStats["blasterDirectHits",%c]);
            %file.writeLine("blasterDmgTaken" @ "%t" @ %dtStats.ctfStats["blasterDmgTaken",%c]);
            %file.writeLine("elfDmg" @ "%t" @ %dtStats.ctfStats["elfDmg",%c]);
            %file.writeLine("elfDirectHits" @ "%t" @ %dtStats.ctfStats["elfDirectHits",%c]);
            %file.writeLine("elfDmgTaken" @ "%t" @ %dtStats.ctfStats["elfDmgTaken",%c]);
            %file.writeLine("unknownDmg" @ "%t" @ %dtStats.ctfStats["unknownDmg",%c]);
            %file.writeLine("unknownDirectHits" @ "%t" @ %dtStats.ctfStats["unknownDirectHits",%c]);
            %file.writeLine("unknownDmgTaken" @ "%t" @ %dtStats.ctfStats["unknownDmgTaken",%c]);
            %file.writeLine("cgInDmg" @ "%t" @ %dtStats.ctfStats["cgInDmg",%c]);
            %file.writeLine("cgIndirectHits" @ "%t" @ %dtStats.ctfStats["cgIndirectHits",%c]);
            %file.writeLine("cgInDmgTaken" @ "%t" @ %dtStats.ctfStats["cgInDmgTaken",%c]);
            %file.writeLine("discInDmg" @ "%t" @ %dtStats.ctfStats["discInDmg",%c]);
            %file.writeLine("discIndirectHits" @ "%t" @ %dtStats.ctfStats["discIndirectHits",%c]);
            %file.writeLine("discInDmgTaken" @ "%t" @ %dtStats.ctfStats["discInDmgTaken",%c]);
            %file.writeLine("grenadeInDmg" @ "%t" @ %dtStats.ctfStats["grenadeInDmg",%c]);
            %file.writeLine("grenadeIndirectHits" @ "%t" @ %dtStats.ctfStats["grenadeIndirectHits",%c]);
            %file.writeLine("grenadeInDmgTaken" @ "%t" @ %dtStats.ctfStats["grenadeInDmgTaken",%c]);
            %file.writeLine("laserInDmg" @ "%t" @ %dtStats.ctfStats["laserInDmg",%c]);
            %file.writeLine("laserIndirectHits" @ "%t" @ %dtStats.ctfStats["laserIndirectHits",%c]);
            %file.writeLine("laserInDmgTaken" @ "%t" @ %dtStats.ctfStats["laserInDmgTaken",%c]);
            %file.writeLine("mortarInDmg" @ "%t" @ %dtStats.ctfStats["mortarInDmg",%c]);
            %file.writeLine("mortarIndirectHits" @ "%t" @ %dtStats.ctfStats["mortarIndirectHits",%c]);
            %file.writeLine("mortarInDmgTaken" @ "%t" @ %dtStats.ctfStats["mortarInDmgTaken",%c]);
            %file.writeLine("missileInDmg" @ "%t" @ %dtStats.ctfStats["missileInDmg",%c]);
            %file.writeLine("missileIndirectHits" @ "%t" @ %dtStats.ctfStats["missileIndirectHits",%c]);
            %file.writeLine("missileInDmgTaken" @ "%t" @ %dtStats.ctfStats["missileInDmgTaken",%c]);
            %file.writeLine("shockLanceInDmg" @ "%t" @ %dtStats.ctfStats["shockLanceInDmg",%c]);
            %file.writeLine("shockLanceIndirectHits" @ "%t" @ %dtStats.ctfStats["shockLanceIndirectHits",%c]);
            %file.writeLine("shockLanceInDmgTaken" @ "%t" @ %dtStats.ctfStats["shockLanceInDmgTaken",%c]);
            %file.writeLine("plasmaInDmg" @ "%t" @ %dtStats.ctfStats["plasmaInDmg",%c]);
            %file.writeLine("plasmaIndirectHits" @ "%t" @ %dtStats.ctfStats["plasmaIndirectHits",%c]);
            %file.writeLine("plasmaInDmgTaken" @ "%t" @ %dtStats.ctfStats["plasmaInDmgTaken",%c]);
            %file.writeLine("blasterInDmg" @ "%t" @ %dtStats.ctfStats["blasterInDmg",%c]);
            %file.writeLine("blasterIndirectHits" @ "%t" @ %dtStats.ctfStats["blasterIndirectHits",%c]);
            %file.writeLine("blasterInDmgTaken" @ "%t" @ %dtStats.ctfStats["blasterInDmgTaken",%c]);
            %file.writeLine("elfInDmg" @ "%t" @ %dtStats.ctfStats["elfInDmg",%c]);
            %file.writeLine("elfIndirectHits" @ "%t" @ %dtStats.ctfStats["elfIndirectHits",%c]);
            %file.writeLine("elfInDmgTaken" @ "%t" @ %dtStats.ctfStats["elfInDmgTaken",%c]);
            %file.writeLine("unknownInDmg" @ "%t" @ %dtStats.ctfStats["unknownInDmg",%c]);
            %file.writeLine("unknownIndirectHits" @ "%t" @ %dtStats.ctfStats["unknownIndirectHits",%c]);
            %file.writeLine("unknownInDmgTaken" @ "%t" @ %dtStats.ctfStats["unknownInDmgTaken",%c]);
            %file.writeLine("cgShotsFired" @ "%t" @ %dtStats.ctfStats["cgShotsFired",%c]);
            %file.writeLine("discShotsFired" @ "%t" @ %dtStats.ctfStats["discShotsFired",%c]);
            %file.writeLine("grenadeShotsFired" @ "%t" @ %dtStats.ctfStats["grenadeShotsFired",%c]);
            %file.writeLine("laserShotsFired" @ "%t" @ %dtStats.ctfStats["laserShotsFired",%c]);
            %file.writeLine("mortarShotsFired" @ "%t" @ %dtStats.ctfStats["mortarShotsFired",%c]);
            %file.writeLine("missileShotsFired" @ "%t" @ %dtStats.ctfStats["missileShotsFired",%c]);
            %file.writeLine("shockLanceShotsFired" @ "%t" @ %dtStats.ctfStats["shockLanceShotsFired",%c]);
            %file.writeLine("plasmaShotsFired" @ "%t" @ %dtStats.ctfStats["plasmaShotsFired",%c]);
            %file.writeLine("blasterShotsFired" @ "%t" @ %dtStats.ctfStats["blasterShotsFired",%c]);
            %file.writeLine("elfShotsFired" @ "%t" @ %dtStats.ctfStats["elfShotsFired",%c]);
            %file.writeLine("unknownShotsFired" @ "%t" @ %dtStats.ctfStats["unknownShotsFired",%c]);
            %file.close();
         }
      }
      %file.delete();
   }
}
function saveCTFStatsSlow(%dtStats,%c){ // called when client leaves server.cs onDrop
   
   //if(!isObject(%file)){ error("no object");}
   if(%dtStats.ctfStatsOverWrite $= ""){
      %dtStats.ctfStatsOverWrite = 0;
   }
   if(%c <= %dtStats.ctfGameCount){
      //error("saveSlow" SPC %dtStats SPC %c SPC %dtStats.ctfGameCount SPC %file);
      if(%dtStats.guid !$= ""){
         %file = new FileObject();
         %filename = "serverStats/CTF/" @ %dtStats.guid @ "/" @ %c @ ".cs";
         
         %file.OpenForWrite(%filename);
         
         %file.writeLine("ctfGameCount" @ "%t" @ %dtStats.ctfGameCount);
         %file.writeLine("ctfStatsOverWrite" @ "%t" @ %dtStats.ctfStatsOverWrite);
         %file.writeLine("timeStamp" @ "%t" @ %dtStats.ctfStats["timeStamp",%c]);
         
         %file.writeLine("kills" @ "%t" @ %dtStats.ctfStats["kills",%c]);
         %file.writeLine("deaths" @ "%t" @ %dtStats.ctfStats["deaths", %c]);
         %file.writeLine("suicides" @ "%t" @ %dtStats.ctfStats["suicides",%c]);
         %file.writeLine("teamKills" @ "%t" @ %dtStats.ctfStats["teamKills",%c]);
         %file.writeLine("flagCaps" @ "%t" @ %dtStats.ctfStats["flagCaps",%c]);
         %file.writeLine("flagGrabs" @ "%t" @ %dtStats.ctfStats["flagGrabs",%c]);
         %file.writeLine("carrierKills" @ "%t" @ %dtStats.ctfStats["carrierKills",%c]);
         %file.writeLine("flagReturns" @ "%t" @ %dtStats.ctfStats["flagReturns",%c]);
         %file.writeLine("score" @ "%t" @ %dtStats.ctfStats["score",%c]);
         %file.writeLine("scoreMidAir" @ "%t" @ %dtStats.ctfStats["scoreMidAir",%c]);
         %file.writeLine("scoreHeadshot" @ "%t" @ %dtStats.ctfStats["scoreHeadshot",%c]);
         %file.writeLine("minePlusDisc" @ "%t" @ %dtStats.ctfStats["minePlusDisc",%c]);
         
         %file.writeLine("scoreRearshot" @ "%t" @ %dtStats.ctfStats["scoreRearshot",%c]);
         %file.writeLine("escortAssists" @ "%t" @ %dtStats.ctfStats["escortAssists",%c]);
         %file.writeLine("defenseScore" @ "%t" @ %dtStats.ctfStats["defenseScore",%c]);
         %file.writeLine("offenseScore" @ "%t" @ %dtStats.ctfStats["offenseScore",%c]);
         %file.writeLine("flagDefends" @ "%t" @ %dtStats.ctfStats["flagDefends",%c]);
         
         %file.writeLine("cgKills" @ "%t" @ %dtStats.ctfStats["cgKills",%c]);
         %file.writeLine("cgDeaths" @ "%t" @ %dtStats.ctfStats["cgDeaths",%c]);
         %file.writeLine("discKills" @ "%t" @ %dtStats.ctfStats["discKills",%c]);
         %file.writeLine("discDeaths" @ "%t" @ %dtStats.ctfStats["discDeaths",%c]);
         %file.writeLine("grenadeKills" @ "%t" @ %dtStats.ctfStats["grenadeKills",%c]);
         %file.writeLine("grenadeDeaths" @ "%t" @ %dtStats.ctfStats["grenadeDeaths",%c]);
         %file.writeLine("Headshot" @ "%t" @ %dtStats.ctfStats["laserKills",%c]);
         %file.writeLine("laserDeaths" @ "%t" @ %dtStats.ctfStats["laserDeaths",%c]);
         %file.writeLine("mortarKills" @ "%t" @ %dtStats.ctfStats["mortarKills",%c]);
         %file.writeLine("mortarDeaths" @ "%t" @ %dtStats.ctfStats["mortarDeaths",%c]);
         %file.writeLine("missileKills" @ "%t" @ %dtStats.ctfStats["missileKills",%c]);
         %file.writeLine("missileDeaths" @ "%t" @ %dtStats.ctfStats["missileDeaths",%c]);
         %file.writeLine("shockLanceKills" @ "%t" @ %dtStats.ctfStats["shockLanceKills",%c]);
         %file.writeLine("shockLanceDeaths" @ "%t" @ %dtStats.ctfStats["shockLanceDeaths",%c]);
         %file.writeLine("plasmaKills" @ "%t" @ %dtStats.ctfStats["plasmaKills",%c]);
         %file.writeLine("plasmaDeaths" @ "%t" @ %dtStats.ctfStats["plasmaDeaths",%c]);
         %file.writeLine("blasterKills" @ "%t" @ %dtStats.ctfStats["blasterKills",%c]);
         %file.writeLine("blasterDeaths" @ "%t" @ %dtStats.ctfStats["blasterDeaths",%c]);
         %file.writeLine("elfKills" @ "%t" @ %dtStats.ctfStats["elfKills",%c]);
         %file.writeLine("elfDeaths" @ "%t" @ %dtStats.ctfStats["elfDeaths",%c]);
         %file.writeLine("mineKills" @ "%t" @ %dtStats.ctfStats["mineKills",%c]);
         %file.writeLine("mineDeaths" @ "%t" @ %dtStats.ctfStats["mineDeaths",%c]);
         %file.writeLine("explosionKills" @ "%t" @ %dtStats.ctfStats["explosionKills",%c]);
         %file.writeLine("explosionDeaths" @ "%t" @ %dtStats.ctfStats["explosionDeaths",%c]);
         %file.writeLine("impactKills" @ "%t" @ %dtStats.ctfStats["impactKills",%c]);
         %file.writeLine("impactDeaths" @ "%t" @ %dtStats.ctfStats["impactDeaths",%c]);
         %file.writeLine("groundKills" @ "%t" @ %dtStats.ctfStats["groundKills",%c]);
         %file.writeLine("groundDeaths" @ "%t" @ %dtStats.ctfStats["groundDeaths",%c]);
         %file.writeLine("turretKills" @ "%t" @ %dtStats.ctfStats["turretKills",%c]);
         %file.writeLine("turretDeaths" @ "%t" @ %dtStats.ctfStats["turretDeaths",%c]);
         %file.writeLine("plasmaTurretKills" @ "%t" @ %dtStats.ctfStats["plasmaTurretKills",%c]);
         %file.writeLine("plasmaTurretDeaths" @ "%t" @ %dtStats.ctfStats["plasmaTurretDeaths",%c]);
         %file.writeLine("aaTurretKills" @ "%t" @ %dtStats.ctfStats["aaTurretKills",%c]);
         %file.writeLine("aaTurretDeaths" @ "%t" @ %dtStats.ctfStats["aaTurretDeaths",%c]);
         %file.writeLine("elfTurretKills" @ "%t" @ %dtStats.ctfStats["elfTurretKills",%c]);
         %file.writeLine("elfTurretDeaths" @ "%t" @ %dtStats.ctfStats["elfTurretDeaths",%c]);
         %file.writeLine("mortarTurretKills" @ "%t" @ %dtStats.ctfStats["mortarTurretKills",%c]);
         %file.writeLine("mortarTurretDeaths" @ "%t" @ %dtStats.ctfStats["mortarTurretDeaths",%c]);
         %file.writeLine("missileTurretKills" @ "%t" @ %dtStats.ctfStats["missileTurretKills",%c]);
         %file.writeLine("missileTurretDeaths" @ "%t" @ %dtStats.ctfStats["missileTurretDeaths",%c]);
         %file.writeLine("indoorDepTurretKills" @ "%t" @ %dtStats.ctfStats["indoorDepTurretKills",%c]);
         %file.writeLine("indoorDepTurretDeaths" @ "%t" @ %dtStats.ctfStats["indoorDepTurretDeaths",%c]);
         %file.writeLine("outdoorDepTurretKills" @ "%t" @ %dtStats.ctfStats["outdoorDepTurretKills",%c]);
         %file.writeLine("outdoorDepTurretDeaths" @ "%t" @ %dtStats.ctfStats["outdoorDepTurretDeaths",%c]);
         %file.writeLine("sentryTurretKills" @ "%t" @ %dtStats.ctfStats["sentryTurretKills",%c]);
         %file.writeLine("sentryTurretDeaths" @ "%t" @ %dtStats.ctfStats["sentryTurretDeaths",%c]);
         %file.writeLine("outOfBoundKills" @ "%t" @ %dtStats.ctfStats["outOfBoundKills",%c]);
         %file.writeLine("outOfBoundDeaths" @ "%t" @ %dtStats.ctfStats["outOfBoundDeaths",%c]);
         %file.writeLine("lavaKills" @ "%t" @ %dtStats.ctfStats["lavaKills",%c]);
         %file.writeLine("lavaDeaths" @ "%t" @ %dtStats.ctfStats["lavaDeaths",%c]);
         %file.writeLine("shrikeBlasterKills" @ "%t" @ %dtStats.ctfStats["shrikeBlasterKills",%c]);
         %file.writeLine("shrikeBlasterDeaths" @ "%t" @ %dtStats.ctfStats["shrikeBlasterDeaths",%c]);
         %file.writeLine("bellyTurretKills" @ "%t" @ %dtStats.ctfStats["bellyTurretKills",%c]);
         %file.writeLine("bellyTurretDeaths" @ "%t" @ %dtStats.ctfStats["bellyTurretDeaths",%c]);
         %file.writeLine("bomberBombsKills" @ "%t" @ %dtStats.ctfStats["bomberBombsKills",%c]);
         %file.writeLine("bomberBombsDeaths" @ "%t" @ %dtStats.ctfStats["bomberBombsDeaths",%c]);
         %file.writeLine("tankChaingunKills" @ "%t" @ %dtStats.ctfStats["tankChaingunKills",%c]);
         %file.writeLine("tankChaingunDeaths" @ "%t" @ %dtStats.ctfStats["tankChaingunDeaths",%c]);
         %file.writeLine("tankMortarKills" @ "%t" @ %dtStats.ctfStats["tankMortarKills",%c]);
         %file.writeLine("tankMortarDeaths" @ "%t" @ %dtStats.ctfStats["tankMortarDeaths",%c]);
         %file.writeLine("satchelChargeKills" @ "%t" @ %dtStats.ctfStats["satchelChargeKills",%c]);
         %file.writeLine("satchelChargeDeaths" @ "%t" @ %dtStats.ctfStats["satchelChargeDeaths",%c]);
         %file.writeLine("mpbMissileKills" @ "%t" @ %dtStats.ctfStats["mpbMissileKills",%c]);
         %file.writeLine("mpbMissileDeaths" @ "%t" @ %dtStats.ctfStats["mpbMissileDeaths",%c]);
         %file.writeLine("lightningKills" @ "%t" @ %dtStats.ctfStats["lightningKills",%c]);
         %file.writeLine("lightningDeaths" @ "%t" @ %dtStats.ctfStats["lightningDeaths",%c]);
         %file.writeLine("vehicleSpawnKills" @ "%t" @ %dtStats.ctfStats["vehicleSpawnKills",%c]);
         %file.writeLine("vehicleSpawnDeaths" @ "%t" @ %dtStats.ctfStats["vehicleSpawnDeaths",%c]);
         %file.writeLine("forceFieldPowerUpKills" @ "%t" @ %dtStats.ctfStats["forceFieldPowerUpKills",%c]);
         %file.writeLine("forceFieldPowerUpDeaths" @ "%t" @ %dtStats.ctfStats["forceFieldPowerUpDeaths",%c]);
         %file.writeLine("crashKills" @ "%t" @ %dtStats.ctfStats["crashKills",%c]);
         %file.writeLine("crashDeaths" @ "%t" @ %dtStats.ctfStats["crashDeaths",%c]);
         %file.writeLine("waterKills" @ "%t" @ %dtStats.ctfStats["waterKills",%c]);
         %file.writeLine("waterDeaths" @ "%t" @ %dtStats.ctfStats["waterDeaths",%c]);
         %file.writeLine("nexusCampingKills" @ "%t" @ %dtStats.ctfStats["nexusCampingKills",%c]);
         %file.writeLine("nexusCampingDeaths" @ "%t" @ %dtStats.ctfStats["nexusCampingDeaths",%c]);
         %file.writeLine("unknownKill" @ "%t" @ %dtStats.ctfStats["unknownKill",%c]);
         %file.writeLine("unknownDeaths" @ "%t" @ %dtStats.ctfStats["unknownDeaths",%c]);
         
         %file.writeLine("cgDmg" @ "%t" @ %dtStats.ctfStats["cgDmg",%c]);
         %file.writeLine("cgDirectHits" @ "%t" @ %dtStats.ctfStats["cgDirectHits",%c]);
         %file.writeLine("cgDmgTaken" @ "%t" @ %dtStats.ctfStats["cgDmgTaken",%c]);
         %file.writeLine("discDmg" @ "%t" @ %dtStats.ctfStats["discDmg",%c]);
         %file.writeLine("discDirectHits" @ "%t" @ %dtStats.ctfStats["discDirectHits",%c]);
         %file.writeLine("discDmgTaken" @ "%t" @ %dtStats.ctfStats["discDmgTaken",%c]);
         %file.writeLine("grenadeDmg" @ "%t" @ %dtStats.ctfStats["grenadeDmg",%c]);
         %file.writeLine("grenadeDirectHits" @ "%t" @ %dtStats.ctfStats["grenadeDirectHits",%c]);
         %file.writeLine("grenadeDmgTaken" @ "%t" @ %dtStats.ctfStats["grenadeDmgTaken",%c]);
         %file.writeLine("laserDmg" @ "%t" @ %dtStats.ctfStats["laserDmg",%c]);
         %file.writeLine("laserDirectHits" @ "%t" @ %dtStats.ctfStats["laserDirectHits",%c]);
         %file.writeLine("laserDmgTaken" @ "%t" @ %dtStats.ctfStats["laserDmgTaken",%c]);
         %file.writeLine("mortarDmg" @ "%t" @ %dtStats.ctfStats["mortarDmg",%c]);
         %file.writeLine("mortarDirectHits" @ "%t" @ %dtStats.ctfStats["mortarDirectHits",%c]);
         %file.writeLine("mortarDmgTaken" @ "%t" @ %dtStats.ctfStats["mortarDmgTaken",%c]);
         %file.writeLine("missileDmg" @ "%t" @ %dtStats.ctfStats["missileDmg",%c]);
         %file.writeLine("missileDirectHits" @ "%t" @ %dtStats.ctfStats["missileDirectHits",%c]);
         %file.writeLine("missileDmgTaken" @ "%t" @ %dtStats.ctfStats["missileDmgTaken",%c]);
         %file.writeLine("shockLanceDmg" @ "%t" @ %dtStats.ctfStats["shockLanceDmg",%c]);
         %file.writeLine("shockLanceDirectHits" @ "%t" @ %dtStats.ctfStats["shockLanceDirectHits",%c]);
         %file.writeLine("shockLanceDmgTaken" @ "%t" @ %dtStats.ctfStats["shockLanceDmgTaken",%c]);
         %file.writeLine("plasmaDmg" @ "%t" @ %dtStats.ctfStats["plasmaDmg",%c]);
         %file.writeLine("plasmaDirectHits" @ "%t" @ %dtStats.ctfStats["plasmaDirectHits",%c]);
         %file.writeLine("plasmaDmgTaken" @ "%t" @ %dtStats.ctfStats["plasmaDmgTaken",%c]);
         %file.writeLine("blasterDmg" @ "%t" @ %dtStats.ctfStats["blasterDmg",%c]);
         %file.writeLine("blasterDirectHits" @ "%t" @ %dtStats.ctfStats["blasterDirectHits",%c]);
         %file.writeLine("blasterDmgTaken" @ "%t" @ %dtStats.ctfStats["blasterDmgTaken",%c]);
         %file.writeLine("elfDmg" @ "%t" @ %dtStats.ctfStats["elfDmg",%c]);
         %file.writeLine("elfDirectHits" @ "%t" @ %dtStats.ctfStats["elfDirectHits",%c]);
         %file.writeLine("elfDmgTaken" @ "%t" @ %dtStats.ctfStats["elfDmgTaken",%c]);
         %file.writeLine("unknownDmg" @ "%t" @ %dtStats.ctfStats["unknownDmg",%c]);
         %file.writeLine("unknownDirectHits" @ "%t" @ %dtStats.ctfStats["unknownDirectHits",%c]);
         %file.writeLine("unknownDmgTaken" @ "%t" @ %dtStats.ctfStats["unknownDmgTaken",%c]);
         %file.writeLine("cgInDmg" @ "%t" @ %dtStats.ctfStats["cgInDmg",%c]);
         %file.writeLine("cgIndirectHits" @ "%t" @ %dtStats.ctfStats["cgIndirectHits",%c]);
         %file.writeLine("cgInDmgTaken" @ "%t" @ %dtStats.ctfStats["cgInDmgTaken",%c]);
         %file.writeLine("discInDmg" @ "%t" @ %dtStats.ctfStats["discInDmg",%c]);
         %file.writeLine("discIndirectHits" @ "%t" @ %dtStats.ctfStats["discIndirectHits",%c]);
         %file.writeLine("discInDmgTaken" @ "%t" @ %dtStats.ctfStats["discInDmgTaken",%c]);
         %file.writeLine("grenadeInDmg" @ "%t" @ %dtStats.ctfStats["grenadeInDmg",%c]);
         %file.writeLine("grenadeIndirectHits" @ "%t" @ %dtStats.ctfStats["grenadeIndirectHits",%c]);
         %file.writeLine("grenadeInDmgTaken" @ "%t" @ %dtStats.ctfStats["grenadeInDmgTaken",%c]);
         %file.writeLine("laserInDmg" @ "%t" @ %dtStats.ctfStats["laserInDmg",%c]);
         %file.writeLine("laserIndirectHits" @ "%t" @ %dtStats.ctfStats["laserIndirectHits",%c]);
         %file.writeLine("laserInDmgTaken" @ "%t" @ %dtStats.ctfStats["laserInDmgTaken",%c]);
         %file.writeLine("mortarInDmg" @ "%t" @ %dtStats.ctfStats["mortarInDmg",%c]);
         %file.writeLine("mortarIndirectHits" @ "%t" @ %dtStats.ctfStats["mortarIndirectHits",%c]);
         %file.writeLine("mortarInDmgTaken" @ "%t" @ %dtStats.ctfStats["mortarInDmgTaken",%c]);
         %file.writeLine("missileInDmg" @ "%t" @ %dtStats.ctfStats["missileInDmg",%c]);
         %file.writeLine("missileIndirectHits" @ "%t" @ %dtStats.ctfStats["missileIndirectHits",%c]);
         %file.writeLine("missileInDmgTaken" @ "%t" @ %dtStats.ctfStats["missileInDmgTaken",%c]);
         %file.writeLine("shockLanceInDmg" @ "%t" @ %dtStats.ctfStats["shockLanceInDmg",%c]);
         %file.writeLine("shockLanceIndirectHits" @ "%t" @ %dtStats.ctfStats["shockLanceIndirectHits",%c]);
         %file.writeLine("shockLanceInDmgTaken" @ "%t" @ %dtStats.ctfStats["shockLanceInDmgTaken",%c]);
         %file.writeLine("plasmaInDmg" @ "%t" @ %dtStats.ctfStats["plasmaInDmg",%c]);
         %file.writeLine("plasmaIndirectHits" @ "%t" @ %dtStats.ctfStats["plasmaIndirectHits",%c]);
         %file.writeLine("plasmaInDmgTaken" @ "%t" @ %dtStats.ctfStats["plasmaInDmgTaken",%c]);
         %file.writeLine("blasterInDmg" @ "%t" @ %dtStats.ctfStats["blasterInDmg",%c]);
         %file.writeLine("blasterIndirectHits" @ "%t" @ %dtStats.ctfStats["blasterIndirectHits",%c]);
         %file.writeLine("blasterInDmgTaken" @ "%t" @ %dtStats.ctfStats["blasterInDmgTaken",%c]);
         %file.writeLine("elfInDmg" @ "%t" @ %dtStats.ctfStats["elfInDmg",%c]);
         %file.writeLine("elfIndirectHits" @ "%t" @ %dtStats.ctfStats["elfIndirectHits",%c]);
         %file.writeLine("elfInDmgTaken" @ "%t" @ %dtStats.ctfStats["elfInDmgTaken",%c]);
         %file.writeLine("unknownInDmg" @ "%t" @ %dtStats.ctfStats["unknownInDmg",%c]);
         %file.writeLine("unknownIndirectHits" @ "%t" @ %dtStats.ctfStats["unknownIndirectHits",%c]);
         %file.writeLine("unknownInDmgTaken" @ "%t" @ %dtStats.ctfStats["unknownInDmgTaken",%c]);
         %file.writeLine("cgShotsFired" @ "%t" @ %dtStats.ctfStats["cgShotsFired",%c]);
         %file.writeLine("discShotsFired" @ "%t" @ %dtStats.ctfStats["discShotsFired",%c]);
         %file.writeLine("grenadeShotsFired" @ "%t" @ %dtStats.ctfStats["grenadeShotsFired",%c]);
         %file.writeLine("laserShotsFired" @ "%t" @ %dtStats.ctfStats["laserShotsFired",%c]);
         %file.writeLine("mortarShotsFired" @ "%t" @ %dtStats.ctfStats["mortarShotsFired",%c]);
         %file.writeLine("missileShotsFired" @ "%t" @ %dtStats.ctfStats["missileShotsFired",%c]);
         %file.writeLine("shockLanceShotsFired" @ "%t" @ %dtStats.ctfStats["shockLanceShotsFired",%c]);
         %file.writeLine("plasmaShotsFired" @ "%t" @ %dtStats.ctfStats["plasmaShotsFired",%c]);
         %file.writeLine("blasterShotsFired" @ "%t" @ %dtStats.ctfStats["blasterShotsFired",%c]);
         %file.writeLine("elfShotsFired" @ "%t" @ %dtStats.ctfStats["elfShotsFired",%c]);
         %file.writeLine("unknownShotsFired" @ "%t" @ %dtStats.ctfStats["unknownShotsFired",%c]);
         %file.close();
         %file.delete();
         schedule($dtStats::slowSaveTime,0,"saveCTFStatsSlow",%dtStats,%c++);
      }
   }
}
function saveLakStats(%dtStats){ // called when client leaves server.cs onDrop
   if($dtStats::Enable  == 0){return;}
   saveLakTotalStats(%dtStats);
   if($dtStats::enableSlowMode){
      saveLakStatsSlow(%dtStats,1);
   }
   else{
      %file = new FileObject();
      if(%dtStats.lakStatsOverWrite $= ""){
         %dtStats.lakStatsOverWrite = 0;
      }
      for(%c = 1; %c <= %dtStats.lakGameCount; %c++){
         if(%dtStats.guid !$= ""){
            %filename = "serverStats/Lak/" @ %dtStats.guid @ "/" @ %c @ ".cs";
            
            %file.OpenForWrite(%filename);
            
            %file.writeLine("lakGameCount" @ "%t" @ %dtStats.lakGameCount);
            %file.writeLine("lakStatsOverWrite" @ "%t" @ %dtStats.lakStatsOverWrite);
            %file.writeLine("timeStamp" @ "%t" @ %dtStats.lakStats["timeStamp",%c]);
            
            %file.writeLine("score" @ "%t" @ %dtStats.lakStats["score",%c]);
            %file.writeLine("kills" @ "%t" @ %dtStats.lakStats["kills",%c]);
            %file.writeLine("deaths" @ "%t" @ %dtStats.lakStats["deaths",%c]);
            %file.writeLine("suicides" @ "%t" @ %dtStats.lakStats["suicides",%c]);
            %file.writeLine("flagGrabs" @ "%t" @ %dtStats.lakStats["flagGrabs",%c]);
            %file.writeLine("flagTimeMS" @ "%t" @ %dtStats.lakStats["flagTimeMS",%c]);
            %file.writeLine("morepoints" @ "%t" @ %dtStats.lakStats["morepoints",%c]);
            %file.writeLine("mas" @ "%t" @ %dtStats.lakStats["mas",%c]);
            %file.writeLine("totalSpeed" @ "%t" @ %dtStats.lakStats["totalSpeed",%c]);
            %file.writeLine("totalDistance" @ "%t" @ %dtStats.lakStats["totalDistance",%c]);
            %file.writeLine("totalChainAccuracy" @ "%t" @ %dtStats.lakStats["totalChainAccuracy",%c]);
            %file.writeLine("totalChainHits" @ "%t" @ %dtStats.lakStats["totalChainHits",%c]);
            %file.writeLine("totalSnipeHits" @ "%t" @ %dtStats.lakStats["totalSnipeHits",%c]);
            %file.writeLine("totalSnipes" @ "%t" @ %dtStats.lakStats["totalSnipes",%c]);
            %file.writeLine("totalShockHits" @ "%t" @ %dtStats.lakStats["totalShockHits",%c]);
            %file.writeLine("totalShocks" @ "%t" @ %dtStats.lakStats["totalShocks",%c]);
            
            %file.writeLine("minePlusDisc" @ "%t" @ %dtStats.lakStats["minePlusDisc",%c]);
            
            %file.writeLine("cgKills" @ "%t" @ %dtStats.lakStats["cgKills",%c]);
            %file.writeLine("cgDeaths" @ "%t" @ %dtStats.lakStats["cgDeaths",%c]);
            %file.writeLine("discKills" @ "%t" @ %dtStats.lakStats["discKills",%c]);
            %file.writeLine("discDeaths" @ "%t" @ %dtStats.lakStats["discDeaths",%c]);
            %file.writeLine("grenadeKills" @ "%t" @ %dtStats.lakStats["grenadeKills",%c]);
            %file.writeLine("grenadeDeaths" @ "%t" @ %dtStats.lakStats["grenadeDeaths",%c]);
            %file.writeLine("Headshot" @ "%t" @ %dtStats.lakStats["laserKills",%c]);
            %file.writeLine("laserDeaths" @ "%t" @ %dtStats.lakStats["laserDeaths",%c]);
            %file.writeLine("mortarKills" @ "%t" @ %dtStats.lakStats["mortarKills",%c]);
            %file.writeLine("mortarDeaths" @ "%t" @ %dtStats.lakStats["mortarDeaths",%c]);
            %file.writeLine("missileKills" @ "%t" @ %dtStats.lakStats["missileKills",%c]);
            %file.writeLine("missileDeaths" @ "%t" @ %dtStats.lakStats["missileDeaths",%c]);
            %file.writeLine("shockLanceKills" @ "%t" @ %dtStats.lakStats["shockLanceKills",%c]);
            %file.writeLine("shockLanceDeaths" @ "%t" @ %dtStats.lakStats["shockLanceDeaths",%c]);
            %file.writeLine("plasmaKills" @ "%t" @ %dtStats.lakStats["plasmaKills",%c]);
            %file.writeLine("plasmaDeaths" @ "%t" @ %dtStats.lakStats["plasmaDeaths",%c]);
            %file.writeLine("blasterKills" @ "%t" @ %dtStats.lakStats["blasterKills",%c]);
            %file.writeLine("blasterDeaths" @ "%t" @ %dtStats.lakStats["blasterDeaths",%c]);
            %file.writeLine("elfKills" @ "%t" @ %dtStats.lakStats["elfKills",%c]);
            %file.writeLine("elfDeaths" @ "%t" @ %dtStats.lakStats["elfDeaths",%c]);
            %file.writeLine("mineKills" @ "%t" @ %dtStats.lakStats["mineKills",%c]);
            %file.writeLine("mineDeaths" @ "%t" @ %dtStats.lakStats["mineDeaths",%c]);
            %file.writeLine("explosionKills" @ "%t" @ %dtStats.lakStats["explosionKills",%c]);
            %file.writeLine("explosionDeaths" @ "%t" @ %dtStats.lakStats["explosionDeaths",%c]);
            %file.writeLine("impactKills" @ "%t" @ %dtStats.lakStats["impactKills",%c]);
            %file.writeLine("impactDeaths" @ "%t" @ %dtStats.lakStats["impactDeaths",%c]);
            %file.writeLine("groundKills" @ "%t" @ %dtStats.lakStats["groundKills",%c]);
            %file.writeLine("groundDeaths" @ "%t" @ %dtStats.lakStats["groundDeaths",%c]);
            
            %file.writeLine("outOfBoundKills" @ "%t" @ %dtStats.lakStats["outOfBoundKills",%c]);
            %file.writeLine("outOfBoundDeaths" @ "%t" @ %dtStats.lakStats["outOfBoundDeaths",%c]);
            %file.writeLine("lavaKills" @ "%t" @ %dtStats.lakStats["lavaKills",%c]);
            %file.writeLine("lavaDeaths" @ "%t" @ %dtStats.lakStats["lavaDeaths",%c]);
            
            %file.writeLine("satchelChargeKills" @ "%t" @ %dtStats.lakStats["satchelChargeKills",%c]);
            %file.writeLine("satchelChargeDeaths" @ "%t" @ %dtStats.lakStats["satchelChargeDeaths",%c]);
            
            %file.writeLine("lightningKills" @ "%t" @ %dtStats.lakStats["lightningKills",%c]);
            %file.writeLine("lightningDeaths" @ "%t" @ %dtStats.lakStats["lightningDeaths",%c]);
            
            %file.writeLine("forceFieldPowerUpKills" @ "%t" @ %dtStats.lakStats["forceFieldPowerUpKills",%c]);
            %file.writeLine("forceFieldPowerUpDeaths" @ "%t" @ %dtStats.lakStats["forceFieldPowerUpDeaths",%c]);
            
            %file.writeLine("waterKills" @ "%t" @ %dtStats.lakStats["waterKills",%c]);
            %file.writeLine("waterDeaths" @ "%t" @ %dtStats.lakStats["waterDeaths",%c]);
            %file.writeLine("nexusCampingKills" @ "%t" @ %dtStats.lakStats["nexusCampingKills",%c]);
            %file.writeLine("nexusCampingDeaths" @ "%t" @ %dtStats.lakStats["nexusCampingDeaths",%c]);
            %file.writeLine("unknownKill" @ "%t" @ %dtStats.lakStats["unknownKill",%c]);
            %file.writeLine("unknownDeaths" @ "%t" @ %dtStats.lakStats["unknownDeaths",%c]);
            
            %file.writeLine("cgDmg" @ "%t" @ %dtStats.lakStats["cgDmg",%c]);
            %file.writeLine("cgDirectHits" @ "%t" @ %dtStats.lakStats["cgDirectHits",%c]);
            %file.writeLine("cgDmgTaken" @ "%t" @ %dtStats.lakStats["cgDmgTaken",%c]);
            %file.writeLine("discDmg" @ "%t" @ %dtStats.lakStats["discDmg",%c]);
            %file.writeLine("discDirectHits" @ "%t" @ %dtStats.lakStats["discDirectHits",%c]);
            %file.writeLine("discDmgTaken" @ "%t" @ %dtStats.lakStats["discDmgTaken",%c]);
            %file.writeLine("grenadeDmg" @ "%t" @ %dtStats.lakStats["grenadeDmg",%c]);
            %file.writeLine("grenadeDirectHits" @ "%t" @ %dtStats.lakStats["grenadeDirectHits",%c]);
            %file.writeLine("grenadeDmgTaken" @ "%t" @ %dtStats.lakStats["grenadeDmgTaken",%c]);
            %file.writeLine("laserDmg" @ "%t" @ %dtStats.lakStats["laserDmg",%c]);
            %file.writeLine("laserDirectHits" @ "%t" @ %dtStats.lakStats["laserDirectHits",%c]);
            %file.writeLine("laserDmgTaken" @ "%t" @ %dtStats.lakStats["laserDmgTaken",%c]);
            %file.writeLine("mortarDmg" @ "%t" @ %dtStats.lakStats["mortarDmg",%c]);
            %file.writeLine("mortarDirectHits" @ "%t" @ %dtStats.lakStats["mortarDirectHits",%c]);
            %file.writeLine("mortarDmgTaken" @ "%t" @ %dtStats.lakStats["mortarDmgTaken",%c]);
            %file.writeLine("missileDmg" @ "%t" @ %dtStats.lakStats["missileDmg",%c]);
            %file.writeLine("missileDirectHits" @ "%t" @ %dtStats.lakStats["missileDirectHits",%c]);
            %file.writeLine("missileDmgTaken" @ "%t" @ %dtStats.lakStats["missileDmgTaken",%c]);
            %file.writeLine("shockLanceDmg" @ "%t" @ %dtStats.lakStats["shockLanceDmg",%c]);
            %file.writeLine("shockLanceDirectHits" @ "%t" @ %dtStats.lakStats["shockLanceDirectHits",%c]);
            %file.writeLine("shockLanceDmgTaken" @ "%t" @ %dtStats.lakStats["shockLanceDmgTaken",%c]);
            %file.writeLine("plasmaDmg" @ "%t" @ %dtStats.lakStats["plasmaDmg",%c]);
            %file.writeLine("plasmaDirectHits" @ "%t" @ %dtStats.lakStats["plasmaDirectHits",%c]);
            %file.writeLine("plasmaDmgTaken" @ "%t" @ %dtStats.lakStats["plasmaDmgTaken",%c]);
            %file.writeLine("blasterDmg" @ "%t" @ %dtStats.lakStats["blasterDmg",%c]);
            %file.writeLine("blasterDirectHits" @ "%t" @ %dtStats.lakStats["blasterDirectHits",%c]);
            %file.writeLine("blasterDmgTaken" @ "%t" @ %dtStats.lakStats["blasterDmgTaken",%c]);
            %file.writeLine("elfDmg" @ "%t" @ %dtStats.lakStats["elfDmg",%c]);
            %file.writeLine("elfDirectHits" @ "%t" @ %dtStats.lakStats["elfDirectHits",%c]);
            %file.writeLine("elfDmgTaken" @ "%t" @ %dtStats.lakStats["elfDmgTaken",%c]);
            %file.writeLine("unknownDmg" @ "%t" @ %dtStats.lakStats["unknownDmg",%c]);
            %file.writeLine("unknownDirectHits" @ "%t" @ %dtStats.lakStats["unknownDirectHits",%c]);
            %file.writeLine("unknownDmgTaken" @ "%t" @ %dtStats.lakStats["unknownDmgTaken",%c]);
            %file.writeLine("cgInDmg" @ "%t" @ %dtStats.lakStats["cgInDmg",%c]);
            %file.writeLine("cgIndirectHits" @ "%t" @ %dtStats.lakStats["cgIndirectHits",%c]);
            %file.writeLine("cgInDmgTaken" @ "%t" @ %dtStats.lakStats["cgInDmgTaken",%c]);
            %file.writeLine("discInDmg" @ "%t" @ %dtStats.lakStats["discInDmg",%c]);
            %file.writeLine("discIndirectHits" @ "%t" @ %dtStats.lakStats["discIndirectHits",%c]);
            %file.writeLine("discInDmgTaken" @ "%t" @ %dtStats.lakStats["discInDmgTaken",%c]);
            %file.writeLine("grenadeInDmg" @ "%t" @ %dtStats.lakStats["grenadeInDmg",%c]);
            %file.writeLine("grenadeIndirectHits" @ "%t" @ %dtStats.lakStats["grenadeIndirectHits",%c]);
            %file.writeLine("grenadeInDmgTaken" @ "%t" @ %dtStats.lakStats["grenadeInDmgTaken",%c]);
            %file.writeLine("laserInDmg" @ "%t" @ %dtStats.lakStats["laserInDmg",%c]);
            %file.writeLine("laserIndirectHits" @ "%t" @ %dtStats.lakStats["laserIndirectHits",%c]);
            %file.writeLine("laserInDmgTaken" @ "%t" @ %dtStats.lakStats["laserInDmgTaken",%c]);
            %file.writeLine("mortarInDmg" @ "%t" @ %dtStats.lakStats["mortarInDmg",%c]);
            %file.writeLine("mortarIndirectHits" @ "%t" @ %dtStats.lakStats["mortarIndirectHits",%c]);
            %file.writeLine("mortarInDmgTaken" @ "%t" @ %dtStats.lakStats["mortarInDmgTaken",%c]);
            %file.writeLine("missileInDmg" @ "%t" @ %dtStats.lakStats["missileInDmg",%c]);
            %file.writeLine("missileIndirectHits" @ "%t" @ %dtStats.lakStats["missileIndirectHits",%c]);
            %file.writeLine("missileInDmgTaken" @ "%t" @ %dtStats.lakStats["missileInDmgTaken",%c]);
            %file.writeLine("shockLanceInDmg" @ "%t" @ %dtStats.lakStats["shockLanceInDmg",%c]);
            %file.writeLine("shockLanceIndirectHits" @ "%t" @ %dtStats.lakStats["shockLanceIndirectHits",%c]);
            %file.writeLine("shockLanceInDmgTaken" @ "%t" @ %dtStats.lakStats["shockLanceInDmgTaken",%c]);
            %file.writeLine("plasmaInDmg" @ "%t" @ %dtStats.lakStats["plasmaInDmg",%c]);
            %file.writeLine("plasmaIndirectHits" @ "%t" @ %dtStats.lakStats["plasmaIndirectHits",%c]);
            %file.writeLine("plasmaInDmgTaken" @ "%t" @ %dtStats.lakStats["plasmaInDmgTaken",%c]);
            %file.writeLine("blasterInDmg" @ "%t" @ %dtStats.lakStats["blasterInDmg",%c]);
            %file.writeLine("blasterIndirectHits" @ "%t" @ %dtStats.lakStats["blasterIndirectHits",%c]);
            %file.writeLine("blasterInDmgTaken" @ "%t" @ %dtStats.lakStats["blasterInDmgTaken",%c]);
            %file.writeLine("elfInDmg" @ "%t" @ %dtStats.lakStats["elfInDmg",%c]);
            %file.writeLine("elfIndirectHits" @ "%t" @ %dtStats.lakStats["elfIndirectHits",%c]);
            %file.writeLine("elfInDmgTaken" @ "%t" @ %dtStats.lakStats["elfInDmgTaken",%c]);
            %file.writeLine("unknownInDmg" @ "%t" @ %dtStats.lakStats["unknownInDmg",%c]);
            %file.writeLine("unknownIndirectHits" @ "%t" @ %dtStats.lakStats["unknownIndirectHits",%c]);
            %file.writeLine("unknownInDmgTaken" @ "%t" @ %dtStats.lakStats["unknownInDmgTaken",%c]);
            %file.writeLine("cgShotsFired" @ "%t" @ %dtStats.lakStats["cgShotsFired",%c]);
            %file.writeLine("discShotsFired" @ "%t" @ %dtStats.lakStats["discShotsFired",%c]);
            %file.writeLine("grenadeShotsFired" @ "%t" @ %dtStats.lakStats["grenadeShotsFired",%c]);
            %file.writeLine("laserShotsFired" @ "%t" @ %dtStats.lakStats["laserShotsFired",%c]);
            %file.writeLine("mortarShotsFired" @ "%t" @ %dtStats.lakStats["mortarShotsFired",%c]);
            %file.writeLine("missileShotsFired" @ "%t" @ %dtStats.lakStats["missileShotsFired",%c]);
            %file.writeLine("shockLanceShotsFired" @ "%t" @ %dtStats.lakStats["shockLanceShotsFired",%c]);
            %file.writeLine("plasmaShotsFired" @ "%t" @ %dtStats.lakStats["plasmaShotsFired",%c]);
            %file.writeLine("blasterShotsFired" @ "%t" @ %dtStats.lakStats["blasterShotsFired",%c]);
            %file.writeLine("elfShotsFired" @ "%t" @ %dtStats.lakStats["elfShotsFired",%c]);
            %file.writeLine("unknownShotsFired" @ "%t" @ %dtStats.lakStats["unknownShotsFired",%c]);
            %file.close();
         }
      }
      %file.delete();
   }
}
 
function saveLakStatsSlow(%dtStats,%c){ // called when client leaves server.cs onDrop
   if(%dtStats.lakStatsOverWrite $= ""){
      %dtStats.lakStatsOverWrite = 0;
   }
   if(%c <= %dtStats.lakGameCount){
         //error("save lak slow" SPC %c);
      if(%dtStats.guid !$= ""){
         %filename = "serverStats/Lak/" @ %dtStats.guid @ "/" @ %c @ ".cs";
         %file = new FileObject();
         %file.OpenForWrite(%filename);
         
         %file.writeLine("lakGameCount" @ "%t" @ %dtStats.lakGameCount);
         %file.writeLine("lakStatsOverWrite" @ "%t" @ %dtStats.lakStatsOverWrite);
         %file.writeLine("timeStamp" @ "%t" @ %dtStats.lakStats["timeStamp",%c]);
         
         %file.writeLine("score" @ "%t" @ %dtStats.lakStats["score",%c]);
         %file.writeLine("kills" @ "%t" @ %dtStats.lakStats["kills",%c]);
         %file.writeLine("deaths" @ "%t" @ %dtStats.lakStats["deaths",%c]);
         %file.writeLine("suicides" @ "%t" @ %dtStats.lakStats["suicides",%c]);
         %file.writeLine("flagGrabs" @ "%t" @ %dtStats.lakStats["flagGrabs",%c]);
         %file.writeLine("flagTimeMS" @ "%t" @ %dtStats.lakStats["flagTimeMS",%c]);
         %file.writeLine("morepoints" @ "%t" @ %dtStats.lakStats["morepoints",%c]);
         %file.writeLine("mas" @ "%t" @ %dtStats.lakStats["mas",%c]);
         %file.writeLine("totalSpeed" @ "%t" @ %dtStats.lakStats["totalSpeed",%c]);
         %file.writeLine("totalDistance" @ "%t" @ %dtStats.lakStats["totalDistance",%c]);
         %file.writeLine("totalChainAccuracy" @ "%t" @ %dtStats.lakStats["totalChainAccuracy",%c]);
         %file.writeLine("totalChainHits" @ "%t" @ %dtStats.lakStats["totalChainHits",%c]);
         %file.writeLine("totalSnipeHits" @ "%t" @ %dtStats.lakStats["totalSnipeHits",%c]);
         %file.writeLine("totalSnipes" @ "%t" @ %dtStats.lakStats["totalSnipes",%c]);
         %file.writeLine("totalShockHits" @ "%t" @ %dtStats.lakStats["totalShockHits",%c]);
         %file.writeLine("totalShocks" @ "%t" @ %dtStats.lakStats["totalShocks",%c]);
         
         %file.writeLine("minePlusDisc" @ "%t" @ %dtStats.lakStats["minePlusDisc",%c]);
         
         %file.writeLine("cgKills" @ "%t" @ %dtStats.lakStats["cgKills",%c]);
         %file.writeLine("cgDeaths" @ "%t" @ %dtStats.lakStats["cgDeaths",%c]);
         %file.writeLine("discKills" @ "%t" @ %dtStats.lakStats["discKills",%c]);
         %file.writeLine("discDeaths" @ "%t" @ %dtStats.lakStats["discDeaths",%c]);
         %file.writeLine("grenadeKills" @ "%t" @ %dtStats.lakStats["grenadeKills",%c]);
         %file.writeLine("grenadeDeaths" @ "%t" @ %dtStats.lakStats["grenadeDeaths",%c]);
         %file.writeLine("Headshot" @ "%t" @ %dtStats.lakStats["laserKills",%c]);
         %file.writeLine("laserDeaths" @ "%t" @ %dtStats.lakStats["laserDeaths",%c]);
         %file.writeLine("mortarKills" @ "%t" @ %dtStats.lakStats["mortarKills",%c]);
         %file.writeLine("mortarDeaths" @ "%t" @ %dtStats.lakStats["mortarDeaths",%c]);
         %file.writeLine("missileKills" @ "%t" @ %dtStats.lakStats["missileKills",%c]);
         %file.writeLine("missileDeaths" @ "%t" @ %dtStats.lakStats["missileDeaths",%c]);
         %file.writeLine("shockLanceKills" @ "%t" @ %dtStats.lakStats["shockLanceKills",%c]);
         %file.writeLine("shockLanceDeaths" @ "%t" @ %dtStats.lakStats["shockLanceDeaths",%c]);
         %file.writeLine("plasmaKills" @ "%t" @ %dtStats.lakStats["plasmaKills",%c]);
         %file.writeLine("plasmaDeaths" @ "%t" @ %dtStats.lakStats["plasmaDeaths",%c]);
         %file.writeLine("blasterKills" @ "%t" @ %dtStats.lakStats["blasterKills",%c]);
         %file.writeLine("blasterDeaths" @ "%t" @ %dtStats.lakStats["blasterDeaths",%c]);
         %file.writeLine("elfKills" @ "%t" @ %dtStats.lakStats["elfKills",%c]);
         %file.writeLine("elfDeaths" @ "%t" @ %dtStats.lakStats["elfDeaths",%c]);
         %file.writeLine("mineKills" @ "%t" @ %dtStats.lakStats["mineKills",%c]);
         %file.writeLine("mineDeaths" @ "%t" @ %dtStats.lakStats["mineDeaths",%c]);
         %file.writeLine("explosionKills" @ "%t" @ %dtStats.lakStats["explosionKills",%c]);
         %file.writeLine("explosionDeaths" @ "%t" @ %dtStats.lakStats["explosionDeaths",%c]);
         %file.writeLine("impactKills" @ "%t" @ %dtStats.lakStats["impactKills",%c]);
         %file.writeLine("impactDeaths" @ "%t" @ %dtStats.lakStats["impactDeaths",%c]);
         %file.writeLine("groundKills" @ "%t" @ %dtStats.lakStats["groundKills",%c]);
         %file.writeLine("groundDeaths" @ "%t" @ %dtStats.lakStats["groundDeaths",%c]);
         
         %file.writeLine("outOfBoundKills" @ "%t" @ %dtStats.lakStats["outOfBoundKills",%c]);
         %file.writeLine("outOfBoundDeaths" @ "%t" @ %dtStats.lakStats["outOfBoundDeaths",%c]);
         %file.writeLine("lavaKills" @ "%t" @ %dtStats.lakStats["lavaKills",%c]);
         %file.writeLine("lavaDeaths" @ "%t" @ %dtStats.lakStats["lavaDeaths",%c]);
         
         %file.writeLine("satchelChargeKills" @ "%t" @ %dtStats.lakStats["satchelChargeKills",%c]);
         %file.writeLine("satchelChargeDeaths" @ "%t" @ %dtStats.lakStats["satchelChargeDeaths",%c]);
         
         %file.writeLine("lightningKills" @ "%t" @ %dtStats.lakStats["lightningKills",%c]);
         %file.writeLine("lightningDeaths" @ "%t" @ %dtStats.lakStats["lightningDeaths",%c]);
         
         %file.writeLine("forceFieldPowerUpKills" @ "%t" @ %dtStats.lakStats["forceFieldPowerUpKills",%c]);
         %file.writeLine("forceFieldPowerUpDeaths" @ "%t" @ %dtStats.lakStats["forceFieldPowerUpDeaths",%c]);
         
         %file.writeLine("waterKills" @ "%t" @ %dtStats.lakStats["waterKills",%c]);
         %file.writeLine("waterDeaths" @ "%t" @ %dtStats.lakStats["waterDeaths",%c]);
         %file.writeLine("nexusCampingKills" @ "%t" @ %dtStats.lakStats["nexusCampingKills",%c]);
         %file.writeLine("nexusCampingDeaths" @ "%t" @ %dtStats.lakStats["nexusCampingDeaths",%c]);
         %file.writeLine("unknownKill" @ "%t" @ %dtStats.lakStats["unknownKill",%c]);
         %file.writeLine("unknownDeaths" @ "%t" @ %dtStats.lakStats["unknownDeaths",%c]);
         
         %file.writeLine("cgDmg" @ "%t" @ %dtStats.lakStats["cgDmg",%c]);
         %file.writeLine("cgDirectHits" @ "%t" @ %dtStats.lakStats["cgDirectHits",%c]);
         %file.writeLine("cgDmgTaken" @ "%t" @ %dtStats.lakStats["cgDmgTaken",%c]);
         %file.writeLine("discDmg" @ "%t" @ %dtStats.lakStats["discDmg",%c]);
         %file.writeLine("discDirectHits" @ "%t" @ %dtStats.lakStats["discDirectHits",%c]);
         %file.writeLine("discDmgTaken" @ "%t" @ %dtStats.lakStats["discDmgTaken",%c]);
         %file.writeLine("grenadeDmg" @ "%t" @ %dtStats.lakStats["grenadeDmg",%c]);
         %file.writeLine("grenadeDirectHits" @ "%t" @ %dtStats.lakStats["grenadeDirectHits",%c]);
         %file.writeLine("grenadeDmgTaken" @ "%t" @ %dtStats.lakStats["grenadeDmgTaken",%c]);
         %file.writeLine("laserDmg" @ "%t" @ %dtStats.lakStats["laserDmg",%c]);
         %file.writeLine("laserDirectHits" @ "%t" @ %dtStats.lakStats["laserDirectHits",%c]);
         %file.writeLine("laserDmgTaken" @ "%t" @ %dtStats.lakStats["laserDmgTaken",%c]);
         %file.writeLine("mortarDmg" @ "%t" @ %dtStats.lakStats["mortarDmg",%c]);
         %file.writeLine("mortarDirectHits" @ "%t" @ %dtStats.lakStats["mortarDirectHits",%c]);
         %file.writeLine("mortarDmgTaken" @ "%t" @ %dtStats.lakStats["mortarDmgTaken",%c]);
         %file.writeLine("missileDmg" @ "%t" @ %dtStats.lakStats["missileDmg",%c]);
         %file.writeLine("missileDirectHits" @ "%t" @ %dtStats.lakStats["missileDirectHits",%c]);
         %file.writeLine("missileDmgTaken" @ "%t" @ %dtStats.lakStats["missileDmgTaken",%c]);
         %file.writeLine("shockLanceDmg" @ "%t" @ %dtStats.lakStats["shockLanceDmg",%c]);
         %file.writeLine("shockLanceDirectHits" @ "%t" @ %dtStats.lakStats["shockLanceDirectHits",%c]);
         %file.writeLine("shockLanceDmgTaken" @ "%t" @ %dtStats.lakStats["shockLanceDmgTaken",%c]);
         %file.writeLine("plasmaDmg" @ "%t" @ %dtStats.lakStats["plasmaDmg",%c]);
         %file.writeLine("plasmaDirectHits" @ "%t" @ %dtStats.lakStats["plasmaDirectHits",%c]);
         %file.writeLine("plasmaDmgTaken" @ "%t" @ %dtStats.lakStats["plasmaDmgTaken",%c]);
         %file.writeLine("blasterDmg" @ "%t" @ %dtStats.lakStats["blasterDmg",%c]);
         %file.writeLine("blasterDirectHits" @ "%t" @ %dtStats.lakStats["blasterDirectHits",%c]);
         %file.writeLine("blasterDmgTaken" @ "%t" @ %dtStats.lakStats["blasterDmgTaken",%c]);
         %file.writeLine("elfDmg" @ "%t" @ %dtStats.lakStats["elfDmg",%c]);
         %file.writeLine("elfDirectHits" @ "%t" @ %dtStats.lakStats["elfDirectHits",%c]);
         %file.writeLine("elfDmgTaken" @ "%t" @ %dtStats.lakStats["elfDmgTaken",%c]);
         %file.writeLine("unknownDmg" @ "%t" @ %dtStats.lakStats["unknownDmg",%c]);
         %file.writeLine("unknownDirectHits" @ "%t" @ %dtStats.lakStats["unknownDirectHits",%c]);
         %file.writeLine("unknownDmgTaken" @ "%t" @ %dtStats.lakStats["unknownDmgTaken",%c]);
         %file.writeLine("cgInDmg" @ "%t" @ %dtStats.lakStats["cgInDmg",%c]);
         %file.writeLine("cgIndirectHits" @ "%t" @ %dtStats.lakStats["cgIndirectHits",%c]);
         %file.writeLine("cgInDmgTaken" @ "%t" @ %dtStats.lakStats["cgInDmgTaken",%c]);
         %file.writeLine("discInDmg" @ "%t" @ %dtStats.lakStats["discInDmg",%c]);
         %file.writeLine("discIndirectHits" @ "%t" @ %dtStats.lakStats["discIndirectHits",%c]);
         %file.writeLine("discInDmgTaken" @ "%t" @ %dtStats.lakStats["discInDmgTaken",%c]);
         %file.writeLine("grenadeInDmg" @ "%t" @ %dtStats.lakStats["grenadeInDmg",%c]);
         %file.writeLine("grenadeIndirectHits" @ "%t" @ %dtStats.lakStats["grenadeIndirectHits",%c]);
         %file.writeLine("grenadeInDmgTaken" @ "%t" @ %dtStats.lakStats["grenadeInDmgTaken",%c]);
         %file.writeLine("laserInDmg" @ "%t" @ %dtStats.lakStats["laserInDmg",%c]);
         %file.writeLine("laserIndirectHits" @ "%t" @ %dtStats.lakStats["laserIndirectHits",%c]);
         %file.writeLine("laserInDmgTaken" @ "%t" @ %dtStats.lakStats["laserInDmgTaken",%c]);
         %file.writeLine("mortarInDmg" @ "%t" @ %dtStats.lakStats["mortarInDmg",%c]);
         %file.writeLine("mortarIndirectHits" @ "%t" @ %dtStats.lakStats["mortarIndirectHits",%c]);
         %file.writeLine("mortarInDmgTaken" @ "%t" @ %dtStats.lakStats["mortarInDmgTaken",%c]);
         %file.writeLine("missileInDmg" @ "%t" @ %dtStats.lakStats["missileInDmg",%c]);
         %file.writeLine("missileIndirectHits" @ "%t" @ %dtStats.lakStats["missileIndirectHits",%c]);
         %file.writeLine("missileInDmgTaken" @ "%t" @ %dtStats.lakStats["missileInDmgTaken",%c]);
         %file.writeLine("shockLanceInDmg" @ "%t" @ %dtStats.lakStats["shockLanceInDmg",%c]);
         %file.writeLine("shockLanceIndirectHits" @ "%t" @ %dtStats.lakStats["shockLanceIndirectHits",%c]);
         %file.writeLine("shockLanceInDmgTaken" @ "%t" @ %dtStats.lakStats["shockLanceInDmgTaken",%c]);
         %file.writeLine("plasmaInDmg" @ "%t" @ %dtStats.lakStats["plasmaInDmg",%c]);
         %file.writeLine("plasmaIndirectHits" @ "%t" @ %dtStats.lakStats["plasmaIndirectHits",%c]);
         %file.writeLine("plasmaInDmgTaken" @ "%t" @ %dtStats.lakStats["plasmaInDmgTaken",%c]);
         %file.writeLine("blasterInDmg" @ "%t" @ %dtStats.lakStats["blasterInDmg",%c]);
         %file.writeLine("blasterIndirectHits" @ "%t" @ %dtStats.lakStats["blasterIndirectHits",%c]);
         %file.writeLine("blasterInDmgTaken" @ "%t" @ %dtStats.lakStats["blasterInDmgTaken",%c]);
         %file.writeLine("elfInDmg" @ "%t" @ %dtStats.lakStats["elfInDmg",%c]);
         %file.writeLine("elfIndirectHits" @ "%t" @ %dtStats.lakStats["elfIndirectHits",%c]);
         %file.writeLine("elfInDmgTaken" @ "%t" @ %dtStats.lakStats["elfInDmgTaken",%c]);
         %file.writeLine("unknownInDmg" @ "%t" @ %dtStats.lakStats["unknownInDmg",%c]);
         %file.writeLine("unknownIndirectHits" @ "%t" @ %dtStats.lakStats["unknownIndirectHits",%c]);
         %file.writeLine("unknownInDmgTaken" @ "%t" @ %dtStats.lakStats["unknownInDmgTaken",%c]);
         %file.writeLine("cgShotsFired" @ "%t" @ %dtStats.lakStats["cgShotsFired",%c]);
         %file.writeLine("discShotsFired" @ "%t" @ %dtStats.lakStats["discShotsFired",%c]);
         %file.writeLine("grenadeShotsFired" @ "%t" @ %dtStats.lakStats["grenadeShotsFired",%c]);
         %file.writeLine("laserShotsFired" @ "%t" @ %dtStats.lakStats["laserShotsFired",%c]);
         %file.writeLine("mortarShotsFired" @ "%t" @ %dtStats.lakStats["mortarShotsFired",%c]);
         %file.writeLine("missileShotsFired" @ "%t" @ %dtStats.lakStats["missileShotsFired",%c]);
         %file.writeLine("shockLanceShotsFired" @ "%t" @ %dtStats.lakStats["shockLanceShotsFired",%c]);
         %file.writeLine("plasmaShotsFired" @ "%t" @ %dtStats.lakStats["plasmaShotsFired",%c]);
         %file.writeLine("blasterShotsFired" @ "%t" @ %dtStats.lakStats["blasterShotsFired",%c]);
         %file.writeLine("elfShotsFired" @ "%t" @ %dtStats.lakStats["elfShotsFired",%c]);
         %file.writeLine("unknownShotsFired" @ "%t" @ %dtStats.lakStats["unknownShotsFired",%c]);
         %file.close();
         %file.delete();
         schedule($dtStats::slowSaveTime,0,"saveLakStatsSlow",%dtStats,%c++);
      }
   }
}

function incCTFStats(%client) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %client.viewMenu = "Reset";
   if(%client.dtStats.ctfGameCount  >= $dtStats::MaxNumOfGames){ // we have the max number allowed
      if(%client.dtStats.ctfStatsOverWrite < $dtStats::MaxNumOfGames){
         %c = %client.dtStats.ctfStatsOverWrite;
         %client.dtStats.ctfStatsOverWrite++;
      }
      else{
         %client.dtStats.ctfStatsOverWrite = 1; //reset
         %c = %client.dtStats.ctfStatsOverWrite;
         %client.dtStats.ctfStatsOverWrite++;
      }
   }
   else{
      %c = %client.dtStats.ctfGameCount++; // number of games this player has played
   }
   //error(%c @ "%t" @ "incCTFStats");
   %client.dtStats.ctfStats["timeStamp",%c] = formattimestring("hh:nn a, mm-dd-yy");
   %client.dtStats.ctfStats["kills",%c] = %client.kills;
   %client.dtStats.ctfStats["deaths",%c] = %client.deaths;
   %client.dtStats.ctfStats["suicides",%c] = %client.suicides;
   %client.dtStats.ctfStats["teamKills",%c] = %client.teamKills;
   %client.dtStats.ctfStats["flagCaps",%c] = %client.flagCaps;
   %client.dtStats.ctfStats["flagGrabs",%c] = %client.flagGrabs;
   %client.dtStats.ctfStats["carrierKills",%c] = %client.carrierKills;
   %client.dtStats.ctfStats["flagReturns",%c] =  %client.flagReturns;
   %client.dtStats.ctfStats["score",%c] = %client.score;
   %client.dtStats.ctfStats["scoreMidAir",%c] = %client.scoreMidAir;
   %client.dtStats.ctfStats["scoreHeadshot",%c] = %client.scoreHeadshot;
   %client.dtStats.ctfStats["minePlusDisc",%c] = %client.minePlusDisc;
   
   %client.dtStats.ctfStats["scoreRearshot",%c] = %client.scoreRearshot;
   %client.dtStats.ctfStats["escortAssists",%c] = %client.escortAssists;
   %client.dtStats.ctfStats["defenseScore",%c] = %client.defenseScore;
   %client.dtStats.ctfStats["offenseScore",%c] = %client.offenseScore;
   %client.dtStats.ctfStats["flagDefends",%c] = %client.flagDefends;
   
   %client.dtStats.ctfStats["cgKills",%c] = %client.cgKills;
   %client.dtStats.ctfStats["cgDeaths",%c] = %client.cgDeaths;
   %client.dtStats.ctfStats["discKills",%c] = %client.discKills;
   %client.dtStats.ctfStats["discDeaths",%c] = %client.discDeaths;
   %client.dtStats.ctfStats["grenadeKills",%c] = %client.grenadeKills;
   %client.dtStats.ctfStats["grenadeDeaths",%c] = %client.grenadeDeaths;
   %client.dtStats.ctfStats["laserKills",%c] = %client.laserKills;
   %client.dtStats.ctfStats["laserDeaths",%c] = %client.laserDeaths;
   %client.dtStats.ctfStats["mortarKills",%c] = %client.mortarKills;
   %client.dtStats.ctfStats["mortarDeaths",%c] = %client.mortarDeaths;
   %client.dtStats.ctfStats["missileKills",%c] = %client.missileKills;
   %client.dtStats.ctfStats["missileDeaths",%c] = %client.missileDeaths;
   %client.dtStats.ctfStats["shockLanceKills",%c] = %client.shockLanceKills;
   %client.dtStats.ctfStats["shockLanceDeaths",%c] = %client.shockLanceDeaths;
   %client.dtStats.ctfStats["plasmaKills",%c] = %client.plasmaKills;
   %client.dtStats.ctfStats["plasmaDeaths",%c] = %client.plasmaDeaths;
   %client.dtStats.ctfStats["blasterKills",%c] = %client.blasterKills;
   %client.dtStats.ctfStats["blasterDeaths",%c] = %client.blasterDeaths;
   %client.dtStats.ctfStats["elfKills",%c] = %client.elfKills;
   %client.dtStats.ctfStats["elfDeaths",%c] = %client.elfDeaths;
   %client.dtStats.ctfStats["mineKills",%c] = %client.mineKills;
   %client.dtStats.ctfStats["mineDeaths",%c] = %client.mineDeaths;
   %client.dtStats.ctfStats["explosionKills",%c] = %client.explosionKills;
   %client.dtStats.ctfStats["explosionDeaths",%c] = %client.explosionDeaths;
   %client.dtStats.ctfStats["impactKills",%c] =  %client.impactKills;
   %client.dtStats.ctfStats["impactDeaths",%c] = %client.impactDeaths;
   %client.dtStats.ctfStats["groundKills",%c] = %client.groundKills;
   %client.dtStats.ctfStats["groundDeaths",%c] = %client.groundDeaths;
   %client.dtStats.ctfStats["turretKills",%c] = %client.turretKills;
   %client.dtStats.ctfStats["turretDeaths",%c] = %client.turretDeaths;
   %client.dtStats.ctfStats["plasmaTurretKills",%c] =  %client.plasmaTurretKills;
   %client.dtStats.ctfStats["plasmaTurretDeaths",%c] = %client.plasmaTurretDeaths;
   %client.dtStats.ctfStats["aaTurretKills",%c] = %client.aaTurretKills;
   %client.dtStats.ctfStats["aaTurretDeaths",%c] = %client.aaTurretDeaths;
   %client.dtStats.ctfStats["elfTurretKills",%c] = %client.elfTurretKills;
   %client.dtStats.ctfStats["elfTurretDeaths",%c] = %client.elfTurretDeaths;
   %client.dtStats.ctfStats["mortarTurretKills",%c] = %client.mortarTurretKills;
   %client.dtStats.ctfStats["mortarTurretDeaths",%c] = %client.mortarTurretDeaths;
   %client.dtStats.ctfStats["missileTurretKills",%c] = %client.missileTurretKills;
   %client.dtStats.ctfStats["missileTurretDeaths",%c] = %client.missileTurretDeaths;
   %client.dtStats.ctfStats["indoorDepTurretKills",%c] = %client.indoorDepTurretKills;
   %client.dtStats.ctfStats["indoorDepTurretDeaths",%c] = %client.indoorDepTurretDeaths;
   %client.dtStats.ctfStats["outdoorDepTurretKills",%c] = %client.outdoorDepTurretKills;
   %client.dtStats.ctfStats["outdoorDepTurretDeaths",%c] =  %client.outdoorDepTurretDeaths;
   %client.dtStats.ctfStats["sentryTurretKills",%c] = %client.sentryTurretKills;
   %client.dtStats.ctfStats["sentryTurretDeaths",%c] = %client.sentryTurretDeaths;
   %client.dtStats.ctfStats["outOfBoundKills",%c] = %client.outOfBoundKills;
   %client.dtStats.ctfStats["outOfBoundDeaths",%c] = %client.outOfBoundDeaths;
   %client.dtStats.ctfStats["lavaKills",%c] = %client.lavaKills;
   %client.dtStats.ctfStats["lavaDeaths",%c] = %client.lavaDeaths;
   %client.dtStats.ctfStats["shrikeBlasterKills",%c] = %client.shrikeBlasterKills;
   %client.dtStats.ctfStats["shrikeBlasterDeaths",%c] = %client.shrikeBlasterDeaths;
   %client.dtStats.ctfStats["bellyTurretKills",%c] = %client.bellyTurretKills;
   %client.dtStats.ctfStats["bellyTurretDeaths",%c] = %client.bellyTurretDeaths;
   %client.dtStats.ctfStats["bomberBombsKills",%c] = %client.bomberBombsKills;
   %client.dtStats.ctfStats["bomberBombsDeaths",%c] = %client.bomberBombsDeaths;
   %client.dtStats.ctfStats["tankChaingunKills",%c] = %client.tankChaingunKills;
   %client.dtStats.ctfStats["tankChaingunDeaths",%c] = %client.tankChaingunDeaths;
   %client.dtStats.ctfStats["tankMortarKills",%c] = %client.tankMortarKills;
   %client.dtStats.ctfStats["tankMortarDeaths",%c] = %client.tankMortarDeaths;
   %client.dtStats.ctfStats["satchelChargeKills",%c] = %client.satchelChargeKills;
   %client.dtStats.ctfStats["satchelChargeDeaths",%c] = %client.satchelChargeDeaths;
   %client.dtStats.ctfStats["mpbMissileKills",%c] = %client.mpbMissileKills;
   %client.dtStats.ctfStats["mpbMissileDeaths",%c] = %client.mpbMissileDeaths;
   %client.dtStats.ctfStats["lightningKills",%c] = %client.lightningKills;
   %client.dtStats.ctfStats["lightningDeaths",%c] = %client.lightningDeaths;
   %client.dtStats.ctfStats["vehicleSpawnKills",%c] = %client.vehicleSpawnKills;
   %client.dtStats.ctfStats["vehicleSpawnDeaths",%c] = %client.vehicleSpawnDeaths;
   %client.dtStats.ctfStats["forceFieldPowerUpKills",%c] = %client.forceFieldPowerUpKills;
   %client.dtStats.ctfStats["forceFieldPowerUpDeaths",%c] = %client.forceFieldPowerUpDeaths;
   %client.dtStats.ctfStats["crashKills",%c] = %client.crashKills;
   %client.dtStats.ctfStats["crashDeaths",%c] = %client.crashDeaths;
   %client.dtStats.ctfStats["waterKills",%c] = %client.waterKills;
   %client.dtStats.ctfStats["waterDeaths",%c] = %client.waterDeaths;
   %client.dtStats.ctfStats["nexusCampingKills",%c] = %client.nexusCampingKills;
   %client.dtStats.ctfStats["nexusCampingDeaths",%c] = %client.nexusCampingDeaths;
   %client.dtStats.ctfStats["unknownKill",%c] = %client.unknownKill;
   %client.dtStats.ctfStats["unknownDeaths",%c] = %client.unknownDeaths;
   %client.dtStats.ctfStats["cgDmg",%c] = %client.cgDmg;
   %client.dtStats.ctfStats["cgDirectHits",%c] = %client.cgDirectHits;
   %client.dtStats.ctfStats["cgDmgTaken",%c] = %client.cgDmgTaken;
   %client.dtStats.ctfStats["discDmg",%c] = %client.discDmg;
   %client.dtStats.ctfStats["discDirectHits",%c] = %client.discDirectHits;
   %client.dtStats.ctfStats["discDmgTaken",%c] = %client.discDmgTaken;
   %client.dtStats.ctfStats["grenadeDmg",%c] = %client.grenadeDmg;
   %client.dtStats.ctfStats["grenadeDirectHits",%c] = %client.grenadeDirectHits;
   %client.dtStats.ctfStats["grenadeDmgTaken",%c] = %client.grenadeDmgTaken;
   %client.dtStats.ctfStats["laserDmg",%c] = %client.laserDmg;
   %client.dtStats.ctfStats["laserDirectHits",%c] = %client.laserDirectHits;
   %client.dtStats.ctfStats["laserDmgTaken",%c] = %client.laserDmgTaken;
   %client.dtStats.ctfStats["mortarDmg",%c] = %client.mortarDmg;
   %client.dtStats.ctfStats["mortarDirectHits",%c] = %client.mortarDirectHits;
   %client.dtStats.ctfStats["mortarDmgTaken",%c] = %client.mortarDmgTaken;
   %client.dtStats.ctfStats["missileDmg",%c] = %client.missileDmg;
   %client.dtStats.ctfStats["missileDirectHits",%c] = %client.missileDirectHits;
   %client.dtStats.ctfStats["missileDmgTaken",%c] = %client.missileDmgTaken;
   %client.dtStats.ctfStats["shockLanceDmg",%c] = %client.shockLanceDmg;
   %client.dtStats.ctfStats["shockLanceDirectHits",%c] = %client.shockLanceDirectHits;
   %client.dtStats.ctfStats["shockLanceDmgTaken",%c] = %client.shockLanceDmgTaken;
   %client.dtStats.ctfStats["plasmaDmg",%c] = %client.plasmaDmg;
   %client.dtStats.ctfStats["plasmaDirectHits",%c] = %client.plasmaDirectHits;
   %client.dtStats.ctfStats["plasmaDmgTaken",%c] = %client.plasmaDmgTaken;
   %client.dtStats.ctfStats["blasterDmg",%c] = %client.blasterDmg;
   %client.dtStats.ctfStats["blasterDirectHits",%c] = %client.blasterDirectHits;
   %client.dtStats.ctfStats["blasterDmgTaken",%c] = %client.blasterDmgTaken;
   %client.dtStats.ctfStats["elfDmg",%c] = %client.elfDmg;
   %client.dtStats.ctfStats["elfDirectHits",%c] = %client.elfDirectHits;
   %client.dtStats.ctfStats["elfDmgTaken",%c] = %client.elfDmgTaken;
   %client.dtStats.ctfStats["unknownDmg",%c] = %client.unknownDmg;
   %client.dtStats.ctfStats["unknownDirectHits",%c] = %client.unknownDirectHits;
   %client.dtStats.ctfStats["unknownDmgTaken",%c] = %client.unknownDmgTaken;
   %client.dtStats.ctfStats["cgInDmg",%c] = %client.cgInDmg;
   %client.dtStats.ctfStats["cgIndirectHits",%c] = %client.cgIndirectHits;
   %client.dtStats.ctfStats["cgInDmgTaken",%c] = %client.cgInDmgTaken;
   %client.dtStats.ctfStats["discInDmg",%c] = %client.discInDmg;
   %client.dtStats.ctfStats["discIndirectHits",%c] = %client.discIndirectHits;
   %client.dtStats.ctfStats["discInDmgTaken",%c] = %client.discInDmgTaken;
   %client.dtStats.ctfStats["grenadeInDmg",%c] = %client.grenadeInDmg;
   %client.dtStats.ctfStats["grenadeIndirectHits",%c] = %client.grenadeIndirectHits;
   %client.dtStats.ctfStats["grenadeInDmgTaken",%c] = %client.grenadeInDmgTaken;
   %client.dtStats.ctfStats["laserInDmg",%c] = %client.laserInDmg;
   %client.dtStats.ctfStats["laserIndirectHits",%c] = %client.laserIndirectHits;
   %client.dtStats.ctfStats["laserInDmgTaken",%c] = %client.laserInDmgTaken;
   %client.dtStats.ctfStats["mortarInDmg",%c] = %client.mortarInDmg;
   %client.dtStats.ctfStats["mortarIndirectHits",%c] = %client.mortarIndirectHits;
   %client.dtStats.ctfStats["mortarInDmgTaken",%c] = %client.mortarInDmgTaken;
   %client.dtStats.ctfStats["missileInDmg",%c] = %client.missileInDmg;
   %client.dtStats.ctfStats["missileIndirectHits",%c] = %client.missileIndirectHits;
   %client.dtStats.ctfStats["missileInDmgTaken",%c] = %client.missileInDmgTaken;
   %client.dtStats.ctfStats["shockLanceInDmg",%c] = %client.shockLanceInDmg;
   %client.dtStats.ctfStats["shockLanceIndirectHits",%c] = %client.shockLanceIndirectHits;
   %client.dtStats.ctfStats["shockLanceInDmgTaken",%c] = %client.shockLanceInDmgTaken;
   %client.dtStats.ctfStats["plasmaInDmg",%c] = %client.plasmaInDmg;
   %client.dtStats.ctfStats["plasmaIndirectHits",%c] = %client.plasmaIndirectHits;
   %client.dtStats.ctfStats["plasmaInDmgTaken",%c] = %client.plasmaInDmgTaken;
   %client.dtStats.ctfStats["blasterInDmg",%c] = %client.blasterInDmg;
   %client.dtStats.ctfStats["blasterIndirectHits",%c] = %client.blasterIndirectHits;
   %client.dtStats.ctfStats["blasterInDmgTaken",%c] = %client.blasterInDmgTaken;
   %client.dtStats.ctfStats["elfInDmg",%c] = %client.elfInDmg;
   %client.dtStats.ctfStats["elfIndirectHits",%c] = %client.elfIndirectHits;
   %client.dtStats.ctfStats["elfInDmgTaken",%c] = %client.elfInDmgTaken;
   %client.dtStats.ctfStats["unknownInDmg",%c] = %client.unknownInDmg;
   %client.dtStats.ctfStats["unknownIndirectHits",%c] = %client.unknownIndirectHits;
   %client.dtStats.ctfStats["unknownInDmgTaken",%c] = %client.unknownInDmgTaken;
   %client.dtStats.ctfStats["cgShotsFired",%c] = %client.cgShotsFired;
   %client.dtStats.ctfStats["discShotsFired",%c] = %client.discShotsFired;
   %client.dtStats.ctfStats["grenadeShotsFired",%c] = %client.grenadeShotsFired;
   %client.dtStats.ctfStats["laserShotsFired",%c] = %client.laserShotsFired;
   %client.dtStats.ctfStats["mortarShotsFired",%c] = %client.mortarShotsFired;
   %client.dtStats.ctfStats["missileShotsFired",%c] = %client.missileShotsFired;
   %client.dtStats.ctfStats["shockLanceShotsFired",%c] = %client.shockLanceShotsFired;
   %client.dtStats.ctfStats["plasmaShotsFired",%c] = %client.plasmaShotsFired;
   %client.dtStats.ctfStats["blasterShotsFired",%c] = %client.blasterShotsFired;
   %client.dtStats.ctfStats["elfShotsFired",%c] = %client.elfShotsFired;
   %client.dtStats.ctfStats["unknownShotsFired",%c] = %client.unknownShotsFired;
   addCTFTotal(%client); // add totals
   initWepStats(%client); // reset to 0 for next game
}
function incBakCTFStats(%dtStats) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   if(%dtStats.ctfGameCount  >= $dtStats::MaxNumOfGames){ // we have the max number allowed
      if(%dtStats.ctfStatsOverWrite < $dtStats::MaxNumOfGames){
         %c = %dtStats.ctfStatsOverWrite;
         %dtStats.ctfStatsOverWrite++;
      }
      else{
         %dtStats.ctfStatsOverWrite = 1; //reset
         %c = %dtStats.ctfStatsOverWrite;
         %dtStats.ctfStatsOverWrite++;
      }
   }
   else{
      %c = %dtStats.ctfGameCount++; // number of games this player has played
   }
   %client.dtStats.ctfStats["timeStamp",%c] = formattimestring("hh:nn a, mm-dd-yy");
   %dtStats.ctfStats["kills",%c] = %dtStats.ctfStats["kills","b"];
   %dtStats.ctfStats["deaths",%c] = %dtStats.ctfStats["deaths","b"];
   %dtStats.ctfStats["suicides",%c] = %dtStats.ctfStats["suicides","b"];
   %dtStats.ctfStats["teamKills",%c] = %dtStats.ctfStats["teamKills","b"];
   %dtStats.ctfStats["flagCaps",%c] = %dtStats.ctfStats["flagCaps","b"];
   %dtStats.ctfStats["flagGrabs",%c] = %dtStats.ctfStats["flagGrabs","b"];
   %dtStats.ctfStats["carrierKills",%c] = %dtStats.ctfStats["carrierKills","b"];
   %dtStats.ctfStats["flagReturns",%c] =  %dtStats.ctfStats["flagReturns","b"];
   %dtStats.ctfStats["score",%c] = %dtStats.ctfStats["score","b"];
   %dtStats.ctfStats["scoreMidAir",%c] = %dtStats.ctfStats["scoreMidAir","b"];
   %dtStats.ctfStats["scoreHeadshot",%c] = %dtStats.ctfStats["scoreHeadshot","b"];
   %dtStats.ctfStats["minePlusDisc",%c] = %dtStats.ctfStats["minePlusDisc","b"];
   
   %dtStats.ctfStats["scoreRearshot",%c] = %dtStats.ctfStats["scoreRearshot","b"];
   %dtStats.ctfStats["escortAssists",%c] = %dtStats.ctfStats["escortAssists","b"];
   %dtStats.ctfStats["defenseScore",%c] = %dtStats.ctfStats["defenseScore","b"];
   %dtStats.ctfStats["offenseScore",%c] = %dtStats.ctfStats["offenseScore","b"];
   %dtStats.ctfStats["flagDefends",%c] = %dtStats.ctfStats["flagDefends","b"];
   
   %dtStats.ctfStats["cgKills",%c] = %dtStats.ctfStats["cgKills","b"];
   %dtStats.ctfStats["cgDeaths",%c] = %dtStats.ctfStats["cgDeaths","b"];
   %dtStats.ctfStats["discKills",%c] = %dtStats.ctfStats["discKills","b"];
   %dtStats.ctfStats["discDeaths",%c] = %dtStats.ctfStats["discDeaths","b"];
   %dtStats.ctfStats["grenadeKills",%c] = %dtStats.ctfStats["grenadeKills","b"];
   %dtStats.ctfStats["grenadeDeaths",%c] = %dtStats.ctfStats["grenadeDeaths","b"];
   %dtStats.ctfStats["laserKills",%c] = %dtStats.ctfStats["laserKills","b"];
   %dtStats.ctfStats["laserDeaths",%c] = %dtStats.ctfStats["laserDeaths","b"];
   %dtStats.ctfStats["mortarKills",%c] = %dtStats.ctfStats["mortarKills","b"];
   %dtStats.ctfStats["mortarDeaths",%c] = %dtStats.ctfStats["mortarDeaths","b"];
   %dtStats.ctfStats["missileKills",%c] = %dtStats.ctfStats["missileKills","b"];
   %dtStats.ctfStats["missileDeaths",%c] = %dtStats.ctfStats["missileDeaths","b"];
   %dtStats.ctfStats["shockLanceKills",%c] = %dtStats.ctfStats["shockLanceKills","b"];
   %dtStats.ctfStats["shockLanceDeaths",%c] = %dtStats.ctfStats["shockLanceDeaths","b"];
   %dtStats.ctfStats["plasmaKills",%c] = %dtStats.ctfStats["plasmaKills","b"];
   %dtStats.ctfStats["plasmaDeaths",%c] = %dtStats.ctfStats["plasmaDeaths","b"];
   %dtStats.ctfStats["blasterKills",%c] = %dtStats.ctfStats["blasterKills","b"];
   %dtStats.ctfStats["blasterDeaths",%c] = %dtStats.ctfStats["blasterDeaths","b"];
   %dtStats.ctfStats["elfKills",%c] = %dtStats.ctfStats["elfKills","b"];
   %dtStats.ctfStats["elfDeaths",%c] = %dtStats.ctfStats["elfDeaths","b"];
   %dtStats.ctfStats["mineKills",%c] = %dtStats.ctfStats["mineKills","b"];
   %dtStats.ctfStats["mineDeaths",%c] = %dtStats.ctfStats["mineDeaths","b"];
   %dtStats.ctfStats["explosionKills",%c] = %dtStats.ctfStats["explosionKills","b"];
   %dtStats.ctfStats["explosionDeaths",%c] = %dtStats.ctfStats["explosionDeaths","b"];
   %dtStats.ctfStats["impactKills",%c] = %dtStats.ctfStats["impactKills","b"];
   %dtStats.ctfStats["impactDeaths",%c] = %dtStats.ctfStats["impactDeaths","b"];
   %dtStats.ctfStats["groundKills",%c] = %dtStats.ctfStats["groundKills","b"];
   %dtStats.ctfStats["groundDeaths",%c] = %dtStats.ctfStats["groundDeaths","b"];
   %dtStats.ctfStats["turretKills",%c]  = %dtStats.ctfStats["turretKills","b"];
   %dtStats.ctfStats["turretDeaths",%c] = %dtStats.ctfStats["turretDeaths","b"];
   %dtStats.ctfStats["plasmaTurretKills",%c] =  %dtStats.ctfStats["plasmaTurretKills","b"];
   %dtStats.ctfStats["plasmaTurretDeaths",%c] = %dtStats.ctfStats["plasmaTurretDeaths","b"];
   %dtStats.ctfStats["aaTurretKills",%c] = %dtStats.ctfStats["aaTurretKills","b"];
   %dtStats.ctfStats["aaTurretDeaths",%c] = %dtStats.ctfStats["aaTurretDeaths","b"];
   %dtStats.ctfStats["elfTurretKills",%c] = %dtStats.ctfStats["elfTurretKills","b"];
   %dtStats.ctfStats["elfTurretDeaths",%c] = %dtStats.ctfStats["elfTurretDeaths","b"];
   %dtStats.ctfStats["mortarTurretKills",%c] = %dtStats.ctfStats["mortarTurretKills","b"];
   %dtStats.ctfStats["mortarTurretDeaths",%c] = %dtStats.ctfStats["mortarTurretDeaths","b"];
   %dtStats.ctfStats["missileTurretKills",%c] = %dtStats.ctfStats["missileTurretKills","b"];
   %dtStats.ctfStats["missileTurretDeaths",%c] = %dtStats.ctfStats["missileTurretDeaths","b"];
   %dtStats.ctfStats["indoorDepTurretKills",%c] = %dtStats.ctfStats["indoorDepTurretKills","b"];
   %dtStats.ctfStats["indoorDepTurretDeaths",%c] = %dtStats.ctfStats["indoorDepTurretDeaths","b"];
   %dtStats.ctfStats["outdoorDepTurretKills",%c] = %dtStats.ctfStats["outdoorDepTurretKills","b"];
   %dtStats.ctfStats["outdoorDepTurretDeaths",%c] =  %dtStats.ctfStats["outdoorDepTurretDeaths","b"];
   %dtStats.ctfStats["sentryTurretKills",%c] = %dtStats.ctfStats["sentryTurretKills","b"];
   %dtStats.ctfStats["sentryTurretDeaths",%c] = %dtStats.ctfStats["sentryTurretDeaths","b"];
   %dtStats.ctfStats["outOfBoundKills",%c] = %dtStats.ctfStats["outOfBoundKills","b"];
   %dtStats.ctfStats["outOfBoundDeaths",%c] = %dtStats.ctfStats["outOfBoundDeaths","b"];
   %dtStats.ctfStats["lavaKills",%c] = %dtStats.ctfStats["lavaKills","b"];
   %dtStats.ctfStats["lavaDeaths",%c] = %dtStats.ctfStats["lavaDeaths","b"];
   %dtStats.ctfStats["shrikeBlasterKills",%c] = %dtStats.ctfStats["shrikeBlasterKills","b"];
   %dtStats.ctfStats["shrikeBlasterDeaths",%c] = %dtStats.ctfStats["shrikeBlasterDeaths","b"];
   %dtStats.ctfStats["bellyTurretKills",%c] = %dtStats.ctfStats["bellyTurretKills","b"];
   %dtStats.ctfStats["bellyTurretDeaths",%c] = %dtStats.ctfStats["bellyTurretDeaths","b"];
   %dtStats.ctfStats["bomberBombsKills",%c] = %dtStats.ctfStats["bomberBombsKills","b"];
   %dtStats.ctfStats["bomberBombsDeaths",%c] = %dtStats.ctfStats["bomberBombsDeaths","b"];
   %dtStats.ctfStats["tankChaingunKills",%c] = %dtStats.ctfStats["tankChaingunKills","b"];
   %dtStats.ctfStats["tankChaingunDeaths",%c] = %dtStats.ctfStats["tankChaingunDeaths","b"];
   %dtStats.ctfStats["tankMortarKills",%c] = %dtStats.ctfStats["tankMortarKills","b"];
   %dtStats.ctfStats["tankMortarDeaths",%c] = %dtStats.ctfStats["tankMortarDeaths","b"];
   %dtStats.ctfStats["satchelChargeKills",%c] = %dtStats.ctfStats["satchelChargeKills","b"];
   %dtStats.ctfStats["satchelChargeDeaths",%c] = %dtStats.ctfStats["satchelChargeDeaths","b"];
   %dtStats.ctfStats["mpbMissileKills",%c] = %dtStats.ctfStats["mpbMissileKills","b"];
   %dtStats.ctfStats["mpbMissileDeaths",%c] = %dtStats.ctfStats["mpbMissileDeaths","b"];
   %dtStats.ctfStats["lightningKills",%c] = %dtStats.ctfStats["lightningKills","b"];
   %dtStats.ctfStats["lightningDeaths",%c] = %dtStats.ctfStats["lightningDeaths","b"];
   %dtStats.ctfStats["vehicleSpawnKills",%c] = %dtStats.ctfStats["vehicleSpawnKills","b"];
   %dtStats.ctfStats["vehicleSpawnDeaths",%c] = %dtStats.ctfStats["vehicleSpawnDeaths","b"];
   %dtStats.ctfStats["forceFieldPowerUpKills",%c] = %dtStats.ctfStats["forceFieldPowerUpKills","b"];
   %dtStats.ctfStats["forceFieldPowerUpDeaths",%c] = %dtStats.ctfStats["forceFieldPowerUpDeaths","b"];
   %dtStats.ctfStats["crashKills",%c] = %dtStats.ctfStats["crashKills","b"];
   %dtStats.ctfStats["crashDeaths",%c] = %dtStats.ctfStats["crashDeaths","b"];
   %dtStats.ctfStats["waterKills",%c] = %dtStats.ctfStats["waterKills","b"];
   %dtStats.ctfStats["waterDeaths",%c] = %dtStats.ctfStats["waterDeaths","b"];
   %dtStats.ctfStats["nexusCampingKills",%c] = %dtStats.ctfStats["nexusCampingKills","b"];
   %dtStats.ctfStats["nexusCampingDeaths",%c] = %dtStats.ctfStats["nexusCampingDeaths","b"];
   %dtStats.ctfStats["unknownKill",%c] = %dtStats.ctfStats["unknownKill","b"];
   %dtStats.ctfStats["unknownDeaths",%c]  = %dtStats.ctfStats["unknownDeaths","b"];
   %dtStats.ctfStats["cgDmg",%c]  = %dtStats.ctfStats["cgDmg","b"];
   %dtStats.ctfStats["cgDirectHits",%c]  = %dtStats.ctfStats["cgDirectHits","b"];
   %dtStats.ctfStats["cgDmgTaken",%c]  = %dtStats.ctfStats["cgDmgTaken","b"];
   %dtStats.ctfStats["discDmg",%c]  = %dtStats.ctfStats["discDmg","b"];
   %dtStats.ctfStats["discDirectHits",%c]  = %dtStats.ctfStats["discDirectHits","b"];
   %dtStats.ctfStats["discDmgTaken",%c]  = %dtStats.ctfStats["discDmgTaken","b"];
   %dtStats.ctfStats["grenadeDmg",%c]  = %dtStats.ctfStats["grenadeDmg","b"];
   %dtStats.ctfStats["grenadeDirectHits",%c]  = %dtStats.ctfStats["grenadeDirectHits","b"];
   %dtStats.ctfStats["grenadeDmgTaken",%c]  = %dtStats.ctfStats["grenadeDmgTaken","b"];
   %dtStats.ctfStats["laserDmg",%c]  = %dtStats.ctfStats["laserDmg","b"];
   %dtStats.ctfStats["laserDirectHits",%c]  = %dtStats.ctfStats["laserDirectHits","b"];
   %dtStats.ctfStats["laserDmgTaken",%c]  = %dtStats.ctfStats["laserDmgTaken","b"];
   %dtStats.ctfStats["mortarDmg",%c]  = %dtStats.ctfStats["mortarDmg","b"];
   %dtStats.ctfStats["mortarDirectHits",%c]  = %dtStats.ctfStats["mortarDirectHits","b"];
   %dtStats.ctfStats["mortarDmgTaken",%c]  = %dtStats.ctfStats["mortarDmgTaken","b"];
   %dtStats.ctfStats["missileDmg",%c]  = %dtStats.ctfStats["missileDmg","b"];
   %dtStats.ctfStats["missileDirectHits",%c]  = %dtStats.ctfStats["missileDirectHits","b"];
   %dtStats.ctfStats["missileDmgTaken",%c]  = %dtStats.ctfStats["missileDmgTaken","b"];
   %dtStats.ctfStats["shockLanceDmg",%c]  = %dtStats.ctfStats["shockLanceDmg","b"];
   %dtStats.ctfStats["shockLanceDirectHits",%c]  = %dtStats.ctfStats["shockLanceDirectHits","b"];
   %dtStats.ctfStats["shockLanceDmgTaken",%c]  = %dtStats.ctfStats["shockLanceDmgTaken","b"];
   %dtStats.ctfStats["plasmaDmg",%c]  = %dtStats.ctfStats["plasmaDmg","b"];
   %dtStats.ctfStats["plasmaDirectHits",%c]  = %dtStats.ctfStats["plasmaDirectHits","b"];
   %dtStats.ctfStats["plasmaDmgTaken",%c]  = %dtStats.ctfStats["plasmaDmgTaken","b"];
   %dtStats.ctfStats["blasterDmg",%c]  = %dtStats.ctfStats["blasterDmg","b"];
   %dtStats.ctfStats["blasterDirectHits",%c]  = %dtStats.ctfStats["blasterDirectHits","b"];
   %dtStats.ctfStats["blasterDmgTaken",%c]  = %dtStats.ctfStats["blasterDmgTaken","b"];
   %dtStats.ctfStats["elfDmg",%c]  = %dtStats.ctfStats["elfDmg","b"];
   %dtStats.ctfStats["elfDirectHits",%c]  = %dtStats.ctfStats["elfDirectHits","b"];
   %dtStats.ctfStats["elfDmgTaken",%c]  = %dtStats.ctfStats["elfDmgTaken","b"];
   %dtStats.ctfStats["unknownDmg",%c]  = %dtStats.ctfStats["unknownDmg","b"];
   %dtStats.ctfStats["unknownDirectHits",%c]  = %dtStats.ctfStats["unknownDirectHits","b"];
   %dtStats.ctfStats["unknownDmgTaken",%c]  = %dtStats.ctfStats["unknownDmgTaken","b"];
   %dtStats.ctfStats["cgInDmg",%c]  = %dtStats.ctfStats["cgInDmg","b"];
   %dtStats.ctfStats["cgIndirectHits",%c]  = %dtStats.ctfStats["cgIndirectHits","b"];
   %dtStats.ctfStats["cgInDmgTaken",%c]  = %dtStats.ctfStats["cgInDmgTaken","b"];
   %dtStats.ctfStats["discInDmg",%c]  = %dtStats.ctfStats["discInDmg","b"];
   %dtStats.ctfStats["discIndirectHits",%c]  = %dtStats.ctfStats["discIndirectHits","b"];
   %dtStats.ctfStats["discInDmgTaken",%c]  = %dtStats.ctfStats["discInDmgTaken","b"];
   %dtStats.ctfStats["grenadeInDmg",%c]  = %dtStats.ctfStats["grenadeInDmg","b"];
   %dtStats.ctfStats["grenadeIndirectHits",%c]  = %dtStats.ctfStats["grenadeIndirectHits","b"];
   %dtStats.ctfStats["grenadeInDmgTaken",%c]  = %dtStats.ctfStats["grenadeInDmgTaken","b"];
   %dtStats.ctfStats["laserInDmg",%c]  = %dtStats.ctfStats["laserInDmg","b"];
   %dtStats.ctfStats["laserIndirectHits",%c]  = %dtStats.ctfStats["laserIndirectHits","b"];
   %dtStats.ctfStats["laserInDmgTaken",%c]  = %dtStats.ctfStats["laserInDmgTaken","b"];
   %dtStats.ctfStats["mortarInDmg",%c]  = %dtStats.ctfStats["mortarInDmg","b"];
   %dtStats.ctfStats["mortarIndirectHits",%c]  = %dtStats.ctfStats["mortarIndirectHits","b"];
   %dtStats.ctfStats["mortarInDmgTaken",%c]  = %dtStats.ctfStats["mortarInDmgTaken","b"];
   %dtStats.ctfStats["missileInDmg",%c]  = %dtStats.ctfStats["missileInDmg","b"];
   %dtStats.ctfStats["missileIndirectHits",%c]  = %dtStats.ctfStats["missileIndirectHits","b"];
   %dtStats.ctfStats["missileInDmgTaken",%c]  = %dtStats.ctfStats["missileInDmgTaken","b"];
   %dtStats.ctfStats["shockLanceInDmg",%c]  = %dtStats.ctfStats["shockLanceInDmg","b"];
   %dtStats.ctfStats["shockLanceIndirectHits",%c]  = %dtStats.ctfStats["shockLanceIndirectHits","b"];
   %dtStats.ctfStats["shockLanceInDmgTaken",%c]  = %dtStats.ctfStats["shockLanceInDmgTaken","b"];
   %dtStats.ctfStats["plasmaInDmg",%c]  = %dtStats.ctfStats["plasmaInDmg","b"];
   %dtStats.ctfStats["plasmaIndirectHits",%c]  = %dtStats.ctfStats["plasmaIndirectHits","b"];
   %dtStats.ctfStats["plasmaInDmgTaken",%c]  = %dtStats.ctfStats["plasmaInDmgTaken","b"];
   %dtStats.ctfStats["blasterInDmg",%c]  = %dtStats.ctfStats["blasterInDmg","b"];
   %dtStats.ctfStats["blasterIndirectHits",%c]  = %dtStats.ctfStats["blasterIndirectHits","b"];
   %dtStats.ctfStats["blasterInDmgTaken",%c]  = %dtStats.ctfStats["blasterInDmgTaken","b"];
   %dtStats.ctfStats["elfInDmg",%c]  = %dtStats.ctfStats["elfInDmg","b"];
   %dtStats.ctfStats["elfIndirectHits",%c]  = %dtStats.ctfStats["elfIndirectHits","b"];
   %dtStats.ctfStats["elfInDmgTaken",%c]  = %dtStats.ctfStats["elfInDmgTaken","b"];
   %dtStats.ctfStats["unknownInDmg",%c]  = %dtStats.ctfStats["unknownInDmg","b"];
   %dtStats.ctfStats["unknownIndirectHits",%c]  = %dtStats.ctfStats["unknownIndirectHits","b"];
   %dtStats.ctfStats["unknownInDmgTaken",%c]  = %dtStats.ctfStats["unknownInDmgTaken","b"];
   %dtStats.ctfStats["cgShotsFired",%c]  = %dtStats.ctfStats["cgShotsFired","b"];
   %dtStats.ctfStats["discShotsFired",%c]  = %dtStats.ctfStats["discShotsFired","b"];
   %dtStats.ctfStats["grenadeShotsFired",%c]  = %dtStats.ctfStats["grenadeShotsFired","b"];
   %dtStats.ctfStats["laserShotsFired",%c]  = %dtStats.ctfStats["laserShotsFired","b"];
   %dtStats.ctfStats["mortarShotsFired",%c]  = %dtStats.ctfStats["mortarShotsFired","b"];
   %dtStats.ctfStats["missileShotsFired",%c]  = %dtStats.ctfStats["missileShotsFired","b"];
   %dtStats.ctfStats["shockLanceShotsFired",%c]  = %dtStats.ctfStats["shockLanceShotsFired","b"];
   %dtStats.ctfStats["plasmaShotsFired",%c]  = %dtStats.ctfStats["plasmaShotsFired","b"];
   %dtStats.ctfStats["blasterShotsFired",%c]  = %dtStats.ctfStats["blasterShotsFired","b"];
   %dtStats.ctfStats["elfShotsFired",%c]  = %dtStats.ctfStats["elfShotsFired","b"];
   %dtStats.ctfStats["unknownShotsFired",%c]  = %dtStats.ctfStats["unknownShotsFired","b"];
   addCTFBakTotal(%dtStats); // add totals
}
function addCTFBakTotal(%dtStats) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %client.dtStats.ctfTotalNumGames++;
   %c = "t"; // total
   %dtStats.ctfStats["timeStamp",%c] += formattimestring("hh:nn a, mm-dd-yy");
   %dtStats.ctfStats["kills",%c] += %dtStats.ctfStats["kills","b"];
   %dtStats.ctfStats["deaths",%c] += %dtStats.ctfStats["deaths","b"];
   %dtStats.ctfStats["suicides",%c] += %dtStats.ctfStats["suicides","b"];
   %dtStats.ctfStats["teamKills",%c] += %dtStats.ctfStats["teamKills","b"];
   %dtStats.ctfStats["flagCaps",%c] += %dtStats.ctfStats["flagCaps","b"];
   %dtStats.ctfStats["flagGrabs",%c] += %dtStats.ctfStats["flagGrabs","b"];
   %dtStats.ctfStats["carrierKills",%c] += %dtStats.ctfStats["carrierKills","b"];
   %dtStats.ctfStats["flagReturns",%c]+=  %dtStats.ctfStats["flagReturns","b"];
   %dtStats.ctfStats["score",%c] += %dtStats.ctfStats["score","b"];
   %dtStats.ctfStats["scoreMidAir",%c] += %dtStats.ctfStats["scoreMidAir","b"];
   %dtStats.ctfStats["scoreHeadshot",%c] += %dtStats.ctfStats["scoreHeadshot","b"];
   %dtStats.ctfStats["minePlusDisc",%c] += %dtStats.ctfStats["minePlusDisc","b"];
   
   %dtStats.ctfStats["scoreRearshot",%c] += %dtStats.ctfStats["scoreRearshot","b"];
   %dtStats.ctfStats["escortAssists",%c] += %dtStats.ctfStats["escortAssists","b"];
   %dtStats.ctfStats["defenseScore",%c] += %dtStats.ctfStats["defenseScore","b"];
   %dtStats.ctfStats["offenseScore",%c] += %dtStats.ctfStats["offenseScore","b"];
   %dtStats.ctfStats["flagDefends",%c] += %dtStats.ctfStats["flagDefends","b"];
   
   %dtStats.ctfStats["cgKills",%c] += %dtStats.ctfStats["cgKills","b"];
   %dtStats.ctfStats["cgDeaths",%c] += %dtStats.ctfStats["cgDeaths","b"];
   %dtStats.ctfStats["discKills",%c] += %dtStats.ctfStats["discKills","b"];
   %dtStats.ctfStats["discDeaths",%c] += %dtStats.ctfStats["discDeaths","b"];
   %dtStats.ctfStats["grenadeKills",%c] += %dtStats.ctfStats["grenadeKills","b"];
   %dtStats.ctfStats["grenadeDeaths",%c] += %dtStats.ctfStats["grenadeDeaths","b"];
   %dtStats.ctfStats["laserKills",%c] += %dtStats.ctfStats["laserKills","b"];
   %dtStats.ctfStats["laserDeaths",%c] += %dtStats.ctfStats["laserDeaths","b"];
   %dtStats.ctfStats["mortarKills",%c] += %dtStats.ctfStats["mortarKills","b"];
   %dtStats.ctfStats["mortarDeaths",%c] += %dtStats.ctfStats["mortarDeaths","b"];
   %dtStats.ctfStats["missileKills",%c] += %dtStats.ctfStats["missileKills","b"];
   %dtStats.ctfStats["missileDeaths",%c] += %dtStats.ctfStats["missileDeaths","b"];
   %dtStats.ctfStats["shockLanceKills",%c] += %dtStats.ctfStats["shockLanceKills","b"];
   %dtStats.ctfStats["shockLanceDeaths",%c] += %dtStats.ctfStats["shockLanceDeaths","b"];
   %dtStats.ctfStats["plasmaKills",%c] += %dtStats.ctfStats["plasmaKills","b"];
   %dtStats.ctfStats["plasmaDeaths",%c] += %dtStats.ctfStats["plasmaDeaths","b"];
   %dtStats.ctfStats["blasterKills",%c] += %dtStats.ctfStats["blasterKills","b"];
   %dtStats.ctfStats["blasterDeaths",%c] += %dtStats.ctfStats["blasterDeaths","b"];
   %dtStats.ctfStats["elfKills",%c] += %dtStats.ctfStats["elfKills","b"];
   %dtStats.ctfStats["elfDeaths",%c] += %dtStats.ctfStats["elfDeaths","b"];
   %dtStats.ctfStats["mineKills",%c] += %dtStats.ctfStats["mineKills","b"];
   %dtStats.ctfStats["mineDeaths",%c] += %dtStats.ctfStats["mineDeaths","b"];
   %dtStats.ctfStats["explosionKills",%c] += %dtStats.ctfStats["explosionKills","b"];
   %dtStats.ctfStats["explosionDeaths",%c] += %dtStats.ctfStats["explosionDeaths","b"];
   %dtStats.ctfStats["impactKills",%c] += %dtStats.ctfStats["impactKills","b"];
   %dtStats.ctfStats["impactDeaths",%c] += %dtStats.ctfStats["impactDeaths","b"];
   %dtStats.ctfStats["groundKills",%c] += %dtStats.ctfStats["groundKills","b"];
   %dtStats.ctfStats["groundDeaths",%c] += %dtStats.ctfStats["groundDeaths","b"];
   %dtStats.ctfStats["turretKills",%c]  += %dtStats.ctfStats["turretKills","b"];
   %dtStats.ctfStats["turretDeaths",%c] += %dtStats.ctfStats["turretDeaths","b"];
   %dtStats.ctfStats["plasmaTurretKills",%c] +=  %dtStats.ctfStats["plasmaTurretKills","b"];
   %dtStats.ctfStats["plasmaTurretDeaths",%c] += %dtStats.ctfStats["plasmaTurretDeaths","b"];
   %dtStats.ctfStats["aaTurretKills",%c] += %dtStats.ctfStats["aaTurretKills","b"];
   %dtStats.ctfStats["aaTurretDeaths",%c] += %dtStats.ctfStats["aaTurretDeaths","b"];
   %dtStats.ctfStats["elfTurretKills",%c] += %dtStats.ctfStats["elfTurretKills","b"];
   %dtStats.ctfStats["elfTurretDeaths",%c] += %dtStats.ctfStats["elfTurretDeaths","b"];
   %dtStats.ctfStats["mortarTurretKills",%c] += %dtStats.ctfStats["mortarTurretKills","b"];
   %dtStats.ctfStats["mortarTurretDeaths",%c] += %dtStats.ctfStats["mortarTurretDeaths","b"];
   %dtStats.ctfStats["missileTurretKills",%c] += %dtStats.ctfStats["missileTurretKills","b"];
   %dtStats.ctfStats["missileTurretDeaths",%c] += %dtStats.ctfStats["missileTurretDeaths","b"];
   %dtStats.ctfStats["indoorDepTurretKills",%c] += %dtStats.ctfStats["indoorDepTurretKills","b"];
   %dtStats.ctfStats["indoorDepTurretDeaths",%c] += %dtStats.ctfStats["indoorDepTurretDeaths","b"];
   %dtStats.ctfStats["outdoorDepTurretKills",%c] += %dtStats.ctfStats["outdoorDepTurretKills","b"];
   %dtStats.ctfStats["outdoorDepTurretDeaths",%c]+=  %dtStats.ctfStats["outdoorDepTurretDeaths","b"];
   %dtStats.ctfStats["sentryTurretKills",%c] += %dtStats.ctfStats["sentryTurretKills","b"];
   %dtStats.ctfStats["sentryTurretDeaths",%c] += %dtStats.ctfStats["sentryTurretDeaths","b"];
   %dtStats.ctfStats["outOfBoundKills",%c] += %dtStats.ctfStats["outOfBoundKills","b"];
   %dtStats.ctfStats["outOfBoundDeaths",%c] += %dtStats.ctfStats["outOfBoundDeaths","b"];
   %dtStats.ctfStats["lavaKills",%c] += %dtStats.ctfStats["lavaKills","b"];
   %dtStats.ctfStats["lavaDeaths",%c] += %dtStats.ctfStats["lavaDeaths","b"];
   %dtStats.ctfStats["shrikeBlasterKills",%c] += %dtStats.ctfStats["shrikeBlasterKills","b"];
   %dtStats.ctfStats["shrikeBlasterDeaths",%c] += %dtStats.ctfStats["shrikeBlasterDeaths","b"];
   %dtStats.ctfStats["bellyTurretKills",%c] += %dtStats.ctfStats["bellyTurretKills","b"];
   %dtStats.ctfStats["bellyTurretDeaths",%c] += %dtStats.ctfStats["bellyTurretDeaths","b"];
   %dtStats.ctfStats["bomberBombsKills",%c] += %dtStats.ctfStats["bomberBombsKills","b"];
   %dtStats.ctfStats["bomberBombsDeaths",%c] += %dtStats.ctfStats["bomberBombsDeaths","b"];
   %dtStats.ctfStats["tankChaingunKills",%c] += %dtStats.ctfStats["tankChaingunKills","b"];
   %dtStats.ctfStats["tankChaingunDeaths",%c] += %dtStats.ctfStats["tankChaingunDeaths","b"];
   %dtStats.ctfStats["tankMortarKills",%c] += %dtStats.ctfStats["tankMortarKills","b"];
   %dtStats.ctfStats["tankMortarDeaths",%c] += %dtStats.ctfStats["tankMortarDeaths","b"];
   %dtStats.ctfStats["satchelChargeKills",%c] += %dtStats.ctfStats["satchelChargeKills","b"];
   %dtStats.ctfStats["satchelChargeDeaths",%c] += %dtStats.ctfStats["satchelChargeDeaths","b"];
   %dtStats.ctfStats["mpbMissileKills",%c] += %dtStats.ctfStats["mpbMissileKills","b"];
   %dtStats.ctfStats["mpbMissileDeaths",%c] += %dtStats.ctfStats["mpbMissileDeaths","b"];
   %dtStats.ctfStats["lightningKills",%c] += %dtStats.ctfStats["lightningKills","b"];
   %dtStats.ctfStats["lightningDeaths",%c] += %dtStats.ctfStats["lightningDeaths","b"];
   %dtStats.ctfStats["vehicleSpawnKills",%c] += %dtStats.ctfStats["vehicleSpawnKills","b"];
   %dtStats.ctfStats["vehicleSpawnDeaths",%c] += %dtStats.ctfStats["vehicleSpawnDeaths","b"];
   %dtStats.ctfStats["forceFieldPowerUpKills",%c] += %dtStats.ctfStats["forceFieldPowerUpKills","b"];
   %dtStats.ctfStats["forceFieldPowerUpDeaths",%c] += %dtStats.ctfStats["forceFieldPowerUpDeaths","b"];
   %dtStats.ctfStats["crashKills",%c] += %dtStats.ctfStats["crashKills","b"];
   %dtStats.ctfStats["crashDeaths",%c] += %dtStats.ctfStats["crashDeaths","b"];
   %dtStats.ctfStats["waterKills",%c] += %dtStats.ctfStats["waterKills","b"];
   %dtStats.ctfStats["waterDeaths",%c] += %dtStats.ctfStats["waterDeaths","b"];
   %dtStats.ctfStats["nexusCampingKills",%c] += %dtStats.ctfStats["nexusCampingKills","b"];
   %dtStats.ctfStats["nexusCampingDeaths",%c] += %dtStats.ctfStats["nexusCampingDeaths","b"];
   %dtStats.ctfStats["unknownKill",%c] += %dtStats.ctfStats["unknownKill","b"];
   %dtStats.ctfStats["unknownDeaths",%c]  += %dtStats.ctfStats["unknownDeaths","b"];
   %dtStats.ctfStats["cgDmg",%c]  += %dtStats.ctfStats["cgDmg","b"];
   %dtStats.ctfStats["cgDirectHits",%c]  += %dtStats.ctfStats["cgDirectHits","b"];
   %dtStats.ctfStats["cgDmgTaken",%c]  += %dtStats.ctfStats["cgDmgTaken","b"];
   %dtStats.ctfStats["discDmg",%c]  += %dtStats.ctfStats["discDmg","b"];
   %dtStats.ctfStats["discDirectHits",%c]  += %dtStats.ctfStats["discDirectHits","b"];
   %dtStats.ctfStats["discDmgTaken",%c]  += %dtStats.ctfStats["discDmgTaken","b"];
   %dtStats.ctfStats["grenadeDmg",%c]  += %dtStats.ctfStats["grenadeDmg","b"];
   %dtStats.ctfStats["grenadeDirectHits",%c]  += %dtStats.ctfStats["grenadeDirectHits","b"];
   %dtStats.ctfStats["grenadeDmgTaken",%c]  += %dtStats.ctfStats["grenadeDmgTaken","b"];
   %dtStats.ctfStats["laserDmg",%c]  += %dtStats.ctfStats["laserDmg","b"];
   %dtStats.ctfStats["laserDirectHits",%c]  += %dtStats.ctfStats["laserDirectHits","b"];
   %dtStats.ctfStats["laserDmgTaken",%c]  += %dtStats.ctfStats["laserDmgTaken","b"];
   %dtStats.ctfStats["mortarDmg",%c]  += %dtStats.ctfStats["mortarDmg","b"];
   %dtStats.ctfStats["mortarDirectHits",%c]  += %dtStats.ctfStats["mortarDirectHits","b"];
   %dtStats.ctfStats["mortarDmgTaken",%c]  += %dtStats.ctfStats["mortarDmgTaken","b"];
   %dtStats.ctfStats["missileDmg",%c]  += %dtStats.ctfStats["missileDmg","b"];
   %dtStats.ctfStats["missileDirectHits",%c]  += %dtStats.ctfStats["missileDirectHits","b"];
   %dtStats.ctfStats["missileDmgTaken",%c]  += %dtStats.ctfStats["missileDmgTaken","b"];
   %dtStats.ctfStats["shockLanceDmg",%c]  += %dtStats.ctfStats["shockLanceDmg","b"];
   %dtStats.ctfStats["shockLanceDirectHits",%c]  += %dtStats.ctfStats["shockLanceDirectHits","b"];
   %dtStats.ctfStats["shockLanceDmgTaken",%c]  += %dtStats.ctfStats["shockLanceDmgTaken","b"];
   %dtStats.ctfStats["plasmaDmg",%c]  += %dtStats.ctfStats["plasmaDmg","b"];
   %dtStats.ctfStats["plasmaDirectHits",%c]  += %dtStats.ctfStats["plasmaDirectHits","b"];
   %dtStats.ctfStats["plasmaDmgTaken",%c]  += %dtStats.ctfStats["plasmaDmgTaken","b"];
   %dtStats.ctfStats["blasterDmg",%c]  += %dtStats.ctfStats["blasterDmg","b"];
   %dtStats.ctfStats["blasterDirectHits",%c]  += %dtStats.ctfStats["blasterDirectHits","b"];
   %dtStats.ctfStats["blasterDmgTaken",%c]  += %dtStats.ctfStats["blasterDmgTaken","b"];
   %dtStats.ctfStats["elfDmg",%c]  += %dtStats.ctfStats["elfDmg","b"];
   %dtStats.ctfStats["elfDirectHits",%c]  += %dtStats.ctfStats["elfDirectHits","b"];
   %dtStats.ctfStats["elfDmgTaken",%c]  += %dtStats.ctfStats["elfDmgTaken","b"];
   %dtStats.ctfStats["unknownDmg",%c]  += %dtStats.ctfStats["unknownDmg","b"];
   %dtStats.ctfStats["unknownDirectHits",%c]  += %dtStats.ctfStats["unknownDirectHits","b"];
   %dtStats.ctfStats["unknownDmgTaken",%c]  += %dtStats.ctfStats["unknownDmgTaken","b"];
   %dtStats.ctfStats["cgInDmg",%c]  += %dtStats.ctfStats["cgInDmg","b"];
   %dtStats.ctfStats["cgIndirectHits",%c]  += %dtStats.ctfStats["cgIndirectHits","b"];
   %dtStats.ctfStats["cgInDmgTaken",%c]  += %dtStats.ctfStats["cgInDmgTaken","b"];
   %dtStats.ctfStats["discInDmg",%c]  += %dtStats.ctfStats["discInDmg","b"];
   %dtStats.ctfStats["discIndirectHits",%c]  += %dtStats.ctfStats["discIndirectHits","b"];
   %dtStats.ctfStats["discInDmgTaken",%c]  += %dtStats.ctfStats["discInDmgTaken","b"];
   %dtStats.ctfStats["grenadeInDmg",%c]  += %dtStats.ctfStats["grenadeInDmg","b"];
   %dtStats.ctfStats["grenadeIndirectHits",%c]  += %dtStats.ctfStats["grenadeIndirectHits","b"];
   %dtStats.ctfStats["grenadeInDmgTaken",%c]  += %dtStats.ctfStats["grenadeInDmgTaken","b"];
   %dtStats.ctfStats["laserInDmg",%c]  += %dtStats.ctfStats["laserInDmg","b"];
   %dtStats.ctfStats["laserIndirectHits",%c]  += %dtStats.ctfStats["laserIndirectHits","b"];
   %dtStats.ctfStats["laserInDmgTaken",%c]  += %dtStats.ctfStats["laserInDmgTaken","b"];
   %dtStats.ctfStats["mortarInDmg",%c]  += %dtStats.ctfStats["mortarInDmg","b"];
   %dtStats.ctfStats["mortarIndirectHits",%c]  += %dtStats.ctfStats["mortarIndirectHits","b"];
   %dtStats.ctfStats["mortarInDmgTaken",%c]  += %dtStats.ctfStats["mortarInDmgTaken","b"];
   %dtStats.ctfStats["missileInDmg",%c]  += %dtStats.ctfStats["missileInDmg","b"];
   %dtStats.ctfStats["missileIndirectHits",%c]  += %dtStats.ctfStats["missileIndirectHits","b"];
   %dtStats.ctfStats["missileInDmgTaken",%c]  += %dtStats.ctfStats["missileInDmgTaken","b"];
   %dtStats.ctfStats["shockLanceInDmg",%c]  += %dtStats.ctfStats["shockLanceInDmg","b"];
   %dtStats.ctfStats["shockLanceIndirectHits",%c]  += %dtStats.ctfStats["shockLanceIndirectHits","b"];
   %dtStats.ctfStats["shockLanceInDmgTaken",%c]  += %dtStats.ctfStats["shockLanceInDmgTaken","b"];
   %dtStats.ctfStats["plasmaInDmg",%c]  += %dtStats.ctfStats["plasmaInDmg","b"];
   %dtStats.ctfStats["plasmaIndirectHits",%c]  += %dtStats.ctfStats["plasmaIndirectHits","b"];
   %dtStats.ctfStats["plasmaInDmgTaken",%c]  += %dtStats.ctfStats["plasmaInDmgTaken","b"];
   %dtStats.ctfStats["blasterInDmg",%c]  += %dtStats.ctfStats["blasterInDmg","b"];
   %dtStats.ctfStats["blasterIndirectHits",%c]  += %dtStats.ctfStats["blasterIndirectHits","b"];
   %dtStats.ctfStats["blasterInDmgTaken",%c]  += %dtStats.ctfStats["blasterInDmgTaken","b"];
   %dtStats.ctfStats["elfInDmg",%c]  += %dtStats.ctfStats["elfInDmg","b"];
   %dtStats.ctfStats["elfIndirectHits",%c]  += %dtStats.ctfStats["elfIndirectHits","b"];
   %dtStats.ctfStats["elfInDmgTaken",%c]  += %dtStats.ctfStats["elfInDmgTaken","b"];
   %dtStats.ctfStats["unknownInDmg",%c]  += %dtStats.ctfStats["unknownInDmg","b"];
   %dtStats.ctfStats["unknownIndirectHits",%c]  += %dtStats.ctfStats["unknownIndirectHits","b"];
   %dtStats.ctfStats["unknownInDmgTaken",%c]  += %dtStats.ctfStats["unknownInDmgTaken","b"];
   %dtStats.ctfStats["cgShotsFired",%c]  += %dtStats.ctfStats["cgShotsFired","b"];
   %dtStats.ctfStats["discShotsFired",%c]  += %dtStats.ctfStats["discShotsFired","b"];
   %dtStats.ctfStats["grenadeShotsFired",%c]  += %dtStats.ctfStats["grenadeShotsFired","b"];
   %dtStats.ctfStats["laserShotsFired",%c]  += %dtStats.ctfStats["laserShotsFired","b"];
   %dtStats.ctfStats["mortarShotsFired",%c]  += %dtStats.ctfStats["mortarShotsFired","b"];
   %dtStats.ctfStats["missileShotsFired",%c]  += %dtStats.ctfStats["missileShotsFired","b"];
   %dtStats.ctfStats["shockLanceShotsFired",%c]  += %dtStats.ctfStats["shockLanceShotsFired","b"];
   %dtStats.ctfStats["plasmaShotsFired",%c]  += %dtStats.ctfStats["plasmaShotsFired","b"];
   %dtStats.ctfStats["blasterShotsFired",%c]  += %dtStats.ctfStats["blasterShotsFired","b"];
   %dtStats.ctfStats["elfShotsFired",%c]  += %dtStats.ctfStats["elfShotsFired","b"];
   %dtStats.ctfStats["unknownShotsFired",%c]  += %dtStats.ctfStats["unknownShotsFired","b"];
}
function bakCTFStats(%client) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %c = "b";//backup
   %client.dtStats.ctfStats["kills",%c] = %client.kills;
   %client.dtStats.ctfStats["deaths",%c] = %client.deaths;
   %client.dtStats.ctfStats["suicides",%c] = %client.suicides;
   %client.dtStats.ctfStats["teamKills",%c] = %client.teamKills;
   %client.dtStats.ctfStats["flagCaps",%c] = %client.flagCaps;
   %client.dtStats.ctfStats["flagGrabs",%c] = %client.flagGrabs;
   %client.dtStats.ctfStats["carrierKills",%c] = %client.carrierKills;
   %client.dtStats.ctfStats["flagReturns",%c] =  %client.flagReturns;
   %client.dtStats.ctfStats["score",%c] = %client.score;
   %client.dtStats.ctfStats["scoreMidAir",%c] = %client.scoreMidAir;
   %client.dtStats.ctfStats["scoreHeadshot",%c] = %client.scoreHeadshot;
   %client.dtStats.ctfStats["minePlusDisc",%c] = %client.minePlusDisc;
   
   %client.dtStats.ctfStats["scoreRearshot",%c] = %client.scoreRearshot;
   %client.dtStats.ctfStats["escortAssists",%c] = %client.escortAssists;
   %client.dtStats.ctfStats["defenseScore",%c] = %client.defenseScore;
   %client.dtStats.ctfStats["offenseScore",%c] = %client.offenseScore;
   %client.dtStats.ctfStats["flagDefends",%c] = %client.flagDefends;
   
   %client.dtStats.ctfStats["cgKills",%c] = %client.cgKills;
   %client.dtStats.ctfStats["cgDeaths",%c] = %client.cgDeaths;
   %client.dtStats.ctfStats["discKills",%c] = %client.discKills;
   %client.dtStats.ctfStats["discDeaths",%c] = %client.discDeaths;
   %client.dtStats.ctfStats["grenadeKills",%c] = %client.grenadeKills;
   %client.dtStats.ctfStats["grenadeDeaths",%c] = %client.grenadeDeaths;
   %client.dtStats.ctfStats["laserKills",%c] = %client.laserKills;
   %client.dtStats.ctfStats["laserDeaths",%c] = %client.laserDeaths;
   %client.dtStats.ctfStats["mortarKills",%c] = %client.mortarKills;
   %client.dtStats.ctfStats["mortarDeaths",%c] = %client.mortarDeaths;
   %client.dtStats.ctfStats["missileKills",%c] = %client.missileKills;
   %client.dtStats.ctfStats["missileDeaths",%c] = %client.missileDeaths;
   %client.dtStats.ctfStats["shockLanceKills",%c] = %client.shockLanceKills;
   %client.dtStats.ctfStats["shockLanceDeaths",%c] = %client.shockLanceDeaths;
   %client.dtStats.ctfStats["plasmaKills",%c] = %client.plasmaKills;
   %client.dtStats.ctfStats["plasmaDeaths",%c] = %client.plasmaDeaths;
   %client.dtStats.ctfStats["blasterKills",%c] = %client.blasterKills;
   %client.dtStats.ctfStats["blasterDeaths",%c] = %client.blasterDeaths;
   %client.dtStats.ctfStats["elfKills",%c] = %client.elfKills;
   %client.dtStats.ctfStats["elfDeaths",%c] = %client.elfDeaths;
   %client.dtStats.ctfStats["mineKills",%c] = %client.mineKills;
   %client.dtStats.ctfStats["mineDeaths",%c] = %client.mineDeaths;
   %client.dtStats.ctfStats["explosionKills",%c] = %client.explosionKills;
   %client.dtStats.ctfStats["explosionDeaths",%c] = %client.explosionDeaths;
   %client.dtStats.ctfStats["impactKills",%c] =  %client.impactKills;
   %client.dtStats.ctfStats["impactDeaths",%c] = %client.impactDeaths;
   %client.dtStats.ctfStats["groundKills",%c] = %client.groundKills;
   %client.dtStats.ctfStats["groundDeaths",%c] = %client.groundDeaths;
   %client.dtStats.ctfStats["turretKills",%c] = %client.turretKills;
   %client.dtStats.ctfStats["turretDeaths",%c] = %client.turretDeaths;
   %client.dtStats.ctfStats["plasmaTurretKills",%c] =  %client.plasmaTurretKills;
   %client.dtStats.ctfStats["plasmaTurretDeaths",%c] = %client.plasmaTurretDeaths;
   %client.dtStats.ctfStats["aaTurretKills",%c] = %client.aaTurretKills;
   %client.dtStats.ctfStats["aaTurretDeaths",%c] = %client.aaTurretDeaths;
   %client.dtStats.ctfStats["elfTurretKills",%c] = %client.elfTurretKills;
   %client.dtStats.ctfStats["elfTurretDeaths",%c] = %client.elfTurretDeaths;
   %client.dtStats.ctfStats["mortarTurretKills",%c] = %client.mortarTurretKills;
   %client.dtStats.ctfStats["mortarTurretDeaths",%c] = %client.mortarTurretDeaths;
   %client.dtStats.ctfStats["missileTurretKills",%c] = %client.missileTurretKills;
   %client.dtStats.ctfStats["missileTurretDeaths",%c] = %client.missileTurretDeaths;
   %client.dtStats.ctfStats["indoorDepTurretKills",%c] = %client.indoorDepTurretKills;
   %client.dtStats.ctfStats["indoorDepTurretDeaths",%c] = %client.indoorDepTurretDeaths;
   %client.dtStats.ctfStats["outdoorDepTurretKills",%c] = %client.outdoorDepTurretKills;
   %client.dtStats.ctfStats["outdoorDepTurretDeaths",%c] =  %client.outdoorDepTurretDeaths;
   %client.dtStats.ctfStats["sentryTurretKills",%c] = %client.sentryTurretKills;
   %client.dtStats.ctfStats["sentryTurretDeaths",%c] = %client.sentryTurretDeaths;
   %client.dtStats.ctfStats["outOfBoundKills",%c] = %client.outOfBoundKills;
   %client.dtStats.ctfStats["outOfBoundDeaths",%c] = %client.outOfBoundDeaths;
   %client.dtStats.ctfStats["lavaKills",%c] = %client.lavaKills;
   %client.dtStats.ctfStats["lavaDeaths",%c] = %client.lavaDeaths;
   %client.dtStats.ctfStats["shrikeBlasterKills",%c] = %client.shrikeBlasterKills;
   %client.dtStats.ctfStats["shrikeBlasterDeaths",%c] = %client.shrikeBlasterDeaths;
   %client.dtStats.ctfStats["bellyTurretKills",%c] = %client.bellyTurretKills;
   %client.dtStats.ctfStats["bellyTurretDeaths",%c] = %client.bellyTurretDeaths;
   %client.dtStats.ctfStats["bomberBombsKills",%c] = %client.bomberBombsKills;
   %client.dtStats.ctfStats["bomberBombsDeaths",%c] = %client.bomberBombsDeaths;
   %client.dtStats.ctfStats["tankChaingunKills",%c] = %client.tankChaingunKills;
   %client.dtStats.ctfStats["tankChaingunDeaths",%c] = %client.tankChaingunDeaths;
   %client.dtStats.ctfStats["tankMortarKills",%c] = %client.tankMortarKills;
   %client.dtStats.ctfStats["tankMortarDeaths",%c] = %client.tankMortarDeaths;
   %client.dtStats.ctfStats["satchelChargeKills",%c] = %client.satchelChargeKills;
   %client.dtStats.ctfStats["satchelChargeDeaths",%c] = %client.satchelChargeDeaths;
   %client.dtStats.ctfStats["mpbMissileKills",%c] = %client.mpbMissileKills;
   %client.dtStats.ctfStats["mpbMissileDeaths",%c] = %client.mpbMissileDeaths;
   %client.dtStats.ctfStats["lightningKills",%c] = %client.lightningKills;
   %client.dtStats.ctfStats["lightningDeaths",%c] = %client.lightningDeaths;
   %client.dtStats.ctfStats["vehicleSpawnKills",%c] = %client.vehicleSpawnKills;
   %client.dtStats.ctfStats["vehicleSpawnDeaths",%c] = %client.vehicleSpawnDeaths;
   %client.dtStats.ctfStats["forceFieldPowerUpKills",%c] = %client.forceFieldPowerUpKills;
   %client.dtStats.ctfStats["forceFieldPowerUpDeaths",%c] = %client.forceFieldPowerUpDeaths;
   %client.dtStats.ctfStats["crashKills",%c] = %client.crashKills;
   %client.dtStats.ctfStats["crashDeaths",%c] = %client.crashDeaths;
   %client.dtStats.ctfStats["waterKills",%c] = %client.waterKills;
   %client.dtStats.ctfStats["waterDeaths",%c] = %client.waterDeaths;
   %client.dtStats.ctfStats["nexusCampingKills",%c] = %client.nexusCampingKills;
   %client.dtStats.ctfStats["nexusCampingDeaths",%c] = %client.nexusCampingDeaths;
   %client.dtStats.ctfStats["unknownKill",%c] = %client.unknownKill;
   %client.dtStats.ctfStats["unknownDeaths",%c] = %client.unknownDeaths;
   %client.dtStats.ctfStats["cgDmg",%c] = %client.cgDmg;
   %client.dtStats.ctfStats["cgDirectHits",%c] = %client.cgDirectHits;
   %client.dtStats.ctfStats["cgDmgTaken",%c] = %client.cgDmgTaken;
   %client.dtStats.ctfStats["discDmg",%c] = %client.discDmg;
   %client.dtStats.ctfStats["discDirectHits",%c] = %client.discDirectHits;
   %client.dtStats.ctfStats["discDmgTaken",%c] = %client.discDmgTaken;
   %client.dtStats.ctfStats["grenadeDmg",%c] = %client.grenadeDmg;
   %client.dtStats.ctfStats["grenadeDirectHits",%c] = %client.grenadeDirectHits;
   %client.dtStats.ctfStats["grenadeDmgTaken",%c] = %client.grenadeDmgTaken;
   %client.dtStats.ctfStats["laserDmg",%c] = %client.laserDmg;
   %client.dtStats.ctfStats["laserDirectHits",%c] = %client.laserDirectHits;
   %client.dtStats.ctfStats["laserDmgTaken",%c] = %client.laserDmgTaken;
   %client.dtStats.ctfStats["mortarDmg",%c] = %client.mortarDmg;
   %client.dtStats.ctfStats["mortarDirectHits",%c] = %client.mortarDirectHits;
   %client.dtStats.ctfStats["mortarDmgTaken",%c] = %client.mortarDmgTaken;
   %client.dtStats.ctfStats["missileDmg",%c] = %client.missileDmg;
   %client.dtStats.ctfStats["missileDirectHits",%c] = %client.missileDirectHits;
   %client.dtStats.ctfStats["missileDmgTaken",%c] = %client.missileDmgTaken;
   %client.dtStats.ctfStats["shockLanceDmg",%c] = %client.shockLanceDmg;
   %client.dtStats.ctfStats["shockLanceDirectHits",%c] = %client.shockLanceDirectHits;
   %client.dtStats.ctfStats["shockLanceDmgTaken",%c] = %client.shockLanceDmgTaken;
   %client.dtStats.ctfStats["plasmaDmg",%c] = %client.plasmaDmg;
   %client.dtStats.ctfStats["plasmaDirectHits",%c] = %client.plasmaDirectHits;
   %client.dtStats.ctfStats["plasmaDmgTaken",%c] = %client.plasmaDmgTaken;
   %client.dtStats.ctfStats["blasterDmg",%c] = %client.blasterDmg;
   %client.dtStats.ctfStats["blasterDirectHits",%c] = %client.blasterDirectHits;
   %client.dtStats.ctfStats["blasterDmgTaken",%c] = %client.blasterDmgTaken;
   %client.dtStats.ctfStats["elfDmg",%c] = %client.elfDmg;
   %client.dtStats.ctfStats["elfDirectHits",%c] = %client.elfDirectHits;
   %client.dtStats.ctfStats["elfDmgTaken",%c] = %client.elfDmgTaken;
   %client.dtStats.ctfStats["unknownDmg",%c] = %client.unknownDmg;
   %client.dtStats.ctfStats["unknownDirectHits",%c] = %client.unknownDirectHits;
   %client.dtStats.ctfStats["unknownDmgTaken",%c] = %client.unknownDmgTaken;
   %client.dtStats.ctfStats["cgInDmg",%c] = %client.cgInDmg;
   %client.dtStats.ctfStats["cgIndirectHits",%c] = %client.cgIndirectHits;
   %client.dtStats.ctfStats["cgInDmgTaken",%c] = %client.cgInDmgTaken;
   %client.dtStats.ctfStats["discInDmg",%c] = %client.discInDmg;
   %client.dtStats.ctfStats["discIndirectHits",%c] = %client.discIndirectHits;
   %client.dtStats.ctfStats["discInDmgTaken",%c] = %client.discInDmgTaken;
   %client.dtStats.ctfStats["grenadeInDmg",%c] = %client.grenadeInDmg;
   %client.dtStats.ctfStats["grenadeIndirectHits",%c] = %client.grenadeIndirectHits;
   %client.dtStats.ctfStats["grenadeInDmgTaken",%c] = %client.grenadeInDmgTaken;
   %client.dtStats.ctfStats["laserInDmg",%c] = %client.laserInDmg;
   %client.dtStats.ctfStats["laserIndirectHits",%c] = %client.laserIndirectHits;
   %client.dtStats.ctfStats["laserInDmgTaken",%c] = %client.laserInDmgTaken;
   %client.dtStats.ctfStats["mortarInDmg",%c] = %client.mortarInDmg;
   %client.dtStats.ctfStats["mortarIndirectHits",%c] = %client.mortarIndirectHits;
   %client.dtStats.ctfStats["mortarInDmgTaken",%c] = %client.mortarInDmgTaken;
   %client.dtStats.ctfStats["missileInDmg",%c] = %client.missileInDmg;
   %client.dtStats.ctfStats["missileIndirectHits",%c] = %client.missileIndirectHits;
   %client.dtStats.ctfStats["missileInDmgTaken",%c] = %client.missileInDmgTaken;
   %client.dtStats.ctfStats["shockLanceInDmg",%c] = %client.shockLanceInDmg;
   %client.dtStats.ctfStats["shockLanceIndirectHits",%c] = %client.shockLanceIndirectHits;
   %client.dtStats.ctfStats["shockLanceInDmgTaken",%c] = %client.shockLanceInDmgTaken;
   %client.dtStats.ctfStats["plasmaInDmg",%c] = %client.plasmaInDmg;
   %client.dtStats.ctfStats["plasmaIndirectHits",%c] = %client.plasmaIndirectHits;
   %client.dtStats.ctfStats["plasmaInDmgTaken",%c] = %client.plasmaInDmgTaken;
   %client.dtStats.ctfStats["blasterInDmg",%c] = %client.blasterInDmg;
   %client.dtStats.ctfStats["blasterIndirectHits",%c] = %client.blasterIndirectHits;
   %client.dtStats.ctfStats["blasterInDmgTaken",%c] = %client.blasterInDmgTaken;
   %client.dtStats.ctfStats["elfInDmg",%c] = %client.elfInDmg;
   %client.dtStats.ctfStats["elfIndirectHits",%c] = %client.elfIndirectHits;
   %client.dtStats.ctfStats["elfInDmgTaken",%c] = %client.elfInDmgTaken;
   %client.dtStats.ctfStats["unknownInDmg",%c] = %client.unknownInDmg;
   %client.dtStats.ctfStats["unknownIndirectHits",%c] = %client.unknownIndirectHits;
   %client.dtStats.ctfStats["unknownInDmgTaken",%c] = %client.unknownInDmgTaken;
   %client.dtStats.ctfStats["cgShotsFired",%c] = %client.cgShotsFired;
   %client.dtStats.ctfStats["discShotsFired",%c] = %client.discShotsFired;
   %client.dtStats.ctfStats["grenadeShotsFired",%c] = %client.grenadeShotsFired;
   %client.dtStats.ctfStats["laserShotsFired",%c] = %client.laserShotsFired;
   %client.dtStats.ctfStats["mortarShotsFired",%c] = %client.mortarShotsFired;
   %client.dtStats.ctfStats["missileShotsFired",%c] = %client.missileShotsFired;
   %client.dtStats.ctfStats["shockLanceShotsFired",%c] = %client.shockLanceShotsFired;
   %client.dtStats.ctfStats["plasmaShotsFired",%c] = %client.plasmaShotsFired;
   %client.dtStats.ctfStats["blasterShotsFired",%c] = %client.blasterShotsFired;
   %client.dtStats.ctfStats["elfShotsFired",%c] = %client.elfShotsFired;
   %client.dtStats.ctfStats["unknownShotsFired",%c] = %client.unknownShotsFired;
}
function resCTFStats(%client) {// copy data back over to client
   if($dtStats::Enable  == 0){return;}
   %c = "b";
   %client.kills = %client.dtStats.ctfStats["kills",%c];
   %client.deaths = %client.dtStats.ctfStats["deaths",%c];
   %client.suicides = %client.dtStats.ctfStats["suicides",%c];
   %client.teamKills = %client.dtStats.ctfStats["teamKills",%c];
   %client.flagCaps = %client.dtStats.ctfStats["flagCaps",%c];
   %client.flagGrabs = %client.dtStats.ctfStats["flagGrabs",%c];
   %client.carrierKills = %client.dtStats.ctfStats["carrierKills",%c];
   %client.flagReturns = %client.dtStats.ctfStats["flagReturns",%c];
   %client.score  =  %client.dtStats.ctfStats["score",%c];
   %client.scoreMidAir = %client.dtStats.ctfStats["scoreMidAir",%c];
   %client.scoreHeadshot = %client.dtStats.ctfStats["scoreHeadshot",%c];
   %client.minePlusDisc = %client.dtStats.ctfStats["minePlusDisc",%c];
   
   %client.scoreRearshot = %client.dtStats.ctfStats["scoreRearshot",%c];
   %client.escortAssists = %client.dtStats.ctfStats["escortAssists",%c];
   %client.defenseScore = %client.dtStats.ctfStats["defenseScore",%c];
   %client.offenseScore = %client.dtStats.ctfStats["offenseScore",%c];
   %client.flagDefends = %client.dtStats.ctfStats["flagDefends",%c];
   
   %client.cgKills = %client.dtStats.ctfStats["cgKills",%c];
   %client.cgDeaths = %client.dtStats.ctfStats["cgDeaths",%c];
   %client.discKills = %client.dtStats.ctfStats["discKills",%c];
   %client.discDeaths = %client.dtStats.ctfStats["discDeaths",%c];
   %client.grenadeKills = %client.dtStats.ctfStats["grenadeKills",%c] = %client.grenadeKills;
   %client.grenadeDeaths = %client.dtStats.ctfStats["grenadeDeaths",%c];
   %client.laserKills = %client.dtStats.ctfStats["laserKills",%c];
   %client.laserDeaths = %client.dtStats.ctfStats["laserDeaths",%c];
   %client.mortarKills = %client.dtStats.ctfStats["mortarKills",%c];
   %client.mortarDeaths = %client.dtStats.ctfStats["mortarDeaths",%c];
   %client.missileKills = %client.dtStats.ctfStats["missileKills",%c];
   %client.missileDeaths = %client.dtStats.ctfStats["missileDeaths",%c];
   %client.shockLanceKills = %client.dtStats.ctfStats["shockLanceKills",%c];
   %client.shockLanceDeaths = %client.dtStats.ctfStats["shockLanceDeaths",%c];
   %client.plasmaKills = %client.dtStats.ctfStats["plasmaKills",%c];
   %client.plasmaDeaths = %client.dtStats.ctfStats["plasmaDeaths",%c];
   %client.blasterKills = %client.dtStats.ctfStats["blasterKills",%c];
   %client.blasterDeaths = %client.dtStats.ctfStats["blasterDeaths",%c];
   %client.elfKills = %client.dtStats.ctfStats["elfKills",%c];
   %client.elfDeaths = %client.dtStats.ctfStats["elfDeaths",%c];
   %client.mineKills = %client.dtStats.ctfStats["mineKills",%c];
   %client.mineDeaths = %client.dtStats.ctfStats["mineDeaths",%c];
   %client.explosionKills = %client.dtStats.ctfStats["explosionKills",%c];
   %client.explosionDeaths = %client.dtStats.ctfStats["explosionDeaths",%c];
   %client.impactKills = %client.dtStats.ctfStats["impactKills",%c];
   %client.impactDeaths = %client.dtStats.ctfStats["impactDeaths",%c];
   %client.groundKills = %client.dtStats.ctfStats["groundKills",%c];
   %client.groundDeaths = %client.dtStats.ctfStats["groundDeaths",%c];
   %client.turretKills = %client.dtStats.ctfStats["turretKills",%c];
   %client.turretDeaths = %client.dtStats.ctfStats["turretDeaths",%c];
   %client.plasmaTurretKills = %client.dtStats.ctfStats["plasmaTurretKills",%c];
   %client.plasmaTurretDeaths = %client.dtStats.ctfStats["plasmaTurretDeaths",%c];
   %client.aaTurretKills = %client.dtStats.ctfStats["aaTurretKills",%c];
   %client.aaTurretDeaths = %client.dtStats.ctfStats["aaTurretDeaths",%c];
   %client.elfTurretKills = %client.dtStats.ctfStats["elfTurretKills",%c];
   %client.elfTurretDeaths = %client.dtStats.ctfStats["elfTurretDeaths",%c];
   %client.mortarTurretKills = %client.dtStats.ctfStats["mortarTurretKills",%c];
   %client.mortarTurretDeaths = %client.dtStats.ctfStats["mortarTurretDeaths",%c];
   %client.missileTurretKills = %client.dtStats.ctfStats["missileTurretKills",%c];
   %client.missileTurretDeaths = %client.dtStats.ctfStats["missileTurretDeaths",%c];
   %client.indoorDepTurretKills = %client.dtStats.ctfStats["indoorDepTurretKills",%c];
   %client.indoorDepTurretDeaths = %client.dtStats.ctfStats["indoorDepTurretDeaths",%c];
   %client.outdoorDepTurretKills = %client.dtStats.ctfStats["outdoorDepTurretKills",%c];
   %client.outdoorDepTurretDeaths = %client.dtStats.ctfStats["outdoorDepTurretDeaths",%c];
   %client.sentryTurretKills = %client.dtStats.ctfStats["sentryTurretKills",%c];
   %client.sentryTurretDeaths = %client.dtStats.ctfStats["sentryTurretDeaths",%c];
   %client.outOfBoundKills = %client.dtStats.ctfStats["outOfBoundKills",%c];
   %client.outOfBoundDeaths = %client.dtStats.ctfStats["outOfBoundDeaths",%c];
   %client.lavaKills = %client.dtStats.ctfStats["lavaKills",%c];
   %client.lavaDeaths = %client.dtStats.ctfStats["lavaDeaths",%c];
   %client.shrikeBlasterKills = %client.dtStats.ctfStats["shrikeBlasterKills",%c];
   %client.shrikeBlasterDeaths = %client.dtStats.ctfStats["shrikeBlasterDeaths",%c];
   %client.bellyTurretKills = %client.dtStats.ctfStats["bellyTurretKills",%c];
   %client.bellyTurretDeaths = %client.dtStats.ctfStats["bellyTurretDeaths",%c];
   %client.bomberBombsKills = %client.dtStats.ctfStats["bomberBombsKills",%c];
   %client.bomberBombsDeaths = %client.dtStats.ctfStats["bomberBombsDeaths",%c];
   %client.tankChaingunKills = %client.dtStats.ctfStats["tankChaingunKills",%c];
   %client.tankChaingunDeaths = %client.dtStats.ctfStats["tankChaingunDeaths",%c];
   %client.tankMortarKills = %client.dtStats.ctfStats["tankMortarKills",%c];
   %client.tankMortarDeaths = %client.dtStats.ctfStats["tankMortarDeaths",%c];
   %client.satchelChargeKills = %client.dtStats.ctfStats["satchelChargeKills",%c];
   %client.satchelChargeDeaths = %client.dtStats.ctfStats["satchelChargeDeaths",%c];
   %client.mpbMissileKills = %client.dtStats.ctfStats["mpbMissileKills",%c];
   %client.mpbMissileDeaths = %client.dtStats.ctfStats["mpbMissileDeaths",%c];
   %client.lightningKills = %client.dtStats.ctfStats["lightningKills",%c];
   %client.lightningDeaths = %client.dtStats.ctfStats["lightningDeaths",%c];
   %client.vehicleSpawnKills = %client.dtStats.ctfStats["vehicleSpawnKills",%c];
   %client.vehicleSpawnDeaths = %client.dtStats.ctfStats["vehicleSpawnDeaths",%c];
   %client.forceFieldPowerUpKills = %client.dtStats.ctfStats["forceFieldPowerUpKills",%c];
   %client.forceFieldPowerUpDeaths = %client.dtStats.ctfStats["forceFieldPowerUpDeaths",%c];
   %client.crashKills = %client.dtStats.ctfStats["crashKills",%c];
   %client.crashDeaths = %client.dtStats.ctfStats["crashDeaths",%c];
   %client.waterKills = %client.dtStats.ctfStats["waterKills",%c];
   %client.waterDeaths = %client.dtStats.ctfStats["waterDeaths",%c];
   %client.nexusCampingKills = %client.dtStats.ctfStats["nexusCampingKills",%c];
   %client.nexusCampingDeaths = %client.dtStats.ctfStats["nexusCampingDeaths",%c];
   %client.unknownKill = %client.dtStats.ctfStats["unknownKill",%c];
   %client.unknownDeaths = %client.dtStats.ctfStats["unknownDeaths",%c];
   %client.cgDmg = %client.dtStats.ctfStats["cgDmg",%c];
   %client.cgDirectHits = %client.dtStats.ctfStats["cgDirectHits",%c];
   %client.cgDmgTaken = %client.dtStats.ctfStats["cgDmgTaken",%c];
   %client.discDmg = %client.dtStats.ctfStats["discDmg",%c];
   %client.discDirectHits = %client.dtStats.ctfStats["discDirectHits",%c];
   %client.discDmgTaken = %client.dtStats.ctfStats["discDmgTaken",%c];
   %client.grenadeDmg = %client.dtStats.ctfStats["grenadeDmg",%c];
   %client.grenadeDirectHits = %client.dtStats.ctfStats["grenadeDirectHits",%c];
   %client.grenadeDmgTaken = %client.dtStats.ctfStats["grenadeDmgTaken",%c];
   %client.laserDmg = %client.dtStats.ctfStats["laserDmg",%c];
   %client.laserDirectHits = %client.dtStats.ctfStats["laserDirectHits",%c];
   %client.laserDmgTaken = %client.dtStats.ctfStats["laserDmgTaken",%c];
   %client.mortarDmg = %client.dtStats.ctfStats["mortarDmg",%c];
   %client.mortarDirectHits = %client.dtStats.ctfStats["mortarDirectHits",%c];
   %client.mortarDmgTaken = %client.dtStats.ctfStats["mortarDmgTaken",%c];
   %client.missileDmg = %client.dtStats.ctfStats["missileDmg",%c];
   %client.missileDirectHits = %client.dtStats.ctfStats["missileDirectHits",%c];
   %client.missileDmgTaken = %client.dtStats.ctfStats["missileDmgTaken",%c];
   %client.shockLanceDmg = %client.dtStats.ctfStats["shockLanceDmg",%c];
   %client.shockLanceDirectHits = %client.dtStats.ctfStats["shockLanceDirectHits",%c];
   %client.shockLanceDmgTaken = %client.dtStats.ctfStats["shockLanceDmgTaken",%c];
   %client.plasmaDmg = %client.dtStats.ctfStats["plasmaDmg",%c];
   %client.plasmaDirectHits = %client.dtStats.ctfStats["plasmaDirectHits",%c];
   %client.plasmaDmgTaken = %client.dtStats.ctfStats["plasmaDmgTaken",%c];
   %client.blasterDmg = %client.dtStats.ctfStats["blasterDmg",%c];
   %client.blasterDirectHits = %client.dtStats.ctfStats["blasterDirectHits",%c];
   %client.blasterDmgTaken = %client.dtStats.ctfStats["blasterDmgTaken",%c];
   %client.elfDmg = %client.dtStats.ctfStats["elfDmg",%c];
   %client.elfDirectHits = %client.dtStats.ctfStats["elfDirectHits",%c];
   %client.elfDmgTaken = %client.dtStats.ctfStats["elfDmgTaken",%c];
   %client.unknownDmg = %client.dtStats.ctfStats["unknownDmg",%c];
   %client.unknownDirectHits = %client.dtStats.ctfStats["unknownDirectHits",%c];
   %client.unknownDmgTaken = %client.dtStats.ctfStats["unknownDmgTaken",%c];
   %client.cgInDmg = %client.dtStats.ctfStats["cgInDmg",%c];
   %client.cgIndirectHits = %client.dtStats.ctfStats["cgIndirectHits",%c];
   %client.cgInDmgTaken = %client.dtStats.ctfStats["cgInDmgTaken",%c];
   %client.discInDmg = %client.dtStats.ctfStats["discInDmg",%c];
   %client.discIndirectHits = %client.dtStats.ctfStats["discIndirectHits",%c];
   %client.discInDmgTaken = %client.dtStats.ctfStats["discInDmgTaken",%c];
   %client.grenadeInDmg = %client.dtStats.ctfStats["grenadeInDmg",%c];
   %client.grenadeIndirectHits = %client.dtStats.ctfStats["grenadeIndirectHits",%c];
   %client.grenadeInDmgTaken = %client.dtStats.ctfStats["grenadeInDmgTaken",%c];
   %client.laserInDmg = %client.dtStats.ctfStats["laserInDmg",%c];
   %client.laserIndirectHits = %client.dtStats.ctfStats["laserIndirectHits",%c];
   %client.laserInDmgTaken = %client.dtStats.ctfStats["laserInDmgTaken",%c];
   %client.mortarInDmg = %client.dtStats.ctfStats["mortarInDmg",%c];
   %client.mortarIndirectHits = %client.dtStats.ctfStats["mortarIndirectHits",%c];
   %client.mortarInDmgTaken = %client.dtStats.ctfStats["mortarInDmgTaken",%c];
   %client.missileInDmg = %client.dtStats.ctfStats["missileInDmg",%c];
   %client.missileIndirectHits = %client.dtStats.ctfStats["missileIndirectHits",%c];
   %client.missileInDmgTaken = %client.dtStats.ctfStats["missileInDmgTaken",%c];
   %client.shockLanceInDmg = %client.dtStats.ctfStats["shockLanceInDmg",%c];
   %client.shockLanceIndirectHits = %client.dtStats.ctfStats["shockLanceIndirectHits",%c];
   %client.shockLanceInDmgTaken = %client.dtStats.ctfStats["shockLanceInDmgTaken",%c];
   %client.plasmaInDmg = %client.dtStats.ctfStats["plasmaInDmg",%c];
   %client.plasmaIndirectHits = %client.dtStats.ctfStats["plasmaIndirectHits",%c];
   %client.plasmaInDmgTaken = %client.dtStats.ctfStats["plasmaInDmgTaken",%c];
   %client.blasterInDmg = %client.dtStats.ctfStats["blasterInDmg",%c];
   %client.blasterIndirectHits = %client.dtStats.ctfStats["blasterIndirectHits",%c];
   %client.blasterInDmgTaken = %client.dtStats.ctfStats["blasterInDmgTaken",%c];
   %client.elfInDmg = %client.dtStats.ctfStats["elfInDmg",%c];
   %client.elfIndirectHits = %client.dtStats.ctfStats["elfIndirectHits",%c];
   %client.elfInDmgTaken = %client.dtStats.ctfStats["elfInDmgTaken",%c];
   %client.unknownInDmg = %client.dtStats.ctfStats["unknownInDmg",%c];
   %client.unknownIndirectHits = %client.dtStats.ctfStats["unknownIndirectHits",%c];
   %client.unknownInDmgTaken = %client.dtStats.ctfStats["unknownInDmgTaken",%c];
   %client.cgShotsFired = %client.dtStats.ctfStats["cgShotsFired",%c];
   %client.discShotsFired = %client.dtStats.ctfStats["discShotsFired",%c];
   %client.grenadeShotsFired = %client.dtStats.ctfStats["grenadeShotsFired",%c];
   %client.laserShotsFired = %client.dtStats.ctfStats["laserShotsFired",%c];
   %client.mortarShotsFired = %client.dtStats.ctfStats["mortarShotsFired",%c];
   %client.missileShotsFired = %client.dtStats.ctfStats["missileShotsFired",%c];
   %client.shockLanceShotsFired = %client.dtStats.ctfStats["shockLanceShotsFired",%c];
   %client.plasmaShotsFired = %client.dtStats.ctfStats["plasmaShotsFired",%c];
   %client.blasterShotsFired = %client.dtStats.ctfStats["blasterShotsFired",%c];
   %client.elfShotsFired = %client.dtStats.ctfStats["elfShotsFired",%c];
   %client.unknownShotsFired = %client.dtStats.ctfStats["unknownShotsFired",%c];
}
function incLakStats(%client) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %client.viewMenu = "Reset";
   if(%client.dtStats.lakGameCount  >= $dtStats::MaxNumOfGames){ // we have the max number allowed
      if(%client.dtStats.lakStatsOverWrite < $dtStats::MaxNumOfGames){
         %c = %client.dtStats.lakStatsOverWrite;
         %client.dtStats.lakStatsOverWrite++;
      }
      else{
         %client.dtStats.lakStatsOverWrite = 1; //reset
         %c = %client.dtStatslakStatsOverWrite;
         %client.dtStats.lakStatsOverWrite++;
      }
   }
   else{
      %c = %client.dtStats.lakGameCount++; // number of games this player has played
   }
   //error(%c @ "%t" @ "inclakStats");
   %client.dtStats.lakStats["timeStamp",%c] = formattimestring("hh:nn a, mm-dd-yy");
   %client.dtStats.lakStats["score",%c] = %client.score;
   %client.dtStats.lakStats["kills",%c] = %client.kills;
   %client.dtStats.lakStats["deaths",%c] = %client.deaths;
   %client.dtStats.lakStats["suicides",%c] = %client.suicides;
   %client.dtStats.lakStats["flagGrabs",%c] = %client.flagGrabs;
   %client.dtStats.lakStats["flagTimeMS",%c] = (%client.flagTimeMS / 1000)/60; // convert to mins
   %client.dtStats.lakStats["morepoints",%c] = %client.morepoints;
   %client.dtStats.lakStats["mas",%c] = %client.mas;
   %client.dtStats.lakStats["totalSpeed",%c] =  %client.totalSpeed;
   %client.dtStats.lakStats["totalDistance",%c] = %client.totalDistance;
   %client.dtStats.lakStats["totalChainAccuracy",%c] = %client.totalChainAccuracy;
   %client.dtStats.lakStats["totalChainHits",%c] = %client.totalChainHits;
   %client.dtStats.lakStats["totalSnipeHits",%c] = %client.totalSnipeHits;
   %client.dtStats.lakStats["totalSnipes",%c] = %client.totalSnipes;
   %client.dtStats.lakStats["totalShockHits",%c] = %client.totalShockHits;
   %client.dtStats.lakStats["totalShocks",%c] = %client.totalShocks;
   
   %client.dtStats.lakStats["minePlusDisc",%c] = %client.minePlusDisc;
   
   %client.dtStats.lakStats["cgKills",%c] = %client.cgKills;
   %client.dtStats.lakStats["cgDeaths",%c] = %client.cgDeaths;
   %client.dtStats.lakStats["discKills",%c] = %client.discKills;
   %client.dtStats.lakStats["discDeaths",%c] = %client.discDeaths;
   %client.dtStats.lakStats["grenadeKills",%c] = %client.grenadeKills;
   %client.dtStats.lakStats["grenadeDeaths",%c] = %client.grenadeDeaths;
   %client.dtStats.lakStats["laserKills",%c] = %client.laserKills;
   %client.dtStats.lakStats["laserDeaths",%c] = %client.laserDeaths;
   %client.dtStats.lakStats["mortarKills",%c] = %client.mortarKills;
   %client.dtStats.lakStats["mortarDeaths",%c] = %client.mortarDeaths;
   %client.dtStats.lakStats["missileKills",%c] = %client.missileKills;
   %client.dtStats.lakStats["missileDeaths",%c] = %client.missileDeaths;
   %client.dtStats.lakStats["shockLanceKills",%c] = %client.shockLanceKills;
   %client.dtStats.lakStats["shockLanceDeaths",%c] = %client.shockLanceDeaths;
   %client.dtStats.lakStats["plasmaKills",%c] = %client.plasmaKills;
   %client.dtStats.lakStats["plasmaDeaths",%c] = %client.plasmaDeaths;
   %client.dtStats.lakStats["blasterKills",%c] = %client.blasterKills;
   %client.dtStats.lakStats["blasterDeaths",%c] = %client.blasterDeaths;
   %client.dtStats.lakStats["elfKills",%c] = %client.elfKills;
   %client.dtStats.lakStats["elfDeaths",%c] = %client.elfDeaths;
   %client.dtStats.lakStats["mineKills",%c] = %client.mineKills;
   %client.dtStats.lakStats["mineDeaths",%c] = %client.mineDeaths;
   %client.dtStats.lakStats["explosionKills",%c] = %client.explosionKills;
   %client.dtStats.lakStats["explosionDeaths",%c] = %client.explosionDeaths;
   %client.dtStats.lakStats["impactKills",%c] =  %client.impactKills;
   %client.dtStats.lakStats["impactDeaths",%c] = %client.impactDeaths;
   %client.dtStats.lakStats["groundKills",%c] = %client.groundKills;
   %client.dtStats.lakStats["groundDeaths",%c] = %client.groundDeaths;
   
   %client.dtStats.lakStats["outOfBoundKills",%c] = %client.outOfBoundKills;
   %client.dtStats.lakStats["outOfBoundDeaths",%c] = %client.outOfBoundDeaths;
   %client.dtStats.lakStats["lavaKills",%c] = %client.lavaKills;
   %client.dtStats.lakStats["lavaDeaths",%c] = %client.lavaDeaths;
   
   %client.dtStats.lakStats["satchelChargeKills",%c] = %client.satchelChargeKills;
   %client.dtStats.lakStats["satchelChargeDeaths",%c] = %client.satchelChargeDeaths;
   
   %client.dtStats.lakStats["lightningKills",%c] = %client.lightningKills;
   %client.dtStats.lakStats["lightningDeaths",%c] = %client.lightningDeaths;
   
   %client.dtStats.lakStats["forceFieldPowerUpKills",%c] = %client.forceFieldPowerUpKills;
   %client.dtStats.lakStats["forceFieldPowerUpDeaths",%c] = %client.forceFieldPowerUpDeaths;
   
   %client.dtStats.lakStats["waterKills",%c] = %client.waterKills;
   %client.dtStats.lakStats["waterDeaths",%c] = %client.waterDeaths;
   %client.dtStats.lakStats["nexusCampingKills",%c] = %client.nexusCampingKills;
   %client.dtStats.lakStats["nexusCampingDeaths",%c] = %client.nexusCampingDeaths;
   %client.dtStats.lakStats["unknownKill",%c] = %client.unknownKill;
   %client.dtStats.lakStats["unknownDeaths",%c] = %client.unknownDeaths;
   
   %client.dtStats.lakStats["cgDmg",%c] = %client.cgDmg;
   %client.dtStats.lakStats["cgDirectHits",%c] = %client.cgDirectHits;
   %client.dtStats.lakStats["cgDmgTaken",%c] = %client.cgDmgTaken;
   %client.dtStats.lakStats["discDmg",%c] = %client.discDmg;
   %client.dtStats.lakStats["discDirectHits",%c] = %client.discDirectHits;
   %client.dtStats.lakStats["discDmgTaken",%c] = %client.discDmgTaken;
   %client.dtStats.lakStats["grenadeDmg",%c] = %client.grenadeDmg;
   %client.dtStats.lakStats["grenadeDirectHits",%c] = %client.grenadeDirectHits;
   %client.dtStats.lakStats["grenadeDmgTaken",%c] = %client.grenadeDmgTaken;
   %client.dtStats.lakStats["laserDmg",%c] = %client.laserDmg;
   %client.dtStats.lakStats["laserDirectHits",%c] = %client.laserDirectHits;
   %client.dtStats.lakStats["laserDmgTaken",%c] = %client.laserDmgTaken;
   %client.dtStats.lakStats["mortarDmg",%c] = %client.mortarDmg;
   %client.dtStats.lakStats["mortarDirectHits",%c] = %client.mortarDirectHits;
   %client.dtStats.lakStats["mortarDmgTaken",%c] = %client.mortarDmgTaken;
   %client.dtStats.lakStats["missileDmg",%c] = %client.missileDmg;
   %client.dtStats.lakStats["missileDirectHits",%c] = %client.missileDirectHits;
   %client.dtStats.lakStats["missileDmgTaken",%c] = %client.missileDmgTaken;
   %client.dtStats.lakStats["shockLanceDmg",%c] = %client.shockLanceDmg;
   %client.dtStats.lakStats["shockLanceDirectHits",%c] = %client.shockLanceDirectHits;
   %client.dtStats.lakStats["shockLanceDmgTaken",%c] = %client.shockLanceDmgTaken;
   %client.dtStats.lakStats["plasmaDmg",%c] = %client.plasmaDmg;
   %client.dtStats.lakStats["plasmaDirectHits",%c] = %client.plasmaDirectHits;
   %client.dtStats.lakStats["plasmaDmgTaken",%c] = %client.plasmaDmgTaken;
   %client.dtStats.lakStats["blasterDmg",%c] = %client.blasterDmg;
   %client.dtStats.lakStats["blasterDirectHits",%c] = %client.blasterDirectHits;
   %client.dtStats.lakStats["blasterDmgTaken",%c] = %client.blasterDmgTaken;
   %client.dtStats.lakStats["elfDmg",%c] = %client.elfDmg;
   %client.dtStats.lakStats["elfDirectHits",%c] = %client.elfDirectHits;
   %client.dtStats.lakStats["elfDmgTaken",%c] = %client.elfDmgTaken;
   %client.dtStats.lakStats["unknownDmg",%c] = %client.unknownDmg;
   %client.dtStats.lakStats["unknownDirectHits",%c] = %client.unknownDirectHits;
   %client.dtStats.lakStats["unknownDmgTaken",%c] = %client.unknownDmgTaken;
   %client.dtStats.lakStats["cgInDmg",%c] = %client.cgInDmg;
   %client.dtStats.lakStats["cgIndirectHits",%c] = %client.cgIndirectHits;
   %client.dtStats.lakStats["cgInDmgTaken",%c] = %client.cgInDmgTaken;
   %client.dtStats.lakStats["discInDmg",%c] = %client.discInDmg;
   %client.dtStats.lakStats["discIndirectHits",%c] = %client.discIndirectHits;
   %client.dtStats.lakStats["discInDmgTaken",%c] = %client.discInDmgTaken;
   %client.dtStats.lakStats["grenadeInDmg",%c] = %client.grenadeInDmg;
   %client.dtStats.lakStats["grenadeIndirectHits",%c] = %client.grenadeIndirectHits;
   %client.dtStats.lakStats["grenadeInDmgTaken",%c] = %client.grenadeInDmgTaken;
   %client.dtStats.lakStats["laserInDmg",%c] = %client.laserInDmg;
   %client.dtStats.lakStats["laserIndirectHits",%c] = %client.laserIndirectHits;
   %client.dtStats.lakStats["laserInDmgTaken",%c] = %client.laserInDmgTaken;
   %client.dtStats.lakStats["mortarInDmg",%c] = %client.mortarInDmg;
   %client.dtStats.lakStats["mortarIndirectHits",%c] = %client.mortarIndirectHits;
   %client.dtStats.lakStats["mortarInDmgTaken",%c] = %client.mortarInDmgTaken;
   %client.dtStats.lakStats["missileInDmg",%c] = %client.missileInDmg;
   %client.dtStats.lakStats["missileIndirectHits",%c] = %client.missileIndirectHits;
   %client.dtStats.lakStats["missileInDmgTaken",%c] = %client.missileInDmgTaken;
   %client.dtStats.lakStats["shockLanceInDmg",%c] = %client.shockLanceInDmg;
   %client.dtStats.lakStats["shockLanceIndirectHits",%c] = %client.shockLanceIndirectHits;
   %client.dtStats.lakStats["shockLanceInDmgTaken",%c] = %client.shockLanceInDmgTaken;
   %client.dtStats.lakStats["plasmaInDmg",%c] = %client.plasmaInDmg;
   %client.dtStats.lakStats["plasmaIndirectHits",%c] = %client.plasmaIndirectHits;
   %client.dtStats.lakStats["plasmaInDmgTaken",%c] = %client.plasmaInDmgTaken;
   %client.dtStats.lakStats["blasterInDmg",%c] = %client.blasterInDmg;
   %client.dtStats.lakStats["blasterIndirectHits",%c] = %client.blasterIndirectHits;
   %client.dtStats.lakStats["blasterInDmgTaken",%c] = %client.blasterInDmgTaken;
   %client.dtStats.lakStats["elfInDmg",%c] = %client.elfInDmg;
   %client.dtStats.lakStats["elfIndirectHits",%c] = %client.elfIndirectHits;
   %client.dtStats.lakStats["elfInDmgTaken",%c] = %client.elfInDmgTaken;
   %client.dtStats.lakStats["unknownInDmg",%c] = %client.unknownInDmg;
   %client.dtStats.lakStats["unknownIndirectHits",%c] = %client.unknownIndirectHits;
   %client.dtStats.lakStats["unknownInDmgTaken",%c] = %client.unknownInDmgTaken;
   %client.dtStats.lakStats["cgShotsFired",%c] = %client.cgShotsFired;
   %client.dtStats.lakStats["discShotsFired",%c] = %client.discShotsFired;
   %client.dtStats.lakStats["grenadeShotsFired",%c] = %client.grenadeShotsFired;
   %client.dtStats.lakStats["laserShotsFired",%c] = %client.laserShotsFired;
   %client.dtStats.lakStats["mortarShotsFired",%c] = %client.mortarShotsFired;
   %client.dtStats.lakStats["missileShotsFired",%c] = %client.missileShotsFired;
   %client.dtStats.lakStats["shockLanceShotsFired",%c] = %client.shockLanceShotsFired;
   %client.dtStats.lakStats["plasmaShotsFired",%c] = %client.plasmaShotsFired;
   %client.dtStats.lakStats["blasterShotsFired",%c] = %client.blasterShotsFired;
   %client.dtStats.lakStats["elfShotsFired",%c] = %client.elfShotsFired;
   %client.dtStats.lakStats["unknownShotsFired",%c] = %client.unknownShotsFired;
   addLAKTotal(%client); // add totals
   initWepStats(%client); // reset to 0 for next game
}
function bakLakStats(%client) {// backupLakStats
   if($dtStats::Enable  == 0){return;}
   %c = "b";
   %client.dtStats.lakStats["score",%c] = %client.score;
   %client.dtStats.lakStats["kills",%c] = %client.kills;
   %client.dtStats.lakStats["deaths",%c] = %client.deaths;
   %client.dtStats.lakStats["suicides",%c] = %client.suicides;
   %client.dtStats.lakStats["flagGrabs",%c] = %client.flagGrabs;
   %client.dtStats.lakStats["flagTimeMS",%c] = %client.flagTimeMS;
   %client.dtStats.lakStats["morepoints",%c] = %client.morepoints;
   %client.dtStats.lakStats["mas",%c] = %client.mas;
   %client.dtStats.lakStats["totalSpeed",%c] =  %client.totalSpeed;
   %client.dtStats.lakStats["totalDistance",%c] = %client.totalDistance;
   %client.dtStats.lakStats["totalChainAccuracy",%c] = %client.totalChainAccuracy;
   %client.dtStats.lakStats["totalChainHits",%c] = %client.totalChainHits;
   %client.dtStats.lakStats["totalSnipeHits",%c] = %client.totalSnipeHits;
   %client.dtStats.lakStats["totalSnipes",%c] = %client.totalSnipes;
   %client.dtStats.lakStats["totalShockHits",%c] = %client.totalShockHits;
   %client.dtStats.lakStats["totalShocks",%c] = %client.totalShocks;
   
   %client.dtStats.lakStats["minePlusDisc",%c] = %client.minePlusDisc;
   
   %client.dtStats.lakStats["cgKills",%c] = %client.cgKills;
   %client.dtStats.lakStats["cgDeaths",%c] = %client.cgDeaths;
   %client.dtStats.lakStats["discKills",%c] = %client.discKills;
   %client.dtStats.lakStats["discDeaths",%c] = %client.discDeaths;
   %client.dtStats.lakStats["grenadeKills",%c] = %client.grenadeKills;
   %client.dtStats.lakStats["grenadeDeaths",%c] = %client.grenadeDeaths;
   %client.dtStats.lakStats["laserKills",%c] = %client.laserKills;
   %client.dtStats.lakStats["laserDeaths",%c] = %client.laserDeaths;
   %client.dtStats.lakStats["mortarKills",%c] = %client.mortarKills;
   %client.dtStats.lakStats["mortarDeaths",%c] = %client.mortarDeaths;
   %client.dtStats.lakStats["missileKills",%c] = %client.missileKills;
   %client.dtStats.lakStats["missileDeaths",%c] = %client.missileDeaths;
   %client.dtStats.lakStats["shockLanceKills",%c] = %client.shockLanceKills;
   %client.dtStats.lakStats["shockLanceDeaths",%c] = %client.shockLanceDeaths;
   %client.dtStats.lakStats["plasmaKills",%c] = %client.plasmaKills;
   %client.dtStats.lakStats["plasmaDeaths",%c] = %client.plasmaDeaths;
   %client.dtStats.lakStats["blasterKills",%c] = %client.blasterKills;
   %client.dtStats.lakStats["blasterDeaths",%c] = %client.blasterDeaths;
   %client.dtStats.lakStats["elfKills",%c] = %client.elfKills;
   %client.dtStats.lakStats["elfDeaths",%c] = %client.elfDeaths;
   %client.dtStats.lakStats["mineKills",%c] = %client.mineKills;
   %client.dtStats.lakStats["mineDeaths",%c] = %client.mineDeaths;
   %client.dtStats.lakStats["explosionKills",%c] = %client.explosionKills;
   %client.dtStats.lakStats["explosionDeaths",%c] = %client.explosionDeaths;
   %client.dtStats.lakStats["impactKills",%c] =  %client.impactKills;
   %client.dtStats.lakStats["impactDeaths",%c] = %client.impactDeaths;
   %client.dtStats.lakStats["groundKills",%c] = %client.groundKills;
   %client.dtStats.lakStats["groundDeaths",%c] = %client.groundDeaths;
   
   %client.dtStats.lakStats["outOfBoundKills",%c] = %client.outOfBoundKills;
   %client.dtStats.lakStats["outOfBoundDeaths",%c] = %client.outOfBoundDeaths;
   %client.dtStats.lakStats["lavaKills",%c] = %client.lavaKills;
   %client.dtStats.lakStats["lavaDeaths",%c] = %client.lavaDeaths;
   
   %client.dtStats.lakStats["satchelChargeKills",%c] = %client.satchelChargeKills;
   %client.dtStats.lakStats["satchelChargeDeaths",%c] = %client.satchelChargeDeaths;
   
   %client.dtStats.lakStats["lightningKills",%c] = %client.lightningKills;
   %client.dtStats.lakStats["lightningDeaths",%c] = %client.lightningDeaths;
   
   %client.dtStats.lakStats["forceFieldPowerUpKills",%c] = %client.forceFieldPowerUpKills;
   %client.dtStats.lakStats["forceFieldPowerUpDeaths",%c] = %client.forceFieldPowerUpDeaths;
   
   %client.dtStats.lakStats["waterKills",%c] = %client.waterKills;
   %client.dtStats.lakStats["waterDeaths",%c] = %client.waterDeaths;
   %client.dtStats.lakStats["nexusCampingKills",%c] = %client.nexusCampingKills;
   %client.dtStats.lakStats["nexusCampingDeaths",%c] = %client.nexusCampingDeaths;
   %client.dtStats.lakStats["unknownKill",%c] = %client.unknownKill;
   %client.dtStats.lakStats["unknownDeaths",%c] = %client.unknownDeaths;
   
   %client.dtStats.lakStats["cgDmg",%c] = %client.cgDmg;
   %client.dtStats.lakStats["cgDirectHits",%c] = %client.cgDirectHits;
   %client.dtStats.lakStats["cgDmgTaken",%c] = %client.cgDmgTaken;
   %client.dtStats.lakStats["discDmg",%c] = %client.discDmg;
   %client.dtStats.lakStats["discDirectHits",%c] = %client.discDirectHits;
   %client.dtStats.lakStats["discDmgTaken",%c] = %client.discDmgTaken;
   %client.dtStats.lakStats["grenadeDmg",%c] = %client.grenadeDmg;
   %client.dtStats.lakStats["grenadeDirectHits",%c] = %client.grenadeDirectHits;
   %client.dtStats.lakStats["grenadeDmgTaken",%c] = %client.grenadeDmgTaken;
   %client.dtStats.lakStats["laserDmg",%c] = %client.laserDmg;
   %client.dtStats.lakStats["laserDirectHits",%c] = %client.laserDirectHits;
   %client.dtStats.lakStats["laserDmgTaken",%c] = %client.laserDmgTaken;
   %client.dtStats.lakStats["mortarDmg",%c] = %client.mortarDmg;
   %client.dtStats.lakStats["mortarDirectHits",%c] = %client.mortarDirectHits;
   %client.dtStats.lakStats["mortarDmgTaken",%c] = %client.mortarDmgTaken;
   %client.dtStats.lakStats["missileDmg",%c] = %client.missileDmg;
   %client.dtStats.lakStats["missileDirectHits",%c] = %client.missileDirectHits;
   %client.dtStats.lakStats["missileDmgTaken",%c] = %client.missileDmgTaken;
   %client.dtStats.lakStats["shockLanceDmg",%c] = %client.shockLanceDmg;
   %client.dtStats.lakStats["shockLanceDirectHits",%c] = %client.shockLanceDirectHits;
   %client.dtStats.lakStats["shockLanceDmgTaken",%c] = %client.shockLanceDmgTaken;
   %client.dtStats.lakStats["plasmaDmg",%c] = %client.plasmaDmg;
   %client.dtStats.lakStats["plasmaDirectHits",%c] = %client.plasmaDirectHits;
   %client.dtStats.lakStats["plasmaDmgTaken",%c] = %client.plasmaDmgTaken;
   %client.dtStats.lakStats["blasterDmg",%c] = %client.blasterDmg;
   %client.dtStats.lakStats["blasterDirectHits",%c] = %client.blasterDirectHits;
   %client.dtStats.lakStats["blasterDmgTaken",%c] = %client.blasterDmgTaken;
   %client.dtStats.lakStats["elfDmg",%c] = %client.elfDmg;
   %client.dtStats.lakStats["elfDirectHits",%c] = %client.elfDirectHits;
   %client.dtStats.lakStats["elfDmgTaken",%c] = %client.elfDmgTaken;
   %client.dtStats.lakStats["unknownDmg",%c] = %client.unknownDmg;
   %client.dtStats.lakStats["unknownDirectHits",%c] = %client.unknownDirectHits;
   %client.dtStats.lakStats["unknownDmgTaken",%c] = %client.unknownDmgTaken;
   %client.dtStats.lakStats["cgInDmg",%c] = %client.cgInDmg;
   %client.dtStats.lakStats["cgIndirectHits",%c] = %client.cgIndirectHits;
   %client.dtStats.lakStats["cgInDmgTaken",%c] = %client.cgInDmgTaken;
   %client.dtStats.lakStats["discInDmg",%c] = %client.discInDmg;
   %client.dtStats.lakStats["discIndirectHits",%c] = %client.discIndirectHits;
   %client.dtStats.lakStats["discInDmgTaken",%c] = %client.discInDmgTaken;
   %client.dtStats.lakStats["grenadeInDmg",%c] = %client.grenadeInDmg;
   %client.dtStats.lakStats["grenadeIndirectHits",%c] = %client.grenadeIndirectHits;
   %client.dtStats.lakStats["grenadeInDmgTaken",%c] = %client.grenadeInDmgTaken;
   %client.dtStats.lakStats["laserInDmg",%c] = %client.laserInDmg;
   %client.dtStats.lakStats["laserIndirectHits",%c] = %client.laserIndirectHits;
   %client.dtStats.lakStats["laserInDmgTaken",%c] = %client.laserInDmgTaken;
   %client.dtStats.lakStats["mortarInDmg",%c] = %client.mortarInDmg;
   %client.dtStats.lakStats["mortarIndirectHits",%c] = %client.mortarIndirectHits;
   %client.dtStats.lakStats["mortarInDmgTaken",%c] = %client.mortarInDmgTaken;
   %client.dtStats.lakStats["missileInDmg",%c] = %client.missileInDmg;
   %client.dtStats.lakStats["missileIndirectHits",%c] = %client.missileIndirectHits;
   %client.dtStats.lakStats["missileInDmgTaken",%c] = %client.missileInDmgTaken;
   %client.dtStats.lakStats["shockLanceInDmg",%c] = %client.shockLanceInDmg;
   %client.dtStats.lakStats["shockLanceIndirectHits",%c] = %client.shockLanceIndirectHits;
   %client.dtStats.lakStats["shockLanceInDmgTaken",%c] = %client.shockLanceInDmgTaken;
   %client.dtStats.lakStats["plasmaInDmg",%c] = %client.plasmaInDmg;
   %client.dtStats.lakStats["plasmaIndirectHits",%c] = %client.plasmaIndirectHits;
   %client.dtStats.lakStats["plasmaInDmgTaken",%c] = %client.plasmaInDmgTaken;
   %client.dtStats.lakStats["blasterInDmg",%c] = %client.blasterInDmg;
   %client.dtStats.lakStats["blasterIndirectHits",%c] = %client.blasterIndirectHits;
   %client.dtStats.lakStats["blasterInDmgTaken",%c] = %client.blasterInDmgTaken;
   %client.dtStats.lakStats["elfInDmg",%c] = %client.elfInDmg;
   %client.dtStats.lakStats["elfIndirectHits",%c] = %client.elfIndirectHits;
   %client.dtStats.lakStats["elfInDmgTaken",%c] = %client.elfInDmgTaken;
   %client.dtStats.lakStats["unknownInDmg",%c] = %client.unknownInDmg;
   %client.dtStats.lakStats["unknownIndirectHits",%c] = %client.unknownIndirectHits;
   %client.dtStats.lakStats["unknownInDmgTaken",%c] = %client.unknownInDmgTaken;
   %client.dtStats.lakStats["cgShotsFired",%c] = %client.cgShotsFired;
   %client.dtStats.lakStats["discShotsFired",%c] = %client.discShotsFired;
   %client.dtStats.lakStats["grenadeShotsFired",%c] = %client.grenadeShotsFired;
   %client.dtStats.lakStats["laserShotsFired",%c] = %client.laserShotsFired;
   %client.dtStats.lakStats["mortarShotsFired",%c] = %client.mortarShotsFired;
   %client.dtStats.lakStats["missileShotsFired",%c] = %client.missileShotsFired;
   %client.dtStats.lakStats["shockLanceShotsFired",%c] = %client.shockLanceShotsFired;
   %client.dtStats.lakStats["plasmaShotsFired",%c] = %client.plasmaShotsFired;
   %client.dtStats.lakStats["blasterShotsFired",%c] = %client.blasterShotsFired;
   %client.dtStats.lakStats["elfShotsFired",%c] = %client.elfShotsFired;
   %client.dtStats.lakStats["unknownShotsFired",%c] = %client.unknownShotsFired;
}
function resLakStats(%client) {// restore
   if($dtStats::Enable  == 0){return;}
   %c = "b";
   %client.score = %client.dtStats.lakStats["score",%c];
   %client.kills = %client.dtStats.lakStats["kills",%c];
   %client.deaths = %client.dtStats.lakStats["deaths",%c];
   %client.suicides = %client.dtStats.lakStats["suicides",%c];
   %client.flagGrabs = %client.dtStats.lakStats["flagGrabs",%c];
   %client.flagTimeMS = %client.dtStats.lakStats["flagTimeMS",%c];
   %client.morepoints = %client.dtStats.lakStats["morepoints",%c];
   %client.mas = %client.dtStats.lakStats["mas",%c];
   %client.totalSpeed = %client.dtStats.lakStats["totalSpeed",%c];
   %client.totalDistance = %client.dtStats.lakStats["totalDistance",%c];
   %client.totalChainAccuracy = %client.dtStats.lakStats["totalChainAccuracy",%c];
   %client.totalChainHits = %client.dtStats.lakStats["totalChainHits",%c];
   %client.totalSnipeHits = %client.dtStats.lakStats["totalSnipeHits",%c];
   %client.totalSnipes = %client.dtStats.lakStats["totalSnipes",%c];
   %client.totalShockHits = %client.dtStats.lakStats["totalShockHits",%c];
   %client.totalShocks = %client.dtStats.lakStats["totalShocks",%c];
   
   %client.minePlusDisc = %client.dtStats.lakStats["minePlusDisc",%c];
   
   %client.cgKills = %client.dtStats.lakStats["cgKills",%c];
   %client.cgDeaths = %client.dtStats.lakStats["cgDeaths",%c];
   %client.discKills = %client.dtStats.lakStats["discKills",%c];
   %client.discDeaths = %client.dtStats.lakStats["discDeaths",%c];
   %client.grenadeKills = %client.dtStats.lakStats["grenadeKills",%c];
   %client.grenadeDeaths = %client.dtStats.lakStats["grenadeDeaths",%c];
   %client.laserKills = %client.dtStats.lakStats["laserKills",%c];
   %client.laserDeaths = %client.dtStats.lakStats["laserDeaths",%c];
   %client.mortarKills = %client.dtStats.lakStats["mortarKills",%c];
   %client.mortarDeaths = %client.dtStats.lakStats["mortarDeaths",%c];
   %client.missileKills = %client.dtStats.lakStats["missileKills",%c];
   %client.missileDeaths = %client.dtStats.lakStats["missileDeaths",%c];
   %client.shockLanceKills = %client.dtStats.lakStats["shockLanceKills",%c];
   %client.shockLanceDeaths = %client.dtStats.lakStats["shockLanceDeaths",%c];
   %client.plasmaKills = %client.dtStats.lakStats["plasmaKills",%c];
   %client.plasmaDeaths = %client.dtStats.lakStats["plasmaDeaths",%c];
   %client.blasterKills = %client.dtStats.lakStats["blasterKills",%c];
   %client.blasterDeaths = %client.dtStats.lakStats["blasterDeaths",%c];
   %client.elfKills = %client.dtStats.lakStats["elfKills",%c];
   %client.elfDeaths = %client.dtStats.lakStats["elfDeaths",%c];
   %client.mineKills = %client.dtStats.lakStats["mineKills",%c];
   %client.mineDeaths = %client.dtStats.lakStats["mineDeaths",%c];
   %client.explosionKills = %client.dtStats.lakStats["explosionKills",%c];
   %client.explosionDeaths = %client.dtStats.lakStats["explosionDeaths",%c];
   %client.impactKills = %client.dtStats.lakStats["impactKills",%c];
   %client.impactDeaths = %client.dtStats.lakStats["impactDeaths",%c];
   %client.groundKills = %client.dtStats.lakStats["groundKills",%c];
   %client.groundDeaths = %client.dtStats.lakStats["groundDeaths",%c];
   
   %client.outOfBoundKills = %client.dtStats.lakStats["outOfBoundKills",%c];
   %client.outOfBoundDeaths = %client.dtStats.lakStats["outOfBoundDeaths",%c];
   %client.lavaKills= %client.dtStats.lakStats["lavaKills",%c];
   %client.lavaDeaths = %client.dtStats.lakStats["lavaDeaths",%c];
   
   %client.satchelChargeKills = %client.dtStats.lakStats["satchelChargeKills",%c];
   %client.satchelChargeDeaths = %client.dtStats.lakStats["satchelChargeDeaths",%c];
   
   %client.lightningKills = %client.dtStats.lakStats["lightningKills",%c];
   %client.lightningDeaths = %client.dtStats.lakStats["lightningDeaths",%c];
   
   %client.forceFieldPowerUpKills = %client.dtStats.lakStats["forceFieldPowerUpKills",%c];
   %client.forceFieldPowerUpDeaths = %client.dtStats.lakStats["forceFieldPowerUpDeaths",%c];
   
   %client.waterKills = %client.dtStats.lakStats["waterKills",%c];
   %client.waterDeaths = %client.dtStats.lakStats["waterDeaths",%c];
   %client.nexusCampingKills = %client.dtStats.lakStats["nexusCampingKills",%c];
   %client.nexusCampingDeaths = %client.dtStats.lakStats["nexusCampingDeaths",%c];
   %client.unknownKill = %client.dtStats.lakStats["unknownKill",%c];
   %client.unknownDeaths = %client.dtStats.lakStats["unknownDeaths",%c];
   
   %client.cgDmg = %client.dtStats.lakStats["cgDmg",%c];
   %client.cgDirectHits = %client.dtStats.lakStats["cgDirectHits",%c];
   %client.cgDmgTaken = %client.dtStats.lakStats["cgDmgTaken",%c];
   %client.discDmg = %client.dtStats.lakStats["discDmg",%c];
   %client.discDirectHits = %client.dtStats.lakStats["discDirectHits",%c];
   %client.discDmgTaken = %client.dtStats.lakStats["discDmgTaken",%c];
   %client.grenadeDmg = %client.dtStats.lakStats["grenadeDmg",%c];
   %client.grenadeDirectHits = %client.dtStats.lakStats["grenadeDirectHits",%c];
   %client.grenadeDmgTaken = %client.dtStats.lakStats["grenadeDmgTaken",%c];
   %client.laserDmg = %client.dtStats.lakStats["laserDmg",%c];
   %client.laserDirectHits = %client.dtStats.lakStats["laserDirectHits",%c];
   %client.laserDmgTaken = %client.dtStats.lakStats["laserDmgTaken",%c];
   %client.mortarDmg = %client.dtStats.lakStats["mortarDmg",%c];
   %client.mortarDirectHits = %client.dtStats.lakStats["mortarDirectHits",%c];
   %client.mortarDmgTaken = %client.dtStats.lakStats["mortarDmgTaken",%c];
   %client.missileDmg = %client.dtStats.lakStats["missileDmg",%c];
   %client.missileDirectHits = %client.dtStats.lakStats["missileDirectHits",%c];
   %client.missileDmgTaken = %client.dtStats.lakStats["missileDmgTaken",%c];
   %client.shockLanceDmg = %client.dtStats.lakStats["shockLanceDmg",%c];
   %client.shockLanceDirectHits = %client.dtStats.lakStats["shockLanceDirectHits",%c];
   %client.shockLanceDmgTaken = %client.dtStats.lakStats["shockLanceDmgTaken",%c];
   %client.plasmaDmg = %client.dtStats.lakStats["plasmaDmg",%c];
   %client.plasmaDirectHits = %client.dtStats.lakStats["plasmaDirectHits",%c];
   %client.plasmaDmgTaken = %client.dtStats.lakStats["plasmaDmgTaken",%c];
   %client.blasterDmg = %client.dtStats.lakStats["blasterDmg",%c];
   %client.blasterDirectHits = %client.dtStats.lakStats["blasterDirectHits",%c];
   %client.blasterDmgTaken = %client.dtStats.lakStats["blasterDmgTaken",%c];
   %client.elfDmg = %client.dtStats.lakStats["elfDmg",%c];
   %client.elfDirectHits = %client.dtStats.lakStats["elfDirectHits",%c];
   %client.elfDmgTaken = %client.dtStats.lakStats["elfDmgTaken",%c];
   %client.unknownDmg = %client.dtStats.lakStats["unknownDmg",%c];
   %client.unknownDirectHits = %client.dtStats.lakStats["unknownDirectHits",%c];
   %client.unknownDmgTaken = %client.dtStats.lakStats["unknownDmgTaken",%c];
   %client.cgInDmg = %client.dtStats.lakStats["cgInDmg",%c];
   %client.cgIndirectHits = %client.dtStats.lakStats["cgIndirectHits",%c];
   %client.cgInDmgTaken = %client.dtStats.lakStats["cgInDmgTaken",%c];
   %client.discInDmg = %client.dtStats.lakStats["discInDmg",%c];
   %client.discIndirectHits = %client.dtStats.lakStats["discIndirectHits",%c];
   %client.discInDmgTaken = %client.dtStats.lakStats["discInDmgTaken",%c];
   %client.grenadeInDmg = %client.dtStats.lakStats["grenadeInDmg",%c];
   %client.grenadeIndirectHits = %client.dtStats.lakStats["grenadeIndirectHits",%c];
   %client.grenadeInDmgTaken = %client.dtStats.lakStats["grenadeInDmgTaken",%c];
   %client.laserInDmg = %client.dtStats.lakStats["laserInDmg",%c];
   %client.laserIndirectHits = %client.dtStats.lakStats["laserIndirectHits",%c];
   %client.laserInDmgTaken = %client.dtStats.lakStats["laserInDmgTaken",%c];
   %client.mortarInDmg = %client.dtStats.lakStats["mortarInDmg",%c];
   %client.mortarIndirectHits = %client.dtStats.lakStats["mortarIndirectHits",%c];
   %client.mortarInDmgTaken = %client.dtStats.lakStats["mortarInDmgTaken",%c];
   %client.missileInDmg = %client.dtStats.lakStats["missileInDmg",%c];
   %client.missileIndirectHits = %client.dtStats.lakStats["missileIndirectHits",%c];
   %client.missileInDmgTaken = %client.dtStats.lakStats["missileInDmgTaken",%c];
   %client.shockLanceInDmg = %client.dtStats.lakStats["shockLanceInDmg",%c];
   %client.shockLanceIndirectHits = %client.dtStats.lakStats["shockLanceIndirectHits",%c];
   %client.shockLanceInDmgTaken = %client.dtStats.lakStats["shockLanceInDmgTaken",%c];
   %client.plasmaInDmg = %client.dtStats.lakStats["plasmaInDmg",%c];
   %client.plasmaIndirectHits = %client.dtStats.lakStats["plasmaIndirectHits",%c];
   %client.plasmaInDmgTaken = %client.dtStats.lakStats["plasmaInDmgTaken",%c];
   %client.blasterInDmg = %client.dtStats.lakStats["blasterInDmg",%c];
   %client.blasterIndirectHits = %client.dtStats.lakStats["blasterIndirectHits",%c];
   %client.blasterInDmgTaken = %client.dtStats.lakStats["blasterInDmgTaken",%c];
   %client.elfInDmg = %client.dtStats.lakStats["elfInDmg",%c];
   %client.elfIndirectHits = %client.dtStats.lakStats["elfIndirectHits",%c];
   %client.elfInDmgTaken = %client.dtStats.lakStats["elfInDmgTaken",%c];
   %client.unknownInDmg = %client.dtStats.lakStats["unknownInDmg",%c];
   %client.unknownIndirectHits = %client.dtStats.lakStats["unknownIndirectHits",%c];
   %client.unknownInDmgTaken = %client.dtStats.lakStats["unknownInDmgTaken",%c];
   %client.cgShotsFired = %client.dtStats.lakStats["cgShotsFired",%c];
   %client.discShotsFired = %client.dtStats.lakStats["discShotsFired",%c];
   %client.grenadeShotsFired = %client.dtStats.lakStats["grenadeShotsFired",%c];
   %client.laserShotsFired = %client.dtStats.lakStats["laserShotsFired",%c];
   %client.mortarShotsFired = %client.dtStats.lakStats["mortarShotsFired",%c];
   %client.missileShotsFired = %client.dtStats.lakStats["missileShotsFired",%c];
   %client.shockLanceShotsFired = %client.dtStats.lakStats["shockLanceShotsFired",%c];
   %client.plasmaShotsFired = %client.dtStats.lakStats["plasmaShotsFired",%c];
   %client.blasterShotsFired = %client.dtStats.lakStats["blasterShotsFired",%c];
   %client.elfShotsFired = %client.dtStats.lakStats["elfShotsFired",%c];
   %client.unknownShotsFired = %client.dtStats.lakStats["unknownShotsFired",%c];
}
function incBakLakStats(%dtStats) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   if(%dtStats.lakGameCount  >= $dtStats::MaxNumOfGames){ // we have the max number allowed
      if(%dtStats.lakStatsOverWrite < $dtStats::MaxNumOfGames){
         %c = %dtStats.lakStatsOverWrite;
         %dtStats.lakStatsOverWrite++;
      }
      else{
         %dtStats.lakStatsOverWrite = 1; //reset
         %c = %dtStats.lakStatsOverWrite;
         %dtStats.lakStatsOverWrite++;
      }
   }
   else{
      %c = %dtStats.lakGameCount++; // number of games this player has played
   }
   %dtStats.lakStats["timeStamp",%c] = formattimestring("hh:nn a, mm-dd-yy");
   %dtStats.lakStats["score",%c] = %dtStats.lakStats["score","b"];
   %dtStats.lakStats["kills",%c] = %dtStats.lakStats["kills","b"];
   %dtStats.lakStats["deaths",%c] = %dtStats.lakStats["deaths","b"];
   %dtStats.lakStats["suicides",%c] = %dtStats.lakStats["suicides","b"];
   %dtStats.lakStats["flagGrabs",%c] = %dtStats.lakStats["flagGrabs","b"];
   %dtStats.lakStats["flagTimeMS",%c] = (%dtStats.lakStats["flagTimeMS","b"] / 1000) / 60;//convert to min
   %dtStats.lakStats["morepoints",%c] = %dtStats.lakStats["morepoints","b"];
   %dtStats.lakStats["mas",%c] = %dtStats.lakStats["mas","b"];
   %dtStats.lakStats["totalSpeed",%c] =  %client.lakStats["totalSpeed","b"];
   %dtStats.lakStats["totalDistance",%c] = %dtStats.lakStats["totalDistance","b"];
   %dtStats.lakStats["totalChainAccuracy",%c] = %dtStats.lakStats["totalChainAccuracy","b"];
   %dtStats.lakStats["totalChainHits",%c] = %dtStats.lakStats["totalChainHits","b"];
   %dtStats.lakStats["totalSnipeHits",%c] = %dtStats.lakStats["totalSnipeHits","b"];
   %dtStats.lakStats["totalSnipes",%c] = %dtStats.lakStats["totalSnipes","b"];
   %dtStats.lakStats["totalShockHits",%c] = %dtStats.lakStats["totalShockHits","b"];
   %dtStats.lakStats["totalShocks",%c] = %dtStats.lakStats["totalShocks","b"];
   
   %dtStats.lakStats["minePlusDisc",%c] = %dtStats.lakStats["minePlusDisc","b"];
   
   %dtStats.lakStats["cgKills",%c] = %dtStats.lakStats["cgKills","b"];
   %dtStats.lakStats["cgDeaths",%c] = %dtStats.lakStats["cgDeaths","b"];
   %dtStats.lakStats["discKills",%c] = %dtStats.lakStats["discKills","b"];
   %dtStats.lakStats["discDeaths",%c] = %dtStats.lakStats["discDeaths","b"];
   %dtStats.lakStats["grenadeKills",%c] = %dtStats.lakStats["grenadeKills","b"];
   %dtStats.lakStats["grenadeDeaths",%c] = %dtStats.lakStats["grenadeDeaths","b"];
   %dtStats.lakStats["laserKills",%c] = %dtStats.lakStats["laserKills","b"];
   %dtStats.lakStats["laserDeaths",%c] = %dtStats.lakStats["laserDeaths","b"];
   %dtStats.lakStats["mortarKills",%c] = %dtStats.lakStats["mortarKills","b"];
   %dtStats.lakStats["mortarDeaths",%c] = %dtStats.lakStats["mortarDeaths","b"];
   %dtStats.lakStats["missileKills",%c] = %dtStats.lakStats["missileKills","b"];
   %dtStats.lakStats["missileDeaths",%c] = %dtStats.lakStats["missileDeaths","b"];
   %dtStats.lakStats["shockLanceKills",%c] = %dtStats.lakStats["shockLanceKills","b"];
   %dtStats.lakStats["shockLanceDeaths",%c] = %dtStats.lakStats["shockLanceDeaths","b"];
   %dtStats.lakStats["plasmaKills",%c] = %dtStats.lakStats["plasmaKills","b"];
   %dtStats.lakStats["plasmaDeaths",%c] = %dtStats.lakStats["plasmaDeaths","b"];
   %dtStats.lakStats["blasterKills",%c] = %dtStats.lakStats["blasterKills","b"];
   %dtStats.lakStats["blasterDeaths",%c] = %dtStats.lakStats["blasterDeaths","b"];
   %dtStats.lakStats["elfKills",%c] = %dtStats.lakStats["elfKills","b"];
   %dtStats.lakStats["elfDeaths",%c] = %dtStats.lakStats["elfDeaths","b"];
   %dtStats.lakStats["mineKills",%c] = %dtStats.lakStats["mineKills","b"];
   %dtStats.lakStats["mineDeaths",%c] = %dtStats.lakStats["mineDeaths","b"];
   %dtStats.lakStats["explosionKills",%c] = %dtStats.lakStats["explosionKills","b"];
   %dtStats.lakStats["explosionDeaths",%c] = %dtStats.lakStats["explosionDeaths","b"];
   %dtStats.lakStats["impactKills",%c] =  %dtStats.lakStats["impactKills","b"];
   %dtStats.lakStats["impactDeaths",%c] = %dtStats.lakStats["impactDeaths","b"];
   %dtStats.lakStats["groundKills",%c] = %dtStats.lakStats["groundKills","b"];
   %dtStats.lakStats["groundDeaths",%c] = %dtStats.lakStats["groundDeaths","b"];
   
   %dtStats.lakStats["outOfBoundKills",%c] = %dtStats.lakStats["outOfBoundKills","b"];
   %dtStats.lakStats["outOfBoundDeaths",%c] = %dtStats.lakStats["outOfBoundDeaths","b"];
   %dtStats.lakStats["lavaKills",%c] = %dtStats.lakStats["lavaKills","b"];
   %dtStats.lakStats["lavaDeaths",%c] = %dtStats.lakStats["lavaDeaths","b"];
   
   %dtStats.lakStats["satchelChargeKills",%c] = %dtStats.lakStats["satchelChargeKills","b"];
   %dtStats.lakStats["satchelChargeDeaths",%c] = %dtStats.lakStats["satchelChargeDeaths","b"];
   
   %dtStats.lakStats["lightningKills",%c] = %dtStats.lakStats["lightningKills","b"];
   %dtStats.lakStats["lightningDeaths",%c] = %dtStats.lakStats["lightningDeaths","b"];
   
   %dtStats.lakStats["forceFieldPowerUpKills",%c] = %dtStats.lakStats["forceFieldPowerUpKills","b"];
   %dtStats.lakStats["forceFieldPowerUpDeaths",%c] = %dtStats.lakStats["forceFieldPowerUpDeaths","b"];
   
   %dtStats.lakStats["waterKills",%c] = %dtStats.lakStats["waterKills","b"];
   %dtStats.lakStats["waterDeaths",%c] = %dtStats.lakStats["waterDeaths","b"];
   %dtStats.lakStats["nexusCampingKills",%c] = %dtStats.lakStats["nexusCampingKills","b"];
   %dtStats.lakStats["nexusCampingDeaths",%c] = %dtStats.lakStats["nexusCampingDeaths","b"];
   %dtStats.lakStats["unknownKill",%c] = %dtStats.lakStats["unknownKill","b"];
   %dtStats.lakStats["unknownDeaths",%c] = %dtStats.lakStats["unknownDeaths","b"];
   
   %dtStats.lakStats["cgDmg",%c] = %dtStats.lakStats["cgDmg","b"];
   %dtStats.lakStats["cgDirectHits",%c] = %dtStats.lakStats["cgDirectHits","b"];
   %dtStats.lakStats["cgDmgTaken",%c] = %dtStats.lakStats["cgDmgTaken","b"];
   %dtStats.lakStats["discDmg",%c] = %dtStats.lakStats["discDmg","b"];
   %dtStats.lakStats["discDirectHits",%c] = %dtStats.lakStats["discDirectHits","b"];
   %dtStats.lakStats["discDmgTaken",%c] = %dtStats.lakStats["discDmgTaken","b"];
   %dtStats.lakStats["grenadeDmg",%c] = %dtStats.lakStats["grenadeDmg","b"];
   %dtStats.lakStats["grenadeDirectHits",%c] = %dtStats.lakStats["grenadeDirectHits","b"];
   %dtStats.lakStats["grenadeDmgTaken",%c] = %dtStats.lakStats["grenadeDmgTaken","b"];
   %dtStats.lakStats["laserDmg",%c] = %dtStats.lakStats["laserDmg","b"];
   %dtStats.lakStats["laserDirectHits",%c] = %dtStats.lakStats["laserDirectHits","b"];
   %dtStats.lakStats["laserDmgTaken",%c] = %dtStats.lakStats["laserDmgTaken","b"];
   %dtStats.lakStats["mortarDmg",%c] = %dtStats.lakStats["mortarDmg","b"];
   %dtStats.lakStats["mortarDirectHits",%c] = %dtStats.lakStats["mortarDirectHits","b"];
   %dtStats.lakStats["mortarDmgTaken",%c] = %dtStats.lakStats["mortarDmgTaken","b"];
   %dtStats.lakStats["missileDmg",%c] = %dtStats.lakStats["missileDmg","b"];
   %dtStats.lakStats["missileDirectHits",%c] = %dtStats.lakStats["missileDirectHits","b"];
   %dtStats.lakStats["missileDmgTaken",%c] = %dtStats.lakStats["missileDmgTaken","b"];
   %dtStats.lakStats["shockLanceDmg",%c] = %dtStats.lakStats["shockLanceDmg","b"];
   %dtStats.lakStats["shockLanceDirectHits",%c] = %dtStats.lakStats["shockLanceDirectHits","b"];
   %dtStats.lakStats["shockLanceDmgTaken",%c] = %dtStats.lakStats["shockLanceDmgTaken","b"];
   %dtStats.lakStats["plasmaDmg",%c] = %dtStats.lakStats["plasmaDmg","b"];
   %dtStats.lakStats["plasmaDirectHits",%c] = %dtStats.lakStats["plasmaDirectHits","b"];
   %dtStats.lakStats["plasmaDmgTaken",%c] = %dtStats.lakStats["plasmaDmgTaken","b"];
   %dtStats.lakStats["blasterDmg",%c] = %dtStats.lakStats["blasterDmg","b"];
   %dtStats.lakStats["blasterDirectHits",%c] = %dtStats.lakStats["blasterDirectHits","b"];
   %dtStats.lakStats["blasterDmgTaken",%c] = %dtStats.lakStats["blasterDmgTaken","b"];
   %dtStats.lakStats["elfDmg",%c] = %dtStats.lakStats["elfDmg","b"];
   %dtStats.lakStats["elfDirectHits",%c] = %dtStats.lakStats["elfDirectHits","b"];
   %dtStats.lakStats["elfDmgTaken",%c] = %dtStats.lakStats["elfDmgTaken","b"];
   %dtStats.lakStats["unknownDmg",%c] = %dtStats.lakStats["unknownDmg","b"];
   %dtStats.lakStats["unknownDirectHits",%c] = %dtStats.lakStats["unknownDirectHits","b"];
   %dtStats.lakStats["unknownDmgTaken",%c] = %dtStats.lakStats["unknownDmgTaken","b"];
   %dtStats.lakStats["cgInDmg",%c] = %dtStats.lakStats["cgInDmg","b"];
   %dtStats.lakStats["cgIndirectHits",%c] = %dtStats.lakStats["cgIndirectHits","b"];
   %dtStats.lakStats["cgInDmgTaken",%c] = %dtStats.lakStats["cgInDmgTaken","b"];
   %dtStats.lakStats["discInDmg",%c] = %dtStats.lakStats["discInDmg","b"];
   %dtStats.lakStats["discIndirectHits",%c] = %dtStats.lakStats["discIndirectHits","b"];
   %dtStats.lakStats["discInDmgTaken",%c] = %dtStats.lakStats["discInDmgTaken","b"];
   %dtStats.lakStats["grenadeInDmg",%c] = %dtStats.lakStats["grenadeInDmg","b"];
   %dtStats.lakStats["grenadeIndirectHits",%c] = %dtStats.lakStats["grenadeIndirectHits","b"];
   %dtStats.lakStats["grenadeInDmgTaken",%c] = %dtStats.lakStats["grenadeInDmgTaken","b"];
   %dtStats.lakStats["laserInDmg",%c] = %dtStats.lakStats["laserInDmg","b"];
   %dtStats.lakStats["laserIndirectHits",%c] = %dtStats.lakStats["laserIndirectHits","b"];
   %dtStats.lakStats["laserInDmgTaken",%c] = %dtStats.lakStats["laserInDmgTaken","b"];
   %dtStats.lakStats["mortarInDmg",%c] = %dtStats.lakStats["mortarInDmg","b"];
   %dtStats.lakStats["mortarIndirectHits",%c] = %dtStats.lakStats["mortarIndirectHits","b"];
   %dtStats.lakStats["mortarInDmgTaken",%c] = %dtStats.lakStats["mortarInDmgTaken","b"];
   %dtStats.lakStats["missileInDmg",%c] = %dtStats.lakStats["missileInDmg","b"];
   %dtStats.lakStats["missileIndirectHits",%c] = %dtStats.lakStats["missileIndirectHits","b"];
   %dtStats.lakStats["missileInDmgTaken",%c] = %dtStats.lakStats["missileInDmgTaken","b"];
   %dtStats.lakStats["shockLanceInDmg",%c] = %dtStats.lakStats["shockLanceInDmg","b"];
   %dtStats.lakStats["shockLanceIndirectHits",%c] = %dtStats.lakStats["shockLanceIndirectHits","b"];
   %dtStats.lakStats["shockLanceInDmgTaken",%c] = %dtStats.lakStats["shockLanceInDmgTaken","b"];
   %dtStats.lakStats["plasmaInDmg",%c] = %dtStats.lakStats["plasmaInDmg","b"];
   %dtStats.lakStats["plasmaIndirectHits",%c] = %dtStats.lakStats["plasmaIndirectHits","b"];
   %dtStats.lakStats["plasmaInDmgTaken",%c] = %dtStats.lakStats["plasmaInDmgTaken","b"];
   %dtStats.lakStats["blasterInDmg",%c] = %dtStats.lakStats["blasterInDmg","b"];
   %dtStats.lakStats["blasterIndirectHits",%c] = %dtStats.lakStats["blasterIndirectHits","b"];
   %dtStats.lakStats["blasterInDmgTaken",%c] = %dtStats.lakStats["blasterInDmgTaken","b"];
   %dtStats.lakStats["elfInDmg",%c] = %dtStats.lakStats["elfInDmg","b"];
   %dtStats.lakStats["elfIndirectHits",%c] = %dtStats.lakStats["elfIndirectHits","b"];
   %dtStats.lakStats["elfInDmgTaken",%c] = %dtStats.lakStats["elfInDmgTaken","b"];
   %dtStats.lakStats["unknownInDmg",%c] = %dtStats.lakStats["unknownInDmg","b"];
   %dtStats.lakStats["unknownIndirectHits",%c] = %dtStats.lakStats["unknownIndirectHits","b"];
   %dtStats.lakStats["unknownInDmgTaken",%c] = %dtStats.lakStats["unknownInDmgTaken","b"];
   %dtStats.lakStats["cgShotsFired",%c] = %dtStats.lakStats["cgShotsFired","b"];
   %dtStats.lakStats["discShotsFired",%c] = %dtStats.lakStats["discShotsFired","b"];
   %dtStats.lakStats["grenadeShotsFired",%c] = %dtStats.lakStats["grenadeShotsFired","b"];
   %dtStats.lakStats["laserShotsFired",%c] = %dtStats.lakStats["laserShotsFired","b"];
   %dtStats.lakStats["mortarShotsFired",%c] = %dtStats.lakStats["mortarShotsFired","b"];
   %dtStats.lakStats["missileShotsFired",%c] = %dtStats.lakStats["missileShotsFired","b"];
   %dtStats.lakStats["shockLanceShotsFired",%c] = %dtStats.lakStats["shockLanceShotsFired","b"];
   %dtStats.lakStats["plasmaShotsFired",%c] = %dtStats.lakStats["plasmaShotsFired","b"];
   %dtStats.lakStats["blasterShotsFired",%c] = %dtStats.lakStats["blasterShotsFired","b"];
   %dtStats.lakStats["elfShotsFired",%c] = %dtStats.lakStats["elfShotsFired","b"];
   %dtStats.lakStats["unknownShotsFired",%c] = %dtStats.lakStats["unknownShotsFired","b"];
   addBakLAKTotal(%dtStats); // add totals
}
function addBakLAKTotal(%dtStats) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %dtStats.lakTotalNumGames++;
   %c = "t";
   %dtStats.lakStats["timeStamp",%c] = formattimestring("hh:nn a, mm-dd-yy");
   %dtStats.lakStats["score",%c] += %dtStats.lakStats["score","b"];
   %dtStats.lakStats["kills",%c] += %dtStats.lakStats["kills","b"];
   %dtStats.lakStats["deaths",%c] += %dtStats.lakStats["deaths","b"];
   %dtStats.lakStats["suicides",%c] += %dtStats.lakStats["suicides","b"];
   %dtStats.lakStats["flagGrabs",%c] += %dtStats.lakStats["flagGrabs","b"];
   %dtStats.lakStats["flagTimeMS",%c] += (%dtStats.lakStats["flagTimeMS","b"] / 1000) / 60; // convert to min
   %dtStats.lakStats["morepoints",%c] += %dtStats.lakStats["morepoints","b"];
   %dtStats.lakStats["mas",%c] += %dtStats.lakStats["mas","b"];
   %dtStats.lakStats["totalSpeed",%c] +=  %client.lakStats["totalSpeed","b"];
   %dtStats.lakStats["totalDistance",%c] += %dtStats.lakStats["totalDistance","b"];
   %dtStats.lakStats["totalChainAccuracy",%c] += %dtStats.lakStats["totalChainAccuracy","b"];
   %dtStats.lakStats["totalChainHits",%c] += %dtStats.lakStats["totalChainHits","b"];
   %dtStats.lakStats["totalSnipeHits",%c] += %dtStats.lakStats["totalSnipeHits","b"];
   %dtStats.lakStats["totalSnipes",%c] += %dtStats.lakStats["totalSnipes","b"];
   %dtStats.lakStats["totalShockHits",%c] += %dtStats.lakStats["totalShockHits","b"];
   %dtStats.lakStats["totalShocks",%c] += %dtStats.lakStats["totalShocks","b"];
   
   %dtStats.lakStats["minePlusDisc",%c] += %dtStats.lakStats["minePlusDisc","b"];
   
   %dtStats.lakStats["cgKills",%c] += %dtStats.lakStats["cgKills","b"];
   %dtStats.lakStats["cgDeaths",%c] += %dtStats.lakStats["cgDeaths","b"];
   %dtStats.lakStats["discKills",%c] += %dtStats.lakStats["discKills","b"];
   %dtStats.lakStats["discDeaths",%c] += %dtStats.lakStats["discDeaths","b"];
   %dtStats.lakStats["grenadeKills",%c] += %dtStats.lakStats["grenadeKills","b"];
   %dtStats.lakStats["grenadeDeaths",%c] += %dtStats.lakStats["grenadeDeaths","b"];
   %dtStats.lakStats["laserKills",%c] += %dtStats.lakStats["laserKills","b"];
   %dtStats.lakStats["laserDeaths",%c] += %dtStats.lakStats["laserDeaths","b"];
   %dtStats.lakStats["mortarKills",%c] += %dtStats.lakStats["mortarKills","b"];
   %dtStats.lakStats["mortarDeaths",%c] += %dtStats.lakStats["mortarDeaths","b"];
   %dtStats.lakStats["missileKills",%c] += %dtStats.lakStats["missileKills","b"];
   %dtStats.lakStats["missileDeaths",%c] += %dtStats.lakStats["missileDeaths","b"];
   %dtStats.lakStats["shockLanceKills",%c] += %dtStats.lakStats["shockLanceKills","b"];
   %dtStats.lakStats["shockLanceDeaths",%c] += %dtStats.lakStats["shockLanceDeaths","b"];
   %dtStats.lakStats["plasmaKills",%c] += %dtStats.lakStats["plasmaKills","b"];
   %dtStats.lakStats["plasmaDeaths",%c] += %dtStats.lakStats["plasmaDeaths","b"];
   %dtStats.lakStats["blasterKills",%c] += %dtStats.lakStats["blasterKills","b"];
   %dtStats.lakStats["blasterDeaths",%c] += %dtStats.lakStats["blasterDeaths","b"];
   %dtStats.lakStats["elfKills",%c] += %dtStats.lakStats["elfKills","b"];
   %dtStats.lakStats["elfDeaths",%c] += %dtStats.lakStats["elfDeaths","b"];
   %dtStats.lakStats["mineKills",%c] += %dtStats.lakStats["mineKills","b"];
   %dtStats.lakStats["mineDeaths",%c] += %dtStats.lakStats["mineDeaths","b"];
   %dtStats.lakStats["explosionKills",%c] += %dtStats.lakStats["explosionKills","b"];
   %dtStats.lakStats["explosionDeaths",%c] += %dtStats.lakStats["explosionDeaths","b"];
   %dtStats.lakStats["impactKills",%c] +=  %dtStats.lakStats["impactKills","b"];
   %dtStats.lakStats["impactDeaths",%c] += %dtStats.lakStats["impactDeaths","b"];
   %dtStats.lakStats["groundKills",%c] += %dtStats.lakStats["groundKills","b"];
   %dtStats.lakStats["groundDeaths",%c] += %dtStats.lakStats["groundDeaths","b"];
   
   %dtStats.lakStats["outOfBoundKills",%c] += %dtStats.lakStats["outOfBoundKills","b"];
   %dtStats.lakStats["outOfBoundDeaths",%c] += %dtStats.lakStats["outOfBoundDeaths","b"];
   %dtStats.lakStats["lavaKills",%c] += %dtStats.lakStats["lavaKills","b"];
   %dtStats.lakStats["lavaDeaths",%c] += %dtStats.lakStats["lavaDeaths","b"];
   
   %dtStats.lakStats["satchelChargeKills",%c] += %dtStats.lakStats["satchelChargeKills","b"];
   %dtStats.lakStats["satchelChargeDeaths",%c] += %dtStats.lakStats["satchelChargeDeaths","b"];
   
   %dtStats.lakStats["lightningKills",%c] += %dtStats.lakStats["lightningKills","b"];
   %dtStats.lakStats["lightningDeaths",%c] += %dtStats.lakStats["lightningDeaths","b"];
   
   %dtStats.lakStats["forceFieldPowerUpKills",%c] += %dtStats.lakStats["forceFieldPowerUpKills","b"];
   %dtStats.lakStats["forceFieldPowerUpDeaths",%c] += %dtStats.lakStats["forceFieldPowerUpDeaths","b"];
   
   %dtStats.lakStats["waterKills",%c] += %dtStats.lakStats["waterKills","b"];
   %dtStats.lakStats["waterDeaths",%c] += %dtStats.lakStats["waterDeaths","b"];
   %dtStats.lakStats["nexusCampingKills",%c] += %dtStats.lakStats["nexusCampingKills","b"];
   %dtStats.lakStats["nexusCampingDeaths",%c] += %dtStats.lakStats["nexusCampingDeaths","b"];
   %dtStats.lakStats["unknownKill",%c] += %dtStats.lakStats["unknownKill","b"];
   %dtStats.lakStats["unknownDeaths",%c] += %dtStats.lakStats["unknownDeaths","b"];
   
   %dtStats.lakStats["cgDmg",%c] += %dtStats.lakStats["cgDmg","b"];
   %dtStats.lakStats["cgDirectHits",%c] += %dtStats.lakStats["cgDirectHits","b"];
   %dtStats.lakStats["cgDmgTaken",%c] += %dtStats.lakStats["cgDmgTaken","b"];
   %dtStats.lakStats["discDmg",%c] += %dtStats.lakStats["discDmg","b"];
   %dtStats.lakStats["discDirectHits",%c] += %dtStats.lakStats["discDirectHits","b"];
   %dtStats.lakStats["discDmgTaken",%c] += %dtStats.lakStats["discDmgTaken","b"];
   %dtStats.lakStats["grenadeDmg",%c] += %dtStats.lakStats["grenadeDmg","b"];
   %dtStats.lakStats["grenadeDirectHits",%c] += %dtStats.lakStats["grenadeDirectHits","b"];
   %dtStats.lakStats["grenadeDmgTaken",%c] += %dtStats.lakStats["grenadeDmgTaken","b"];
   %dtStats.lakStats["laserDmg",%c] += %dtStats.lakStats["laserDmg","b"];
   %dtStats.lakStats["laserDirectHits",%c] += %dtStats.lakStats["laserDirectHits","b"];
   %dtStats.lakStats["laserDmgTaken",%c] += %dtStats.lakStats["laserDmgTaken","b"];
   %dtStats.lakStats["mortarDmg",%c] += %dtStats.lakStats["mortarDmg","b"];
   %dtStats.lakStats["mortarDirectHits",%c] += %dtStats.lakStats["mortarDirectHits","b"];
   %dtStats.lakStats["mortarDmgTaken",%c] += %dtStats.lakStats["mortarDmgTaken","b"];
   %dtStats.lakStats["missileDmg",%c] += %dtStats.lakStats["missileDmg","b"];
   %dtStats.lakStats["missileDirectHits",%c] += %dtStats.lakStats["missileDirectHits","b"];
   %dtStats.lakStats["missileDmgTaken",%c] += %dtStats.lakStats["missileDmgTaken","b"];
   %dtStats.lakStats["shockLanceDmg",%c] += %dtStats.lakStats["shockLanceDmg","b"];
   %dtStats.lakStats["shockLanceDirectHits",%c] += %dtStats.lakStats["shockLanceDirectHits","b"];
   %dtStats.lakStats["shockLanceDmgTaken",%c] += %dtStats.lakStats["shockLanceDmgTaken","b"];
   %dtStats.lakStats["plasmaDmg",%c] += %dtStats.lakStats["plasmaDmg","b"];
   %dtStats.lakStats["plasmaDirectHits",%c] += %dtStats.lakStats["plasmaDirectHits","b"];
   %dtStats.lakStats["plasmaDmgTaken",%c] += %dtStats.lakStats["plasmaDmgTaken","b"];
   %dtStats.lakStats["blasterDmg",%c] += %dtStats.lakStats["blasterDmg","b"];
   %dtStats.lakStats["blasterDirectHits",%c] += %dtStats.lakStats["blasterDirectHits","b"];
   %dtStats.lakStats["blasterDmgTaken",%c] += %dtStats.lakStats["blasterDmgTaken","b"];
   %dtStats.lakStats["elfDmg",%c] += %dtStats.lakStats["elfDmg","b"];
   %dtStats.lakStats["elfDirectHits",%c] += %dtStats.lakStats["elfDirectHits","b"];
   %dtStats.lakStats["elfDmgTaken",%c] += %dtStats.lakStats["elfDmgTaken","b"];
   %dtStats.lakStats["unknownDmg",%c] += %dtStats.lakStats["unknownDmg","b"];
   %dtStats.lakStats["unknownDirectHits",%c] += %dtStats.lakStats["unknownDirectHits","b"];
   %dtStats.lakStats["unknownDmgTaken",%c] += %dtStats.lakStats["unknownDmgTaken","b"];
   %dtStats.lakStats["cgInDmg",%c] += %dtStats.lakStats["cgInDmg","b"];
   %dtStats.lakStats["cgIndirectHits",%c] += %dtStats.lakStats["cgIndirectHits","b"];
   %dtStats.lakStats["cgInDmgTaken",%c] += %dtStats.lakStats["cgInDmgTaken","b"];
   %dtStats.lakStats["discInDmg",%c] += %dtStats.lakStats["discInDmg","b"];
   %dtStats.lakStats["discIndirectHits",%c] += %dtStats.lakStats["discIndirectHits","b"];
   %dtStats.lakStats["discInDmgTaken",%c] += %dtStats.lakStats["discInDmgTaken","b"];
   %dtStats.lakStats["grenadeInDmg",%c] += %dtStats.lakStats["grenadeInDmg","b"];
   %dtStats.lakStats["grenadeIndirectHits",%c] += %dtStats.lakStats["grenadeIndirectHits","b"];
   %dtStats.lakStats["grenadeInDmgTaken",%c] += %dtStats.lakStats["grenadeInDmgTaken","b"];
   %dtStats.lakStats["laserInDmg",%c] += %dtStats.lakStats["laserInDmg","b"];
   %dtStats.lakStats["laserIndirectHits",%c] += %dtStats.lakStats["laserIndirectHits","b"];
   %dtStats.lakStats["laserInDmgTaken",%c] += %dtStats.lakStats["laserInDmgTaken","b"];
   %dtStats.lakStats["mortarInDmg",%c] += %dtStats.lakStats["mortarInDmg","b"];
   %dtStats.lakStats["mortarIndirectHits",%c] += %dtStats.lakStats["mortarIndirectHits","b"];
   %dtStats.lakStats["mortarInDmgTaken",%c] += %dtStats.lakStats["mortarInDmgTaken","b"];
   %dtStats.lakStats["missileInDmg",%c] += %dtStats.lakStats["missileInDmg","b"];
   %dtStats.lakStats["missileIndirectHits",%c] += %dtStats.lakStats["missileIndirectHits","b"];
   %dtStats.lakStats["missileInDmgTaken",%c] += %dtStats.lakStats["missileInDmgTaken","b"];
   %dtStats.lakStats["shockLanceInDmg",%c] += %dtStats.lakStats["shockLanceInDmg","b"];
   %dtStats.lakStats["shockLanceIndirectHits",%c] += %dtStats.lakStats["shockLanceIndirectHits","b"];
   %dtStats.lakStats["shockLanceInDmgTaken",%c] += %dtStats.lakStats["shockLanceInDmgTaken","b"];
   %dtStats.lakStats["plasmaInDmg",%c] += %dtStats.lakStats["plasmaInDmg","b"];
   %dtStats.lakStats["plasmaIndirectHits",%c] += %dtStats.lakStats["plasmaIndirectHits","b"];
   %dtStats.lakStats["plasmaInDmgTaken",%c] += %dtStats.lakStats["plasmaInDmgTaken","b"];
   %dtStats.lakStats["blasterInDmg",%c] += %dtStats.lakStats["blasterInDmg","b"];
   %dtStats.lakStats["blasterIndirectHits",%c] += %dtStats.lakStats["blasterIndirectHits","b"];
   %dtStats.lakStats["blasterInDmgTaken",%c] += %dtStats.lakStats["blasterInDmgTaken","b"];
   %dtStats.lakStats["elfInDmg",%c] += %dtStats.lakStats["elfInDmg","b"];
   %dtStats.lakStats["elfIndirectHits",%c] += %dtStats.lakStats["elfIndirectHits","b"];
   %dtStats.lakStats["elfInDmgTaken",%c] += %dtStats.lakStats["elfInDmgTaken","b"];
   %dtStats.lakStats["unknownInDmg",%c] += %dtStats.lakStats["unknownInDmg","b"];
   %dtStats.lakStats["unknownIndirectHits",%c] += %dtStats.lakStats["unknownIndirectHits","b"];
   %dtStats.lakStats["unknownInDmgTaken",%c] += %dtStats.lakStats["unknownInDmgTaken","b"];
   %dtStats.lakStats["cgShotsFired",%c] += %dtStats.lakStats["cgShotsFired","b"];
   %dtStats.lakStats["discShotsFired",%c] += %dtStats.lakStats["discShotsFired","b"];
   %dtStats.lakStats["grenadeShotsFired",%c] += %dtStats.lakStats["grenadeShotsFired","b"];
   %dtStats.lakStats["laserShotsFired",%c] += %dtStats.lakStats["laserShotsFired","b"];
   %dtStats.lakStats["mortarShotsFired",%c] += %dtStats.lakStats["mortarShotsFired","b"];
   %dtStats.lakStats["missileShotsFired",%c] += %dtStats.lakStats["missileShotsFired","b"];
   %dtStats.lakStats["shockLanceShotsFired",%c] += %dtStats.lakStats["shockLanceShotsFired","b"];
   %dtStats.lakStats["plasmaShotsFired",%c] += %dtStats.lakStats["plasmaShotsFired","b"];
   %dtStats.lakStats["blasterShotsFired",%c] += %dtStats.lakStats["blasterShotsFired","b"];
   %dtStats.lakStats["elfShotsFired",%c] += %dtStats.lakStats["elfShotsFired","b"];
   %dtStats.lakStats["unknownShotsFired",%c] += %dtStats.lakStats["unknownShotsFired","b"];
}

function addCTFTotal(%client) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %client.dtStats.ctfTotalNumGames++;
   %client.dtStats.ctfStats["timeStamp","t"] = formattimestring("hh:nn a, mm-dd-yy");
   %client.dtStats.ctfStats["kills","t"] += %client.kills;
   %client.dtStats.ctfStats["deaths","t"] += %client.deaths;
   %client.dtStats.ctfStats["suicides","t"] += %client.suicides;
   %client.dtStats.ctfStats["teamKills","t"] += %client.teamKills;
   %client.dtStats.ctfStats["flagCaps","t"] += %client.flagCaps;
   %client.dtStats.ctfStats["flagGrabs","t"] += %client.flagGrabs;
   %client.dtStats.ctfStats["carrierKills","t"] += %client.carrierKills;
   %client.dtStats.ctfStats["flagReturns","t"] +=  %client.flagReturns;
   %client.dtStats.ctfStats["score","t"] += %client.score;
   %client.dtStats.ctfStats["scoreMidAir","t"] += %client.scoreMidAir;
   %client.dtStats.ctfStats["scoreHeadshot","t"] += %client.scoreHeadshot;
   %client.dtStats.ctfStats["minePlusDisc","t"] += %client.minePlusDisc;
   
   %client.dtStats.ctfStats["scoreRearshot","t"] += %client.scoreRearshot;
   %client.dtStats.ctfStats["escortAssists","t"] += %client.escortAssists;
   %client.dtStats.ctfStats["defenseScore","t"] += %client.defenseScore;
   %client.dtStats.ctfStats["offenseScore","t"] += %client.offenseScore;
   %client.dtStats.ctfStats["flagDefends","t"] += %client.flagDefends;
   
   %client.dtStats.ctfStats["cgKills","t"] += %client.cgKills;
   %client.dtStats.ctfStats["cgDeaths","t"] += %client.cgDeaths;
   %client.dtStats.ctfStats["discKills","t"] += %client.discKills;
   %client.dtStats.ctfStats["discDeaths","t"] += %client.discDeaths;
   %client.dtStats.ctfStats["grenadeKills","t"] += %client.grenadeKills;
   %client.dtStats.ctfStats["grenadeDeaths","t"] += %client.grenadeDeaths;
   %client.dtStats.ctfStats["laserKills","t"] += %client.laserKills;
   %client.dtStats.ctfStats["laserDeaths","t"] += %client.laserDeaths;
   %client.dtStats.ctfStats["mortarKills","t"] += %client.mortarKills;
   %client.dtStats.ctfStats["mortarDeaths","t"] += %client.mortarDeaths;
   %client.dtStats.ctfStats["missileKills","t"] += %client.missileKills;
   %client.dtStats.ctfStats["missileDeaths","t"] += %client.missileDeaths;
   %client.dtStats.ctfStats["shockLanceKills","t"] += %client.shockLanceKills;
   %client.dtStats.ctfStats["shockLanceDeaths","t"] += %client.shockLanceDeaths;
   %client.dtStats.ctfStats["plasmaKills","t"] += %client.plasmaKills;
   %client.dtStats.ctfStats["plasmaDeaths","t"] += %client.plasmaDeaths;
   %client.dtStats.ctfStats["blasterKills","t"] += %client.blasterKills;
   %client.dtStats.ctfStats["blasterDeaths","t"] += %client.blasterDeaths;
   %client.dtStats.ctfStats["elfKills","t"] += %client.elfKills;
   %client.dtStats.ctfStats["elfDeaths","t"] += %client.elfDeaths;
   %client.dtStats.ctfStats["mineKills","t"] += %client.mineKills;
   %client.dtStats.ctfStats["mineDeaths","t"] += %client.mineDeaths;
   %client.dtStats.ctfStats["explosionKills","t"] += %client.explosionKills;
   %client.dtStats.ctfStats["explosionDeaths","t"] += %client.explosionDeaths;
   %client.dtStats.ctfStats["impactKills","t"] +=  %client.impactKills;
   %client.dtStats.ctfStats["impactDeaths","t"] += %client.impactDeaths;
   %client.dtStats.ctfStats["groundKills","t"] += %client.groundKills;
   %client.dtStats.ctfStats["groundDeaths","t"] += %client.groundDeaths;
   %client.dtStats.ctfStats["turretKills",%c] = %client.turretKills;
   %client.dtStats.ctfStats["turretDeaths","t"] += %client.turretDeaths;
   %client.dtStats.ctfStats["plasmaTurretKills","t"] +=  %client.plasmaTurretKills;
   %client.dtStats.ctfStats["plasmaTurretDeaths","t"] += %client.plasmaTurretDeaths;
   %client.dtStats.ctfStats["aaTurretKills","t"] += %client.aaTurretKills;
   %client.dtStats.ctfStats["aaTurretDeaths","t"] += %client.aaTurretDeaths;
   %client.dtStats.ctfStats["elfTurretKills","t"] += %client.elfTurretKills;
   %client.dtStats.ctfStats["elfTurretDeaths","t"] += %client.elfTurretDeaths;
   %client.dtStats.ctfStats["mortarTurretKills","t"] += %client.mortarTurretKills;
   %client.dtStats.ctfStats["mortarTurretDeaths","t"] += %client.mortarTurretDeaths;
   %client.dtStats.ctfStats["missileTurretKills","t"] += %client.missileTurretKills;
   %client.dtStats.ctfStats["missileTurretDeaths","t"] += %client.missileTurretDeaths;
   %client.dtStats.ctfStats["indoorDepTurretKills","t"] += %client.indoorDepTurretKills;
   %client.dtStats.ctfStats["indoorDepTurretDeaths","t"] += %client.indoorDepTurretDeaths;
   %client.dtStats.ctfStats["outdoorDepTurretKills","t"] += %client.outdoorDepTurretKills;
   %client.dtStats.ctfStats["outdoorDepTurretDeaths","t"] +=  %client.outdoorDepTurretDeaths;
   %client.dtStats.ctfStats["sentryTurretKills","t"] += %client.sentryTurretKills;
   %client.dtStats.ctfStats["sentryTurretDeaths","t"] += %client.sentryTurretDeaths;
   %client.dtStats.ctfStats["outOfBoundKills","t"] += %client.outOfBoundKills;
   %client.dtStats.ctfStats["outOfBoundDeaths","t"] += %client.outOfBoundDeaths;
   %client.dtStats.ctfStats["lavaKills","t"] += %client.lavaKills;
   %client.dtStats.ctfStats["lavaDeaths","t"] += %client.lavaDeaths;
   %client.dtStats.ctfStats["shrikeBlasterKills","t"] += %client.shrikeBlasterKills;
   %client.dtStats.ctfStats["shrikeBlasterDeaths","t"] += %client.shrikeBlasterDeaths;
   %client.dtStats.ctfStats["bellyTurretKills","t"] += %client.bellyTurretKills;
   %client.dtStats.ctfStats["bellyTurretDeaths","t"] += %client.bellyTurretDeaths;
   %client.dtStats.ctfStats["bomberBombsKills","t"] += %client.bomberBombsKills;
   %client.dtStats.ctfStats["bomberBombsDeaths","t"] += %client.bomberBombsDeaths;
   %client.dtStats.ctfStats["tankChaingunKills","t"] += %client.tankChaingunKills;
   %client.dtStats.ctfStats["tankChaingunDeaths","t"] += %client.tankChaingunDeaths;
   %client.dtStats.ctfStats["tankMortarKills","t"] += %client.tankMortarKills;
   %client.dtStats.ctfStats["tankMortarDeaths","t"] += %client.tankMortarDeaths;
   %client.dtStats.ctfStats["satchelChargeKills","t"] += %client.satchelChargeKills;
   %client.dtStats.ctfStats["satchelChargeDeaths","t"] += %client.satchelChargeDeaths;
   %client.dtStats.ctfStats["mpbMissileKills","t"] += %client.mpbMissileKills;
   %client.dtStats.ctfStats["mpbMissileDeaths","t"] += %client.mpbMissileDeaths;
   %client.dtStats.ctfStats["lightningKills","t"] += %client.lightningKills;
   %client.dtStats.ctfStats["lightningDeaths","t"] += %client.lightningDeaths;
   %client.dtStats.ctfStats["vehicleSpawnKills","t"] += %client.vehicleSpawnKills;
   %client.dtStats.ctfStats["vehicleSpawnDeaths","t"] += %client.vehicleSpawnDeaths;
   %client.dtStats.ctfStats["forceFieldPowerUpKills","t"] += %client.forceFieldPowerUpKills;
   %client.dtStats.ctfStats["forceFieldPowerUpDeaths","t"] += %client.forceFieldPowerUpDeaths;
   %client.dtStats.ctfStats["crashKills","t"] += %client.crashKills;
   %client.dtStats.ctfStats["crashDeaths","t"] += %client.crashDeaths;
   %client.dtStats.ctfStats["waterKills","t"] += %client.waterKills;
   %client.dtStats.ctfStats["waterDeaths","t"] += %client.waterDeaths;
   %client.dtStats.ctfStats["nexusCampingKills","t"] += %client.nexusCampingKills;
   %client.dtStats.ctfStats["nexusCampingDeaths","t"] += %client.nexusCampingDeaths;
   %client.dtStats.ctfStats["unknownKill","t"] += %client.unknownKill;
   %client.dtStats.ctfStats["unknownDeaths","t"] += %client.unknownDeaths;
   %client.dtStats.ctfStats["cgDmg","t"] += %client.cgDmg;
   %client.dtStats.ctfStats["cgDirectHits","t"] += %client.cgDirectHits;
   %client.dtStats.ctfStats["cgDmgTaken","t"] += %client.cgDmgTaken;
   %client.dtStats.ctfStats["discDmg","t"] += %client.discDmg;
   %client.dtStats.ctfStats["discDirectHits","t"] += %client.discDirectHits;
   %client.dtStats.ctfStats["discDmgTaken","t"] += %client.discDmgTaken;
   %client.dtStats.ctfStats["grenadeDmg","t"] += %client.grenadeDmg;
   %client.dtStats.ctfStats["grenadeDirectHits","t"] += %client.grenadeDirectHits;
   %client.dtStats.ctfStats["grenadeDmgTaken","t"] += %client.grenadeDmgTaken;
   %client.dtStats.ctfStats["laserDmg","t"] += %client.laserDmg;
   %client.dtStats.ctfStats["laserDirectHits","t"] += %client.laserDirectHits;
   %client.dtStats.ctfStats["laserDmgTaken","t"] += %client.laserDmgTaken;
   %client.dtStats.ctfStats["mortarDmg","t"] += %client.mortarDmg;
   %client.dtStats.ctfStats["mortarDirectHits","t"] += %client.mortarDirectHits;
   %client.dtStats.ctfStats["mortarDmgTaken","t"] += %client.mortarDmgTaken;
   %client.dtStats.ctfStats["missileDmg","t"] += %client.missileDmg;
   %client.dtStats.ctfStats["missileDirectHits","t"] += %client.missileDirectHits;
   %client.dtStats.ctfStats["missileDmgTaken","t"] += %client.missileDmgTaken;
   %client.dtStats.ctfStats["shockLanceDmg","t"] += %client.shockLanceDmg;
   %client.dtStats.ctfStats["shockLanceDirectHits","t"] += %client.shockLanceDirectHits;
   %client.dtStats.ctfStats["shockLanceDmgTaken","t"] += %client.shockLanceDmgTaken;
   %client.dtStats.ctfStats["plasmaDmg","t"] += %client.plasmaDmg;
   %client.dtStats.ctfStats["plasmaDirectHits","t"] += %client.plasmaDirectHits;
   %client.dtStats.ctfStats["plasmaDmgTaken","t"] += %client.plasmaDmgTaken;
   %client.dtStats.ctfStats["blasterDmg","t"] += %client.blasterDmg;
   %client.dtStats.ctfStats["blasterDirectHits","t"] += %client.blasterDirectHits;
   %client.dtStats.ctfStats["blasterDmgTaken","t"] += %client.blasterDmgTaken;
   %client.dtStats.ctfStats["elfDmg","t"] += %client.elfDmg;
   %client.dtStats.ctfStats["elfDirectHits","t"] += %client.elfDirectHits;
   %client.dtStats.ctfStats["elfDmgTaken","t"] += %client.elfDmgTaken;
   %client.dtStats.ctfStats["unknownDmg","t"] += %client.unknownDmg;
   %client.dtStats.ctfStats["unknownDirectHits","t"] += %client.unknownDirectHits;
   %client.dtStats.ctfStats["unknownDmgTaken","t"] += %client.unknownDmgTaken;
   %client.dtStats.ctfStats["cgInDmg","t"] += %client.cgInDmg;
   %client.dtStats.ctfStats["cgIndirectHits","t"] += %client.cgIndirectHits;
   %client.dtStats.ctfStats["cgInDmgTaken","t"] += %client.cgInDmgTaken;
   %client.dtStats.ctfStats["discInDmg","t"] += %client.discInDmg;
   %client.dtStats.ctfStats["discIndirectHits","t"] += %client.discIndirectHits;
   %client.dtStats.ctfStats["discInDmgTaken","t"] += %client.discInDmgTaken;
   %client.dtStats.ctfStats["grenadeInDmg","t"] += %client.grenadeInDmg;
   %client.dtStats.ctfStats["grenadeIndirectHits","t"] += %client.grenadeIndirectHits;
   %client.dtStats.ctfStats["grenadeInDmgTaken","t"] += %client.grenadeInDmgTaken;
   %client.dtStats.ctfStats["laserInDmg","t"] += %client.laserInDmg;
   %client.dtStats.ctfStats["laserIndirectHits","t"] += %client.laserIndirectHits;
   %client.dtStats.ctfStats["laserInDmgTaken","t"] += %client.laserInDmgTaken;
   %client.dtStats.ctfStats["mortarInDmg","t"] += %client.mortarInDmg;
   %client.dtStats.ctfStats["mortarIndirectHits","t"] += %client.mortarIndirectHits;
   %client.dtStats.ctfStats["mortarInDmgTaken","t"] += %client.mortarInDmgTaken;
   %client.dtStats.ctfStats["missileInDmg","t"] += %client.missileInDmg;
   %client.dtStats.ctfStats["missileIndirectHits","t"] += %client.missileIndirectHits;
   %client.dtStats.ctfStats["missileInDmgTaken","t"] += %client.missileInDmgTaken;
   %client.dtStats.ctfStats["shockLanceInDmg","t"] += %client.shockLanceInDmg;
   %client.dtStats.ctfStats["shockLanceIndirectHits","t"] += %client.shockLanceIndirectHits;
   %client.dtStats.ctfStats["shockLanceInDmgTaken","t"] += %client.shockLanceInDmgTaken;
   %client.dtStats.ctfStats["plasmaInDmg","t"] += %client.plasmaInDmg;
   %client.dtStats.ctfStats["plasmaIndirectHits","t"] += %client.plasmaIndirectHits;
   %client.dtStats.ctfStats["plasmaInDmgTaken","t"] += %client.plasmaInDmgTaken;
   %client.dtStats.ctfStats["blasterInDmg","t"] += %client.blasterInDmg;
   %client.dtStats.ctfStats["blasterIndirectHits","t"] += %client.blasterIndirectHits;
   %client.dtStats.ctfStats["blasterInDmgTaken","t"] += %client.blasterInDmgTaken;
   %client.dtStats.ctfStats["elfInDmg","t"] += %client.elfInDmg;
   %client.dtStats.ctfStats["elfIndirectHits","t"] += %client.elfIndirectHits;
   %client.dtStats.ctfStats["elfInDmgTaken","t"] += %client.elfInDmgTaken;
   %client.dtStats.ctfStats["unknownInDmg","t"] += %client.unknownInDmg;
   %client.dtStats.ctfStats["unknownIndirectHits","t"] += %client.unknownIndirectHits;
   %client.dtStats.ctfStats["unknownInDmgTaken","t"] += %client.unknownInDmgTaken;
   %client.dtStats.ctfStats["cgShotsFired","t"] += %client.cgShotsFired;
   %client.dtStats.ctfStats["discShotsFired","t"] += %client.discShotsFired;
   %client.dtStats.ctfStats["grenadeShotsFired","t"] += %client.grenadeShotsFired;
   %client.dtStats.ctfStats["laserShotsFired","t"] += %client.laserShotsFired;
   %client.dtStats.ctfStats["mortarShotsFired","t"] += %client.mortarShotsFired;
   %client.dtStats.ctfStats["missileShotsFired","t"] += %client.missileShotsFired;
   %client.dtStats.ctfStats["shockLanceShotsFired","t"] += %client.shockLanceShotsFired;
   %client.dtStats.ctfStats["plasmaShotsFired","t"] += %client.plasmaShotsFired;
   %client.dtStats.ctfStats["blasterShotsFired","t"] += %client.blasterShotsFired;
   %client.dtStats.ctfStats["elfShotsFired","t"] += %client.elfShotsFired;
   %client.dtStats.ctfStats["unknownShotsFired","t"] += %client.unknownShotsFired;
}
function addLAKTotal(%client) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %client.dtStats.lakTotalNumGames++;
   %client.dtStats.lakStats["timeStamp","t"] = formattimestring("hh:nn a, mm-dd-yy");
   
   %client.dtStats.lakStats["score","t"] += %client.score;
   %client.dtStats.lakStats["kills","t"] += %client.kills;
   %client.dtStats.lakStats["deaths","t"] += %client.deaths;
   %client.dtStats.lakStats["suicides","t"] += %client.suicides;
   %client.dtStats.lakStats["flagGrabs","t"] += %client.flagGrabs;
   %client.dtStats.lakStats["flagTimeMS","t"] += (%client.flagTimeMS / 1000) / 60;
   %client.dtStats.lakStats["morepoints","t"] += %client.morepoints;
   %client.dtStats.lakStats["mas","t"] += %client.mas;
   %client.dtStats.lakStats["totalSpeed","t"] +=  %client.totalSpeed;
   %client.dtStats.lakStats["totalDistance","t"] += %client.totalDistance;
   %client.dtStats.lakStats["totalChainAccuracy","t"] += %client.totalChainAccuracy;
   %client.dtStats.lakStats["totalChainHits","t"] += %client.totalChainHits;
   %client.dtStats.lakStats["totalSnipeHits","t"] += %client.totalSnipeHits;
   %client.dtStats.lakStats["totalSnipes","t"] += %client.totalSnipes;
   %client.dtStats.lakStats["totalShockHits","t"] += %client.totalShockHits;
   %client.dtStats.lakStats["totalShocks","t"] += %client.totalShocks;
   
   %client.dtStats.lakStats["minePlusDisc","t"] += %client.minePlusDisc;
   
   %client.dtStats.lakStats["cgKills","t"] += %client.cgKills;
   %client.dtStats.lakStats["cgDeaths","t"] += %client.cgDeaths;
   %client.dtStats.lakStats["discKills","t"] += %client.discKills;
   %client.dtStats.lakStats["discDeaths","t"] += %client.discDeaths;
   %client.dtStats.lakStats["grenadeKills","t"] += %client.grenadeKills;
   %client.dtStats.lakStats["grenadeDeaths","t"] += %client.grenadeDeaths;
   %client.dtStats.lakStats["laserKills","t"] += %client.laserKills;
   %client.dtStats.lakStats["laserDeaths","t"] += %client.laserDeaths;
   %client.dtStats.lakStats["mortarKills","t"] += %client.mortarKills;
   %client.dtStats.lakStats["mortarDeaths","t"] += %client.mortarDeaths;
   %client.dtStats.lakStats["missileKills","t"] += %client.missileKills;
   %client.dtStats.lakStats["missileDeaths","t"] += %client.missileDeaths;
   %client.dtStats.lakStats["shockLanceKills","t"] += %client.shockLanceKills;
   %client.dtStats.lakStats["shockLanceDeaths","t"] += %client.shockLanceDeaths;
   %client.dtStats.lakStats["plasmaKills","t"] += %client.plasmaKills;
   %client.dtStats.lakStats["plasmaDeaths","t"] += %client.plasmaDeaths;
   %client.dtStats.lakStats["blasterKills","t"] += %client.blasterKills;
   %client.dtStats.lakStats["blasterDeaths","t"] += %client.blasterDeaths;
   %client.dtStats.lakStats["elfKills","t"] += %client.elfKills;
   %client.dtStats.lakStats["elfDeaths","t"] += %client.elfDeaths;
   %client.dtStats.lakStats["mineKills","t"] += %client.mineKills;
   %client.dtStats.lakStats["mineDeaths","t"] += %client.mineDeaths;
   %client.dtStats.lakStats["explosionKills","t"] += %client.explosionKills;
   %client.dtStats.lakStats["explosionDeaths","t"] += %client.explosionDeaths;
   %client.dtStats.lakStats["impactKills","t"] +=  %client.impactKills;
   %client.dtStats.lakStats["impactDeaths","t"] += %client.impactDeaths;
   %client.dtStats.lakStats["groundKills","t"] += %client.groundKills;
   %client.dtStats.lakStats["groundDeaths","t"] += %client.groundDeaths;
   
   %client.dtStats.lakStats["outOfBoundKills","t"] += %client.outOfBoundKills;
   %client.dtStats.lakStats["outOfBoundDeaths","t"] += %client.outOfBoundDeaths;
   %client.dtStats.lakStats["lavaKills","t"] += %client.lavaKills;
   %client.dtStats.lakStats["lavaDeaths","t"] += %client.lavaDeaths;
   
   %client.dtStats.lakStats["satchelChargeKills","t"] += %client.satchelChargeKills;
   %client.dtStats.lakStats["satchelChargeDeaths","t"] += %client.satchelChargeDeaths;
   
   %client.dtStats.lakStats["lightningKills","t"] += %client.lightningKills;
   %client.dtStats.lakStats["lightningDeaths","t"] += %client.lightningDeaths;
   
   %client.dtStats.lakStats["forceFieldPowerUpKills","t"] += %client.forceFieldPowerUpKills;
   %client.dtStats.lakStats["forceFieldPowerUpDeaths","t"] += %client.forceFieldPowerUpDeaths;
   
   %client.dtStats.lakStats["waterKills","t"] += %client.waterKills;
   %client.dtStats.lakStats["waterDeaths","t"] += %client.waterDeaths;
   %client.dtStats.lakStats["nexusCampingKills","t"] += %client.nexusCampingKills;
   %client.dtStats.lakStats["nexusCampingDeaths","t"] += %client.nexusCampingDeaths;
   %client.dtStats.lakStats["unknownKill","t"] += %client.unknownKill;
   %client.dtStats.lakStats["unknownDeaths","t"] += %client.unknownDeaths;
   
   %client.dtStats.lakStats["cgDmg","t"] += %client.cgDmg;
   %client.dtStats.lakStats["cgDirectHits","t"] += %client.cgDirectHits;
   %client.dtStats.lakStats["cgDmgTaken","t"] += %client.cgDmgTaken;
   %client.dtStats.lakStats["discDmg","t"] += %client.discDmg;
   %client.dtStats.lakStats["discDirectHits","t"] += %client.discDirectHits;
   %client.dtStats.lakStats["discDmgTaken","t"] += %client.discDmgTaken;
   %client.dtStats.lakStats["grenadeDmg","t"] += %client.grenadeDmg;
   %client.dtStats.lakStats["grenadeDirectHits","t"] += %client.grenadeDirectHits;
   %client.dtStats.lakStats["grenadeDmgTaken","t"] += %client.grenadeDmgTaken;
   %client.dtStats.lakStats["laserDmg","t"] += %client.laserDmg;
   %client.dtStats.lakStats["laserDirectHits","t"] += %client.laserDirectHits;
   %client.dtStats.lakStats["laserDmgTaken","t"] += %client.laserDmgTaken;
   %client.dtStats.lakStats["mortarDmg","t"] += %client.mortarDmg;
   %client.dtStats.lakStats["mortarDirectHits","t"] += %client.mortarDirectHits;
   %client.dtStats.lakStats["mortarDmgTaken","t"] += %client.mortarDmgTaken;
   %client.dtStats.lakStats["missileDmg","t"] += %client.missileDmg;
   %client.dtStats.lakStats["missileDirectHits","t"] += %client.missileDirectHits;
   %client.dtStats.lakStats["missileDmgTaken","t"] += %client.missileDmgTaken;
   %client.dtStats.lakStats["shockLanceDmg","t"] += %client.shockLanceDmg;
   %client.dtStats.lakStats["shockLanceDirectHits","t"] += %client.shockLanceDirectHits;
   %client.dtStats.lakStats["shockLanceDmgTaken","t"] += %client.shockLanceDmgTaken;
   %client.dtStats.lakStats["plasmaDmg","t"] += %client.plasmaDmg;
   %client.dtStats.lakStats["plasmaDirectHits","t"] += %client.plasmaDirectHits;
   %client.dtStats.lakStats["plasmaDmgTaken","t"] += %client.plasmaDmgTaken;
   %client.dtStats.lakStats["blasterDmg","t"] += %client.blasterDmg;
   %client.dtStats.lakStats["blasterDirectHits","t"] += %client.blasterDirectHits;
   %client.dtStats.lakStats["blasterDmgTaken","t"] += %client.blasterDmgTaken;
   %client.dtStats.lakStats["elfDmg","t"] += %client.elfDmg;
   %client.dtStats.lakStats["elfDirectHits","t"] += %client.elfDirectHits;
   %client.dtStats.lakStats["elfDmgTaken","t"] += %client.elfDmgTaken;
   %client.dtStats.lakStats["unknownDmg","t"] += %client.unknownDmg;
   %client.dtStats.lakStats["unknownDirectHits","t"] += %client.unknownDirectHits;
   %client.dtStats.lakStats["unknownDmgTaken","t"] += %client.unknownDmgTaken;
   %client.dtStats.lakStats["cgInDmg","t"] += %client.cgInDmg;
   %client.dtStats.lakStats["cgIndirectHits","t"] += %client.cgIndirectHits;
   %client.dtStats.lakStats["cgInDmgTaken","t"] += %client.cgInDmgTaken;
   %client.dtStats.lakStats["discInDmg","t"] += %client.discInDmg;
   %client.dtStats.lakStats["discIndirectHits","t"] += %client.discIndirectHits;
   %client.dtStats.lakStats["discInDmgTaken","t"] += %client.discInDmgTaken;
   %client.dtStats.lakStats["grenadeInDmg","t"] += %client.grenadeInDmg;
   %client.dtStats.lakStats["grenadeIndirectHits","t"] += %client.grenadeIndirectHits;
   %client.dtStats.lakStats["grenadeInDmgTaken","t"] += %client.grenadeInDmgTaken;
   %client.dtStats.lakStats["laserInDmg","t"] += %client.laserInDmg;
   %client.dtStats.lakStats["laserIndirectHits","t"] += %client.laserIndirectHits;
   %client.dtStats.lakStats["laserInDmgTaken","t"] += %client.laserInDmgTaken;
   %client.dtStats.lakStats["mortarInDmg","t"] += %client.mortarInDmg;
   %client.dtStats.lakStats["mortarIndirectHits","t"] += %client.mortarIndirectHits;
   %client.dtStats.lakStats["mortarInDmgTaken","t"] += %client.mortarInDmgTaken;
   %client.dtStats.lakStats["missileInDmg","t"] += %client.missileInDmg;
   %client.dtStats.lakStats["missileIndirectHits","t"] += %client.missileIndirectHits;
   %client.dtStats.lakStats["missileInDmgTaken","t"] += %client.missileInDmgTaken;
   %client.dtStats.lakStats["shockLanceInDmg","t"] += %client.shockLanceInDmg;
   %client.dtStats.lakStats["shockLanceIndirectHits","t"] += %client.shockLanceIndirectHits;
   %client.dtStats.lakStats["shockLanceInDmgTaken","t"] += %client.shockLanceInDmgTaken;
   %client.dtStats.lakStats["plasmaInDmg","t"] += %client.plasmaInDmg;
   %client.dtStats.lakStats["plasmaIndirectHits","t"] += %client.plasmaIndirectHits;
   %client.dtStats.lakStats["plasmaInDmgTaken","t"] += %client.plasmaInDmgTaken;
   %client.dtStats.lakStats["blasterInDmg","t"] += %client.blasterInDmg;
   %client.dtStats.lakStats["blasterIndirectHits","t"] += %client.blasterIndirectHits;
   %client.dtStats.lakStats["blasterInDmgTaken","t"] += %client.blasterInDmgTaken;
   %client.dtStats.lakStats["elfInDmg","t"] += %client.elfInDmg;
   %client.dtStats.lakStats["elfIndirectHits","t"] += %client.elfIndirectHits;
   %client.dtStats.lakStats["elfInDmgTaken","t"] += %client.elfInDmgTaken;
   %client.dtStats.lakStats["unknownInDmg","t"] += %client.unknownInDmg;
   %client.dtStats.lakStats["unknownIndirectHits","t"] += %client.unknownIndirectHits;
   %client.dtStats.lakStats["unknownInDmgTaken","t"] += %client.unknownInDmgTaken;
   %client.dtStats.lakStats["cgShotsFired","t"] += %client.cgShotsFired;
   %client.dtStats.lakStats["discShotsFired","t"] += %client.discShotsFired;
   %client.dtStats.lakStats["grenadeShotsFired","t"] += %client.grenadeShotsFired;
   %client.dtStats.lakStats["laserShotsFired","t"] += %client.laserShotsFired;
   %client.dtStats.lakStats["mortarShotsFired","t"] += %client.mortarShotsFired;
   %client.dtStats.lakStats["missileShotsFired","t"] += %client.missileShotsFired;
   %client.dtStats.lakStats["shockLanceShotsFired","t"] += %client.shockLanceShotsFired;
   %client.dtStats.lakStats["plasmaShotsFired","t"] += %client.plasmaShotsFired;
   %client.dtStats.lakStats["blasterShotsFired","t"] += %client.blasterShotsFired;
   %client.dtStats.lakStats["elfShotsFired","t"] += %client.elfShotsFired;
   %client.dtStats.lakStats["unknownShotsFired","t"] += %client.unknownShotsFired;
}
function saveCTFTotalStats(%dtStats){ // saved by the main save function
   if($dtStats::Enable  == 0){return;}
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/CTF/" @ %dtStats.guid @ "/" @ "totalStats" @ ".cs";
      %file = new FileObject();
      %file.OpenForWrite(%filename);
      
      %file.writeLine("ctfTotalNumGames" @ "%t" @ %dtStats.ctfTotalNumGames);
      
      %file.writeLine("timeStamp" @ "%t" @ %dtStats.ctfStats["timeStamp","t"]);
      %file.writeLine("kills" @ "%t" @ %dtStats.ctfStats["kills","t"]);
      %file.writeLine("deaths" @ "%t" @ %dtStats.ctfStats["deaths", "t"]);
      %file.writeLine("suicides" @ "%t" @ %dtStats.ctfStats["suicides","t"]);
      %file.writeLine("teamKills" @ "%t" @ %dtStats.ctfStats["teamKills","t"]);
      %file.writeLine("flagCaps" @ "%t" @ %dtStats.ctfStats["flagCaps","t"]);
      %file.writeLine("flagGrabs" @ "%t" @ %dtStats.ctfStats["flagGrabs","t"]);
      %file.writeLine("carrierKills" @ "%t" @ %dtStats.ctfStats["carrierKills","t"]);
      %file.writeLine("flagReturns" @ "%t" @ %dtStats.ctfStats["flagReturns","t"]);
      %file.writeLine("score" @ "%t" @ %dtStats.ctfStats["score","t"]);
      %file.writeLine("scoreMidAir" @ "%t" @ %dtStats.ctfStats["scoreMidAir","t"]);
      %file.writeLine("scoreHeadshot" @ "%t" @ %dtStats.ctfStats["scoreHeadshot","t"]);
      %file.writeLine("minePlusDisc" @ "%t" @ %dtStats.ctfStats["minePlusDisc","t"]);
      
      %file.writeLine("scoreRearshot" @ "%t" @ %dtStats.ctfStats["scoreRearshot","t"]);
      %file.writeLine("escortAssists" @ "%t" @ %dtStats.ctfStats["escortAssists","t"]);
      %file.writeLine("defenseScore" @ "%t" @ %dtStats.ctfStats["defenseScore","t"]);
      %file.writeLine("offenseScore" @ "%t" @ %dtStats.ctfStats["offenseScore","t"]);
      %file.writeLine("flagDefends" @ "%t" @ %dtStats.ctfStats["flagDefends","t"]);
      
      %file.writeLine("cgKills" @ "%t" @ %dtStats.ctfStats["cgKills","t"]);
      %file.writeLine("cgDeaths" @ "%t" @ %dtStats.ctfStats["cgDeaths","t"]);
      %file.writeLine("discKills" @ "%t" @ %dtStats.ctfStats["discKills","t"]);
      %file.writeLine("discDeaths" @ "%t" @ %dtStats.ctfStats["discDeaths","t"]);
      %file.writeLine("grenadeKills" @ "%t" @ %dtStats.ctfStats["grenadeKills","t"]);
      %file.writeLine("grenadeDeaths" @ "%t" @ %dtStats.ctfStats["grenadeDeaths","t"]);
      %file.writeLine("Headshot" @ "%t" @ %dtStats.ctfStats["laserKills","t"]);
      %file.writeLine("laserDeaths" @ "%t" @ %dtStats.ctfStats["laserDeaths","t"]);
      %file.writeLine("mortarKills" @ "%t" @ %dtStats.ctfStats["mortarKills","t"]);
      %file.writeLine("mortarDeaths" @ "%t" @ %dtStats.ctfStats["mortarDeaths","t"]);
      %file.writeLine("missileKills" @ "%t" @ %dtStats.ctfStats["missileKills","t"]);
      %file.writeLine("missileDeaths" @ "%t" @ %dtStats.ctfStats["missileDeaths","t"]);
      %file.writeLine("shockLanceKills" @ "%t" @ %dtStats.ctfStats["shockLanceKills","t"]);
      %file.writeLine("shockLanceDeaths" @ "%t" @ %dtStats.ctfStats["shockLanceDeaths","t"]);
      %file.writeLine("plasmaKills" @ "%t" @ %dtStats.ctfStats["plasmaKills","t"]);
      %file.writeLine("plasmaDeaths" @ "%t" @ %dtStats.ctfStats["plasmaDeaths","t"]);
      %file.writeLine("blasterKills" @ "%t" @ %dtStats.ctfStats["blasterKills","t"]);
      %file.writeLine("blasterDeaths" @ "%t" @ %dtStats.ctfStats["blasterDeaths","t"]);
      %file.writeLine("elfKills" @ "%t" @ %dtStats.ctfStats["elfKills","t"]);
      %file.writeLine("elfDeaths" @ "%t" @ %dtStats.ctfStats["elfDeaths","t"]);
      %file.writeLine("mineKills" @ "%t" @ %dtStats.ctfStats["mineKills","t"]);
      %file.writeLine("mineDeaths" @ "%t" @ %dtStats.ctfStats["mineDeaths","t"]);
      %file.writeLine("explosionKills" @ "%t" @ %dtStats.ctfStats["explosionKills","t"]);
      %file.writeLine("explosionDeaths" @ "%t" @ %dtStats.ctfStats["explosionDeaths","t"]);
      %file.writeLine("impactKills" @ "%t" @ %dtStats.ctfStats["impactKills","t"]);
      %file.writeLine("impactDeaths" @ "%t" @ %dtStats.ctfStats["impactDeaths","t"]);
      %file.writeLine("groundKills" @ "%t" @ %dtStats.ctfStats["groundKills","t"]);
      %file.writeLine("groundDeaths" @ "%t" @ %dtStats.ctfStats["groundDeaths","t"]);
      %file.writeLine("turretKills" @ "%t" @ %dtStats.ctfStats["turretKills","t"]);
      %file.writeLine("turretDeaths" @ "%t" @ %dtStats.ctfStats["turretDeaths","t"]);
      %file.writeLine("plasmaTurretKills" @ "%t" @ %dtStats.ctfStats["plasmaTurretKills","t"]);
      %file.writeLine("plasmaTurretDeaths" @ "%t" @ %dtStats.ctfStats["plasmaTurretDeaths","t"]);
      %file.writeLine("aaTurretKills" @ "%t" @ %dtStats.ctfStats["aaTurretKills","t"]);
      %file.writeLine("aaTurretDeaths" @ "%t" @ %dtStats.ctfStats["aaTurretDeaths","t"]);
      %file.writeLine("elfTurretKills" @ "%t" @ %dtStats.ctfStats["elfTurretKills","t"]);
      %file.writeLine("elfTurretDeaths" @ "%t" @ %dtStats.ctfStats["elfTurretDeaths","t"]);
      %file.writeLine("mortarTurretKills" @ "%t" @ %dtStats.ctfStats["mortarTurretKills","t"]);
      %file.writeLine("mortarTurretDeaths" @ "%t" @ %dtStats.ctfStats["mortarTurretDeaths","t"]);
      %file.writeLine("missileTurretKills" @ "%t" @ %dtStats.ctfStats["missileTurretKills","t"]);
      %file.writeLine("missileTurretDeaths" @ "%t" @ %dtStats.ctfStats["missileTurretDeaths","t"]);
      %file.writeLine("indoorDepTurretKills" @ "%t" @ %dtStats.ctfStats["indoorDepTurretKills","t"]);
      %file.writeLine("indoorDepTurretDeaths" @ "%t" @ %dtStats.ctfStats["indoorDepTurretDeaths","t"]);
      %file.writeLine("outdoorDepTurretKills" @ "%t" @ %dtStats.ctfStats["outdoorDepTurretKills","t"]);
      %file.writeLine("outdoorDepTurretDeaths" @ "%t" @ %dtStats.ctfStats["outdoorDepTurretDeaths","t"]);
      %file.writeLine("sentryTurretKills" @ "%t" @ %dtStats.ctfStats["sentryTurretKills","t"]);
      %file.writeLine("sentryTurretDeaths" @ "%t" @ %dtStats.ctfStats["sentryTurretDeaths","t"]);
      %file.writeLine("outOfBoundKills" @ "%t" @ %dtStats.ctfStats["outOfBoundKills","t"]);
      %file.writeLine("outOfBoundDeaths" @ "%t" @ %dtStats.ctfStats["outOfBoundDeaths","t"]);
      %file.writeLine("lavaKills" @ "%t" @ %dtStats.ctfStats["lavaKills","t"]);
      %file.writeLine("lavaDeaths" @ "%t" @ %dtStats.ctfStats["lavaDeaths","t"]);
      %file.writeLine("shrikeBlasterKills" @ "%t" @ %dtStats.ctfStats["shrikeBlasterKills","t"]);
      %file.writeLine("shrikeBlasterDeaths" @ "%t" @ %dtStats.ctfStats["shrikeBlasterDeaths","t"]);
      %file.writeLine("bellyTurretKills" @ "%t" @ %dtStats.ctfStats["bellyTurretKills","t"]);
      %file.writeLine("bellyTurretDeaths" @ "%t" @ %dtStats.ctfStats["bellyTurretDeaths","t"]);
      %file.writeLine("bomberBombsKills" @ "%t" @ %dtStats.ctfStats["bomberBombsKills","t"]);
      %file.writeLine("bomberBombsDeaths" @ "%t" @ %dtStats.ctfStats["bomberBombsDeaths","t"]);
      %file.writeLine("tankChaingunKills" @ "%t" @ %dtStats.ctfStats["tankChaingunKills","t"]);
      %file.writeLine("tankChaingunDeaths" @ "%t" @ %dtStats.ctfStats["tankChaingunDeaths","t"]);
      %file.writeLine("tankMortarKills" @ "%t" @ %dtStats.ctfStats["tankMortarKills","t"]);
      %file.writeLine("tankMortarDeaths" @ "%t" @ %dtStats.ctfStats["tankMortarDeaths","t"]);
      %file.writeLine("satchelChargeKills" @ "%t" @ %dtStats.ctfStats["satchelChargeKills","t"]);
      %file.writeLine("satchelChargeDeaths" @ "%t" @ %dtStats.ctfStats["satchelChargeDeaths","t"]);
      %file.writeLine("mpbMissileKills" @ "%t" @ %dtStats.ctfStats["mpbMissileKills","t"]);
      %file.writeLine("mpbMissileDeaths" @ "%t" @ %dtStats.ctfStats["mpbMissileDeaths","t"]);
      %file.writeLine("lightningKills" @ "%t" @ %dtStats.ctfStats["lightningKills","t"]);
      %file.writeLine("lightningDeaths" @ "%t" @ %dtStats.ctfStats["lightningDeaths","t"]);
      %file.writeLine("vehicleSpawnKills" @ "%t" @ %dtStats.ctfStats["vehicleSpawnKills","t"]);
      %file.writeLine("vehicleSpawnDeaths" @ "%t" @ %dtStats.ctfStats["vehicleSpawnDeaths","t"]);
      %file.writeLine("forceFieldPowerUpKills" @ "%t" @ %dtStats.ctfStats["forceFieldPowerUpKills","t"]);
      %file.writeLine("forceFieldPowerUpDeaths" @ "%t" @ %dtStats.ctfStats["forceFieldPowerUpDeaths","t"]);
      %file.writeLine("crashKills" @ "%t" @ %dtStats.ctfStats["crashKills","t"]);
      %file.writeLine("crashDeaths" @ "%t" @ %dtStats.ctfStats["crashDeaths","t"]);
      %file.writeLine("waterKills" @ "%t" @ %dtStats.ctfStats["waterKills","t"]);
      %file.writeLine("waterDeaths" @ "%t" @ %dtStats.ctfStats["waterDeaths","t"]);
      %file.writeLine("nexusCampingKills" @ "%t" @ %dtStats.ctfStats["nexusCampingKills","t"]);
      %file.writeLine("nexusCampingDeaths" @ "%t" @ %dtStats.ctfStats["nexusCampingDeaths","t"]);
      %file.writeLine("unknownKill" @ "%t" @ %dtStats.ctfStats["unknownKill","t"]);
      %file.writeLine("unknownDeaths" @ "%t" @ %dtStats.ctfStats["unknownDeaths","t"]);
      
      %file.writeLine("cgDmg" @ "%t" @ %dtStats.ctfStats["cgDmg","t"]);
      %file.writeLine("cgDirectHits" @ "%t" @ %dtStats.ctfStats["cgDirectHits","t"]);
      %file.writeLine("cgDmgTaken" @ "%t" @ %dtStats.ctfStats["cgDmgTaken","t"]);
      %file.writeLine("discDmg" @ "%t" @ %dtStats.ctfStats["discDmg","t"]);
      %file.writeLine("discDirectHits" @ "%t" @ %dtStats.ctfStats["discDirectHits","t"]);
      %file.writeLine("discDmgTaken" @ "%t" @ %dtStats.ctfStats["discDmgTaken","t"]);
      %file.writeLine("grenadeDmg" @ "%t" @ %dtStats.ctfStats["grenadeDmg","t"]);
      %file.writeLine("grenadeDirectHits" @ "%t" @ %dtStats.ctfStats["grenadeDirectHits","t"]);
      %file.writeLine("grenadeDmgTaken" @ "%t" @ %dtStats.ctfStats["grenadeDmgTaken","t"]);
      %file.writeLine("laserDmg" @ "%t" @ %dtStats.ctfStats["laserDmg","t"]);
      %file.writeLine("laserDirectHits" @ "%t" @ %dtStats.ctfStats["laserDirectHits","t"]);
      %file.writeLine("laserDmgTaken" @ "%t" @ %dtStats.ctfStats["laserDmgTaken","t"]);
      %file.writeLine("mortarDmg" @ "%t" @ %dtStats.ctfStats["mortarDmg","t"]);
      %file.writeLine("mortarDirectHits" @ "%t" @ %dtStats.ctfStats["mortarDirectHits","t"]);
      %file.writeLine("mortarDmgTaken" @ "%t" @ %dtStats.ctfStats["mortarDmgTaken","t"]);
      %file.writeLine("missileDmg" @ "%t" @ %dtStats.ctfStats["missileDmg","t"]);
      %file.writeLine("missileDirectHits" @ "%t" @ %dtStats.ctfStats["missileDirectHits","t"]);
      %file.writeLine("missileDmgTaken" @ "%t" @ %dtStats.ctfStats["missileDmgTaken","t"]);
      %file.writeLine("shockLanceDmg" @ "%t" @ %dtStats.ctfStats["shockLanceDmg","t"]);
      %file.writeLine("shockLanceDirectHits" @ "%t" @ %dtStats.ctfStats["shockLanceDirectHits","t"]);
      %file.writeLine("shockLanceDmgTaken" @ "%t" @ %dtStats.ctfStats["shockLanceDmgTaken","t"]);
      %file.writeLine("plasmaDmg" @ "%t" @ %dtStats.ctfStats["plasmaDmg","t"]);
      %file.writeLine("plasmaDirectHits" @ "%t" @ %dtStats.ctfStats["plasmaDirectHits","t"]);
      %file.writeLine("plasmaDmgTaken" @ "%t" @ %dtStats.ctfStats["plasmaDmgTaken","t"]);
      %file.writeLine("blasterDmg" @ "%t" @ %dtStats.ctfStats["blasterDmg","t"]);
      %file.writeLine("blasterDirectHits" @ "%t" @ %dtStats.ctfStats["blasterDirectHits","t"]);
      %file.writeLine("blasterDmgTaken" @ "%t" @ %dtStats.ctfStats["blasterDmgTaken","t"]);
      %file.writeLine("elfDmg" @ "%t" @ %dtStats.ctfStats["elfDmg","t"]);
      %file.writeLine("elfDirectHits" @ "%t" @ %dtStats.ctfStats["elfDirectHits","t"]);
      %file.writeLine("elfDmgTaken" @ "%t" @ %dtStats.ctfStats["elfDmgTaken","t"]);
      %file.writeLine("unknownDmg" @ "%t" @ %dtStats.ctfStats["unknownDmg","t"]);
      %file.writeLine("unknownDirectHits" @ "%t" @ %dtStats.ctfStats["unknownDirectHits","t"]);
      %file.writeLine("unknownDmgTaken" @ "%t" @ %dtStats.ctfStats["unknownDmgTaken","t"]);
      %file.writeLine("cgInDmg" @ "%t" @ %dtStats.ctfStats["cgInDmg","t"]);
      %file.writeLine("cgIndirectHits" @ "%t" @ %dtStats.ctfStats["cgIndirectHits","t"]);
      %file.writeLine("cgInDmgTaken" @ "%t" @ %dtStats.ctfStats["cgInDmgTaken","t"]);
      %file.writeLine("discInDmg" @ "%t" @ %dtStats.ctfStats["discInDmg","t"]);
      %file.writeLine("discIndirectHits" @ "%t" @ %dtStats.ctfStats["discIndirectHits","t"]);
      %file.writeLine("discInDmgTaken" @ "%t" @ %dtStats.ctfStats["discInDmgTaken","t"]);
      %file.writeLine("grenadeInDmg" @ "%t" @ %dtStats.ctfStats["grenadeInDmg","t"]);
      %file.writeLine("grenadeIndirectHits" @ "%t" @ %dtStats.ctfStats["grenadeIndirectHits","t"]);
      %file.writeLine("grenadeInDmgTaken" @ "%t" @ %dtStats.ctfStats["grenadeInDmgTaken","t"]);
      %file.writeLine("laserInDmg" @ "%t" @ %dtStats.ctfStats["laserInDmg","t"]);
      %file.writeLine("laserIndirectHits" @ "%t" @ %dtStats.ctfStats["laserIndirectHits","t"]);
      %file.writeLine("laserInDmgTaken" @ "%t" @ %dtStats.ctfStats["laserInDmgTaken","t"]);
      %file.writeLine("mortarInDmg" @ "%t" @ %dtStats.ctfStats["mortarInDmg","t"]);
      %file.writeLine("mortarIndirectHits" @ "%t" @ %dtStats.ctfStats["mortarIndirectHits","t"]);
      %file.writeLine("mortarInDmgTaken" @ "%t" @ %dtStats.ctfStats["mortarInDmgTaken","t"]);
      %file.writeLine("missileInDmg" @ "%t" @ %dtStats.ctfStats["missileInDmg","t"]);
      %file.writeLine("missileIndirectHits" @ "%t" @ %dtStats.ctfStats["missileIndirectHits","t"]);
      %file.writeLine("missileInDmgTaken" @ "%t" @ %dtStats.ctfStats["missileInDmgTaken","t"]);
      %file.writeLine("shockLanceInDmg" @ "%t" @ %dtStats.ctfStats["shockLanceInDmg","t"]);
      %file.writeLine("shockLanceIndirectHits" @ "%t" @ %dtStats.ctfStats["shockLanceIndirectHits","t"]);
      %file.writeLine("shockLanceInDmgTaken" @ "%t" @ %dtStats.ctfStats["shockLanceInDmgTaken","t"]);
      %file.writeLine("plasmaInDmg" @ "%t" @ %dtStats.ctfStats["plasmaInDmg","t"]);
      %file.writeLine("plasmaIndirectHits" @ "%t" @ %dtStats.ctfStats["plasmaIndirectHits","t"]);
      %file.writeLine("plasmaInDmgTaken" @ "%t" @ %dtStats.ctfStats["plasmaInDmgTaken","t"]);
      %file.writeLine("blasterInDmg" @ "%t" @ %dtStats.ctfStats["blasterInDmg","t"]);
      %file.writeLine("blasterIndirectHits" @ "%t" @ %dtStats.ctfStats["blasterIndirectHits","t"]);
      %file.writeLine("blasterInDmgTaken" @ "%t" @ %dtStats.ctfStats["blasterInDmgTaken","t"]);
      %file.writeLine("elfInDmg" @ "%t" @ %dtStats.ctfStats["elfInDmg","t"]);
      %file.writeLine("elfIndirectHits" @ "%t" @ %dtStats.ctfStats["elfIndirectHits","t"]);
      %file.writeLine("elfInDmgTaken" @ "%t" @ %dtStats.ctfStats["elfInDmgTaken","t"]);
      %file.writeLine("unknownInDmg" @ "%t" @ %dtStats.ctfStats["unknownInDmg","t"]);
      %file.writeLine("unknownIndirectHits" @ "%t" @ %dtStats.ctfStats["unknownIndirectHits","t"]);
      %file.writeLine("unknownInDmgTaken" @ "%t" @ %dtStats.ctfStats["unknownInDmgTaken","t"]);
      %file.writeLine("cgShotsFired" @ "%t" @ %dtStats.ctfStats["cgShotsFired","t"]);
      %file.writeLine("discShotsFired" @ "%t" @ %dtStats.ctfStats["discShotsFired","t"]);
      %file.writeLine("grenadeShotsFired" @ "%t" @ %dtStats.ctfStats["grenadeShotsFired","t"]);
      %file.writeLine("laserShotsFired" @ "%t" @ %dtStats.ctfStats["laserShotsFired","t"]);
      %file.writeLine("mortarShotsFired" @ "%t" @ %dtStats.ctfStats["mortarShotsFired","t"]);
      %file.writeLine("missileShotsFired" @ "%t" @ %dtStats.ctfStats["missileShotsFired","t"]);
      %file.writeLine("shockLanceShotsFired" @ "%t" @ %dtStats.ctfStats["shockLanceShotsFired","t"]);
      %file.writeLine("plasmaShotsFired" @ "%t" @ %dtStats.ctfStats["plasmaShotsFired","t"]);
      %file.writeLine("blasterShotsFired" @ "%t" @ %dtStats.ctfStats["blasterShotsFired","t"]);
      %file.writeLine("elfShotsFired" @ "%t" @ %dtStats.ctfStats["elfShotsFired","t"]);
      %file.writeLine("unknownShotsFired" @ "%t" @ %dtStats.ctfStats["unknownShotsFired","t"]);
      
      %file.close();
   }
   %file.delete();
}
function saveLAKTotalStats(%dtStats){ // saved by the main save function
   if($dtStats::Enable  == 0){return;}
   //error(%dtStats @ "%t" @ "savelakStats");
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/Lak/" @ %dtStats.guid @ "/" @ "totalStats" @ ".cs";
      %file = new FileObject();
      %file.OpenForWrite(%filename);
      
      %file.writeLine("lakTotalNumGames" @ "%t" @ %dtStats.lakTotalNumGames);
      
      %file.writeLine("timeStamp" @ "%t" @ %dtStats.lakStats["timeStamp","t"]);
      %file.writeLine("score" @ "%t" @ %dtStats.lakStats["score","t"]);
      %file.writeLine("kills" @ "%t" @ %dtStats.lakStats["kills","t"]);
      %file.writeLine("deaths" @ "%t" @ %dtStats.lakStats["deaths", "t"]);
      %file.writeLine("suicides" @ "%t" @ %dtStats.lakStats["suicides","t"]);
      %file.writeLine("flagGrabs" @ "%t" @ %dtStats.lakStats["flagGrabs","t"]);
      %file.writeLine("flagTimeMS" @ "%t" @ %dtStats.lakStats["flagTimeMS","t"]);
      %file.writeLine("morepoints" @ "%t" @ %dtStats.lakStats["morepoints","t"]);
      %file.writeLine("mas" @ "%t" @ %dtStats.lakStats["mas","t"]);
      %file.writeLine("totalSpeed" @ "%t" @ %dtStats.lakStats["totalSpeed","t"]);
      %file.writeLine("totalDistance" @ "%t" @ %dtStats.lakStats["totalDistance","t"]);
      %file.writeLine("totalChainAccuracy" @ "%t" @ %dtStats.lakStats["totalChainAccuracy","t"]);
      %file.writeLine("totalChainHits" @ "%t" @ %dtStats.lakStats["totalChainHits","t"]);
      %file.writeLine("totalSnipeHits" @ "%t" @ %dtStats.lakStats["totalSnipeHits","t"]);
      %file.writeLine("totalSnipes" @ "%t" @ %dtStats.lakStats["totalSnipes","t"]);
      %file.writeLine("totalShockHits" @ "%t" @ %dtStats.lakStats["totalShockHits","t"]);
      %file.writeLine("totalShocks" @ "%t" @ %dtStats.lakStats["totalShocks","t"]);
      
      %file.writeLine("minePlusDisc" @ "%t" @ %dtStats.lakStats["minePlusDisc","t"]);
      
      %file.writeLine("cgKills" @ "%t" @ %dtStats.lakStats["cgKills","t"]);
      %file.writeLine("cgDeaths" @ "%t" @ %dtStats.lakStats["cgDeaths","t"]);
      %file.writeLine("discKills" @ "%t" @ %dtStats.lakStats["discKills","t"]);
      %file.writeLine("discDeaths" @ "%t" @ %dtStats.lakStats["discDeaths","t"]);
      %file.writeLine("grenadeKills" @ "%t" @ %dtStats.lakStats["grenadeKills","t"]);
      %file.writeLine("grenadeDeaths" @ "%t" @ %dtStats.lakStats["grenadeDeaths","t"]);
      %file.writeLine("Headshot" @ "%t" @ %dtStats.lakStats["laserKills","t"]);
      %file.writeLine("laserDeaths" @ "%t" @ %dtStats.lakStats["laserDeaths","t"]);
      %file.writeLine("mortarKills" @ "%t" @ %dtStats.lakStats["mortarKills","t"]);
      %file.writeLine("mortarDeaths" @ "%t" @ %dtStats.lakStats["mortarDeaths","t"]);
      %file.writeLine("missileKills" @ "%t" @ %dtStats.lakStats["missileKills","t"]);
      %file.writeLine("missileDeaths" @ "%t" @ %dtStats.lakStats["missileDeaths","t"]);
      %file.writeLine("shockLanceKills" @ "%t" @ %dtStats.lakStats["shockLanceKills","t"]);
      %file.writeLine("shockLanceDeaths" @ "%t" @ %dtStats.lakStats["shockLanceDeaths","t"]);
      %file.writeLine("plasmaKills" @ "%t" @ %dtStats.lakStats["plasmaKills","t"]);
      %file.writeLine("plasmaDeaths" @ "%t" @ %dtStats.lakStats["plasmaDeaths","t"]);
      %file.writeLine("blasterKills" @ "%t" @ %dtStats.lakStats["blasterKills","t"]);
      %file.writeLine("blasterDeaths" @ "%t" @ %dtStats.lakStats["blasterDeaths","t"]);
      %file.writeLine("elfKills" @ "%t" @ %dtStats.lakStats["elfKills","t"]);
      %file.writeLine("elfDeaths" @ "%t" @ %dtStats.lakStats["elfDeaths","t"]);
      %file.writeLine("mineKills" @ "%t" @ %dtStats.lakStats["mineKills","t"]);
      %file.writeLine("mineDeaths" @ "%t" @ %dtStats.lakStats["mineDeaths","t"]);
      %file.writeLine("explosionKills" @ "%t" @ %dtStats.lakStats["explosionKills","t"]);
      %file.writeLine("explosionDeaths" @ "%t" @ %dtStats.lakStats["explosionDeaths","t"]);
      %file.writeLine("impactKills" @ "%t" @ %dtStats.lakStats["impactKills","t"]);
      %file.writeLine("impactDeaths" @ "%t" @ %dtStats.lakStats["impactDeaths","t"]);
      %file.writeLine("groundKills" @ "%t" @ %dtStats.lakStats["groundKills","t"]);
      %file.writeLine("groundDeaths" @ "%t" @ %dtStats.lakStats["groundDeaths","t"]);
      
      %file.writeLine("outOfBoundKills" @ "%t" @ %dtStats.lakStats["outOfBoundKills","t"]);
      %file.writeLine("outOfBoundDeaths" @ "%t" @ %dtStats.lakStats["outOfBoundDeaths","t"]);
      %file.writeLine("lavaKills" @ "%t" @ %dtStats.lakStats["lavaKills","t"]);
      %file.writeLine("lavaDeaths" @ "%t" @ %dtStats.lakStats["lavaDeaths","t"]);
      
      %file.writeLine("satchelChargeKills" @ "%t" @ %dtStats.lakStats["satchelChargeKills","t"]);
      %file.writeLine("satchelChargeDeaths" @ "%t" @ %dtStats.lakStats["satchelChargeDeaths","t"]);
      
      %file.writeLine("lightningKills" @ "%t" @ %dtStats.lakStats["lightningKills","t"]);
      %file.writeLine("lightningDeaths" @ "%t" @ %dtStats.lakStats["lightningDeaths","t"]);
      
      %file.writeLine("forceFieldPowerUpKills" @ "%t" @ %dtStats.lakStats["forceFieldPowerUpKills","t"]);
      %file.writeLine("forceFieldPowerUpDeaths" @ "%t" @ %dtStats.lakStats["forceFieldPowerUpDeaths","t"]);
      
      %file.writeLine("waterKills" @ "%t" @ %dtStats.lakStats["waterKills","t"]);
      %file.writeLine("waterDeaths" @ "%t" @ %dtStats.lakStats["waterDeaths","t"]);
      %file.writeLine("nexusCampingKills" @ "%t" @ %dtStats.lakStats["nexusCampingKills","t"]);
      %file.writeLine("nexusCampingDeaths" @ "%t" @ %dtStats.lakStats["nexusCampingDeaths","t"]);
      %file.writeLine("unknownKill" @ "%t" @ %dtStats.lakStats["unknownKill","t"]);
      %file.writeLine("unknownDeaths" @ "%t" @ %dtStats.lakStats["unknownDeaths","t"]);
      
      %file.writeLine("cgDmg" @ "%t" @ %dtStats.lakStats["cgDmg","t"]);
      %file.writeLine("cgDirectHits" @ "%t" @ %dtStats.lakStats["cgDirectHits","t"]);
      %file.writeLine("cgDmgTaken" @ "%t" @ %dtStats.lakStats["cgDmgTaken","t"]);
      %file.writeLine("discDmg" @ "%t" @ %dtStats.lakStats["discDmg","t"]);
      %file.writeLine("discDirectHits" @ "%t" @ %dtStats.lakStats["discDirectHits","t"]);
      %file.writeLine("discDmgTaken" @ "%t" @ %dtStats.lakStats["discDmgTaken","t"]);
      %file.writeLine("grenadeDmg" @ "%t" @ %dtStats.lakStats["grenadeDmg","t"]);
      %file.writeLine("grenadeDirectHits" @ "%t" @ %dtStats.lakStats["grenadeDirectHits","t"]);
      %file.writeLine("grenadeDmgTaken" @ "%t" @ %dtStats.lakStats["grenadeDmgTaken","t"]);
      %file.writeLine("laserDmg" @ "%t" @ %dtStats.lakStats["laserDmg","t"]);
      %file.writeLine("laserDirectHits" @ "%t" @ %dtStats.lakStats["laserDirectHits","t"]);
      %file.writeLine("laserDmgTaken" @ "%t" @ %dtStats.lakStats["laserDmgTaken","t"]);
      %file.writeLine("mortarDmg" @ "%t" @ %dtStats.lakStats["mortarDmg","t"]);
      %file.writeLine("mortarDirectHits" @ "%t" @ %dtStats.lakStats["mortarDirectHits","t"]);
      %file.writeLine("mortarDmgTaken" @ "%t" @ %dtStats.lakStats["mortarDmgTaken","t"]);
      %file.writeLine("missileDmg" @ "%t" @ %dtStats.lakStats["missileDmg","t"]);
      %file.writeLine("missileDirectHits" @ "%t" @ %dtStats.lakStats["missileDirectHits","t"]);
      %file.writeLine("missileDmgTaken" @ "%t" @ %dtStats.lakStats["missileDmgTaken","t"]);
      %file.writeLine("shockLanceDmg" @ "%t" @ %dtStats.lakStats["shockLanceDmg","t"]);
      %file.writeLine("shockLanceDirectHits" @ "%t" @ %dtStats.lakStats["shockLanceDirectHits","t"]);
      %file.writeLine("shockLanceDmgTaken" @ "%t" @ %dtStats.lakStats["shockLanceDmgTaken","t"]);
      %file.writeLine("plasmaDmg" @ "%t" @ %dtStats.lakStats["plasmaDmg","t"]);
      %file.writeLine("plasmaDirectHits" @ "%t" @ %dtStats.lakStats["plasmaDirectHits","t"]);
      %file.writeLine("plasmaDmgTaken" @ "%t" @ %dtStats.lakStats["plasmaDmgTaken","t"]);
      %file.writeLine("blasterDmg" @ "%t" @ %dtStats.lakStats["blasterDmg","t"]);
      %file.writeLine("blasterDirectHits" @ "%t" @ %dtStats.lakStats["blasterDirectHits","t"]);
      %file.writeLine("blasterDmgTaken" @ "%t" @ %dtStats.lakStats["blasterDmgTaken","t"]);
      %file.writeLine("elfDmg" @ "%t" @ %dtStats.lakStats["elfDmg","t"]);
      %file.writeLine("elfDirectHits" @ "%t" @ %dtStats.lakStats["elfDirectHits","t"]);
      %file.writeLine("elfDmgTaken" @ "%t" @ %dtStats.lakStats["elfDmgTaken","t"]);
      %file.writeLine("unknownDmg" @ "%t" @ %dtStats.lakStats["unknownDmg","t"]);
      %file.writeLine("unknownDirectHits" @ "%t" @ %dtStats.lakStats["unknownDirectHits","t"]);
      %file.writeLine("unknownDmgTaken" @ "%t" @ %dtStats.lakStats["unknownDmgTaken","t"]);
      %file.writeLine("cgInDmg" @ "%t" @ %dtStats.lakStats["cgInDmg","t"]);
      %file.writeLine("cgIndirectHits" @ "%t" @ %dtStats.lakStats["cgIndirectHits","t"]);
      %file.writeLine("cgInDmgTaken" @ "%t" @ %dtStats.lakStats["cgInDmgTaken","t"]);
      %file.writeLine("discInDmg" @ "%t" @ %dtStats.lakStats["discInDmg","t"]);
      %file.writeLine("discIndirectHits" @ "%t" @ %dtStats.lakStats["discIndirectHits","t"]);
      %file.writeLine("discInDmgTaken" @ "%t" @ %dtStats.lakStats["discInDmgTaken","t"]);
      %file.writeLine("grenadeInDmg" @ "%t" @ %dtStats.lakStats["grenadeInDmg","t"]);
      %file.writeLine("grenadeIndirectHits" @ "%t" @ %dtStats.lakStats["grenadeIndirectHits","t"]);
      %file.writeLine("grenadeInDmgTaken" @ "%t" @ %dtStats.lakStats["grenadeInDmgTaken","t"]);
      %file.writeLine("laserInDmg" @ "%t" @ %dtStats.lakStats["laserInDmg","t"]);
      %file.writeLine("laserIndirectHits" @ "%t" @ %dtStats.lakStats["laserIndirectHits","t"]);
      %file.writeLine("laserInDmgTaken" @ "%t" @ %dtStats.lakStats["laserInDmgTaken","t"]);
      %file.writeLine("mortarInDmg" @ "%t" @ %dtStats.lakStats["mortarInDmg","t"]);
      %file.writeLine("mortarIndirectHits" @ "%t" @ %dtStats.lakStats["mortarIndirectHits","t"]);
      %file.writeLine("mortarInDmgTaken" @ "%t" @ %dtStats.lakStats["mortarInDmgTaken","t"]);
      %file.writeLine("missileInDmg" @ "%t" @ %dtStats.lakStats["missileInDmg","t"]);
      %file.writeLine("missileIndirectHits" @ "%t" @ %dtStats.lakStats["missileIndirectHits","t"]);
      %file.writeLine("missileInDmgTaken" @ "%t" @ %dtStats.lakStats["missileInDmgTaken","t"]);
      %file.writeLine("shockLanceInDmg" @ "%t" @ %dtStats.lakStats["shockLanceInDmg","t"]);
      %file.writeLine("shockLanceIndirectHits" @ "%t" @ %dtStats.lakStats["shockLanceIndirectHits","t"]);
      %file.writeLine("shockLanceInDmgTaken" @ "%t" @ %dtStats.lakStats["shockLanceInDmgTaken","t"]);
      %file.writeLine("plasmaInDmg" @ "%t" @ %dtStats.lakStats["plasmaInDmg","t"]);
      %file.writeLine("plasmaIndirectHits" @ "%t" @ %dtStats.lakStats["plasmaIndirectHits","t"]);
      %file.writeLine("plasmaInDmgTaken" @ "%t" @ %dtStats.lakStats["plasmaInDmgTaken","t"]);
      %file.writeLine("blasterInDmg" @ "%t" @ %dtStats.lakStats["blasterInDmg","t"]);
      %file.writeLine("blasterIndirectHits" @ "%t" @ %dtStats.lakStats["blasterIndirectHits","t"]);
      %file.writeLine("blasterInDmgTaken" @ "%t" @ %dtStats.lakStats["blasterInDmgTaken","t"]);
      %file.writeLine("elfInDmg" @ "%t" @ %dtStats.lakStats["elfInDmg","t"]);
      %file.writeLine("elfIndirectHits" @ "%t" @ %dtStats.lakStats["elfIndirectHits","t"]);
      %file.writeLine("elfInDmgTaken" @ "%t" @ %dtStats.lakStats["elfInDmgTaken","t"]);
      %file.writeLine("unknownInDmg" @ "%t" @ %dtStats.lakStats["unknownInDmg","t"]);
      %file.writeLine("unknownIndirectHits" @ "%t" @ %dtStats.lakStats["unknownIndirectHits","t"]);
      %file.writeLine("unknownInDmgTaken" @ "%t" @ %dtStats.lakStats["unknownInDmgTaken","t"]);
      %file.writeLine("cgShotsFired" @ "%t" @ %dtStats.lakStats["cgShotsFired","t"]);
      %file.writeLine("discShotsFired" @ "%t" @ %dtStats.lakStats["discShotsFired","t"]);
      %file.writeLine("grenadeShotsFired" @ "%t" @ %dtStats.lakStats["grenadeShotsFired","t"]);
      %file.writeLine("laserShotsFired" @ "%t" @ %dtStats.lakStats["laserShotsFired","t"]);
      %file.writeLine("mortarShotsFired" @ "%t" @ %dtStats.lakStats["mortarShotsFired","t"]);
      %file.writeLine("missileShotsFired" @ "%t" @ %dtStats.lakStats["missileShotsFired","t"]);
      %file.writeLine("shockLanceShotsFired" @ "%t" @ %dtStats.lakStats["shockLanceShotsFired","t"]);
      %file.writeLine("plasmaShotsFired" @ "%t" @ %dtStats.lakStats["plasmaShotsFired","t"]);
      %file.writeLine("blasterShotsFired" @ "%t" @ %dtStats.lakStats["blasterShotsFired","t"]);
      %file.writeLine("elfShotsFired" @ "%t" @ %dtStats.lakStats["elfShotsFired","t"]);
      %file.writeLine("unknownShotsFired" @ "%t" @ %dtStats.lakStats["unknownShotsFired","t"]);
      
      %file.close();
   }
   %file.delete();
}
function loadCTFTotalStats(%dtStats){
   if($dtStats::Enable  == 0){return;}
   %file = new FileObject();
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/CTF/" @ %dtStats.guid @ "/" @ "totalStats" @ ".cs";
      if(isFile(%filename)){
         %file.OpenForRead(%filename);
         while( !%file.isEOF() ){
            %line = %file.readline();
            %line = strreplace(%line,"%t","\t");
            %var = trim(getField(%line,0));
            %val = trim(getField(%line,1));
            if(%var $= "ctfTotalNumGames"){
               %dtStats.ctfTotalNumGames = %val;
            }
            else{
               if(%val > 2000000000){//
                  %val = 0;
                  //error(%val);
               }
               %dtStats.ctfStats[%var,"t"] =  %val;
            }
         }
         %file.close();
      }
   }
   %file.delete();
}
function loadLAKTotalStats(%dtStats){
   if($dtStats::Enable  == 0){return;}
   %file = new FileObject();
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/Lak/" @ %dtStats.guid @ "/" @ "totalStats" @ ".cs";
      if(isFile(%filename)){
         %file.OpenForRead(%filename);
         while( !%file.isEOF() ){
            %line = %file.readline();
            %line = strreplace(%line,"%t","\t");
            %var = trim(getField(%line,0));
            %val = trim(getField(%line,1));
            if(%var $= "lakTotalNumGames"){
               %dtStats.lakTotalNumGames = %val;
            }
            else{
               if(%val > 2000000000){
                  %val = 0;
                  //error(%val);
               }
               %dtStats.lakStats[%var,"t"] =  %val;
            }
         }
         %file.close();
      }
   }
   %file.delete();
}
function initWepStats(%client){ // start them at 0 instead of ""
   if($dtStats::Enable  == 0){return;}
   %client.cgKills = 0;
   %client.cgDeaths = 0;
   %client.discKills = 0;
   %client.discDeaths = 0;
   %client.grenadeKills = 0;
   %client.grenadeDeaths = 0;
   %client.laserKills = 0;
   %client.laserDeaths = 0;
   %client.mortarKills = 0;
   %client.mortarDeaths = 0;
   %client.missileKills = 0;
   %client.missileDeaths = 0;
   %client.shockLanceKills = 0;
   %client.shockLanceDeaths = 0;
   %client.plasmaKills = 0;
   %client.plasmaDeaths = 0;
   %client.blasterKills = 0;
   %client.blasterDeaths = 0;
   %client.elfKills = 0;
   %client.elfDeaths = 0;
   %client.mineKills = 0;
   %client.mineDeaths = 0;
   %client.explosionKills = 0;
   %client.explosionDeaths = 0;
   %client.impactKills = 0;
   %client.impactDeaths = 0;
   %client.groundKills = 0;
   %client.groundDeaths = 0;
   %client.turretKills = 0;
   %client.turretDeaths = 0;
   %client.plasmaTurretKills = 0;
   %client.plasmaTurretDeaths = 0;
   %client.aaTurretKills = 0;
   %client.aaTurretDeaths = 0;
   %client.elfTurretKills = 0;
   %client.elfTurretDeaths = 0;
   %client.mortarTurretKills = 0;
   %client.mortarTurretDeaths = 0;
   %client.missileTurretKills = 0;
   %client.missileTurretDeaths = 0;
   %client.indoorDepTurretKills = 0;
   %client.indoorDepTurretDeaths = 0;
   %client.outdoorDepTurretKills = 0;
   %client.outdoorDepTurretDeaths = 0;
   %client.sentryTurretKills = 0;
   %client.sentryTurretDeaths = 0;
   %client.outOfBoundKills = 0;
   %client.outOfBoundDeaths = 0;
   %client.lavaKills = 0;
   %client.lavaDeaths = 0;
   %client.shrikeBlasterKills = 0;
   %client.shrikeBlasterDeaths = 0;
   %client.bellyTurretKills = 0;
   %client.bellyTurretDeaths = 0;
   %client.bomberBombsKills = 0;
   %client.bomberBombsDeaths = 0;
   %client.tankChaingunKills = 0;
   %client.tankChaingunDeaths = 0;
   %client.tankMortarKills = 0;
   %client.tankMortarDeaths = 0;
   %client.satchelChargeKills = 0;
   %client.satchelChargeDeaths = 0;
   %client.mpbMissileKills = 0;
   %client.mpbMissileDeaths = 0;
   %client.lightningKills = 0;
   %client.lightningDeaths = 0;
   %client.vehicleSpawnKills = 0;
   %clVictim.vehicleSpawnDeaths = 0;
   %client.forceFieldPowerUpKills = 0;
   %client.forceFieldPowerUpDeaths = 0;
   %client.crashKills = 0;
   %client.crashDeaths = 0;
   %client.waterKills = 0;
   %client.waterDeaths = 0;
   %client.nexusCampingKills = 0;
   %client.nexusCampingDeaths = 0;
   %client.unknownKill = 0;
   %client.unknownDeaths = 0;
   
   %client.cgDmg = 0;
   %client.cgDirectHits = 0;
   %client.cgDmgTaken = 0;
   %client.discDmg = 0;
   %client.discDirectHits = 0;
   %client.discDmgTaken = 0;
   %client.grenadeDmg = 0;
   %client.grenadeDirectHits = 0;
   %client.grenadeDmgTaken = 0;
   %client.laserDmg = 0;
   %client.laserDirectHits = 0;
   %client.laserDmgTaken = 0;
   %client.mortarDmg = 0;
   %client.mortarDirectHits = 0;
   %client.mortarDmgTaken = 0;
   %client.missileDmg = 0;
   %client.missileDirectHits = 0;
   %client.missileDmgTaken = 0;
   %client.shockLanceDmg = 0;
   %client.shockLanceDirectHits = 0;
   %client.shockLanceDmgTaken = 0;
   %client.plasmaDmg = 0;
   %client.plasmaDirectHits = 0;
   %client.plasmaDmgTaken = 0;
   %client.blasterDmg = 0;
   %client.blasterDirectHits = 0;
   %client.blasterDmgTaken = 0;
   %client.elfDmg = 0;
   %client.elfDirectHits = 0;
   %client.elfDmgTaken = 0;
   %client.unknownDmg = 0;
   %client.unknownDirectHits = 0;
   %client.unknownDmgTaken = 0;
   %client.cgInDmg = 0;
   %client.cgIndirectHits = 0;
   %client.cgInDmgTaken = 0;
   %client.discInDmg = 0;
   %client.discIndirectHits = 0;
   %client.discInDmgTaken = 0;
   %client.grenadeInDmg = 0;
   %client.grenadeIndirectHits = 0;
   %client.grenadeInDmgTaken = 0;
   %client.laserInDmg = 0;
   %client.laserIndirectHits = 0;
   %client.laserInDmgTaken = 0;
   %client.mortarInDmg = 0;
   %client.mortarIndirectHits = 0;
   %client.mortarInDmgTaken = 0;
   %client.missileInDmg = 0;
   %client.missileIndirectHits = 0;
   %client.missileInDmgTaken = 0;
   %client.shockLanceInDmg = 0;
   %client.shockLanceIndirectHits = 0;
   %client.shockLanceInDmgTaken = 0;
   %client.plasmaInDmg = 0;
   %client.plasmaIndirectHits = 0;
   %client.plasmaInDmgTaken = 0;
   %client.blasterInDmg = 0;
   %client.blasterIndirectHits = 0;
   %client.blasterInDmgTaken = 0;
   %client.elfInDmg = 0;
   %client.elfIndirectHits = 0;
   %client.elfInDmgTaken = 0;
   %client.unknownInDmg = 0;
   %client.unknownIndirectHits = 0;
   %client.unknownInDmgTaken = 0;
   %client.cgShotsFired = 0;
   %client.discShotsFired = 0;
   %client.grenadeShotsFired = 0;
   %client.laserShotsFired = 0;
   %client.mortarShotsFired = 0;
   %client.missileShotsFired = 0;
   %client.shockLanceShotsFired = 0;
   %client.plasmaShotsFired = 0;
   %client.blasterShotsFired = 0;
   %client.elfShotsFired = 0;
   %client.minePlusDisc = 0;
   %client.unknownShotsFired = 0;
}
function clientKillStats(%game, %clVictim, %clKiller, %damageType, %damageLocation){
   if($dtStats::Enable  == 0){return;}
   if(%clKiller.client.team != %clVictim.client.team){
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %clKiller.cgKills++;
            %clVictim.cgDeaths++;
         case $DamageType::Disc:
            %clKiller.discKills++;
            %clVictim.discDeaths++;
         case $DamageType::Grenade:
            %clKiller.grenadeKills++;
            %clVictim.grenadeDeaths++;
         case $DamageType::Laser:
            %clKiller.laserKills++;
            %clVictim.laserDeaths++;
         case $DamageType::Mortar:
            %clKiller.mortarKills++;
            %clVictim.mortarDeaths++;
         case $DamageType::Missile:
            %clKiller.missileKills++;
            %clVictim.missileDeaths++;
         case $DamageType::ShockLance:
            %clKiller.shockLanceKills++;
            %clVictim.shockLanceDeaths++;
         case $DamageType::Plasma:
            %clKiller.plasmaKills++;
            %clVictim.plasmaDeaths++;
         case $DamageType::Blaster:
            %clKiller.blasterKills++;
            %clVictim.blasterDeaths++;
         case $DamageType::ELF:
            %clKiller.elfKills++;
            %clVictim.elfDeaths++;
         case $DamageType::Mine:
            %clKiller.mineKills++;
            %clVictim.mineDeaths++;
         case $DamageType::Explosion:
            %clKiller.explosionKills++;
            %clVictim.explosionDeaths++;
         case $DamageType::Impact:
            %clKiller.impactKills++;
            %clVictim.impactDeaths++;
         case $DamageType::Ground:
            %clKiller.groundKills++;
            %clVictim.groundDeaths++;
         case $DamageType::Turret:
            %clKiller.turretKills++;
            %clVictim.turretDeaths++;
         case $DamageType::PlasmaTurret:
            %clKiller.plasmaTurretKills++;
            %clVictim.plasmaTurretDeaths++;
         case $DamageType::AATurret:
            %clKiller.aaTurretKills++;
            %clVictim.aaTurretDeaths++;
         case $DamageType::ElfTurret:
            %clKiller.elfTurretKills++;
            %clVictim.elfTurretDeaths++;
         case $DamageType::MortarTurret:
            %clKiller.mortarTurretKills++;
            %clVictim.mortarTurretDeaths++;
         case $DamageType::MissileTurret:
            %clKiller.missileTurretKills++;
            %clVictim.missileTurretDeaths++;
         case $DamageType::IndoorDepTurret:
            %clKiller.indoorDepTurretKills++;
            %clVictim.indoorDepTurretDeaths++;
         case $DamageType::OutdoorDepTurret:
            %clKiller.outdoorDepTurretKills++;
            %clVictim.outdoorDepTurretDeaths++;
         case $DamageType::SentryTurret:
            %clKiller.sentryTurretKills++;
            %clVictim.sentryTurretDeaths++;
         case $DamageType::OutOfBounds:
            %clKiller.outOfBoundKills++;
            %clVictim.outOfBoundDeaths++;
         case $DamageType::Lava:
            %clKiller.lavaKills++;
            %clVictim.lavaDeaths++;
         case $DamageType::ShrikeBlaster:
            %clKiller.shrikeBlasterKills++;
            %clVictim.shrikeBlasterDeaths++;
         case $DamageType::BellyTurret:
            %clKiller.bellyTurretKills++;
            %clVictim.bellyTurretDeaths++;
         case $DamageType::BomberBombs:
            %clKiller.bomberBombsKills++;
            %clVictim.bomberBombsDeaths++;
         case $DamageType::TankChaingun:
            %clKiller.tankChaingunKills++;
            %clVictim.tankChaingunDeaths++;
         case $DamageType::TankMortar:
            %clKiller.tankMortarKills++;
            %clVictim.tankMortarDeaths++;
         case $DamageType::SatchelCharge:
            %clKiller.satchelChargeKills++;
            %clVictim.satchelChargeDeaths++;
         case $DamageType::MPBMissile:
            %clKiller.mpbMissileKills++;
            %clVictim.mpbMissileDeaths++;
         case $DamageType::Lightning:
            %clKiller.lightningKills++;
            %clVictim.lightningDeaths++;
         case $DamageType::VehicleSpawn:
            %clKiller.vehicleSpawnKills++;
            %clVictim.vehicleSpawnDeaths++;
         case $DamageType::ForceFieldPowerup:
            %clKiller.forceFieldPowerUpKills++;
            %clVictim.forceFieldPowerUpDeaths++;
         case $DamageType::Crash:
            %clKiller.crashKills++;
            %clVictim.crashDeaths++;
         case $DamageType::Water:
            %clKiller.waterKills++;
            %clVictim.waterDeaths++;
         case $DamageType::NexusCamping:
            %clKiller.nexusCampingKills++;
            %clVictim.nexusCampingDeaths++;
         default:
            %clKiller.unknownKill++;
            %clVictim.unknownDeaths++;
      }
   }
}

function clientDirectDmgStats(%game, %data, %projectile, %targetObject){ // projectile
   if($dtStats::Enable  == 0){return;}
       // echo(isObject(%targetObject) SPC %targetObject.getClassName() SPC %targetObject.client.team SPC %projectile.sourceObject.client.team);
   if(isObject(%targetObject) && %targetObject.getClassName() $= "Player" && %targetObject.client.team != %projectile.sourceObject.client.team){
      if(%data.directDamageType !$= ""){
         %damageType = %data.directDamageType;
         %amount = %data.directDamage;
      }
      else{
         %damageType =  %data.radiusDamageType;
         %amount = %data.indirectDamage;// counts as full
      }
      %armorData = %targetObject.getDatablock();
      %damageScale = %armorData.damageScale[%damageType];
      if(%damageScale !$= "")
         %amount *= %damageScale;
      %client = %projectile.sourceObject.client;
      %targetClient = %targetObject.client;
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %client.cgDmg += %amount;
            %client.cgDirectHits++;
            %targetClient.cgDmgTaken += %amount;
         case $DamageType::Disc:
            %client.discDmg += %amount;
            %client.discDirectHits++;
            %targetClient.discDmgTaken += %amount;
         case $DamageType::Grenade:
            %client.grenadeDmg += %amount;
            %client.grenadeDirectHits++;
            %targetClient.grenadeDmgTaken += %amount;
         case $DamageType::Laser:
            %client.laserDmg += %amount;
            %client.laserDirectHits++;
            %targetClient.laserDmgTaken += %amount;
         case $DamageType::Mortar:
            %client.mortarDmg += %amount;
            %client.mortarDirectHits++;
            %targetClient.mortarDmgTaken += %amount;
         case $DamageType::Missile:
            %client.missileDmg += %amount;
            %client.missileDirectHits++;
            %targetClient.missileDmgTaken += %amount;
         case $DamageType::ShockLance:
            %client.shockLanceDmg += %amount;
            %client.shockLanceDirectHits++;
            %targetClient.shockLanceDmgTaken += %amount;
         case $DamageType::Plasma:
            %client.plasmaDmg += %amount;
            %client.plasmaDirectHits++;
            %targetClient.plasmaDmgTaken += %amount;
         case $DamageType::Blaster:
            %client.blasterDmg += %amount;
            %client.blasterDirectHits++;
            %targetClient.blasterDmgTaken += %amount;
         case $DamageType::ELF:
            %client.elfDmg += %amount;
            %client.elfDirectHits++;
            %targetClient.elfDmgTaken += %amount;
         default:
            %client.unknownDmg += %amount;
            %client.unknownDirectHits++;
            %targetClient.unknownDmgTaken += %amount;
      }
   }
}

function clientIndirectDmgStats(%game,%data,%sourceObject, %targetObject, %damageType,%amount){
   // echo(%data SPC %sourceObject SPC %targetObject SPC %damageType SPC %amount);
   //error(%damageType SPC %targetObject SPC %targetObject.client.mineDisc );
   //error(getObjectTypeMask(%targetObject));
   if($dtStats::Enable  == 0){return;}
   if(isObject(%targetObject) && %targetObject.getClassName() $= "Player" && %sourceObject.client.team != %targetObject.client.team){  // only care about pvp
      %damageScale = %data.damageScale[%damageType];
      if(%damageScale !$= ""){
         %amount *= %damageScale;
      }
      %client = %sourceObject.client;
      %targetClient = %targetObject.client;
      //echo(%damageType SPC %targetClient SPC %targetClient.mineDisc);
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %client.cgInDmg += %amount;
            %client.cgIndirectHits++;
            %targetClient.cgInDmgTaken += %amount;
         case $DamageType::Disc:
            %client.discInDmg += %amount;
            %client.discIndirectHits++;
            %targetClient.discInDmgTaken += %amount;
            if(%targetClient.mineDisc){
               %client.minePlusDisc++;
            }
         case $DamageType::Mine:
            if(%targetClient.mineDisc){
               %client.minePlusDisc++;
            }
         case $DamageType::Grenade:
            %client.grenadeInDmg += %amount;
            %client.grenadeIndirectHits++;
            %targetClient.grenadeInDmgTaken += %amount;
         case $DamageType::Laser:
            %client.laserInDmg += %amount;
            %client.laserIndirectHits++;
            %targetClient.laserInDmgTaken += %amount;
         case $DamageType::Mortar:
            %client.mortarInDmg += %amount;
            %client.mortarIndirectHits++;
            %targetClient.mortarInDmgTaken += %amount;
         case $DamageType::Missile:
            %client.missileInDmg += %amount;
            %client.missileIndirectHits++;
            %targetClient.missileInDmgTaken += %amount;
         case $DamageType::ShockLance:
            %client.shockLanceInDmg += %amount;
            %client.shockLanceIndirectHits++;
            %targetClient.shockLanceInDmgTaken += %amount;
         case $DamageType::Plasma:
            %client.plasmaInDmg += %amount;
            %client.plasmaIndirectHits++;
            %targetClient.plasmaInDmgTaken += %amount;
         case $DamageType::Blaster:
            %client.blasterInDmg += %amount;
            %client.blasterIndirectHits++;
            %targetClient.blasterInDmgTaken += %amount;
         case $DamageType::ELF:
            %client.elfInDmg += %amount;
            %client.elfIndirectHits++;
            %targetClient.elfInDmgTaken += %amount;
         default:
            %client.unknownInDmg += %amount;
            %client.unknownIndirectHits++;
            %targetClient.unknownInDmgTaken += %amount;
      }
   }
}
function clientShotsFired(%game, %data, %projectile){ // could do a fov check to see if we are trying to aim at a player
   if($dtStats::Enable  == 0){return;}
   %client = %projectile.sourceObject.client;
   if(!isObject(%client) || %client.isAiControlled()){ return;}
   if(%data.directDamageType !$= ""){
      %damageType = %data.directDamageType;
   }
   else{
      %damageType =  %data.radiusDamageType;
   }
   // echo(%damageType);
   switch$(%damageType){// list of all damage types to track see damageTypes.cs
      case $DamageType::Bullet:
         %client.cgShotsFired++;
      case $DamageType::Disc:
         %client.discShotsFired++;
      case $DamageType::Grenade:
         %client.grenadeShotsFired++;
      case $DamageType::Laser:
         %client.laserShotsFired++;
      case $DamageType::Mortar:
         %client.mortarShotsFired++;
      case $DamageType::Missile:
         %client.missileShotsFired++;
      case $DamageType::ShockLance:
         %client.shockLanceShotsFired++;
      case $DamageType::Plasma:
         %client.plasmaShotsFired++;
      case $DamageType::Blaster:
         %client.blasterShotsFired++;
      case $DamageType::ELF:
         %client.elfShotsFired++;
      default:
         %client.unknownShotsFired++;
   }
}
function getCtfRunAvg(%client, %value){
   %c = 0;
   if(%client.dtStats.ctfGameCount != 0 && %client.dtStats.ctfGameCount !$= ""){
      for(%i=1; %i <= %client.dtStats.ctfGameCount; %i++){
         if(!$dtStats::skipZeros){
            %val += %client.dtStats.ctfStats[%value,%i];
         }
         else if(%client.dtStats.ctfStats[%value,%i] != 0 && %client.dtStats.ctfStats[%value,%i] !$= ""){
            %val += %client.dtStats.ctfStats[%value,%i];
            %c++;
         }
      }
      if(!$dtStats::skipZeros)
         return %val / %client.dtStats.ctfGameCount;
      else if(%c > 0)
         return %val / %c;
      else
         return 0;
   }
   else{
      return 0;
   }
}
function getCtfTotalAvg(%vClient,%value){
   //error(%vClient SPC %value);
   if(%vClient.dtStats.ctfStats[%value,"t"] !$= "" && %vClient.dtStats.ctfTotalNumGames > 0)
      %totalAvg = %vClient.dtStats.ctfStats[%value,"t"] / %vClient.dtStats.ctfTotalNumGames;
   else
      %totalAvg = 0;

   return %totalAvg;
}
function getCtfTotal(%vClient,%value){
   %total = %vClient.dtStats.ctfStats[%value,"t"];
   if(%total !$= ""){
      return %total;
   }
   else{
      return 0;
   }
}
function getCtfGameDetails(%vClient,%value,%game){
   %total = %vClient.dtStats.ctfStats[%value,%game];
   if(%total !$= ""){
      return %total;
   }
   else{
      return 0;
   }
}
/////////////////////////////////////////
function getLakRunAvg(%client, %value){
   %c = 0;
   if(%client.dtStats.lakGameCount != 0 && %client.dtStats.lakGameCount !$= ""){
      for(%i=1; %i <= %client.dtStats.lakGameCount; %i++){
         if(!$dtStats::skipZeros){
            %val += %client.dtStats.lakStats[%value,%i];
         }
         else if(%client.dtStats.lakStats[%value,%i] != 0 && %client.dtStats.lakStats[%value,%i] !$= ""){
            %val += %client.dtStats.lakStats[%value,%i];
            %c++;
         }
      }
      if(!$dtStats::skipZeros)
         return %val / %client.dtStats.lakGameCount;
      else if(%c > 0)
         return %val / %c;
      else
         return 0;
   }
   else{
      return 0;
   }
}
function getLakTotalAvg(%vClient,%value){
   if(%vClient.dtStats.lakStats[%value,"t"] !$= "" && %vClient.dtStats.lakTotalNumGames > 0)
      %totalAvg = %vClient.dtStats.lakStats[%value,"t"] / %vClient.dtStats.lakTotalNumGames;
   else
      %totalAvg = 0;
   return %totalAvg;
}
function getLakTotal(%vClient,%value){
   %total = %vClient.dtStats.lakStats[%value,"t"];
   if(%total !$= ""){
      return %total;
   }
   else{
      return 0;
   }
}
function getLakGameDetails(%vClient,%value,%game){
   %total = %vClient.dtStats.lakStats[%value,%game];
   if(%total !$= ""){
      return %total;
   }
   else{
      return 0;
   }
}
//$twbbitMap[1] = "twb/twb_action_01";
//$twbbitMap[2] = "twb/twb_action_02";
//$twbbitMap[3] = "twb/twb_action_03";
//$twbbitMap[4] = "twb/twb_action_04";
//$twbbitMap[5] = "twb/twb_action_05";
//$twbbitMap[6] = "twb/twb_action_06";
//$twbbitMap[7] = "twb/twb_action_07";
//$twbbitMap[8] = "twb/twb_action_08";
//$twbbitMap[9] = "twb/twb_action_09";
//$twbbitMap[10] = "twb/twb_action_10";
//$twbbitMap[11] = "twb/twb_BE_FLight";
//$twbbitMap[12] = "twb/twb_BE_FMed";
//$twbbitMap[13] = "twb/twb_BE_Heavy";
//$twbbitMap[14] = "twb/twb_BE_MLight";
//$twbbitMap[15] = "twb/twb_BE_MMed";
//$twbbitMap[16] = "twb/twb_Bioderm";
//$twbbitMap[17] = "twb/twb_Bioderm_Light";
//$twbbitMap[18] = "twb/twb_Bioderm_Medium";
//$twbbitMap[19] = "twb/twb_Blaster";
//$twbbitMap[20] = "twb/twb_BloodEagle";
//$twbbitMap[21] = "twb/twb_blowngen_01";
//$twbbitMap[22] = "twb/twb_DiamondSword";
//$twbbitMap[23] = "twb/twb_DS_FLight";
//$twbbitMap[24] = "twb/twb_DS_Fmed";
//$twbbitMap[25] = "twb/twb_DS_Heavy";
//$twbbitMap[26] = "twb/twb_DS_MMed";
//$twbbitMap[27] = "twb/twb_Harbingers";
//$twbbitMap[28] = "twb/twb_Havoc";
//$twbbitMap[29] = "twb/twb_HR_FLight";
//$twbbitMap[30] = "twb/twb_HR_FMed";
//$twbbitMap[31] = "twb/twb_HR_Heavy";
//$twbbitMap[32] = "twb/twb_HR_MLight";
//$twbbitMap[33] = "twb/twb_HR_MMed";
//$twbbitMap[34] = "twb/twb_inferno_01";
//$twbbitMap[35] = "twb/twb_inferno_02";
//$twbbitMap[36] = "twb/twb_inferno_03";
//$twbbitMap[37] = "twb/twb_lakedebris_01";
//$twbbitMap[38] = "twb/twb_lakedebris_03";
//$twbbitMap[39] = "twb/twb_Lineup";
//$twbbitMap[40] = "twb/twb_Shrike";
//$twbbitMap[41] = "twb/twb_soclose";
//$twbbitMap[42] = "twb/twb_starwolf_fem";
//$twbbitMap[43] = "twb/twb_starwolf_shrike";
//$twbbitMap[44] = "twb/twb_Starwolves";
//$twbbitMap[45] = "twb/twb_SW_FLight";
//$twbbitMap[46] = "twb/twb_SW_FMedium";
//$twbbitMap[47] = "twb/twb_SW_Heavy";
//$twbbitMap[48] = "twb/twb_SW_MLight";
//$twbbitMap[49] = "twb/twb_SW_MMed";
//$twbbitMap[50] = "twb/twb_Thundersword";
//$twbbitMap[51] = "twb/twb_TRIBES2";
//$twbbitMap[52] = "twb/twb_wateraction_01";
//$twbbitMap[53] = "twb/twb_waterdemise_01";
//$twbbitMap[54] = "twb/twb_waterdemise_03";
//$twbbitMap[55] = "twb/twb_waterdemise_04";
//$twbbitMap[56] = "twb/twb_woohoo_01";

function statsMenu(%client,%game){
   if($dtStats::Enable  == 0){
      %client.viewMenu = 0;
      %client.viewClient = 0;
      %client.viewStats = 0;
      return;
   }
   %menu = %client.viewMenu; 
   cancel(%client.rtmt); // if new action  then restart timer 
   %client.rtmt = schedule($dtStats::returnToMenuTimer,0,"menuReset",%client);
   %vClient = %client.viewClient;
   %tag = 'scoreScreen';
   //error(%menu SPC %vClient);
   
   %isTargetSelf = (%client == %vClient);
   %isAdmin = (%client.isAdmin || %client.isSuperAdmin);
   
   messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );
   %index = -1;
   if(%game $= "CTFGame" || %game $= "LakRabbitGame")
   {
      switch$(%menu)
	  {
         case "View":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase@ "'s Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", "<a:gamelink\tStats\tReset>Return To Score Screen</a>");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>  Main Options Menu");
            if(%game $= "CTFGame")
			{
				messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTF\t%1>  + CTF Match Stats</a>',%vClient);
				if(%isTargetSelf || %isAdmin)
				{
					messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTFW\t%1>  + CTF Weapon Stats</a>',%vClient);
					messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTFA\t%1>  + CTF Kills/Deaths</a>',%vClient);

					//messageClient( %client, 'SetLineHud', "", %tag, %index++, '(CTF Games Played = %2) (CTF Running Average %3/%4) (OW %5)',%vClient,%vClient.dtStats.ctfTotalNumGames,%vClient.dtStats.ctfGameCount,$dtStats::MaxNumOfGames,%vClient.dtStats.ctfStatsOverWrite);

				}
			}
			//messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            if(%game $= "LakRabbitGame")
			{
				messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLak\t%1>  + Lak Match Stats</a>',%vClient);
				if(%isTargetSelf || %isAdmin)
				{
					messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLAKW\t%1>  + Lak Weapon Stats</a>',%vClient);
					//messageClient( %client, 'SetLineHud', "", %tag, %index++, '(LakRabbit Games Played = %2) (LakRabbit Running Average %3/%4) (OW %5)',%vClient,%vClient.dtStats.lakTotalNumGames,%vClient.dtStats.lakGameCount,$dtStats::MaxNumOfGames,%vClient.dtStats.lakStatsOverWrite);
				}
			}
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			if(%isTargetSelf || %isAdmin)
			{
				messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTFH\t%1>  + Previous CTF Games</a>',%vClient);
				messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLAKH\t%1>  + Previous Lak Games</a>',%vClient);
			}
			
			if(%game $= "LakRabbitGame") //CTF has extra line
				messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");	
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");	
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");	
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");	
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");	
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");	
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");	
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");		
			messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>Updates are at the start of every new map.');
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Based on the last" SPC $dtStats::MaxNumOfGames SPC "games.");			
         case "LAKHIST":
             %game = %client.GlArg4;
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.lakStats["timeStamp",%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKH\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175>Game Details<lmargin:330>Totals<lmargin:450>TA Per Game";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            %line = '<color:0befe7><lmargin%:0>Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"score",%game),getLakTotal(%vClient,"score"),mCeil(getLakTotalAvg(%vClient,"score")));
            %line = '<color:0befe7>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"kills",%game),getLakTotal(%vClient,"kills"),mCeil(getLakTotalAvg(%vClient,"kills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"deaths",%game),getLakTotal(%vClient,"deaths"),mCeil(getLakTotalAvg(%vClient,"deaths")));
            %line = '<color:0befe7>Suicides<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"suicides")),getLakTotal(%vClient,"suicides"),mCeil(getLakTotalAvg(%vClient,"suicides")));
            %line = '<color:0befe7>Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"flagGrabs",%game),getLakTotal(%vClient,"flagGrabs"),mCeil(getLakTotalAvg(%vClient,"flagGrabs")));
            %line = '<color:0befe7>Flag Time Min<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"flagTimeMS",%game),getLakTotal(%vClient,"flagTimeMS"),mCeil(getLakTotalAvg(%vClient,"flagTimeMS")));
            %line = '<color:0befe7>Bonus Points<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"morepoints",%game),getLakTotal(%vClient,"morepoints"),mCeil(getLakTotalAvg(%vClient,"morepoints")));
            %line = '<color:0befe7>Mid-Airs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"mas",%game),getLakTotal(%vClient,"mas"),mCeil(getLakTotalAvg(%vClient,"mas")));
            %line = '<color:0befe7>Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"minePlusDisc",%game),getLakTotal(%vClient,"minePlusDisc"),mCeil(getLakTotalAvg(%vClient,"minePlusDisc")));
            %line = '<color:0befe7>Total Speed<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"totalSpeed",%game),getLakTotal(%vClient,"totalSpeed"),mCeil(getLakTotalAvg(%vClient,"totalSpeed")));
            %line = '<color:0befe7>Total Distance<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"totalDistance",%game),getLakTotal(%vClient,"totalDistance"),mCeil(getLakTotalAvg(%vClient,"totalDistance")));
            //%line = '<color:0befe7>Total Chain Accuracy<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"totalChainAccuracy",%game),getLakTotal(%vClient,"totalChainAccuracy"),mCeil(getLakTotalAvg(%vClient,"totalChainAccuracy")));
            //%line = '<color:0befe7>Total Chain Hits Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"totalChainHits",%game),getLakTotal(%vClient,"totalChainHits"),mCeil(getLakTotalAvg(%vClient,"totalChainHits")));
            //%line = '<color:0befe7>Total Snipe Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"totalSnipeHits",%game),getLakTotal(%vClient,"totalSnipeHits"),mCeil(getLakTotalAvg(%vClient,"totalSnipeHits")));
            //%line = '<color:0befe7>Total Snipes<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"totalSnipes",%game),getLakTotal(%vClient,"totalSnipes"),mCeil(getLakTotalAvg(%vClient,"totalSnipes")));
            %line = '<color:0befe7>Total Shock Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"totalShockHits",%game),getLakTotal(%vClient,"totalShockHits"),mCeil(getLakTotalAvg(%vClient,"totalShockHits")));
            %line = '<color:0befe7>Total Shocks<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getLakGameDetails(%vClient,"totalShocks",%game),getLakTotal(%vClient,"totalShocks"),mCeil(getLakTotalAvg(%vClient,"totalShocks")));
         case "LAKW":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Weapon Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            //%header = "<color:0befe7>Weapons";
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            //%line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tChaingunLAK\t%1> View Chaingun Stats</a><lmargin:230> <bitmap:%2>';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,$twbbitMap[getRandom(1,56)]);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tSpinfusorLAK\t%1>  + Spinfusor Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tGrenadeLauncherLAK\t%1>  + Grenade Launcher Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            //%line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tLaserRifleLAK\t%1> View Laser Rifle Stats</a>';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tFusionMortarLAK\t%1>  + Fusion Mortar Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            //%line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tMissileLauncherLAK\t%1> View Missile Launcher Stats</a>';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tShocklanceLAK\t%1>  + Shocklance Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tPlasmaRifleLAK\t%1>  + Plasma Rifle Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tBlasterLAK\t%1>  + Blaster Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            //%line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tELFLAK\t%1> View ELF Projector Stats</a>';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         case "LAKH":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s Lak History");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center><bitmap:Flag_T2><bitmap:Flag_Beagle><bitmap:Flag_Bioderm><bitmap:Flag_DSword><bitmap:Flag_Phoenix><bitmap:Flag_Starwolf><bitmap:Flag_T2>");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Game history is set to" SPC $dtStats::MaxNumOfGames SPC "games.");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>The oldest game will be overwritten.");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");			
            if(%vClient.dtStats.lakStatsOverWrite > 0){
               for(%b = %vClient.dtStats.lakStatsOverWrite; %b <= %vClient.dtStats.lakGameCount; %b++){
                  %timeDate = %vClient.dtStats.lakStats["timeStamp",%b];
                 // echo(%timeDate SPC %b SPC 1);
                  if(%b == %vClient.dtStats.lakStatsOverWrite){
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3>  + %2</a> <color:02d404><just:center>This game will be overwritten',%vClient,%timeDate,%b);
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3>  + %2</a> ',%vClient,%timeDate,%b);
                  }
               }
               for(%z = 1; %z < %vClient.dtStats.lakStatsOverWrite; %z++){
                  %timeDate = %vClient.dtStats.lakStats["timeStamp",%z];
                 // echo(%timeDate SPC %b SPC 2);
                  if(%z == %vClient.dtStats.lakStatsOverWrite){
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3>  + %2</a> <color:02d404><just:center>This game will be overwritten',%vClient,%timeDate,%z);
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3>  + %2</a> ',%vClient,%timeDate,%z);
                  }
               }
            }
            else{
               for(%z = 1; %z <= %vClient.dtStats.lakGameCount; %z++){
                  %timeDate = %vClient.dtStats.lakStats["timeStamp",%z];
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3>  + %2</a> ',%vClient,%timeDate,%z);
               }
            }
         case "Lak":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175>Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            %line = '<color:0befe7><lmargin%:0>Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"score")),getLakTotal(%vClient,"score"),mCeil(getLakTotalAvg(%vClient,"score")));
            %line = '<color:0befe7>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"kills")),getLakTotal(%vClient,"kills"),mCeil(getLakTotalAvg(%vClient,"kills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"deaths")),getLakTotal(%vClient,"deaths"),mCeil(getLakTotalAvg(%vClient,"deaths")));
            %line = '<color:0befe7>Suicides<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"suicides")),getLakTotal(%vClient,"suicides"),mCeil(getLakTotalAvg(%vClient,"suicides")));
            %line = '<color:0befe7>Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"flagGrabs")),getLakTotal(%vClient,"flagGrabs"),mCeil(getLakTotalAvg(%vClient,"flagGrabs")));
            %line = '<color:0befe7>Flag Time Min<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"flagTimeMS")),getLakTotal(%vClient,"flagTimeMS"),mCeil(getLakTotalAvg(%vClient,"flagTimeMS")));
            %line = '<color:0befe7>Bonus Points<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"morepoints")),getLakTotal(%vClient,"morepoints"),mCeil(getLakTotalAvg(%vClient,"morepoints")));
            %line = '<color:0befe7>Mid-Airs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mas")),getLakTotal(%vClient,"mas"),mCeil(getLakTotalAvg(%vClient,"mas")));
            %line = '<color:0befe7>Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"minedisc")),getLakTotal(%vClient,"minePlusDisc"),mCeil(getLakTotalAvg(%vClient,"minePlusDisc")));
            %line = '<color:0befe7>Total Speed<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"totalSpeed")),getLakTotal(%vClient,"totalSpeed"),mCeil(getLakTotalAvg(%vClient,"totalSpeed")));
            %line = '<color:0befe7>Total Distance<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"totalDistance")),getLakTotal(%vClient,"totalDistance"),mCeil(getLakTotalAvg(%vClient,"totalDistance")));
            //%line = '<color:0befe7>Total Chain Accuracy<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"totalChainAccuracy")),getLakTotal(%vClient,"totalChainAccuracy"),mCeil(getLakTotalAvg(%vClient,"totalChainAccuracy")));
            //%line = '<color:0befe7>Total Chain Hits Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"totalChainHits")),getLakTotal(%vClient,"totalChainHits"),mCeil(getLakTotalAvg(%vClient,"totalChainHits")));
            //%line = '<color:0befe7>Total Snipe Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"totalSnipeHits")),getLakTotal(%vClient,"totalSnipeHits"),mCeil(getLakTotalAvg(%vClient,"totalSnipeHits")));
            //%line = '<color:0befe7>Total Snipes<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"totalSnipes")),getLakTotal(%vClient,"totalSnipes"),mCeil(getLakTotalAvg(%vClient,"totalSnipes")));
            %line = '<color:0befe7>Total Shock Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"totalShockHits")),getLakTotal(%vClient,"totalShockHits"),mCeil(getLakTotalAvg(%vClient,"totalShockHits")));
            %line = '<color:0befe7>Total Shocks<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"totalShocks")),getLakTotal(%vClient,"totalShocks"),mCeil(getLakTotalAvg(%vClient,"totalShocks")));
            
         case "CTFA":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s Kills/Deaths");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            %a1 = getCtfTotal(%vClient,"cgKills"); %b2 = getCtfTotal(%vClient,"cgDeaths"); %c3 = getCtfTotal(%vClient,"discKills"); %d4 = getCtfTotal(%vClient,"discDeaths"); %e5 = getCtfTotal(%vClient,"grenadeKills"); %f6 = getCtfTotal(%vClient,"grenadeDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Chaingun: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Spinfusor: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Grenade Launcher: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"laserKills"); %b2 = getCtfTotal(%vClient,"laserDeaths"); %c3 = getCtfTotal(%vClient,"mortarKills"); %d4 = getCtfTotal(%vClient,"mortarDeaths"); %e5 = getCtfTotal(%vClient,"shockLanceKills"); %f6 = getCtfTotal(%vClient,"shockLanceDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Laser Rifle: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Fusion Mortar: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Shocklance: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"plasmaKills"); %b2 = getCtfTotal(%vClient,"plasmaDeaths"); %c3 = getCtfTotal(%vClient,"blasterKills"); %d4 = getCtfTotal(%vClient,"blasterDeaths"); %e5 = getCtfTotal(%vClient,"elfKills"); %f6 = getCtfTotal(%vClient,"elfDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Plasma Rifle: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Blaster: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>ELF Projector: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "-----------------------------------------------------------------------------------------------------------------");
            
            %a1 = getCtfTotal(%vClient,"mineKills"); %b2 = getCtfTotal(%vClient,"mineDeaths"); %c3 = getCtfTotal(%vClient,"explosionKills"); %d4 = getCtfTotal(%vClient,"explosionDeaths"); %e5 = getCtfTotal(%vClient,"impactKills"); %f6 = getCtfTotal(%vClient,"impactDeaths");
            %line = '<font:univers condensed:18><font:univers condensed:18><color:0befe7><lmargin:0>Mines: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Explosion: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Impact: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"groundKills"); %b2 = getCtfTotal(%vClient,"groundDeaths"); %c3 = getCtfTotal(%vClient,"turretKills"); %d4 = getCtfTotal(%vClient,"turretDeaths"); %e5 = getCtfTotal(%vClient,"plasmaTurretKills"); %f6 = getCtfTotal(%vClient,"plasmaTurretDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Ground: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Turret: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Plasma Turret: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"aaTurretKills"); %b2 = getCtfTotal(%vClient,"aaTurretDeaths"); %c3 = getCtfTotal(%vClient,"elfTurretKills"); %d4 = getCtfTotal(%vClient,"elfTurretDeaths"); %e5 = getCtfTotal(%vClient,"mortarTurretKills"); %f6 = getCtfTotal(%vClient,"mortarTurretDeaths");
            %line = '<font:univers condensed:18><font:univers condensed:18><color:0befe7><lmargin:0>AA Turret: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>ELF Turret: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Mortar Turret: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"missileTurretKills"); %b2 = getCtfTotal(%vClient,"missileTurretDeaths"); %c3 = getCtfTotal(%vClient,"indoorDepTurretKills"); %d4 = getCtfTotal(%vClient,"indoorDepTurretDeaths"); %e5 = getCtfTotal(%vClient,"outdoorDepTurretKills"); %f6 = getCtfTotal(%vClient,"outdoorDepTurretDeaths");
            %line = '<font:univers condensed:18><font:univers condensed:18><color:0befe7><lmargin:0>Missile Turret: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Spider Camp Turret: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Land Spike Turret: <color:02d404>%5k/%6d';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            
            %a1 = getCtfTotal(%vClient,"sentryTurretKills"); %b2 = getCtfTotal(%vClient,"sentryTurretDeaths"); %c3 = getCtfTotal(%vClient,"outOfBoundKills"); %d4 = getCtfTotal(%vClient,"outOfBoundDeaths"); %e5 = getCtfTotal(%vClient,"lavaKills"); %f6 = getCtfTotal(%vClient,"lavaDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Sentry Turret: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Out Of Bounds: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Lava: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"shrikeBlasterKills"); %b2 = getCtfTotal(%vClient,"shrikeBlasterDeaths"); %c3 = getCtfTotal(%vClient,"bellyTurretKills"); %d4 = getCtfTotal(%vClient,"bellyTurretDeaths"); %e5 = getCtfTotal(%vClient,"bomberBombsKills"); %f6 = getCtfTotal(%vClient,"bomberBombsDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Shrike Blaster: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Bomber Turret: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Bomber Bombs: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"tankChaingunKills"); %b2 = getCtfTotal(%vClient,"tankChaingunDeaths"); %c3 = getCtfTotal(%vClient,"tankMortarKills"); %d4 = getCtfTotal(%vClient,"tankMortarDeaths"); %e5 = getCtfTotal(%vClient,"mpbMissileKills"); %f6 = getCtfTotal(%vClient,"mpbMissileDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Tank Chaingun: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Tank Mortar: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>MPB Missile: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            
            %a1 = getCtfTotal(%vClient,"satchelChargeKills"); %b2 = getCtfTotal(%vClient,"satchelChargeDeaths"); %c3 = getCtfTotal(%vClient,"lightningKills"); %d4 = getCtfTotal(%vClient,"lightningDeaths"); %e5 = getCtfTotal(%vClient,"vehicleSpawnKills"); %f6 = getCtfTotal(%vClient,"vehicleSpawnDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Satchel Charge: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Lightning: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Vehicle Spawn: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"forceFieldPowerUpKills"); %b2 = getCtfTotal(%vClient,"forceFieldPowerUpDeaths"); %c3 = getCtfTotal(%vClient,"crashKills"); %d4 = getCtfTotal(%vClient,"crashDeaths"); %e5 = getCtfTotal(%vClient,"waterKills"); %f6 = getCtfTotal(%vClient,"waterDeaths");
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Forcefield Power: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Crash: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Water: <color:02d404>%5k/%6d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            %a1 = getCtfTotal(%vClient,"nexusCampingKills"); %b2 = getCtfTotal(%vClient,"nexusCampingDeaths"); %c3 = getCtfTotal(%vClient,"unknownKill"); %d4 = getCtfTotal(%vClient,"unknownDeaths"); %e5 = 0; %f6 = 0;
            %line = '<font:univers condensed:18><color:0befe7><lmargin:0>Nexus Camping: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Unknown??: <color:02d404>%3k/%4d<color:0befe7>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
            
         case "CTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"kills")),getCtfTotal(%vClient,"kills"),mCeil(getCtfTotalAvg(%vClient,"kills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"deaths")),getCtfTotal(%vClient,"deaths"),mCeil(getCtfTotalAvg(%vClient,"deaths")));
            %line = '<color:0befe7>Mid-Air<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"scoreMidAir")),getCtfTotal(%vClient,"scoreMidAir"),mCeil(getCtfTotalAvg(%vClient,"scoreMidAir")));
            %line = '<color:0befe7>Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"minePlusDisc")),getCtfTotal(%vClient,"minePlusDisc"),mCeil(getCtfTotalAvg(%vClient,"minePlusDisc")));
            %line = '<color:0befe7>Flag Caps<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"flagCaps")),getCtfTotal(%vClient,"flagCaps"),mCeil(getCtfTotalAvg(%vClient,"flagCaps")));
            %line = '<color:0befe7>Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"flagGrabs")),getCtfTotal(%vClient,"flagGrabs"),mCeil(getCtfTotalAvg(%vClient,"flagGrabs")));
            %line = '<color:0befe7>Carrier Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"carrierKills")),getCtfTotal(%vClient,"carrierKills"),mCeil(getCtfTotalAvg(%vClient,"carrierKills")));
            %line = '<color:0befe7>Flag Returns<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"flagReturns")),getCtfTotal(%vClient,"flagReturns"),mCeil(getCtfTotalAvg(%vClient,"flagReturns")));
            %line = '<color:0befe7>Flag Assists<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"escortAssists")),getCtfTotal(%vClient,"escortAssists"),mCeil(getCtfTotalAvg(%vClient,"escortAssists")));
            %line = '<color:0befe7>Flag Defends<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"flagDefends")),getCtfTotal(%vClient,"flagDefends"),mCeil(getCtfTotalAvg(%vClient,"flagDefends")));
            %line = '<color:0befe7>Offense Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"offenseScore")),getCtfTotal(%vClient,"offenseScore"),mCeil(getCtfTotalAvg(%vClient,"offenseScore")));
            %line = '<color:0befe7>Defense Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"defenseScore")),getCtfTotal(%vClient,"defenseScore"),mCeil(getCtfTotalAvg(%vClient,"defenseScore")));
            %line = '<color:0befe7>Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"score")),getCtfTotal(%vClient,"score"),mCeil(getCtfTotalAvg(%vClient,"score")));
            %line = '<color:0befe7>Rearshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"scoreRearshot")),getCtfTotal(%vClient,"scoreRearshot"),mCeil(getCtfTotalAvg(%vClient,"scoreRearshot")));
            %line = '<color:0befe7>Headshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"scoreHeadshot")),getCtfTotal(%vClient,"scoreHeadshot"),mCeil(getCtfTotalAvg(%vClient,"scoreHeadshot")));
         case "CTFW":// Weapons
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Weapon Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            //%header = "<color:0befe7>Weapons";
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tChaingunCTF\t%1>  + Chaingun Stats</a><lmargin:230> <bitmap:%2>';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,$twbbitMap[getRandom(1,56)]);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tSpinfusorCTF\t%1>  + Spinfusor Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tGrenadeLauncherCTF\t%1>  + Grenade Launcher Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tLaserRifleCTF\t%1>  + Laser Rifle Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tFusionMortarCTF\t%1>  + Fusion Mortar Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tMissileLauncherCTF\t%1>  + Missile Launcher Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tShocklanceCTF\t%1>  + Shocklance Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tPlasmaRifleCTF\t%1>  + Plasma Rifle Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tBlasterCTF\t%1>  + Blaster Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tELFCTF\t%1>  + ELF Projector Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            
         case "CTFH":// Past Games
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s CTF History");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:Logo_small_beagle><bitmap:Logo_small_bioderm><bitmap:Logo_small_DSword><bitmap:Logo_small_Inferno><bitmap:Logo_small_Phoenix><bitmap:Logo_small_Starwolf><bitmap:Logo_small_Storm>");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Game history is set to" SPC $dtStats::MaxNumOfGames SPC "games.");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>The oldest game will be overwritten.");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");			
            if(%vClient.dtStats.ctfStatsOverWrite > 0){
               for(%b = %vClient.dtStats.ctfStatsOverWrite; %b <= %vClient.dtStats.ctfGameCount; %b++){
                  %timeDate = %vClient.dtStats.ctfStats["timeStamp",%b];
                 // echo(%timeDate SPC %b);
                  if(%b == %vClient.dtStats.ctfStatsOverWrite){
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3>  + %2</a> <color:02d404><just:center>This game will be overwritten',%vClient,%timeDate,%b);
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3>  + %2</a> ',%vClient,%timeDate,%b);
                  }
               }
               for(%z = 1; %z < %vClient.dtStats.ctfStatsOverWrite; %z++){
                  %timeDate = %vClient.dtStats.ctfStats["timeStamp",%z];
                  if(%z == %vClient.dtStats.ctfStatsOverWrite){
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3>  + %2</a> <color:02d404><just:center>This game will be overwritten',%vClient,%timeDate,%z);
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3>%2</a> ',%vClient,%timeDate,%z);
                  }
               }
            }
            else{
               for(%z = 1; %z <= %vClient.dtStats.ctfGameCount; %z++){
                  %timeDate = %vClient.dtStats.ctfStats["timeStamp",%z];
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3>  + %2</a> ',%vClient,%timeDate,%z);
               }
            }
         case "CTFHist":
            %game = %client.GlArg4;
            error(%game);
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.ctfStats["timeStamp",%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFH\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175>Game Details<lmargin:330>Totals<lmargin:450>TA Per Game";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"kills",%game),getCtfTotal(%vClient,"kills"),mCeil(getCtfTotalAvg(%vClient,"kills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"deaths",%game),getCtfTotal(%vClient,"deaths"),mCeil(getCtfTotalAvg(%vClient,"deaths")));
            %line = '<color:0befe7>Mid-Air<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"scoreMidAir",%game),getCtfTotal(%vClient,"scoreMidAir"),mCeil(getCtfTotalAvg(%vClient,"scoreMidAir")));
            %line = '<color:0befe7>Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"minePlusDisc",%game),getCtfTotal(%vClient,"minePlusDisc"),mCeil(getCtfTotalAvg(%vClient,"minePlusDisc")));
            %line = '<color:0befe7>Flag Caps<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"flagCaps",%game),getCtfTotal(%vClient,"flagCaps"),mCeil(getCtfTotalAvg(%vClient,"flagCaps")));
            %line = '<color:0befe7>Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"flagGrabs",%game),getCtfTotal(%vClient,"flagGrabs"),mCeil(getCtfTotalAvg(%vClient,"flagGrabs")));
            %line = '<color:0befe7>Carrier Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"carrierKills",%game),getCtfTotal(%vClient,"carrierKills"),mCeil(getCtfTotalAvg(%vClient,"carrierKills")));
            %line = '<color:0befe7>Flag Returns<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"flagReturns",%game),getCtfTotal(%vClient,"flagReturns"),mCeil(getCtfTotalAvg(%vClient,"flagReturns")));
            %line = '<color:0befe7>Flag Assists<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"escortAssists",%game),getCtfTotal(%vClient,"escortAssists"),mCeil(getCtfTotalAvg(%vClient,"escortAssists")));
            %line = '<color:0befe7>Flag Defends<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"flagDefends",%game),getCtfTotal(%vClient,"flagDefends"),mCeil(getCtfTotalAvg(%vClient,"flagDefends")));
            %line = '<color:0befe7>Offense Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"offenseScore",%game),getCtfTotal(%vClient,"offenseScore"),mCeil(getCtfTotalAvg(%vClient,"offenseScore")));
            %line = '<color:0befe7>Defense Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"defenseScore",%game),getCtfTotal(%vClient,"defenseScore"),mCeil(getCtfTotalAvg(%vClient,"defenseScore")));
            %line = '<color:0befe7>Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"score",%game),getCtfTotal(%vClient,"score"),mCeil(getCtfTotalAvg(%vClient,"score")));
            %line = '<color:0befe7>Rearshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"scoreRearshot",%game),getCtfTotal(%vClient,"scoreRearshot"),mCeil(getCtfTotalAvg(%vClient,"scoreRearshot")));
            %line = '<color:0befe7>Headshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getCtfGameDetails(%vClient,"scoreHeadshot",%game),getCtfTotal(%vClient,"scoreHeadshot"),mCeil(getCtfTotalAvg(%vClient,"scoreHeadshot")));
         case "BlasterCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Blaster Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"blasterKills")),getCtfTotal(%vClient,"blasterKills"),mCeil(getCtfTotalAvg(%vClient,"blasterKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"blasterDeaths")),getCtfTotal(%vClient,"blasterDeaths"),mCeil(getCtfTotalAvg(%vClient,"blasterDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"blasterDmg")),getCtfTotal(%vClient,"blasterDmg"),mCeil(getCtfTotalAvg(%vClient,"blasterDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"blasterDmgTaken")),getCtfTotal(%vClient,"blasterDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"blasterDmgTaken")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"blasterDirectHits")),getCtfTotal(%vClient,"blasterDirectHits"),mCeil(getCtfTotalAvg(%vClient,"blasterDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"blasterShotsFired")),getCtfTotal(%vClient,"blasterShotsFired"),mCeil(getCtfTotalAvg(%vClient,"blasterShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Blaster>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "SpinfusorCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Spinfusor Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discKills")),getCtfTotal(%vClient,"discKills"),mCeil(getCtfTotalAvg(%vClient,"discKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discDeaths")),getCtfTotal(%vClient,"discDeaths"),mCeil(getCtfTotalAvg(%vClient,"discDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discDmg")),getCtfTotal(%vClient,"discDmg"),mCeil(getCtfTotalAvg(%vClient,"discDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discDmgTaken")),getCtfTotal(%vClient,"discDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"discDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discInDmg")),getCtfTotal(%vClient,"discInDmg"),mCeil(getCtfTotalAvg(%vClient,"discInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discInDmgTaken")),getCtfTotal(%vClient,"discInDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"discInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discIndirectHits")),getCtfTotal(%vClient,"discIndirectHits"),mCeil(getCtfTotalAvg(%vClient,"discIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discDirectHits")),getCtfTotal(%vClient,"discDirectHits"),mCeil(getCtfTotalAvg(%vClient,"discDirectHits")));
            %line = '<color:0befe7>Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"discShotsFired")),getCtfTotal(%vClient,"discShotsFired"),mCeil(getCtfTotalAvg(%vClient,"discShotsFired")));
            %line = '<color:0befe7>Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"minePlusDisc")),getCtfTotal(%vClient,"minePlusDisc"),mCeil(getCtfTotalAvg(%vClient,"minePlusDisc")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Spinfusor>");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "ChaingunCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Chaingun Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"cgKills")),getCtfTotal(%vClient,"cgKills"),mCeil(getCtfTotalAvg(%vClient,"cgKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"cgDeaths")),getCtfTotal(%vClient,"cgDeaths"),mCeil(getCtfTotalAvg(%vClient,"cgDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"cgDmg")),getCtfTotal(%vClient,"cgDmg"),mCeil(getCtfTotalAvg(%vClient,"cgDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"cgDmgTaken")),getCtfTotal(%vClient,"cgDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"cgDmgTaken")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"cgDirectHits")),getCtfTotal(%vClient,"cgDirectHits"),mCeil(getCtfTotalAvg(%vClient,"cgDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"cgShotsFired")),getCtfTotal(%vClient,"cgShotsFired"),mCeil(getCtfTotalAvg(%vClient,"cgShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Chaingun>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "GrenadeLauncherCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Grenade Launcher Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeKills")),getCtfTotal(%vClient,"grenadeKills"),mCeil(getCtfTotalAvg(%vClient,"grenadeKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeDeaths")),getCtfTotal(%vClient,"grenadeDeaths"),mCeil(getCtfTotalAvg(%vClient,"grenadeDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeDmg")),getCtfTotal(%vClient,"grenadeDmg"),mCeil(getCtfTotalAvg(%vClient,"grenadeDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeDmgTaken")),getCtfTotal(%vClient,"grenadeDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"grenadeDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeInDmg")),getCtfTotal(%vClient,"grenadeInDmg"),mCeil(getCtfTotalAvg(%vClient,"grenadeInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeInDmgTaken")),getCtfTotal(%vClient,"grenadeInDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"grenadeInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeIndirectHits")),getCtfTotal(%vClient,"grenadeIndirectHits"),mCeil(getCtfTotalAvg(%vClient,"grenadeIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeDirectHits")),getCtfTotal(%vClient,"grenadeDirectHits"),mCeil(getCtfTotalAvg(%vClient,"grenadeDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"grenadeShotsFired")),getCtfTotal(%vClient,"grenadeShotsFired"),mCeil(getCtfTotalAvg(%vClient,"grenadeShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Grenadelauncher>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "LaserRifleCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Laser Rifle Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"laserKills")),getCtfTotal(%vClient,"laserKills"),mCeil(getCtfTotalAvg(%vClient,"laserKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"laserDeaths")),getCtfTotal(%vClient,"laserDeaths"),mCeil(getCtfTotalAvg(%vClient,"laserDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"laserDmg")),getCtfTotal(%vClient,"laserDmg"),mCeil(getCtfTotalAvg(%vClient,"laserDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"laserDmgTaken")),getCtfTotal(%vClient,"laserDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"laserDmgTaken")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"laserDirectHits")),getCtfTotal(%vClient,"laserDirectHits"),mCeil(getCtfTotalAvg(%vClient,"laserDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"laserShotsFired")),getCtfTotal(%vClient,"laserShotsFired"),mCeil(getCtfTotalAvg(%vClient,"laserShotsFired")));
            %line = '<color:0befe7>Head Shots <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"scoreHeadshot")),getCtfTotal(%vClient,"scoreHeadshot"),mCeil(getCtfTotalAvg(%vClient,"scoreHeadshot")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Laserrifle>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            
         case "FusionMortarCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Fusion Mortar Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarKills")),getCtfTotal(%vClient,"mortarKills"),mCeil(getCtfTotalAvg(%vClient,"mortarKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarDeaths")),getCtfTotal(%vClient,"mortarDeaths"),mCeil(getCtfTotalAvg(%vClient,"mortarDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarDmg")),getCtfTotal(%vClient,"mortarDmg"),mCeil(getCtfTotalAvg(%vClient,"mortarDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarDmgTaken")),getCtfTotal(%vClient,"mortarDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"mortarDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarInDmg")),getCtfTotal(%vClient,"mortarInDmg"),mCeil(getCtfTotalAvg(%vClient,"mortarInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarInDmgTaken")),getCtfTotal(%vClient,"mortarInDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"mortarInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarIndirectHits")),getCtfTotal(%vClient,"mortarIndirectHits"),mCeil(getCtfTotalAvg(%vClient,"mortarIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarDirectHits")),getCtfTotal(%vClient,"mortarDirectHits"),mCeil(getCtfTotalAvg(%vClient,"mortarDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"mortarShotsFired")),getCtfTotal(%vClient,"mortarShotsFired"),mCeil(getCtfTotalAvg(%vClient,"mortarShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Fusionmortar>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "MissileLauncherCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Missile Launcher Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileKills")),getCtfTotal(%vClient,"missileKills"),mCeil(getCtfTotalAvg(%vClient,"missileKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileDeaths")),getCtfTotal(%vClient,"missileDeaths"),mCeil(getCtfTotalAvg(%vClient,"missileDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileDmg")),getCtfTotal(%vClient,"missileDmg"),mCeil(getCtfTotalAvg(%vClient,"missileDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileDmgTaken")),getCtfTotal(%vClient,"missileDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"missileDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileInDmg")),getCtfTotal(%vClient,"missileInDmg"),mCeil(getCtfTotalAvg(%vClient,"missileInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileInDmgTaken")),getCtfTotal(%vClient,"missileInDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"missileInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileIndirectHits")),getCtfTotal(%vClient,"missileIndirectHits"),mCeil(getCtfTotalAvg(%vClient,"missileIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileDirectHits")),getCtfTotal(%vClient,"missileDirectHits"),mCeil(getCtfTotalAvg(%vClient,"missileDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"missileShotsFired")),getCtfTotal(%vClient,"missileShotsFired"),mCeil(getCtfTotalAvg(%vClient,"missileShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Missilelauncher>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "ShocklanceCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Shocklance Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"shockLanceKills")),getCtfTotal(%vClient,"shockLanceKills"),mCeil(getCtfTotalAvg(%vClient,"shockLanceKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"shockLanceDeaths")),getCtfTotal(%vClient,"shockLanceDeaths"),mCeil(getCtfTotalAvg(%vClient,"shockLanceDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"shockLanceDmg")),getCtfTotal(%vClient,"shockLanceDmg"),mCeil(getCtfTotalAvg(%vClient,"shockLanceDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"shockLanceDmgTaken")),getCtfTotal(%vClient,"shockLanceDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"shockLanceDmgTaken")));
            
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"shockLanceDirectHits")),getCtfTotal(%vClient,"shockLanceDirectHits"),mCeil(getCtfTotalAvg(%vClient,"shockLanceDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"shockLanceShotsFired")),getCtfTotal(%vClient,"shockLanceShotsFired"),mCeil(getCtfTotalAvg(%vClient,"shockLanceShotsFired")));
            %line = '<color:0befe7>Rearshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"scoreRearshot")),getCtfTotal(%vClient,"scoreRearshot"),mCeil(getCtfTotalAvg(%vClient,"scoreRearshot")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_shocklance>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "PlasmaRifleCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaKills")),getCtfTotal(%vClient,"plasmaKills"),mCeil(getCtfTotalAvg(%vClient,"plasmaKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaDeaths")),getCtfTotal(%vClient,"plasmaDeaths"),mCeil(getCtfTotalAvg(%vClient,"plasmaDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaDmg")),getCtfTotal(%vClient,"plasmaDmg"),mCeil(getCtfTotalAvg(%vClient,"plasmaDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaDmgTaken")),getCtfTotal(%vClient,"plasmaDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"plasmaDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaInDmg")),getCtfTotal(%vClient,"plasmaInDmg"),mCeil(getCtfTotalAvg(%vClient,"plasmaInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaInDmgTaken")),getCtfTotal(%vClient,"plasmaInDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"plasmaInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaIndirectHits")),getCtfTotal(%vClient,"plasmaIndirectHits"),mCeil(getCtfTotalAvg(%vClient,"plasmaIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaDirectHits")),getCtfTotal(%vClient,"plasmaDirectHits"),mCeil(getCtfTotalAvg(%vClient,"plasmaDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"plasmaShotsFired")),getCtfTotal(%vClient,"plasmaShotsFired"),mCeil(getCtfTotalAvg(%vClient,"plasmaShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Plasmarifle>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "ELFCTF":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>ELF Projector Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            //%line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"elfKills")),getCtfTotal(%vClient,"elfKills"),mCeil(getCtfTotalAvg(%vClient,"elfKills")));
            //%line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"elfDeaths")),getCtfTotal(%vClient,"elfDeaths"),mCeil(getCtfTotalAvg(%vClient,"elfDeaths")));
            //%line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"elfDmg")),getCtfTotal(%vClient,"elfDmg"),mCeil(getCtfTotalAvg(%vClient,"elfDmg")));
            //%line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"elfDmgTaken")),getCtfTotal(%vClient,"elfDmgTaken"),mCeil(getCtfTotalAvg(%vClient,"elfDmgTaken")));
            //
            //%line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"elfDirectHits")),getCtfTotal(%vClient,"elfDirectHits"),mCeil(getCtfTotalAvg(%vClient,"elfDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getCtfRunAvg(%vClient,"elfShotsFired")),getCtfTotal(%vClient,"elfShotsFired"),mCeil(getCtfTotalAvg(%vClient,"elfShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Elfprojector>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "BlasterLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Blaster Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"blasterKills")),getLakTotal(%vClient,"blasterKills"),mCeil(getLakTotalAvg(%vClient,"blasterKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"blasterDeaths")),getLakTotal(%vClient,"blasterDeaths"),mCeil(getLakTotalAvg(%vClient,"blasterDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"blasterDmg")),getLakTotal(%vClient,"blasterDmg"),mCeil(getLakTotalAvg(%vClient,"blasterDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"blasterDmgTaken")),getLakTotal(%vClient,"blasterDmgTaken"),mCeil(getLakTotalAvg(%vClient,"blasterDmgTaken")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"blasterDirectHits")),getLakTotal(%vClient,"blasterDirectHits"),mCeil(getLakTotalAvg(%vClient,"blasterDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"blasterShotsFired")),getLakTotal(%vClient,"blasterShotsFired"),mCeil(getLakTotalAvg(%vClient,"blasterShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Blaster>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "SpinfusorLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Spinfusor Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discKills")),getLakTotal(%vClient,"discKills"),mCeil(getLakTotalAvg(%vClient,"discKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discDeaths")),getLakTotal(%vClient,"discDeaths"),mCeil(getLakTotalAvg(%vClient,"discDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discDmg")),getLakTotal(%vClient,"discDmg"),mCeil(getLakTotalAvg(%vClient,"discDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discDmgTaken")),getLakTotal(%vClient,"discDmgTaken"),mCeil(getLakTotalAvg(%vClient,"discDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discInDmg")),getLakTotal(%vClient,"discInDmg"),mCeil(getLakTotalAvg(%vClient,"discInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discInDmgTaken")),getLakTotal(%vClient,"discInDmgTaken"),mCeil(getLakTotalAvg(%vClient,"discInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discIndirectHits")),getLakTotal(%vClient,"discIndirectHits"),mCeil(getLakTotalAvg(%vClient,"discIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discDirectHits")),getLakTotal(%vClient,"discDirectHits"),mCeil(getLakTotalAvg(%vClient,"discDirectHits")));
            %line = '<color:0befe7>Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"discShotsFired")),getLakTotal(%vClient,"discShotsFired"),mCeil(getLakTotalAvg(%vClient,"discShotsFired")));
            %line = '<color:0befe7>Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"minePlusDisc")),getLakTotal(%vClient,"minePlusDisc"),mCeil(getLakTotalAvg(%vClient,"minePlusDisc")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Spinfusor>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "ChaingunLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Chaingun Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"cgKills")),getLakTotal(%vClient,"cgKills"),mCeil(getLakTotalAvg(%vClient,"cgKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"cgDeaths")),getLakTotal(%vClient,"cgDeaths"),mCeil(getLakTotalAvg(%vClient,"cgDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"cgDmg")),getLakTotal(%vClient,"cgDmg"),mCeil(getLakTotalAvg(%vClient,"cgDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"cgDmgTaken")),getLakTotal(%vClient,"cgDmgTaken"),mCeil(getLakTotalAvg(%vClient,"cgDmgTaken")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"cgDirectHits")),getLakTotal(%vClient,"cgDirectHits"),mCeil(getLakTotalAvg(%vClient,"cgDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"cgShotsFired")),getLakTotal(%vClient,"cgShotsFired"),mCeil(getLakTotalAvg(%vClient,"cgShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Chaingun>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "GrenadeLauncherLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Grenade Launcher Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeKills")),getLakTotal(%vClient,"grenadeKills"),mCeil(getLakTotalAvg(%vClient,"grenadeKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeDeaths")),getLakTotal(%vClient,"grenadeDeaths"),mCeil(getLakTotalAvg(%vClient,"grenadeDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeDmg")),getLakTotal(%vClient,"grenadeDmg"),mCeil(getLakTotalAvg(%vClient,"grenadeDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeDmgTaken")),getLakTotal(%vClient,"grenadeDmgTaken"),mCeil(getLakTotalAvg(%vClient,"grenadeDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeInDmg")),getLakTotal(%vClient,"grenadeInDmg"),mCeil(getLakTotalAvg(%vClient,"grenadeInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeInDmgTaken")),getLakTotal(%vClient,"grenadeInDmgTaken"),mCeil(getLakTotalAvg(%vClient,"grenadeInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeIndirectHits")),getLakTotal(%vClient,"grenadeIndirectHits"),mCeil(getLakTotalAvg(%vClient,"grenadeIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeDirectHits")),getLakTotal(%vClient,"grenadeDirectHits"),mCeil(getLakTotalAvg(%vClient,"grenadeDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"grenadeShotsFired")),getLakTotal(%vClient,"grenadeShotsFired"),mCeil(getLakTotalAvg(%vClient,"grenadeShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Grenadelauncher>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "LaserRifleLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Laser Rifle Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"laserKills")),getLakTotal(%vClient,"laserKills"),mCeil(getLakTotalAvg(%vClient,"laserKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"laserDeaths")),getLakTotal(%vClient,"laserDeaths"),mCeil(getLakTotalAvg(%vClient,"laserDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"laserDmg")),getLakTotal(%vClient,"laserDmg"),mCeil(getLakTotalAvg(%vClient,"laserDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"laserDmgTaken")),getLakTotal(%vClient,"laserDmgTaken"),mCeil(getLakTotalAvg(%vClient,"laserDmgTaken")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"laserDirectHits")),getLakTotal(%vClient,"laserDirectHits"),mCeil(getLakTotalAvg(%vClient,"laserDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"laserShotsFired")),getLakTotal(%vClient,"laserShotsFired"),mCeil(getLakTotalAvg(%vClient,"laserShotsFired")));
            %line = '<color:0befe7>Head Shots <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"scoreHeadshot")),getLakTotal(%vClient,"scoreHeadshot"),mCeil(getLakTotalAvg(%vClient,"scoreHeadshot")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Laserrifle>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            
         case "FusionMortarLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Fusion Mortar Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarKills")),getLakTotal(%vClient,"mortarKills"),mCeil(getLakTotalAvg(%vClient,"mortarKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarDeaths")),getLakTotal(%vClient,"mortarDeaths"),mCeil(getLakTotalAvg(%vClient,"mortarDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarDmg")),getLakTotal(%vClient,"mortarDmg"),mCeil(getLakTotalAvg(%vClient,"mortarDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarDmgTaken")),getLakTotal(%vClient,"mortarDmgTaken"),mCeil(getLakTotalAvg(%vClient,"mortarDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarInDmg")),getLakTotal(%vClient,"mortarInDmg"),mCeil(getLakTotalAvg(%vClient,"mortarInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarInDmgTaken")),getLakTotal(%vClient,"mortarInDmgTaken"),mCeil(getLakTotalAvg(%vClient,"mortarInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarIndirectHits")),getLakTotal(%vClient,"mortarIndirectHits"),mCeil(getLakTotalAvg(%vClient,"mortarIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarDirectHits")),getLakTotal(%vClient,"mortarDirectHits"),mCeil(getLakTotalAvg(%vClient,"mortarDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"mortarShotsFired")),getLakTotal(%vClient,"mortarShotsFired"),mCeil(getLakTotalAvg(%vClient,"mortarShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Fusionmortar>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "MissileLauncherLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Missile Launcher Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileKills")),getLakTotal(%vClient,"missileKills"),mCeil(getLakTotalAvg(%vClient,"missileKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileDeaths")),getLakTotal(%vClient,"missileDeaths"),mCeil(getLakTotalAvg(%vClient,"missileDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileDmg")),getLakTotal(%vClient,"missileDmg"),mCeil(getLakTotalAvg(%vClient,"missileDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileDmgTaken")),getLakTotal(%vClient,"missileDmgTaken"),mCeil(getLakTotalAvg(%vClient,"missileDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileInDmg")),getLakTotal(%vClient,"missileInDmg"),mCeil(getLakTotalAvg(%vClient,"missileInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileInDmgTaken")),getLakTotal(%vClient,"missileInDmgTaken"),mCeil(getLakTotalAvg(%vClient,"missileInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileIndirectHits")),getLakTotal(%vClient,"missileIndirectHits"),mCeil(getLakTotalAvg(%vClient,"missileIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileDirectHits")),getLakTotal(%vClient,"missileDirectHits"),mCeil(getLakTotalAvg(%vClient,"missileDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"missileShotsFired")),getLakTotal(%vClient,"missileShotsFired"),mCeil(getLakTotalAvg(%vClient,"missileShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Missilelauncher>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "ShocklanceLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Shocklance Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"shockLanceKills")),getLakTotal(%vClient,"shockLanceKills"),mCeil(getLakTotalAvg(%vClient,"shockLanceKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"shockLanceDeaths")),getLakTotal(%vClient,"shockLanceDeaths"),mCeil(getLakTotalAvg(%vClient,"shockLanceDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"shockLanceDmg")),getLakTotal(%vClient,"shockLanceDmg"),mCeil(getLakTotalAvg(%vClient,"shockLanceDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"shockLanceDmgTaken")),getLakTotal(%vClient,"shockLanceDmgTaken"),mCeil(getLakTotalAvg(%vClient,"shockLanceDmgTaken")));
            
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"shockLanceDirectHits")),getLakTotal(%vClient,"shockLanceDirectHits"),mCeil(getLakTotalAvg(%vClient,"shockLanceDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"shockLanceShotsFired")),getLakTotal(%vClient,"shockLanceShotsFired"),mCeil(getLakTotalAvg(%vClient,"shockLanceShotsFired")));
            %line = '<color:0befe7>Rearshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"scoreRearshot")),getLakTotal(%vClient,"scoreRearshot"),mCeil(getLakTotalAvg(%vClient,"scoreRearshot")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_shocklance>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "PlasmaRifleLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            %line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaKills")),getLakTotal(%vClient,"plasmaKills"),mCeil(getLakTotalAvg(%vClient,"plasmaKills")));
            %line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaDeaths")),getLakTotal(%vClient,"plasmaDeaths"),mCeil(getLakTotalAvg(%vClient,"plasmaDeaths")));
            %line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaDmg")),getLakTotal(%vClient,"plasmaDmg"),mCeil(getLakTotalAvg(%vClient,"plasmaDmg")));
            %line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaDmgTaken")),getLakTotal(%vClient,"plasmaDmgTaken"),mCeil(getLakTotalAvg(%vClient,"plasmaDmgTaken")));
            %line = '<color:0befe7>Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaInDmg")),getLakTotal(%vClient,"plasmaInDmg"),mCeil(getLakTotalAvg(%vClient,"plasmaInDmg")));
            %line = '<color:0befe7>Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaInDmgTaken")),getLakTotal(%vClient,"plasmaInDmgTaken"),mCeil(getLakTotalAvg(%vClient,"plasmaInDmgTaken")));
            
            %line = '<color:0befe7>Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaIndirectHits")),getLakTotal(%vClient,"plasmaIndirectHits"),mCeil(getLakTotalAvg(%vClient,"plasmaIndirectHits")));
            %line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaDirectHits")),getLakTotal(%vClient,"plasmaDirectHits"),mCeil(getLakTotalAvg(%vClient,"plasmaDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"plasmaShotsFired")),getLakTotal(%vClient,"plasmaShotsFired"),mCeil(getLakTotalAvg(%vClient,"plasmaShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Plasmarifle>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         case "ELFLAK":
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>ELF Projector Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
            
            %header = "<color:0befe7><lmargin:0>Stats<lmargin:175> Running Average<lmargin:330>Totals<lmargin:450>Totals Average";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
            
            //%line = '<color:0befe7><lmargin%:0>Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"elfKills")),getLakTotal(%vClient,"elfKills"),mCeil(getLakTotalAvg(%vClient,"elfKills")));
            //%line = '<color:0befe7>Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"elfDeaths")),getLakTotal(%vClient,"elfDeaths"),mCeil(getLakTotalAvg(%vClient,"elfDeaths")));
            //%line = '<color:0befe7>Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"elfDmg")),getLakTotal(%vClient,"elfDmg"),mCeil(getLakTotalAvg(%vClient,"elfDmg")));
            //%line = '<color:0befe7>Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"elfDmgTaken")),getLakTotal(%vClient,"elfDmgTaken"),mCeil(getLakTotalAvg(%vClient,"elfDmgTaken")));
            //
            //%line = '<color:0befe7>Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"elfDirectHits")),getLakTotal(%vClient,"elfDirectHits"),mCeil(getLakTotalAvg(%vClient,"elfDirectHits")));
            %line = '<color:0befe7>Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getLakRunAvg(%vClient,"elfShotsFired")),getLakTotal(%vClient,"elfShotsFired"),mCeil(getLakTotalAvg(%vClient,"elfShotsFired")));
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<bitmap:twb/twb_Elfprojector>");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         default://faill safe / reset
            %client.viewMenu = 0;
            %client.viewClient = 0;
            %client.viewStats = 0;
      }
   }
}
function menuReset(%client){
   //error("menuReset");
            %client.viewMenu = 0;
            %client.viewClient = 0;
            %client.viewStats = 0;
            
}
