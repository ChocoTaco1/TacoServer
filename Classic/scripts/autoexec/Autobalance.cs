//Fire Autobalance
function Autobalance( %game, %client, %respawn )
{
	if( $CurrentMissionType !$= "LakRabbit" && $Host::EnableTeamBalanceNotify && $StatsMsgPlayed $= 1 && !$Host::TournamentMode )
	{				
		//Generate random to get random client
		%team1random = getRandom(1,$PlayerCount[1]);
		%team2random = getRandom(1,$PlayerCount[2]);
		
		%AutobalanceCount[0] = 0;
		%AutobalanceCount[1] = 0;
		%AutobalanceCount[2] = 0;
		
		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			%client = ClientGroup.getObject(%i);
			
			//For autobalance
			//Pick a random client on a team
			if( %client.team == 1 && %team1random == %AutobalanceCount[1] )
				%team1canidate = %client;
			if( %client.team == 2 && %team2random == %AutobalanceCount[2] )
				%team2canidate = %client;
			
			//if(!%client.isAIControlled())
			%AutobalanceCount[%client.team]++;
			
			//Safetynet
			if( %team1canidate $= "" )
				%team1canidate = %client;
			if( %team2canidate $= "" )
				%team2canidate = %client;
		}
		
		if( $Team1Difference == 1 || $Team2Difference == 1 || $PlayerCount[1] == $PlayerCount[2] )
		{
			$StatsMsgPlayed = 0;
			return;
		}
		//Safetynet
		else if( team1canidate $= "" || team2canidate $= "" )
		{
			schedule(2500, 0, "Autobalance", %game, %client, %respawn);
			return;			
		}
		//Team 1
		else if( $Team1Difference >= 2 )
		{	
			%client = %team1canidate;
			%team = %team1canidate.team;
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
			%client = %team2canidate;
			%team = %team2canidate.team;
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