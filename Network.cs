using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TeamMatchGenV2.Models;

namespace TeamMatchGenV2.Network
{
    public class Fetch
    {
        public static List<Event> FetchEvents(int selectedSeason)
        {
            var AllEvents = new List<Event>();
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "bW9saXpvOjA5NDQ4QzM3LUZGQUUtNDExRi1BQ0I2LTE5REZCMDY3RkUwRg==");

            var responseString = http.GetStringAsync("https://ftc-api.firstinspires.org" + "/v2.0/" + selectedSeason + "/events").Result;
            AllEvents = JsonConvert.DeserializeObject<ListOfEvents>(responseString).events.Where(e => e.remote == false).OrderBy(e => e.name).ToList();

            return AllEvents;
        }

        public static async Task<List<Team>> FetchAllTeams(int selectedSeason, string eventCode = "0")
        {
            //I know it's more efficient to fetch each team individually after the match list has been loaded but the problem with that is for big events (>99 teams as of April 2022) there will be more teams than pages in the teams database, so it's more efficient network-wise like this.
            var AllTeams = new List<Team>();
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "bW9saXpvOjA5NDQ4QzM3LUZGQUUtNDExRi1BQ0I2LTE5REZCMDY3RkUwRg==");

            var responseString = await http.GetStringAsync("https://ftc-api.firstinspires.org" + "/v2.0/" + selectedSeason + "/teams?eventCode=" + eventCode);
            var responseList = JsonConvert.DeserializeObject<ListOfTeams>(responseString);
            AllTeams.AddRange(responseList.teams);

            for (int i = 2; i <= responseList.pageTotal; i++)
            {
                var r = await http.GetStringAsync("https://ftc-api.firstinspires.org" + "/v2.0/" + selectedSeason + "/teams?eventCode=" + eventCode + "&page=" + i);
                AllTeams.AddRange(JsonConvert.DeserializeObject<ListOfTeams>(r).teams);
            }

            return AllTeams;
        }

        public static List<Match> FetchMatches(int selectedSeason, Event ev)
        {
            var Matches = new List<Match>();
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "bW9saXpvOjA5NDQ4QzM3LUZGQUUtNDExRi1BQ0I2LTE5REZCMDY3RkUwRg==");

            var responseString = http.GetStringAsync("https://ftc-api.firstinspires.org" + "/v2.0/" + selectedSeason + "/schedule/" + ev.code + "?tournamentLevel=qual").Result;
            Matches.AddRange(JsonConvert.DeserializeObject<ListOfMatches>(responseString).schedule);

            return Matches;
        }
    }
}