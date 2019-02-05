//Fire Autobalance
function Autobalance( %game, %AutobalanceSafetynetTrys )
{
	if( $CurrentMissionType !$= "LakRabbit" && $Host::EnableTeamBalanceNotify && $StatsMsgPlayed $= 1 && !$Host::TournamentMode )
	{
		//%AutobalanceDebug = true;
		
		if( $Team1Difference == 1 || $Team2Difference == 1 || $PlayerCount[1] == $PlayerCount[2] )
		{
			$StatsMsgPlayed = 0;
			return;
		}
		//Safetynet
		else if(( $team1canidate $= "" && $Team1Difference >= 2 )||( $team2canidate $= "" && $Team2Difference >= 2 ))
		{
			%AutobalanceSafetynetTrys++; if(%AutobalanceSafetynetTrys $= 3) return;

			if( %AutobalanceDebug )
			{
				if( $team1canidate $= "" ) $team1canidate = "NULL"; if( $team2canidate $= "" ) $team2canidate = "NULL";
				messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: %1, %2, %3, %4', $team1canidate, $team2canidate, $Team1Difference, $Team2Difference );
				if( $team1canidate $= "NULL" ) $team1canidate = ""; if( $team2canidate $= "NULL" ) $team2canidate = "";
			}
			else if( $team1canidate $= "" && $team2canidate $= "" ) messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Both Teams' );
			else if( $team1canidate $= "" ) messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Team1' );
			else if( $team2canidate $= "" ) messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Team2' );
			
			//Trigger GetCounts
			ResetClientChangedTeams();
			//Rerun in 10 secs
			schedule(10000, 0, "Autobalance", %game, %AutobalanceSafetynetTrys );
			//Clear Canidates
			$team1canidate = ""; $team2canidate = "";
			return;
		}
		//Team 1
		else if( $Team1Difference >= 2 )
		{	
			if( %AutobalanceDebug )
			{
				if( $team1canidate $= "" ) $team1canidate = "NULL"; if( $team2canidate $= "" ) $team2canidate = "NULL";
				messageAll('MsgTeamBalanceNotify', '\c0Autobalance stat: %1, %2, %3, %4', $team1canidate, $team2canidate, $Team1Difference, $Team2Difference );
				if( $team1canidate $= "NULL" ) $team1canidate = ""; if( $team2canidate $= "NULL" ) $team2canidate = "";
			}
			
			%client = $team1canidate;
			%team = $team1canidate.team;
			%otherTeam = ( %team == 1 ) ? 2 : 1;
			
			if( $team1canidate.team $= 1 )
			{
				Game.clientChangeTeam( %client, %otherTeam, 0 );
				messageAll('MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');
			}
			else
				messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Team1 mismatch.' );
			
			//Trigger GetCounts
			ResetClientChangedTeams();
			//Reset Stats.
			$StatsMsgPlayed = 0;
			//Clear Canidates
			$team1canidate = ""; $team2canidate = "";
			return;
		}
		//Team 2
		else if( $Team2Difference >= 2 )
		{
			if( %AutobalanceDebug )
			{
				if( $team1canidate $= "" ) $team1canidate = "NULL"; if( $team2canidate $= "" ) $team2canidate = "NULL";
				messageAll('MsgTeamBalanceNotify', '\c0Autobalance stat: %1, %2, %3, %4', $team1canidate, $team2canidate, $Team1Difference, $Team2Difference );
				if( $team1canidate $= "NULL" ) $team1canidate = ""; if( $team2canidate $= "NULL" ) $team2canidate = "";
			}
			
			%client = $team2canidate;
			%team = $team2canidate.team;
			%otherTeam = ( %team == 1 ) ? 2 : 1;
			
			if( $team2canidate.team $= 2 )
			{
				Game.clientChangeTeam( %client, %otherTeam, 0 );
				messageAll('MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');
			}
			else
				messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Team2 mismatch.' );
			
			//Trigger GetCounts
			ResetClientChangedTeams();
			//Reset Stats.
			$StatsMsgPlayed = 0;
			//Clear Canidates
			$team1canidate = ""; $team2canidate = "";
			return;
		}
	}
}