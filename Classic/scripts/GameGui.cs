//------------------------------------------------------------------------------
//
// GameGui.cs
//
//------------------------------------------------------------------------------

// z0dd - ZOD: Execute the mission and game type skip lists so that 
// arrays are put into memory for function buildMissionList.
exec("prefs/MissionSkip.cs", true);
exec("prefs/GameTypeSkip.cs", true);

//------------------------------------------------------------------------------
function LaunchGame( %pane )
{
   if ( %pane !$= "" )
      GameGui.pane = %pane;

   LaunchTabView.viewTab( "GAME", GameGui, 0 );
}

//------------------------------------------------------------------------------
function GameGui::onWake( %this )
{
   Canvas.pushDialog( LaunchToolbarDlg );

   if ( $PlayingOnline ) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
      GM_Frame.setTitle( "GAME" );
   else
      GM_Frame.setTitle( "LAN GAME" );

   // This is essentially an "isInitialized" flag...
   if ( GM_TabView.tabCount() == 0 )
   {
      // ---------------------------------------------------
      // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
      GM_TabView.addTab( 1, "JOIN" );
      GM_TabView.addTab( 2, "HOST" );
      GM_TabView.addTab( 3, "WARRIOR SETUP", 1 );
      queryMasterGameTypes();
      // ---------------------------------------------------
   }

   switch$ ( %this.pane )
   {
      case "Join":
         GM_TabView.setSelected( 1 );
      case "Host":
         GM_TabView.setSelected( 2 );
      default: // "Warrior"
         GM_TabView.setSelected( 3 );
   }
}

//------------------------------------------------------------------------------
function GameGui::onSleep( %this )
{
   %ctrl = "GM_" @ %this.pane @ "Pane";
   if ( isObject( %ctrl ) )
      %ctrl.onDeactivate();
      
//   if( isObject( $dummySeq ) )
//   {   
//      $dummySeq.delete();
//   }
         
	Canvas.popDialog(LaunchToolbarDlg);
}

//------------------------------------------------------------------------------
function GameGui::setKey( %this, %key )
{
   // To avoid console error
}

//------------------------------------------------------------------------------
function GameGui::onClose( %this, %key )
{
   // To avoid console error
}

//------------------------------------------------------------------------------
function GM_TabView::onAdd( %this )
{
   %this.addSet( 1, "gui/shll_horztabbuttonB", "5 5 5", "50 50 0", "5 5 5" );
}

//------------------------------------------------------------------------------
function GM_TabView::onSelect( %this, %id, %text )
{
   GM_JoinPane.setVisible( %id == 1 );
   GM_HostPane.setVisible( %id == 2 );
   GM_WarriorPane.setVisible( %id == 3 );
   GM_TabFrame.setAltColor( %this.getTabSet( %id ) != 0 );

   %ctrl = "GM_" @ GameGui.pane @ "Pane";
   if ( isObject( %ctrl ) )
      %ctrl.onDeactivate();

   switch ( %id )
   {
      case 1:  // Join
         GM_JoinPane.onActivate();
      case 2:  // Host
         GM_HostPane.onActivate();
      case 3:  // Warrior Setup
         GM_WarriorPane.onActivate();
   }
}

//------------------------------------------------------------------------------
// Join Game pane:
//------------------------------------------------------------------------------
function GM_JoinPane::onActivate( %this )
{
   GameGui.pane = "Join";

   if ( %this.onceOnly $= "" )
   {
		GM_VersionText.setText( "Version" SPC getT2VersionNumber() );
      GMJ_StopBtn.setActive( false );

      %this.onceOnly = 1;
      // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
      GMJ_Browser.lastQuery = $PlayingOnline ? "Master" : "LanServers";
      GMJ_Browser.runQuery();
   }
   
   if ( isObject( BrowserMap ) )
   {
      BrowserMap.pop();
      BrowserMap.delete();
   }
   new ActionMap( BrowserMap );
   // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   BrowserMap.bindCmd( keyboard, insert, "GMJ_Browser.insertIPAddress();", "" );
   BrowserMap.bindCmd( keyboard, "ctrl f", "Canvas.pushDialog( FindServerDlg );", "" );
   BrowserMap.bindCmd( keyboard, F3, "GMJ_Browser.findNextServer();", "" );
   BrowserMap.push();

   GM_VersionText.setVisible( true ); // z0dd - ZOD, 9/29/02. Removed T2 demo code from here

   if ( $pref::ServerBrowser::InfoWindowOpen )
      Canvas.pushDialog( ServerInfoDlg );
}

//------------------------------------------------------------------------------
function GM_JoinPane::onDeactivate( %this )
{
   if ( isObject( BrowserMap ) )
   {
      BrowserMap.pop();
      BrowserMap.delete();
   }
   
   GM_VersionText.setVisible( false );

   $pref::ServerBrowser::InfoWindowOpen = GMJ_Browser.infoWindowOpen;
   if ( GMJ_Browser.infoWindowOpen )
      Canvas.popDialog( ServerInfoDlg );
}

//------------------------------------------------------------------------------
$BrowserColumnCount = 0;
$BrowserColumnName[0] = "Server Name";
$BrowserColumnRange[0] = "25 300";
$BrowserColumnCount++;
$BrowserColumnName[1] = "Status";
$BrowserColumnRange[1] = "25 75";
$BrowserColumnCount++;
$BrowserColumnName[2] = "Favorite";
$BrowserColumnRange[2] = "25 75";
$BrowserColumnCount++;
$BrowserColumnName[3] = "Ping";
$BrowserColumnRange[3] = "25 120";
$BrowserColumnCount++;
$BrowserColumnName[4] = "Game Type";
$BrowserColumnRange[4] = "25 200";
$BrowserColumnCount++;
$BrowserColumnName[5] = "Mission Name";
$BrowserColumnRange[5] = "25 300";
$BrowserColumnCount++;
// ---------------------------------------------------
// z0dd - ZOD, 9/29/02. Removed T2 demo code from here
$BrowserColumnName[6] = "Rules Set";
$BrowserColumnRange[6] = "25 300";
$BrowserColumnCount++;
// ---------------------------------------------------
$BrowserColumnName[7] = "# Players (Bots)";
$BrowserColumnRange[7] = "25 150";
$BrowserColumnCount++;
$BrowserColumnName[8] = "CPU";
$BrowserColumnRange[8] = "25 120";
$BrowserColumnCount++;
$BrowserColumnName[9] = "IP Address";
$BrowserColumnRange[9] = "25 200";
$BrowserColumnCount++;
// ---------------------------------------------------
// z0dd - ZOD, 9/29/02. Removed T2 demo code from here
$BrowserColumnName[10] = "Version";
$BrowserColumnRange[10] = "25 200";
$BrowserColumnCount++;
// ---------------------------------------------------
$BrowserColumnName[11] = "Visibility";
$BrowserColumnRange[11] = "25 120";
$BrowserColumnCount++;

//------------------------------------------------------------------------------
function GMJ_Browser::onAdd( %this )
{
	// Add the Server Browser columns based on the prefs:
	for ( %i = 0;  %i < $BrowserColumnCount; %i++ )
	{
		%key = firstWord( $pref::ServerBrowser::Column[%i] );
		if ( $BrowserColumnName[%key] !$= "" && $BrowserColumnRange[%key] !$= "" )
		{
			%width = getWord( $pref::ServerBrowser::Column[%i], 1 );
			%this.addColumn( %key, $BrowserColumnName[%key], %width, firstWord( $BrowserColumnRange[%key] ), getWord( $BrowserColumnRange[%key], 1 ) );
		}
	}
	%this.setSortColumn( $pref::ServerBrowser::SortColumnKey );
	%this.setSortIncreasing( $pref::ServerBrowser::SortInc );
}

//------------------------------------------------------------------------------
function updateServerBrowser()
{
   GMJ_Browser.sort();
   if ( GMJ_Browser.infoWindowOpen )
      ServerInfoDlg.update();
}

//------------------------------------------------------------------------------
function updateServerBrowserStatus( %text, %percentage )
{
   GMJ_StatusText.setValue( %text );
   if ( %percentage >= 0 && %percentage <= 1 )
   {
      GMJ_ProgressBar.setValue( %percentage );
      if ( %percentage == 0 ) // Query is over.
         GMJ_StopBtn.setActive( false );
   }
}

//------------------------------------------------------------------------------
function GMJ_Browser::runQuery( %this )
{
   GMJ_ProgressBar.setValue( 0 );
   $JoinGameAddress = "";
   GMJ_JoinBtn.setActive( false );
   GMJ_RefreshServerBtn.setActive( false );
   %this.clearList();

   // Clear the Server Info dialog:
   SI_InfoWindow.setText( "No server selected." );
   SI_ContentWindow.setText( "" );

   if ( %this.lastQuery $= "LanServers" )
   {
      GMJ_StatusText.setValue( "Querying LAN servers..." );
      GMJ_FilterBtn.setActive( false );
		GMJ_FilterBtn.setVisible( false );
      GMJ_FilterText.setText( "LAN Servers" );
      queryLanServers( $JoinGamePort );
      GMJ_StopBtn.setActive( true );
   }
   // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   else
   {
		GMJ_FilterBtn.setActive( true );
		GMJ_FilterBtn.setVisible( true );

      if ( $pref::ServerBrowser::activeFilter == 0 )
      {
         GMJ_StatusText.setValue( "Querying the master server..." );
         GMJ_FilterText.setText( "All servers" );
         queryMasterServer( $JoinGamePort );
         GMJ_StopBtn.setActive( true );
      }
      else if ( $pref::ServerBrowser::activeFilter == 1 )
      {
         // Buddy list query:
         GMJ_StatusText.setValue( "Fetching buddy list..." );
         GMJ_FilterText.setText( "Buddies" );
         %this.key = LaunchGui.key++;
         DatabaseQueryArray( 5, 0, "", %this, %this.key );
      }
      else if ( $pref::ServerBrowser::activeFilter == 2 )
      {
         // Favorites only:
         GMJ_FilterText.setText( "Favorites" );
         if ( $pref::ServerBrowser::FavoriteCount <= 0 || $pref::ServerBrowser::Favorite[0] $= "" )
         {
            GMJ_StatusText.setValue( "No favorites found." );
            MessageBoxOK( "INVALID FILTER", "You haven't marked any servers as favorites.  Click the Favorites column to mark a server as a favorite." );
         }
         else
         {
            GMJ_StatusText.setValue( "Querying favorites..." );
            queryFavoriteServers();
            GMJ_StopBtn.setActive( true );
         }
      }
      else
      {
         GMJ_StatusText.setValue( "Querying the master server..." );
         %filterIndex = $pref::ServerBrowser::activeFilter - 3;
         if ( $pref::ServerBrowser::Filter[%filterIndex] !$= "" )
         {
            %filter = $pref::ServerBrowser::Filter[%filterIndex];
            GMJ_FilterText.setText( getField( %filter, 0 ) );
            %rulesSet = getField( %filter, 1 );
            if ( %rulesSet $= "" )
               %rulesSet = "any";
            %missionType = getField( %filter, 2 );
            if ( %missionType $= "" )
               %missionType = "any";
            %maxPlayers = getField( %filter, 4 );
            if ( %maxPlayers $= "" )
               %maxPlayers = 255;
            %maxBots = getField( %filter, 7 );
            if ( %maxBots $= "" )
               %maxBots = 32;
            %regionMask = getField( %filter, 5 );
            if ( %regionMask $= "" )
               %regionMask = 4294967295;

            queryMasterServer( 
                  $JoinGamePort, 
                  0,                         // Flags 
                  %rulesSet,                 // Rules Set 
                  %missionType,              // Mission Type 
                  getField( %filter, 3 ),    // Min Players 
                  %maxPlayers,               // Max Players
                  %maxBots,                  // Max Bots 
                  %regionMask,               // Region Mask 
                  getField( %filter, 6 ),    // Max Ping
                  getField( %filter, 8 ),    // Min CPU Speed
                  getField( %filter, 9 ) );  // Filter flags
            GMJ_StopBtn.setActive( true );
         }
         else
         {
            // Filter is invalid, so fall back to the default:
            $pref::ServerBrowser::activeFilter = 0;
            GMJ_FilterText.setText( "All servers" );
            queryMasterServer( $JoinGamePort );
            GMJ_StopBtn.setActive( true );
         }
      }
   }
}

//------------------------------------------------------------------------------
function GMJ_Browser::onDatabaseQueryResult( %this, %status, %resultString, %key )
{
   if ( %this.key != %key )
      return;

   if ( getField( %resultString, 0 ) <= 0 )
   {
      GMJ_StatusText.setValue( "No buddies found." );
      MessageBoxOK( "INVALID FILTER", "You have no buddies in your buddy list!" );
   }
   else  // Prepare for the incoming buddy list:
      %this.buddyList = "";
}

//------------------------------------------------------------------------------
function GMJ_Browser::onDatabaseRow( %this, %row, %isLastRow, %key )
{
   if ( %this.key != %key )
      return;

   %buddyName = getField( %row, 0 );
   %buddyGuid = getField( %row, 3 );
   echo( "Got buddy: \c9\"" @ %buddyName @ "\": " @ %buddyGuid );
   %this.buddyList = %this.buddyList $= "" ? %buddyGuid : %this.buddyList TAB %buddyGuid;

   if ( %isLastRow )
   {
      GMJ_StatusText.setValue( "Querying the master server..." );
      queryMasterServer( 
            $JoinGamePort,    // Port 
            0,                // Flags 
            "Any",            // Rules Set 
            "Any",            // Mission Type  
            0,                // Min Players 
            255,              // Max Players
            32,               // Max Bots 
            0xFFFFFFFF,       // Region Mask 
            0,                // Max Ping
            0,                // Min CPU Speed
            0,                // Filter flags
            %this.buddyList );
      GMJ_StopBtn.setActive( true );
      %this.buddyList = "";
   }
}

//------------------------------------------------------------------------------
function GMJ_Browser::onSelect( %this, %address )
{
   GMJ_JoinBtn.setActive( true );
   if ( !isServerQueryActive() )
      GMJ_RefreshServerBtn.setActive( true );
   $JoinGamePassword = "";
   $JoinGameAddress = %address;

   if ( %this.infoWindowOpen )
      ServerInfoDlg.update();
}

//------------------------------------------------------------------------------
function GMJ_Browser::refreshSelectedServer( %this )
{
   querySingleServer( $JoinGameAddress );
   if ( %this.infoWindowOpen )
      ServerInfoDlg.update();
}

//------------------------------------------------------------------------------
function GMJ_Browser::onSetSortKey( %this, %sortKey, %isIncreasing )
{
   $pref::ServerBrowser::SortColumnKey = %sortKey;
   $pref::ServerBrowser::SortInc = %isIncreasing;
}

//------------------------------------------------------------------------------
function GMJ_Browser::onColumnResize( %this, %column, %newSize, %key )
{
   $pref::ServerBrowser::Column[%column] = %key SPC %newSize;
}

//------------------------------------------------------------------------------
function GMJ_Browser::onColumnRepositioned( %this, %oldColumn, %newColumn )
{
   // Puke em all...
   %count = %this.getNumColumns();
   for ( %col = 0; %col < %count; %col++ )
      $pref::ServerBrowser::Column[%col] = %this.getColumnKey( %col ) SPC %this.getColumnWidth( %col );
}

//------------------------------------------------------------------------------
function GMJ_Browser::addFavorite( %this, %name, %address )
{
   //error( "** addFavorite( \"" @ %name @ "\", " @ %address @ " ) **" );
   $pref::ServerBrowser::Favorite[$pref::ServerBrowser::FavoriteCount] = %name TAB %address;
   $pref::ServerBrowser::FavoriteCount++;
}

//------------------------------------------------------------------------------
function GMJ_Browser::removeFavorite( %this, %address )
{
   //error( "** removeFavorite( " @ %address @ " ) **" );
   %foundIt = false;
   for ( %i = 0; %i < $pref::ServerBrowser::FavoriteCount; %i++ )
   {
      if ( !%foundIt )
      {
         if ( getField( $pref::ServerBrowser::Favorite[%i], 1 ) $= %address )
            %foundIt = true;
      }

      if ( %foundIt )
         $pref::ServerBrowser::Favorite[%i] = $pref::ServerBrowser::Favorite[%i + 1];
   }

   if ( %foundIt )
      $pref::ServerBrowser::FavoriteCount--;
}

//------------------------------------------------------------------------------
function GMJ_Browser::insertIPAddress( %this )
{
   if ( isServerQueryActive() )
   {
      BrowserMap.pop();
      MessageBoxOK( "ERROR", "Can't insert addresses while a query is running!", "BrowserMap.push();" );
      alxPlay( InputDeniedSound, 0, 0, 0 );
      return;
   }
   
   IPEntry.setText( "IP:" );
   Canvas.pushDialog( EnterIPDlg );
}

//------------------------------------------------------------------------------
function EnterIPDlg::onDone( %this )
{
   Canvas.popDialog( EnterIPDlg );
   %address = IPEntry.getValue();
   if ( getSubStr( %address, 0, 3 ) !$= "IP:" )
      %address = "IP:" @ %address;
   if ( strpos( %address, ":", 3 ) == -1 )
      %address = %address @ ":28000";
   
   echo( "Starting ping to server " @ %address @ "..." );   
   pushServerAddress( %address );
   GMJ_Browser.selectRowByAddress( %address, true );
}

//------------------------------------------------------------------------------
function FindServerDlg::onWake( %this )
{
   FS_SearchPattern.validate();
   FS_SearchPattern.selectAll();
}

//------------------------------------------------------------------------------
function FindServerDlg::onGo( %this )
{
   %pattern = FS_SearchPattern.getValue();
   if ( %pattern !$= "" )
   {
      Canvas.popDialog( FindServerDlg );
      if ( !GMJ_Browser.findServer( %pattern ) )
         MessageBoxOK( "NOT FOUND", "No servers with \"" @ %pattern @ "\" in their name were found." );
   }
   else
      alxPlay( InputDeniedSound, 0, 0, 0 );
}

//------------------------------------------------------------------------------
function FS_SearchPattern::validate( %this )
{
   FS_GoBtn.setActive( %this.getValue() !$= "" );
}

//------------------------------------------------------------------------------
function ServerInfoDlg::onAdd( %this )
{
   %this.headerStyle = "<font:" @ $ShellLabelFont @ ":" @ $ShellFontSize @ "><color:00DC00>";
}

//------------------------------------------------------------------------------
function ServerInfoDlg::onWake( %this )
{
   GMJ_Browser.infoWindowOpen = true;

   // Get the position and size from the prefs:
   %res = getResolution();
   %resW = firstWord( %res );
   %resH = getWord( %res, 1 );
   %w = firstWord( $pref::ServerBrowser::InfoWindowExtent );
   if ( %w > %resW )
      %w = %resW;
   %h = getWord( $pref::ServerBrowser::InfoWindowExtent, 1 );
   if ( %h > %resH )
      %h = %resH;
   %x = firstWord( $pref::ServerBrowser::InfoWindowPos );
   if ( %x > %resW - %w )
      %x = %resW - %w;
   %y = getWord( $pref::ServerBrowser::InfoWindowPos, 1 );
   if ( %y > %resH - %h )
      %y = %resH - %h;
   SI_Window.resize( %x, %y, %w, %h );

   GMJ_InfoBtn.setActive( false );
   SI_RefreshBtn.setActive( false );
   %this.update();
}

//------------------------------------------------------------------------------
function ServerInfoDlg::update( %this )
{
   %status = GMJ_Browser.getServerStatus();
   if ( %status $= "invalid" )
   {
	   SI_InfoWindow.setText( "No server selected." );
      return;
   }

   %info = GMJ_Browser.getServerInfoString();
   %infoText = "<tab:70><spush>" @ %this.headerStyle @ "NAME:<spop><lmargin:70>" TAB getRecord( %info, 0 )
         NL "<lmargin:0><spush>" @ %this.headerStyle @ "ADDRESS:<spop><lmargin:70>" TAB getRecord( %info, 1 ) @ "<lmargin:0>";

   %refreshable = false;
   if ( %status $= "responded" )
   {
      %temp = getRecord( %info, 2 );
      if ( %temp !$= "" )
         %infoText = %infoText NL "<spush>" @ %this.headerStyle @ "RULES SET:<spop><lmargin:70>" TAB %temp @ "<lmargin:0>";
      %temp = getRecord( %info, 3 );
      if ( %temp $= "" )
         %temp = "None";
      %infoText = %infoText NL "<spush>" @ %this.headerStyle @ "FLAGS:<spop><lmargin:70>" TAB %temp @ "<lmargin:0>";
      %temp = getRecord( %info, 4 );
      if ( %temp !$= "" )
         %infoText = %infoText NL "<spush>" @ %this.headerStyle @ "GAME TYPE:<spop><lmargin:70>" TAB %temp @ "<lmargin:0>";
      %temp = getRecord( %info, 5 );
      if ( %temp !$= "" )
         %infoText = %infoText NL "<spush>" @ %this.headerStyle @ "MAP NAME:<spop><lmargin:70>" TAB %temp @ "<lmargin:0>";
      %temp = getRecords( %info, 6, 10 );
      if ( %temp !$= "" )
         %infoText = %infoText NL "<spush>" @ %this.headerStyle @ "SERVER INFO:<spop><lmargin:10>" TAB %temp @ "<lmargin:0>";

      // Fill in the content window:
      %content = GMJ_Browser.getServerContentString();
      SI_ContentWindow.fill( %content );
      %refreshable = !isServerQueryActive();
   }
   else
   {
      switch$ ( %status )
      {
         case "new":
            %temp = "<spush><color:DCDC00>Not queried yet.<spop>";
            SI_ContentWindow.setText( "Not available." );
         case "querying":
            %temp = "<spush><color:00DC00>Querying...<spop>";
            SI_ContentWindow.setText( "Not available." );
         case "updating":
            %temp = "<spush><color:00DC00>Updating...<spop>";
         case "timedOut":
            %temp = "<spush><color:DC1A1A>Timed out.<spop>";
            SI_ContentWindow.setText( "Not available." );
            %refreshable = !isServerQueryActive();
      }
      %infoText = %infoText NL "<lmargin:0><spush>" @ %this.headerStyle @ "STATUS: <spop><lmargin:70>" TAB %temp;
   }

	SI_InfoWindow.setText( %infoText );
   SI_InfoScroll.scrollToTop();
   SI_ContentScroll.scrollToTop();
   SI_RefreshBtn.setActive( %refreshable );
}

//------------------------------------------------------------------------------
function SI_ContentWindow::fill( %this, %content )
{
   if ( getRecordCount( %content ) == 1 )
   {
      %this.setText( "" );
      return;
   }

   %record = 0;
   %teamCount = getRecord( %content, %record );
   %record++;
   if ( %teamCount > 1 )
   {
      %string = "<spush>" @ ServerInfoDlg.headerStyle @ "TEAMS<lmargin%:50>SCORE<spop>";
      for ( %i = 0; %i < %teamCount; %i++ )
      {
         %teamEntry = getRecord( %content, %record );
         %string = %string NL "<lmargin:0><clip%:50>" SPC getField( %teamEntry, 0 ) @ "</clip><lmargin%:50>" SPC getField( %teamEntry, 1 );
         %record++;
      }

      %playerCount = getRecord( %content, %record );
      %record++;
      %string = %string NL "\n<lmargin:0><spush>" @ ServerInfoDlg.headerStyle @ "PLAYERS<lmargin%:40>TEAM<lmargin%:75>SCORE<spop>";
      for ( %i = 0; %i < %playerCount; %i++ )
      {
         %playerEntry = getRecord( %content, %record ); 
         %string = %string NL "<lmargin:0><clip%:40>" SPC getField( %playerEntry, 0 ) @ "</clip><lmargin%:40><clip%:35>" 
               SPC getField( %playerEntry, 1 ) @ "</clip><lmargin%:75><clip%:25>" SPC getField( %playerEntry, 2 ) @ "</clip>";
         %record++;
      }
   }
   else
   {
      %record++;
      %playerCount = getRecord( %content, %record );
      %record++;
      %string = "<spush>" @ ServerInfoDlg.headerStyle @ "PLAYERS<lmargin%:60>SCORE<spop>";
      for ( %i = 0; %i < %playerCount; %i++ )
      {
         %playerEntry = getRecord( %content, %record ); 
         %string = %string NL "<lmargin:0><clip%:60>" SPC getField( %playerEntry, 0 ) @ "</clip><lmargin%:60>" SPC getField( %playerEntry, 2 );
         %record++;
      }
   }

   %this.setText( %string );
}

//------------------------------------------------------------------------------
function ServerInfoDlg::onSleep( %this )
{
   GMJ_Browser.infoWindowOpen = false;

   // Save off the Server Info Window prefs:
   $pref::ServerBrowser::InfoWindowPos = SI_Window.getPosition();
   $pref::ServerBrowser::InfoWindowExtent = SI_Window.getExtent();
   $pref::ServerBrowser::InfoWindowBarPos = getWord( SI_InfoScroll.getExtent(), 1 );
   
   GMJ_InfoBtn.setActive( true );
}

//------------------------------------------------------------------------------
function PasswordDlg::onWake( %this )
{
   $JoinGamePassword = "";
}

//------------------------------------------------------------------------------
function PasswordDlg::accept( %this )
{
   Canvas.popDialog( PasswordDlg );
   JoinSelectedGame();
}

//------------------------------------------------------------------------------
function JoinSelectedGame()
{
	$ServerInfo = GMJ_Browser.getServerInfoString();

	JoinGame($JoinGameAddress);
}

//------------------------------------------------------------------------------
function JoinGame(%address)
{
   MessagePopup( "JOINING SERVER", "CONNECTING" );
   cancelServerQuery();
	echo("Joining Server " @ %address);
	%playerPref = $pref::Player[$pref::Player::Current];
	%playerName = getField( %playerPref, 0 );
	%playerRaceGender = getField( %playerPref, 1 );
	%playerSkin = getField( %playerPref, 2 );
	%playerVoice = getField( %playerPref, 3 );
	%playerVoicePitch = getField( %playerPref, 4 );
   LoadingGui.gotLoadInfo = "";
   connect( %address, $JoinGamePassword, %playerName, %playerRaceGender, %playerSkin, %playerVoice, %playerVoicePitch );
}

//------------------------------------------------------------------------------
// Host Game pane:
//------------------------------------------------------------------------------
function GM_HostPane::onActivate( %this )
{
   GameGui.pane = "Host";

   $HostGameType = $PlayingOnline ? "Online" : "LAN";
   
   buildMissionTypePopup( GMH_MissionType );
   // ---------------------------------------------------
   // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   GMH_BotMinSlider.setValue( $Host::MinBotDifficulty );
   GMH_BotMaxSlider.setValue( $Host::MaxBotDifficulty );
   GMH_BotsEnabledTgl.setValue( $Host::BotsEnabled );
   GMH_BotsEnabledTgl.onAction();

   //clamp and set the bot count slider
   setBotCountSlider();
   // ---------------------------------------------------

   // Select the saved-off prefs:
   if ( $Host::MissionType !$= "" )
   {
      // Find the last selected type:
      for ( %type = 0; %type < $HostTypeCount; %type++ )
      {
         if ( $HostTypeName[%type] $= $Host::MissionType )
            break;
      }

      if ( %type != $HostTypeCount )
      {
         GMH_MissionType.setSelected( %type );
         GMH_MissionType.onSelect( %type, "" );
         if ( $Host::Map !$= "" )
         {
            // Find the last selected mission:
            for ( %index = 0; %index < $HostMissionCount[%type]; %index++ )
            {
               if ( $HostMissionFile[$HostMission[%type, %index]] $= $Host::Map )
                  break;
            }

            if ( %index != $HostMissionCount[%type] )
               GMH_MissionList.setSelectedById( $HostMission[%type, %index] );
         }
      }
   }
   else
   {
      GMH_MissionType.setSelected( 0 );
      GMH_MissionType.onSelect( 0, "" );
   }

	GMH_StartGameBtn.makeFirstResponder( 1 );
}

//------------------------------------------------------------------------------
function GM_HostPane::onDeactivate( %this )
{
}

//------------------------------------------------------------------------------
function buildMissionTypePopup( %popup )
{
   %popup.clear();
   for( %type = 0; %type < $HostTypeCount; %type++ )
      %popup.add( $HostTypeDisplayName[%type], %type );
   %popup.sort( true );   
}

//------------------------------------------------------------------------------
function getMissionTypeDisplayNames()
{
   %file = new FileObject();
   for ( %type = 0; %type < $HostTypeCount; %type++ )
   {
      $HostTypeDisplayName[%type] = $HostTypeName[%type];
      if ( %file.openForRead( "scripts/" @ $HostTypeName[%type] @ "Game.cs" ) )
      {
         while ( !%file.isEOF() )
         {
            %line = %file.readLine();
            if ( getSubStr( %line, 0, 17 ) $= "// DisplayName = " )
            {
               $HostTypeDisplayName[%type] = getSubStr( %line, 17, 1000 );
               break;
            }
         }
      }
   }

   %file.delete();
}

//------------------------------------------------------------------------------
// Eolk - New rotation code
function buildMissionList()
{
   if(isFile($Host::ClassicRotationFile) && $Host::ClassicRotationCustom)
   {
      deleteVariables("$HostMission*");
      deleteVariables("$HostType*");

      exec($Host::ClassicRotationFile);
   }
   else
   {
      %search = "missions/*.mis";
      %ct = 0;
      $HostTypeCount = 0;
      $HostMissionCount = 0;
      %fobject = new FileObject();
      for( %file = findFirstFile( %search ); %file !$= ""; %file = findNextFile( %search ) )
      {
         %name = fileBase( %file ); // get the name
         // Eolk - Remove map skip code
         %idx = $HostMissionCount;
         $HostMissionCount++;
         $HostMissionFile[%idx] = %name;
         $HostMissionName[%idx] = %name;

         if ( !%fobject.openForRead( %file ) )
            continue;

         %typeList = "None";
         while ( !%fobject.isEOF() )
         {
            %line = %fobject.readLine();
            if ( getSubStr( %line, 0, 17 ) $= "// DisplayName = " )
            {
               // Override the mission name:
               $HostMissionName[%idx] = getSubStr( %line, 17, 1000 );
            }
            else if ( getSubStr( %line, 0, 18 ) $= "// MissionTypes = " )
            {
               %typeList = getSubStr( %line, 18, 1000 );
               break;
            }
         }
         %fobject.close();

         // Don't include single player missions:
         if ( strstr( %typeList, "SinglePlayer" ) != -1 )
            continue;

         // Test to see if the mission is bot-enabled:
         %navFile = "terrains/" @ %name @ ".nav";
         $BotEnabled[%idx] = isFile( %navFile );

         for( %word = 0; ( %misType = getWord( %typeList, %word ) ) !$= ""; %word++ )
         {
            // Eolk - remove gametype skip code
            // -------------------------------------------------------------------
            // z0dd - ZOD, 01/02/03. Don't include TR2 gametype if it's turned off
            if(("TR2" $= %misType) && (!$Host::ClassicLoadTR2Gametype))
            {
               continue;
            }
            // -------------------------------------------------------------------

            for ( %i = 0; %i < $HostTypeCount; %i++ )
               if ( $HostTypeName[%i] $= %misType )
                  break;
            if ( %i == $HostTypeCount )
            {
               $HostTypeCount++;
               $HostTypeName[%i] = %misType;
               $HostMissionCount[%i] = 0;
               $MapCycleCount[%i] = 0;
            }
            // add the mission to the type
            %ct = $HostMissionCount[%i];
            $HostMission[%i, $HostMissionCount[%i]] = %idx;
            $HostMissionCount[%i]++;

            $HostFFAMap[%name, %i] = 1;
            $MapCycleList[%i, $MapCycleCount[%i]] = %idx;
            $MapCycleMaxPlayers[%misType, %name] = $Host::MaxPlayers;
            $MapCycleMinPlayers[%misType, %name] = 0;
            $ReverseMapCycle[%name] = 1;
            $MapCycleCount[%i]++;
         }
      }
      %fobject.delete();
      ClassicExportRotation();
   }
   getMissionTypeDisplayNames();
}

function ClassicExportRotation()
{
   %temp = new FileObject();
   %temp.openForWrite($Host::ClassicRotationFile);

   for(%i = 0; %i < $HostTypeCount; %i++)
   {
      %temp.writeLine("// " @ $HostTypeDisplayName[%i]);
      %type = $HostTypeName[%i];

      for (%m = 0; %m < $HostMissionCount[%i]; %m++)
      {
         %idx = $HostMission[%i, %m];
         %mis = $HostMissionFile[%idx];

         %temp.writeLine("addRotationMap(\"" @ %mis @ "\", \"" @ %type @ "\", true, true);");
      }
      %temp.writeLine("");
      %temp.writeLine("");
   }

   %temp.close();
   %temp.delete();
}

function addRotationMap(%missionFile, %gameType, %freeForAll, %cycle, %minPlayers, %maxPlayers)
{
   if(!isFile("missions/"@ %missionFile @".mis"))
      return;

   if(%minPlayers $= "")
      %minPlayers = 0;

   if(%maxPlayers $= "")
      %maxPlayers = $Host::MaxPlayers;

   // Classic:
   if (%gameType $= "TR2" && !$Host::ClassicLoadTR2Gametype)
      return;

   // Check if the map has already been added once
   %found = false;
   for (%mis = 0; %mis < $HostMissionCount; %mis++)
      if ($HostMissionFile[%mis] $= %missionFile)
      {
         %found = true;
         break;
      }

   // Not found, add to mission list
   if (!%found)
   {
      $HostMissionCount++;
      $HostMissionFile[%mis] = %missionFile;
      $HostMissionName[%mis] = %missionFile;
      $BotEnabled[%mis] = isFile("terrains/" @ %missionFile @".nav");
    

      // Load custom display name
      %f = new FileObject();
      if (%f.openForRead("missions/"@ %missionFile @".mis"))
      {
         while (!%f.isEOF())
            if (getsubstr(%line = trim(%f.readLine()), 0,17) $= "// DisplayName = ")
            {
               $HostMissionName[%mis] = getsubstr(%line, 17, 999);
               break;
            }
         %f.close();
      }
      %f.delete();
   }

   // Check if gametype has already been loaded
   %found = false;
   for (%type = 0; %type < $HostTypeCount; %type++)
      if ($HostTypeName[%type] $= %gameType)
      {
         %found = true;
         break;
      }

   // Not found, add to gametype list
   if (!%found)
   {
      $HostTypeCount++;
      $HostTypeName[%type] = %gameType;
      $MapCycleCount[%type] = 0;
      $HostMissionCount[%type] = 0;
   }

   // Add the mission to the gametype
   $HostMission[%type, $HostMissionCount[%type]] = %mis;
   $HostMissionCount[%type]++;

   $HostFFAMap[%file, %type] = %freeForAll;
   if(%cycle)
   {
      $MapCycleList[%type, $MapCycleCount[%type]] = %mis;
      $MapCycleMaxPlayers[%gameType, %missionFile] = %maxPlayers;
      $MapCycleMinPlayers[%gameType, %missionFile] = %minPlayers;
	  //$Host::MapPlayerLimits[%missionFile, %gametype] = %minPlayers SPC %minPlayers;
      $ReverseMapCycle[%missionFile] = 1;
      $MapCycleCount[%type]++;
      if($BotEnabled[%mis]){
         $BotMissionCount[%type]++;
      }
   }
}

// One time only function call:
buildMissionList();

/////////////////////////////////////////////////////////////////////////////////////////////////////
// z0dd - ZOD - Founder(founder@mechina.com):
// Functions to add and remove missions from $SkipMission::name (MissionSkip.cs).

// commandToServer('AddMap', MapFilename);
function serverCmdAddMap(%client, %map)
{
   %map = detag(%map);
   if(%client.isSuperAdmin)
      AddMapToList(%map, %client);
   else
      messageClient(%client, 'MsgNotSuperAdmin', '\c2Only Super Admins can use this command.');
}
// AddMapToList(MapFilename);
function AddMapToList(%map, %client)
{
   if(%map $="")
      return;

   %found = 0;
   for( %i = 0; $SkipMission::name[%i] !$= ""; %i++ ) {
      if($SkipMission::name[%i] $= %map) {
         %found = 1;
          break;
      }
   }
   if(%found)
   {
      error( "Mission " @ %map @ " allready exists in skip list!" );
      return;
   }
   if($MissionSkip::count $= "")
      $MissionSkip::count = 0;

   $SkipMission::name[$MissionSkip::count] = %map;
   $MissionSkip::count++;

   %val = 'removal from';
   writeMissionSkipList(%map, %val, %client);
}

// commandToServer('RemoveMap', MapFilename);
function serverCmdRemoveMap(%client, %map)
{
   %map = detag(%map);
   if(%client.isSuperAdmin)
      RemoveMapFromList(%map, %client);
   else
      messageClient(%client, 'MsgNotSuperAdmin', '\c2Only Super Admins can use this command.');
}

// RemoveMapFromList(MapFilename);
function RemoveMapFromList(%map, %client)
{
   if(%map $="")
      return;

   %count = 0;
   for( %i = 0; %i < $MissionSkip::count; %i++ )
   {
      if($SkipMission::name[%i] !$= %map)
      {
         %Temp[%count] = $SkipMission::name[%i];
         %count++;
      }
   }
   for( %j = 0; %j < %count; %j++ )
      $SkipMission::name[%j] = %Temp[%j];

   $MissionSkip::count = %count;
   //$MissionSkip::count --;

   %val = 'restoration to';
   writeMissionSkipList(%map, %val, %client);
}

function writeMissionSkipList(%name, %val, %client)
{
   %newfile = "prefs/MissionSkip.cs";
   if(isFile(%newfile))
   {
      deleteFile(%newfile);
      if(isFile("prefs/MissionSkip.cs.dso"))
         deleteFile("prefs/MissionSkip.cs.dso");
   }

   %listfile = new fileObject();
   %listfile.openForWrite(%newfile);
   %listfile.writeLine("// ------------------------- Mission Skip List -------------------------");
   %listfile.writeLine("// ----- Mission file names without file extension. Ex: BeggarsRun -----");
   %listfile.writeLine("// ------------ Missions on list are excluded from rotation ------------");
   %listfile.writeLine("");
   for( %k = 0; %k < $MissionSkip::count; %k++ ) {
      %listfile.writeLine("$SkipMission::name[" @ %k @ "] = \"" @ $SkipMission::name[%k] @ "\";");
   }
   %listfile.writeLine("$MissionSkip::count = " @ $MissionSkip::count @ ";");
   %listfile.close();
   %listfile.delete();

   if(%client !$= "")
      messageClient(%client, 'MsgAdmin', '\c3\"%1\"\c2 %2 mission rotation successful.', %name, %val);

   echo( "Mission " @ %name @ " " @ %val @ " mission rotation successful." );
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// z0dd - ZOD - Founder(founder@mechina.com):
// Functions to add and remove mission types from $SkipType::name (GameTypeSkip.cs).

// commandToServer('AddType', Typename);
function serverCmdAddType(%client, %type)
{
   %type = detag(%type);
   if(%client.isSuperAdmin)
      AddTypeToList(%type, %client);
   else
      messageClient(%client, 'MsgNotSuperAdmin', '\c2Only Super Admins can use this command.');
}

// AddTypeToList(Typename);
function AddTypeToList(%type, %client)
{
   if(%type $="")
      return;

   %found = 0;
   for( %i = 0; $SkipType::name[%i] !$= ""; %i++ ) {
      if($SkipType::name[%i] $= %type) {
         %found = 1;
          break;
      }
   }
   if(%found)
   {
      error( "Game type " @ %type @ " allready exists in skip list!" );
      return;
   }
   if($TypeSkip::count $= "")
      $TypeSkip::Count = 0;

   $SkipType::name[$TypeSkip::count] = %type;
   $TypeSkip::count++;

   %val = 'removed';
   writeTypeSkipList(%type, %val, %client);
}

// commandToServer('RemoveType', Typename);
function serverCmdRemoveType(%client, %type)
{
   %type = detag(%type);
   if(%client.isSuperAdmin)
      RemoveTypeFromList(%type, %client);
   else
      messageClient(%client, 'MsgNotSuperAdmin', '\c2Only Super Admins can use this command.');
}

// RemoveTypeFromList(Typename);
function RemoveTypeFromList(%type, %client)
{
   if(%type $="")
      return;

   %count = 0;
   for( %i = 0; %i < $TypeSkip::count; %i++ )
   {
      if($SkipType::name[%i] !$= %type)
      {
         %Temp[%count] = $SkipType::name[%i];
         %count++;
      }
   }
   for( %j = 0; %j < %count; %j++ )
      $SkipType::name[%j] = %Temp[%j];

   $TypeSkip::count = %count;
   //$TypeSkip::count --;

   %val = 'restored';
   writeTypeSkipList(%type, %val, %client);
}

function writeTypeSkipList(%name, %val, %client)
{
   %newfile = "prefs/GameTypeSkip.cs";
   if(isFile(%newfile))
   {
      deleteFile(%newfile);
      if(isFile("prefs/GameTypeSkip.cs.dso"))
         deleteFile("prefs/GameTypeSkip.cs.dso");
   }
   %listfile = new fileObject();
   %listfile.openForWrite(%newfile);
   %listfile.writeLine("// ------------------------- Game Type Skip List -------------------------");
   %listfile.writeLine("// ----------------------- Game type names. Ex: CnH ----------------------");
   %listfile.writeLine("// ------------ Game types on list are excluded from rotation ------------");
   %listfile.writeLine("");
   for( %k = 0; %k < $TypeSkip::count; %k++ ) {
      %listfile.writeLine("$SkipType::name[" @ %k @ "] = \"" @ $SkipType::name[%k] @ "\";");
   }
   %listfile.writeLine("$TypeSkip::count = " @ $TypeSkip::count @ ";");
   %listfile.close();
   %listfile.delete();

   if(%client !$= "")
      messageClient(%client, 'MsgAdmin', '\c2Game type \c3\"%1\"\c2 %2 successfully.', %name, %val);

   echo( "Game type " @ %name @ " " @ %val @ " successfully." );
}

//------------------------------------------------------------------------------
function validateMissionAndType(%misName, %misType)
{
   for ( %mis = 0; %mis < $HostMissionCount; %mis++ )
      if( $HostMissionFile[%mis] $= %misName )
         break;
   if ( %mis == $HostMissionCount )
      return false;
   for ( %type = 0; %type < $HostTypeCount; %type++ )
      if ( $HostTypeName[%type] $= %misType )
         break;
   if(%type == $hostTypeCount)
      return false;
   $Host::Map = $HostMissionFile[%mis];
   $Host::MissionType = $HostTypeName[%type];

   return true;
}

//------------------------------------------------------------------------------
// This function returns the index of the next mission in the mission list.
//------------------------------------------------------------------------------
function getNextMission(%missionName, %gameType)
{
   // Find the gametype index
   for (%type = 0; %type < $HostTypeCount; %type++)
   {
      if ($HostTypeName[%type] $= %gameType)
         break;
   }

   // Didn't find a valid type, display error
   if (%type == $HostTypeCount)
   {
      error("getNextMission(): " @ %missionName @ " does not support " @ %gameType);
      return -1;
   }

   if($MapPlayedCount == $MapCycleCount[%type])
      deleteVariables("$MapPlayed*");
   if($Host::botsEnabled && $BotMissionCount[%type] >= $MapPlayedCount)
      deleteVariables("$MapPlayed*");
      
   %length = 0;
   %index = -1;
   // Build array of missions
   for (%i = 0; %i < $MapCycleCount[%type]; %i++)
   {
      %idx = $MapCycleList[%type, %i];
      %misFile = $HostMissionFile[%idx];
      // If we have bots and this map doesn't, skip...
      if ($Host::botsEnabled && !$BotEnabled[%idx])
         continue;
      // Don't add the current mission to our temp list
      if (%missionName $= %misFile)
         // Note the index of the current mission
         %index = %length;
      else if (!$MapPlayed[%misFile]) {
         %list[%length++] = %idx;
         //error(%list[%length] @": "@ %misFile);
      }
   }

   // If we didn't find any maps, stop...
   if (!%length){
      error("getNextMission(): No valid maps found in rotation...");
      return -1;
   }

   // Randomize if set by pref or if the mission played was not on the cycle
   // TODO: Actually make it go on to the next mission in the list instead of randomize when a mission not in the cycle is played?
//   error("LENGTH: "@%length);
   if ($Host::ClassicRandomMissions || !%list[%index])// { 
      %index = getRandom(1, %length);// error("INDEX: "@%index); }
   // Otherwise, on to the next mission
   else
      %index++;

   if (%index > %length)
      %index -= %length;
//   error("INDEX2: "@%index);

   return %list[%index];
}
//------------------------------------------------------------------------------
function GMH_MissionType::onSelect( %this, %id, %text )
{
   // Fill the mission list:
   GMH_MissionList.clear();
   %lastAdded = 0;
   for ( %i = 0; %i < $HostMissionCount[%id];%i++ )
   {
      %misId = $HostMission[%id,%i];
      GMH_MissionList.addRow( %misId, $HostMissionName[%misId] );
      %lastAdded = %misId;
   }
   GMH_MissionList.sort( 0 );

   // Select the last mission added:
   GMH_MissionList.setSelectedById( %lastAdded );
   $Host::MissionType = $HostTypeName[%id];

   // ---------------------------------------------------
   // z0dd - ZOD, 9/29/02. Removed T2 demo code from here

   // Disable all non bot-enabled maps if bots are enabled:
   if ( GMH_BotsEnabledTgl.getValue() )
      GMH_BotsEnabledTgl.onAction();
   // ---------------------------------------------------
}

//------------------------------------------------------------------------------
function GMH_MissionList::onSelect( %this, %id, %text )
{
   if ( GMH_BotsEnabledTgl.getValue() ) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
      GMH_StartGameBtn.setActive( $BotEnabled[%id] );
}


//------------------------------------------------------------------------------
function GMH_MissionList::onSelect( %this, %id, %text )
{
   if ( GMH_BotsEnabledTgl.getValue() ) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
      GMH_StartGameBtn.setActive( $BotEnabled[%id] );
}

//------------------------------------------------------------------------------
function tryToStartHostedGame()
{
   if ( GMH_BotsEnabledTgl.getValue() ) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   {
      %selId = GMH_MissionList.getSelectedId();
      if ( !$BotEnabled[%selId] )
         return;
   }

   StartHostedGame();
}

//------------------------------------------------------------------------------
function StartHostedGame()
{
   %selId = GMH_MissionList.getSelectedId();
   %misFile = $HostMissionFile[%selId];

   if ( $Host::BotsEnabled ) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
	{
		validateMaxPlayers();
      $HostGameBotCount = $Host::BotCount;
	}
   else
      $HostGameBotCount = 0;

   $ServerName = $Host::GameName;
   $Host::Map = %misFile;

   echo( "exporting server prefs..." );
   export( "$Host::*", $serverprefs, false );

   if ( $Host::Dedicated )
   {
      MessageBoxYesNo( "WARNING", 
            "You are about to launch a dedicated server and quit Tribes 2.  Do you want to continue?", 
            "tryToLaunchDedicatedServer(" @ $Host::PureServer @ ");" );
      return;
   }

	//IRCClient::onJoinGame("", "");

   MessagePopup( "STARTING SERVER", "Initializing..." );
   Canvas.repaint();

   cancelServerQuery();
   setNetPort( $Host::Port );
   CreateServer( $Host::Map, $Host::MissionType );
	%playerPref = $pref::Player[$pref::Player::Current];
	%playerName = getField( %playerPref, 0 );
	%playerRaceGender = getField( %playerPref, 1 );
	%playerSkin = getField( %playerPref, 2 );
	%playerVoice = getField( %playerPref, 3 );
	%playerVoicePitch = getField( %playerPref, 4 );
   localConnect( %playerName, %playerRaceGender, %playerSkin, %playerVoice, %playerVoicePitch );
   if(!$RecordDemo)
   {
      // demos are incompatible with local simulated net params
      ServerConnection.setSimulatedNetParams($pref::Net::simPacketLoss, $pref::net::simPing * 0.5);
      LocalClientConnection.setSimulatedNetParams($pref::Net::simPacketLoss, $pref::net::simPing * 0.5);
   }
}

//------------------------------------------------------------------------------
function tryToLaunchDedicatedServer( %pure )
{
   // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   %numBots = $Host::BotsEnabled ? $Host::BotCount : 0;
   if ( launchDedicatedServer( $Host::MissionType, $Host::Map, %numBots, %pure ) )
      quit();
   else
   {
      error( "Failed to launch the dedicated server." );
      schedule( 0, 0, MessageBoxOK, "FAILED", "Tribes 2 failed to launch the dedicated server." );
   }
}

//------------------------------------------------------------------------------
function GMH_BotsEnabledTgl::onAction( %this )
{
   %count = GMH_MissionList.rowCount();
   if ( %this.getValue() )
   {
      for ( %i = 0; %i < %count; %i++ )
      {
         %id = GMH_MissionList.getRowId( %i );
         GMH_MissionList.setRowActive( %id, $BotEnabled[%id] );
      }
      
      GMH_EnableBotsGroup.setVisible(true);
      %misId = GMH_MissionList.getSelectedId();
      GMH_StartGameBtn.setActive( $BotEnabled[%misId] );
   }
   else
   {
      for ( %i = 0; %i < %count; %i++ )
      {
         %id = GMH_MissionList.getRowId( %i );
         GMH_MissionList.setRowActive( %id, true );
      }

      GMH_EnableBotsGroup.setVisible( false );
      GMH_StartGameBtn.setActive( true );
   }
}

//------------------------------------------------------------------------------
function updateMinBotDifficulty()
{
   %min = GMH_BotMinSlider.getValue();
   $Host::MinBotDifficulty = %min;
   if ( GMH_BotMaxSlider.getValue() < %min )
      GMH_BotMaxSlider.setValue( %min );
}

//------------------------------------------------------------------------------
function updateMaxBotDifficulty()
{
   %max = GMH_BotMaxSlider.getValue();
   $Host::MaxBotDifficulty = %max;
   if ( GMH_BotMinSlider.getValue() > %max )
      GMH_BotMinSlider.setValue( %max );
}

//------------------------------------------------------------------------------
function GMH_BotsEnabledTgl::onAction( %this )
{
   %count = GMH_MissionList.rowCount();
   if ( %this.getValue() )
   {
      for ( %i = 0; %i < %count; %i++ )
      {
         %id = GMH_MissionList.getRowId( %i );
         GMH_MissionList.setRowActive( %id, $BotEnabled[%id] );
      }
      
      GMH_EnableBotsGroup.setVisible(true);
      %misId = GMH_MissionList.getSelectedId();
      GMH_StartGameBtn.setActive( $BotEnabled[%misId] );
   }
   else
   {
      for ( %i = 0; %i < %count; %i++ )
      {
         %id = GMH_MissionList.getRowId( %i );
         GMH_MissionList.setRowActive( %id, true );
      }

      GMH_EnableBotsGroup.setVisible(false);
      GMH_StartGameBtn.setActive( true );
   }
}

//------------------------------------------------------------------------------
function validateMaxPlayers()
{
   %maxPlayers = GMH_MaxPlayersTE.getValue();
   if (%maxPlayers < 1)
      %maxPlayers = 1;

   if (%maxPlayers > 64)
      %maxPlayers = 64;

   //reset the value back into the TE
   GMH_MaxPlayersTE.setValue(%maxPlayers);

   // ---------------------------------------------------
   // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   
   //and make sure the bot sliders reflect the changes..
   setBotCountSlider();
   // ---------------------------------------------------
}

function setBotCountSlider()
{
	%maxBots = 31;
	if (%maxBots > $Host::MaxPlayers - 2)
		%maxBots = $Host::MaxPlayers - 2;
	if ($Host::BotCount > %maxBots + 1)
		$Host::BotCount = %maxBots + 1;

	if (%maxBots <= 1)
		%sliderValue = 0.0;
	else
		%sliderValue = ($Host::BotCount - 0.95) / %maxBots;

   GMH_MinCombatantSlider.setValue(%sliderValue);
}

function setMinCombatants()
{
	%maxBots = 32;
	if (%maxBots > $Host::MaxPlayers - 1)
		%maxBots = $Host::MaxPlayers - 1;
	if (%maxBots <= 0)
	   $Host::BotCount = 0;
	else
	   $Host::BotCount = mFloor( GMH_MinCombatantSlider.getValue() * (%maxBots - 1)) + 1;
   GMH_BotCountText.setValue( "(" @ $Host::BotCount @ ")" );
}

//------------------------------------------------------------------------------
function AdvancedHostDlg::onWake( %this )
{
   // Set all of the controls to the current pref states:
   AH_HostPort.setText( $Host::Port );
   if ( $Host::HiVisibility )
      AH_HiVisibilityRdo.setValue( true );
   else
      AH_HiFPSRdo.setValue( true );   
   AH_DedicatedTgl.setValue( $Host::Dedicated );
   AH_DedicatedTgl.onAction();
   AH_TeamDamageTgl.setValue( $Host::TeamDamageOn );
   AH_TournamentTgl.setValue( $Host::TournamentMode );
   AH_AdminVoteTgl.setValue( $Host::allowAdminPlayerVotes );
   AH_AllowSmurfTgl.setValue( !$Host::NoSmurfs );
   AH_TimeLimit.setText( $Host::TimeLimit );
   AH_AdminPassword.setText( $Host::AdminPassword );
   AH_ServerInfo.setText( $Host::Info );
   AH_VotePassSlider.setValue( $Host::VotePassPercent );
   AH_VoteTimeSlider.setValue( $Host::VoteTime );
   AH_RespawnSlider.setValue( $Host::PlayerRespawnTimeout );
   AH_WarmupSlider.setValue( $Host::WarmupTime );
}

//------------------------------------------------------------------------------
function AdvancedHostDlg::accept( %this )
{
   // Apply all of the changes:
   $Host::Port = AH_HostPort.getValue();
   $Host::HiVisibility = AH_HiVisibilityRdo.getValue();
   $Host::Dedicated = AH_DedicatedTgl.getValue();
   if ( $Host::Dedicated )
      $Host::PureServer = AH_PureServerTgl.getValue();
   $Host::TeamDamageOn = AH_TeamDamageTgl.getValue();   
   $Host::TournamentMode = AH_TournamentTgl.getValue();
   $Host::allowAdminPlayerVotes = AH_AdminVoteTgl.getValue();
   $Host::NoSmurfs = !AH_AllowSmurfTgl.getValue();
   $Host::TimeLimit = AH_TimeLimit.getValue();
   $Host::AdminPassword = AH_AdminPassword.getValue();
   $Host::Info = AH_ServerInfo.getText();
   $Host::VotePassPercent = mFloor( AH_VotePassSlider.getValue() );
   $Host::VoteTime = mFloor( AH_VoteTimeSlider.getValue() );
   $Host::PlayerRespawnTimeout = mFloor( AH_RespawnSlider.getValue() );
   $Host::WarmupTime = mFloor( AH_WarmupSlider.getValue() );

   // Save off the new prefs:
   export( "$Host::*", $serverprefs, false );

   Canvas.popDialog( AdvancedHostDlg );
}

//------------------------------------------------------------------------------
function AH_DedicatedTgl::onAction( %this )
{
   if ( %this.getValue() )
   {
      AH_PureServerTgl.setValue( $Host::PureServer );
      AH_PureServerTgl.setActive( true );
   }
   else
   {
      AH_PureServerTgl.setValue( false );
      AH_PureServerTgl.setActive( false );
   }
}

//------------------------------------------------------------------------------
function AH_VotePassText::update( %this )
{
   %this.setText( mFloor( AH_VotePassSlider.getValue() ) @ "%" );
}

//------------------------------------------------------------------------------
function AH_VoteTimeText::update( %this )
{
   %this.setText( mFloor( AH_VoteTimeSlider.getValue() ) SPC "seconds" );
}

//------------------------------------------------------------------------------
function AH_RespawnText::update( %this )
{
   %this.setText( mFloor( AH_RespawnSlider.getValue() ) SPC "seconds" );
}

//------------------------------------------------------------------------------
function AH_WarmupText::update( %this )
{
   %this.setText( mFloor( AH_WarmupSlider.getValue() ) SPC "seconds" );
}

//------------------------------------------------------------------------------
// Warrior Setup pane:
//------------------------------------------------------------------------------
function GM_WarriorPane::onActivate( %this )
{
   GameGui.pane = "Warrior";
	if ( $pref::Player::Count == 0 )
      %this.createNewAlias();
	else
	{
		// Fill the warrior list:
		GMW_WarriorPopup.clear();
      GMW_LightRdo.setValue( true );

      // First add the warrior corresponding to the player nickname:
      %this.warriorIndex = -1;
      if ( $PlayingOnline )
      {
         %warrior = getField( WONGetAuthInfo(), 0 );
         for ( %i = 0; %i < $pref::Player::Count; %i++ )
         {
            %name = getField( $pref::Player[%i], 0 );
            if ( %name $= %warrior )
            {
               %this.warriorIndex = %i;
				   GMW_WarriorPopup.add( %name, %i, 1 );
               break;
            }
         }
      }

      // Add the rest of the aliases:
		for ( %count = 0; %count < $pref::Player::Count; %count++ )
		{
			if ( $pref::Player[%count] !$= "" && %count != %this.warriorIndex )
			{
				%name = stripTrailingSpaces( strToPlayerName( getField( $pref::Player[%count], 0 ) ) );
				GMW_WarriorPopup.add( %name, %count );
			} 
		}

		// Fill the static menus:
		GMW_RaceGenderPopup.fillList();
      GMW_SkinPrefPopup.fillList();

		// Select the current player:
		GMW_WarriorPopup.setSelected( $pref::Player::Current );
		GMW_WarriorPopup.onSelect( $pref::Player::Current, "" );

		if ( $pref::Player::Count > 1 && $pref::Player::Current != %this.warriorIndex )
			GMW_DeleteWarriorBtn.setActive( true );
		else
			GMW_DeleteWarriorBtn.setActive( false );

      GMW_PlayerPageBtn.setVisible( $PlayingOnline );
	}
}

//------------------------------------------------------------------------------
function GM_WarriorPane::onDeactivate( %this )
{
}

//------------------------------------------------------------------------------
function GM_WarriorPane::createNewAlias( %this )
{
	NW_NameEdit.setValue( "" );
	NW_DoneBtn.setActive( false );
   NW_CancelBtn.setVisible( $pref::Player::Count > 0 );
   Canvas.pushDialog( NewWarriorDlg );
}

//------------------------------------------------------------------------------
function GM_WarriorPane::deleteWarrior( %this )
{
   MessageBoxYesNo( "CONFIRM", "Are you sure you want to delete this alias?", "doDeleteWarrior();", "" );
}

//------------------------------------------------------------------------------
function doDeleteWarrior()
{
   // Make sure we aren't trying to delete the default warrior (should never get this):
   if ( $pref::Player::Current == GM_WarriorPane.warriorIndex )
      return;

	for ( %i = $pref::Player::Current; %i < $pref::Player::Count - 1; %i++ )
		$pref::Player[%i] = $pref::Player[%i + 1];
	$pref::Player[%i] = "";

   if ( GM_WarriorPane.warriorIndex > $pref::Player::Current )
      GM_WarriorPane.warriorIndex--;

	$pref::Player::Count--;
   if ( GM_WarriorPane.warriorIndex != -1 )
      $pref::Player::Current = GM_WarriorPane.warriorIndex;
   else
	   $pref::Player::Current = 0;

   // Update the interface:
   GM_WarriorPane::onActivate();
}

//------------------------------------------------------------------------------
function GM_WarriorPane::gotoPlayerPage( %this )
{
   %warrior = getField( WONGetAuthInfo(), 0 );
   LaunchBrowser( %warrior, "Warrior" );
}

//------------------------------------------------------------------------------
function GMW_PlayerModel::update( %this )
{
   // Get the shape names:
   if ( GMW_HeavyRdo.getValue() )
      %armor = "heavy";
   else if ( GMW_MediumRdo.getValue() )
      %armor = "medium";
   else
      %armor = "light";
      
   switch ( GMW_RaceGenderPopup.getSelected() )
   {
      case 1: 
         if ( %armor $= "heavy" )
            %shape = %armor @ "_male";
         else
            %shape = %armor @ "_female";
      case 2: %shape = "bioderm_" @ %armor;
      default: %shape = %armor @ "_male";
   }
   
	%skin = getField( $pref::Player[$pref::Player::Current], 2 );
   
//   if( isObject( $dummySeq ) )
//   {   
//      $dummySeq.delete();
//   }
//   
//   $dummySeq = new TSShapeConstructor()
//   {
//      baseShape = %shape @ ".dts";
//      sequence0 = %shape @ "_forward.dsq dummyRun";
//   };
   
   %this.setModel( %shape, %skin );
}

//------------------------------------------------------------------------------
function GMW_WarriorPopup::onAdd( %this )
{
   %this.addScheme( 1, "255 255 0", "255 255 128", "128 128 0" );
}

//------------------------------------------------------------------------------
function GMW_WarriorPopup::onSelect( %this, %id, %text )
{
	// Set this as the currently selected player:
	$pref::Player::Current = %id;

	// Select the race/gender:
	%raceGender = getField( $pref::Player[%id], 1 );
	%selId = GMW_RaceGenderPopup.findText( %raceGender );
	if ( %selId == -1 )
		%selId = 0;

	GMW_RaceGenderPopup.setSelected( %selId );
	GMW_VoicePopup.fillList( %selId );

	// Select the skin:
   %skin = getField( $pref::Player[%id], 2 );
   %baseSkin = isDynamixSkin( %skin );
   GMW_SkinPrefPopup.setSelected( !%baseSkin );
	GMW_SkinPopup.fillList( %selId );
   
   %selId = -1;
   for ( %i = 0; %i < GMW_SkinPopup.size(); %i++ )
   {
      if ( GMW_SkinPopup.realSkin[%i] !$= "" )
      {
         if ( %skin $= GMW_SkinPopup.realSkin[%i] )
         {
            %selId = %i;
            break;
         }
      }
      else if ( %skin $= GMW_SkinPopup.getTextById( %i ) )
      {
         %selId = %i;
         break;
      }
   }
   if ( %selId == -1 )
      %selId = 0;
	GMW_SkinPopup.setSelected( %selId );
   GMW_SkinPopup.onSelect( %selId, GMW_SkinPopup.getTextById( %selId ) );

	// Select the voice:
	%voice = getField( $pref::Player[%id], 3 );
	%voiceId = getSubStr( %voice, strlen( %voice ) -1, 1000 ) - 1;
	GMW_VoicePopup.setSelected( %voiceId );
	GMW_VoicePopup.voiceIndex = 0;

   GMW_DeleteWarriorBtn.setActive( $pref::Player::Count > 1 && %id != GM_WarriorPane.warriorIndex );
}

//------------------------------------------------------------------------------
function GMW_RaceGenderPopup::fillList( %this )
{
   if ( %this.size() )
      return;
      
	%this.add( "Human Male", 0 );
	%this.add( "Human Female", 1 );
	%this.add( "Bioderm", 2 );
}

//------------------------------------------------------------------------------
function GMW_RaceGenderPopup::onSelect( %this, %id, %text )
{
	// Update the player pref:
	$pref::Player[$pref::Player::Current] = setField( $pref::Player[$pref::Player::Current], 1, %this.getText() );

	// Fill the skin list:
	%prevSkin = GMW_SkinPopup.getText();
 	GMW_SkinPopup.fillList( %id );
	%selId = GMW_SkinPopup.findText( %prevSkin );
	if ( %selId == -1 )
		%selId = 0;
	GMW_SkinPopup.setSelected( %selId );
   GMW_SkinPopup.onSelect( %selId, GMW_SkinPopup.getTextById( %selId ) );

	// Fill the voice list:
	%prevVoice = GMW_VoicePopup.getText();
	GMW_VoicePopup.fillList( %id );
	%selId = GMW_VoicePopup.findText( %prevVoice );
	if ( %selId == -1 )
		%selId = 0;

	GMW_VoicePopup.setSelected( %selId );
   GMW_VoicePopup.onSelect( %selId, "" );
}

//------------------------------------------------------------------------------
function GMW_SkinPrefPopup::fillList( %this )
{
   if ( %this.size() )
      return;
      
   %this.add( "Dynamix Skins", 0 );
   %this.add( "Custom Skins", 1 );
}

//------------------------------------------------------------------------------
function GMW_SkinPrefPopup::onSelect( %this, %id, %text )
{
   %curSkin = GMW_SkinPopup.getText();
   GMW_SkinPopup.fillList( GMW_RaceGenderPopup.getSelected() ); 
   %selId = GMW_SkinPopup.findText( %curSkin );
   if ( %selId == -1 )
      %selId = 0;
   
   if ( GMW_SkinPopup.size() )
   {   
      GMW_SkinPopup.setSelected( %selId );
      GMW_SkinPopup.onSelect( %selId, GMW_SkinPopup.getTextById( %selId ) );   
   }
}

//------------------------------------------------------------------------------
$SkinCount = 0;
$Skin[$SkinCount, name] = "Blood Eagle";
$Skin[$SkinCount, code] = "beagle";
$SkinCount++;
$Skin[$SkinCount, name] = "Diamond Sword";
$Skin[$SkinCount, code] = "dsword";
$SkinCount++;
$Skin[$SkinCount, name] = "Starwolf";
$Skin[$SkinCount, code] = "swolf";
$SkinCount++;
$Skin[$SkinCount, name] = "Phoenix";
$Skin[$SkinCount, code] = "cotp";
$SkinCount++;
$Skin[$SkinCount, name] = "Storm";
$Skin[$SkinCount, code] = "base";
$SkinCount++;
$Skin[$SkinCount, name] = "Inferno";
$Skin[$SkinCount, code] = "baseb";
$SkinCount++;
$Skin[$SkinCount, name] = "Horde";
$Skin[$SkinCount, code] = "horde";
$SkinCount++;

//------------------------------------------------------------------------------
function isDynamixSkin( %skin )
{
   for ( %i = 0; %i < $SkinCount; %i++ )
   {
      if ( %skin $= $Skin[%i, code] )
         return( true );
   }
   
   return( false );
}

//------------------------------------------------------------------------------
function GMW_SkinPopup::fillList( %this, %raceGender )
{
   for ( %i = 0; %i < %this.size(); %i++ )
      %this.realSkin[%i] = "";

	%this.clear();
   %path = "textures/skins/";
   switch ( %raceGender )
   {
      case 0:  // Human Male
         %pattern = ".lmale.png";
      case 1:  // Human Female
         %pattern = ".lfemale.png";
      case 2:  // Bioderm
         %pattern = ".lbioderm.png";
   }

   %customSkins = GMW_SkinPrefPopup.getSelected();
   %count = 0;
   for ( %file = findFirstFile( %path @ "*" @ %pattern ); %file !$= ""; %file = findNextFile( %path @ "*" @ %pattern ) )
   {
      %skin = getSubStr( %file, strlen( %path ), strlen( %file ) - strlen( %path ) - strlen( %pattern ) );  // strip off the path and postfix

      // Make sure this is not a bot skin:
      if ( %skin !$= "basebot" && %skin !$= "basebbot" )
      {
         // See if this skin has an alias:
         %baseSkin = false;
         for ( %i = 0; %i < $SkinCount; %i++ )
         {
            if ( %skin $= $Skin[%i, code] ) 
            {
               %baseSkin = true;
               %skin = $Skin[%i, name];
               break;
            }
         }

         if ( %customSkins != %baseSkin )
         {
            if ( %baseSkin )
               %this.realSkin[%count] = $Skin[%i, code];
            %this.add( %skin, %count );
            %count++;
         }
      }
   }
   
   %this.sort( true );
}

//------------------------------------------------------------------------------
function GMW_SkinPopup::onSelect( %this, %id, %text )
{
	// Update the player pref:
   if ( %this.realSkin[%id] !$= "" )
	   $pref::Player[$pref::Player::Current] = setField( $pref::Player[$pref::Player::Current], 2, %this.realSkin[%id] );
   else
	   $pref::Player[$pref::Player::Current] = setField( $pref::Player[$pref::Player::Current], 2, %text );

	// Update the player model:
   GMW_PlayerModel.update();
}

//------------------------------------------------------------------------------
// TRANSLATE these voice set display names:
$MaleVoiceCount = 0;
$MaleVoiceName[$MaleVoiceCount] = "Hero";
$MaleVoiceCount++;
$MaleVoiceName[$MaleVoiceCount] = "Iceman";
$MaleVoiceCount++;
$MaleVoiceName[$MaleVoiceCount] = "Rogue";
$MaleVoiceCount++;
$MaleVoiceName[$MaleVoiceCount] = "Hardcase";
$MaleVoiceCount++;
$MaleVoiceName[$MaleVoiceCount] = "Psycho";
$MaleVoiceCount++;

$FemaleVoiceCount = 0;
$FemaleVoiceName[$FemaleVoiceCount] = "Heroine";
$FemaleVoiceCount++;
$FemaleVoiceName[$FemaleVoiceCount] = "Professional";
$FemaleVoiceCount++;
$FemaleVoiceName[$FemaleVoiceCount] = "Cadet";
$FemaleVoiceCount++;
$FemaleVoiceName[$FemaleVoiceCount] = "Veteran";
$FemaleVoiceCount++;
$FemaleVoiceName[$FemaleVoiceCount] = "Amazon";
$FemaleVoiceCount++;

$DermVoiceCount = 0;
$DermVoiceName[$DermVoiceCount] = "Warrior";
$DermVoiceCount++;
$DermVoiceName[$DermVoiceCount] = "Monster";
$DermVoiceCount++;
$DermVoiceName[$DermVoiceCount] = "Predator";
$DermVoiceCount++;

//------------------------------------------------------------------------------
function GMW_VoicePopup::fillList( %this, %raceGender )
{
	%this.clear();

	switch ( %raceGender )	
	{
		case 0: // Human Male
			for ( %i = 0; %i < $MaleVoiceCount; %i++ )
				%this.add( $MaleVoiceName[%i], %i );

		case 1: // Human Female
			for ( %i = 0; %i < $FemaleVoiceCount; %i++ )
				%this.add( $FemaleVoiceName[%i], %i );

		case 2: // Bioderm
			for ( %i = 0; %i < $DermVoiceCount; %i++ )
				%this.add( $DermVoiceName[%i], %i );
	}
}

//------------------------------------------------------------------------------
function GMW_VoicePopup::onSelect( %this, %id, %text )
{
	// Update the player pref:
	switch ( GMW_RaceGenderPopup.getSelected() )
	{
		case 0: %base = "Male";
		case 1: %base = "Fem";
		case 2: %base = "Derm";
	}

	$pref::Player[$pref::Player::Current] = setField( $pref::Player[$pref::Player::Current], 3, %base @ ( %id + 1 ) );

	%this.voiceIndex = 0;
}

//------------------------------------------------------------------------------
function GMW_VoicePitchSlider::setPitch(%this)
{
}

function GMW_VoicePopup::test( %this )
{
	switch ( %this.voiceIndex )
	{
		case 0: %file = "gbl.hi";
		case 1: %file = "gbl.brag";
		case 2: %file = "gbl.woohoo";
		case 3: %file = "gbl.rock";
		case 4: %file = "gbl.obnoxious";
		case 5: %file = "gbl.shazbot";
	}

	switch ( GMW_RaceGenderPopup.getSelected() )
	{
		case 0: %base = "Male";
		case 1: %base = "Fem";
		case 2: %base = "Derm";
	}

   GMW_VoiceTestBtn.setActive( false );
	%voiceId = %this.getSelected() + 1;
	%wav = "voice/" @ %base @ %voiceId @ "/" @ %file @ ".wav";
   %handle = alxCreateSource( AudioGui, %wav );

   //pitch the voice
   //%pitchSliderVal = GMW_VoicePitchSlider.getValue();
   //%pitch = getValidVoicePitch(%voiceId, %pitchSliderVal);
   //if (%pitch != 1.0)
   //   alxSourcef(%handle, "AL_PITCH", %pitch);

   alxPlay( %handle );

   %delay = alxGetWaveLen( %wav );
   schedule( %delay, 0, "restoreVoiceTestButton" );

	if ( %this.voiceIndex == 5 )
		%this.voiceIndex = 0;
	else
		%this.voiceIndex++;
}

//------------------------------------------------------------------------------
function restoreVoiceTestButton()
{
   GMW_VoiceTestBtn.setActive( true );
}

//------------------------------------------------------------------------------
function NewWarriorDlg::createPlayer( %this )
{
	%name = stripTrailingSpaces( NW_NameEdit.getValue() );
	$pref::Player[$pref::Player::Count] = %name @ "\tHuman Male\tbeagle\tMale1";
	$pref::Player::Current = $pref::Player::Count;
	$pref::Player::Count++;
	Canvas.popDialog( NewWarriorDlg );
   GM_WarriorPane.onActivate();  // Refresh the warrior gui
}

//------------------------------------------------------------------------------
function NW_NameEdit::checkValidPlayerName( %this )
{
   %name = %this.getValue();
   %test = strToPlayerName( %name );
   if ( %name !$= %test )
	   %this.setValue( %test );

	NW_DoneBtn.setActive( strlen( stripTrailingSpaces( %test ) ) > 2 );
}

//------------------------------------------------------------------------------
function NW_NameEdit::processEnter( %this )
{
   %this.checkValidPlayerName();
	if ( NW_DoneBtn.isActive() )
      NewWarriorDlg.createPlayer();
}
