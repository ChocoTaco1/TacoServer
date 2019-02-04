//Fire Autobalance
function Autobalance( %game, %client, %respawn )
{
	if( $CurrentMissionType !$= "LakRabbit" && $Host::EnableTeamBalanceNotify && $StatsMsgPlayed $= 1 && !$Host::TournamentMode )
	{
		if( $Team1Difference == 1 || $Team2Difference == 1 || $PlayerCount[1] == $PlayerCount[2] )
		{
			$StatsMsgPlayed = 0;
			return;
		}
		//Safetynet
		else if( $team1canidate $= "" || $team2canidate $= "" )
		{
			messageAll('MsgTeamBalanceNotify', '\c0Autobalance error.');
			//Trigger GetCounts
			ResetClientChangedTeams();
			//Reset Stats.
			$StatsMsgPlayed = 0;
			return;
		}
		//Team 1
		else if( $Team1Difference >= 2 )
		{	
			%client = $team1canidate;
			%team = $team1canidate.team;
			%otherTeam = ( %team == 1 ) ? 2 : 1;
			
			Game.clientChangeTeam( %client, %otherTeam, 0 );
			messageAll('MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');
			
			//Trigger GetCounts
			ResetClientChangedTeams();
			//Reset Stats.
			$StatsMsgPlayed = 0;
			return;
		}
		//Team 2
		else if( $Team2Difference >= 2 )
		{
			%client = $team2canidate;
			%team = $team2canidate.team;
			%otherTeam = ( %team == 1 ) ? 2 : 1;
			
			Game.clientChangeTeam( %client, %otherTeam, 0 );
			messageAll('MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');
			
			//Trigger GetCounts
			ResetClientChangedTeams();
			//Reset Stats.
			$StatsMsgPlayed = 0;
			return;
		}
	}
}