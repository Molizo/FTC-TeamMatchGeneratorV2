namespace TeamMatchGenV2.Models
{
    public class Event
    {
        public string eventId,
            code,
            divisionCode,
            name,
            type,
            typeName,
            regionCode,
            leagueCode,
            districtCode,
            venue,
            address,
            city,
            stateprov,
            country,
            website,
            liveStreamUrl,
            timezone;

        public DateTime dateStart, dateEnd;
        public bool remote, hybrid, published;
        public int fieldCount;
    }

    public class ListOfEvents
    {
        public List<Event> events;
        public int eventCount;
    }

    public class Match
    {
        public string matchNumber, description, field, tournamentLevel;
        public DateTime startTime, modifiedOn;
        public int series;
        public List<Team> teams;
    }

    public class Team
    {
        public string
            station,
            nameFull,
            nameShort,
            schoolName,
            city,
            stateProv,
            country,
            website,
            robotName,
            districtCode,
            homeCMP, teamNumber;

        public int rookieYear;

        public bool surrogate, noShow;
    }

    public class ListOfTeams
    {
        public List<Team> teams;
        public int teamCountTotal, teamCountPage, pageCurrent, pageTotal;
    }

    public class ListOfMatches
    {
        public List<Match> schedule;
    }

    public class PerTeamSchedule
    {
        public string teamName, teamNumber;
        public List<PerTeamScheduleMatch> matches;
    }

    public class PerTeamScheduleMatch
    {
        public string matchNumber;
        public bool isBlueTeam;
        public string pNumber, pName, o1Number, o1Name, o2Number, o2Name;
    }
}