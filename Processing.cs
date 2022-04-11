using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using TeamMatchGenV2.Models;

namespace TeamMatchGenV2
{
    public class Processing
    {
        public static List<Team> ManualXLSXTeamImport(string path)
        {
            var Teams = new List<Team>();
            FileInfo fileInfo = new FileInfo(path);

            ExcelPackage package = new ExcelPackage(fileInfo);
            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

            for (int i = 2; i <= worksheet.Dimension.Rows; i++)
            {
                var t = new Team();
                t.teamNumber = worksheet.Cells[i, 1].Value.ToString();
                t.nameShort = worksheet.Cells[i, 2].Value.ToString();
                Teams.Add(t);
            }

            return Teams;
        }

        public static List<Match> ManualXLSXMatchImport(string path)
        {
            var Matches = new List<Match>();
            FileInfo fileInfo = new FileInfo(path);

            ExcelPackage package = new ExcelPackage(fileInfo);
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

            for (int i = 2; i <= worksheet.Dimension.Rows; i++)
            {
                var m = new Match();
                m.matchNumber = worksheet.Cells[i, 1].Value.ToString();
                m.teams = new List<Team>()
                {
                    new Team(){station="Red1",teamNumber = worksheet.Cells[i, 2].Value.ToString()},
                    new Team(){station="Red2",teamNumber = worksheet.Cells[i, 3].Value.ToString()},
                    new Team(){station="Blue1",teamNumber = worksheet.Cells[i, 4].Value.ToString()},
                    new Team(){station="Blue2",teamNumber = worksheet.Cells[i, 5].Value.ToString()}
                };
                Matches.Add(m);
            }

            return Matches;
        }

        public static List<Team> ExtractTeamsFromMatches(List<Match> Matches, List<Team> AllTeams)
        {
            var ParticipatingTeams = new List<Team>();
            var AllMatchTeams = Matches.SelectMany(m => m.teams);
            foreach (var t in AllTeams)
            {
                if (AllMatchTeams.Any(m => m.teamNumber == t.teamNumber))
                    ParticipatingTeams.Add(t);
            }

            return ParticipatingTeams;
        }

        public static List<Match> CompleteTeamInfoInMatchList(List<Match> Matches, List<Team> ParticipatingTeams)
        {
            foreach (var m in Matches)
            {
                foreach (var t in m.teams)
                {
                    if (t.nameShort == null)
                    {
                        var pt = ParticipatingTeams.First(p => p.teamNumber == t.teamNumber);
                        t.nameShort = pt.nameShort;
                        //Fill in and match other fields as necessary
                    }
                }
            }

            return Matches;
        }

        public static List<PerTeamSchedule> ConvertToPerTeamSchedules(List<Match> Matches, List<Team> Teams)
        {
            List<PerTeamSchedule> schedule = new List<PerTeamSchedule>();

            foreach (var t in Teams)
            {
                var pts = new PerTeamSchedule();
                pts.teamNumber = t.teamNumber;
                pts.teamName = t.nameShort;
                if (pts.teamName.Length > 35)
                    pts.teamName = pts.teamName.Substring(0, 35);
                pts.matches = new List<PerTeamScheduleMatch>();
                foreach (var m in Matches)
                {
                    if (m.teams.Any(mt => mt.teamNumber == t.teamNumber))
                    {
                        var myself = m.teams.First(mt => mt.teamNumber == t.teamNumber);
                        var partner = m.teams.First(mt =>
                            mt.teamNumber != myself.teamNumber && mt.station.ToLower()[0] == myself.station.ToLower()[0]);
                        var opponent1 = m.teams.First(mt =>
                            mt.teamNumber != myself.teamNumber && mt.teamNumber != partner.teamNumber);
                        var opponent2 = m.teams.First(mt =>
                            mt.teamNumber != myself.teamNumber && mt.teamNumber != partner.teamNumber && mt.teamNumber != opponent1.teamNumber);

                        var ptsm = new PerTeamScheduleMatch();
                        ptsm.matchNumber = "Q" + m.matchNumber;
                        if (myself.surrogate)
                            ptsm.matchNumber += "*";
                        if (myself.station.ToLower().Contains("b"))
                            ptsm.isBlueTeam = true;

                        ptsm.pNumber = partner.teamNumber;
                        ptsm.pName = partner.nameShort;
                        ptsm.o1Number = opponent1.teamNumber;
                        ptsm.o1Name = opponent1.nameShort;
                        ptsm.o2Number = opponent2.teamNumber;
                        ptsm.o2Name = opponent2.nameShort;

                        if (ptsm.pName.Length > 20)
                            ptsm.pName = ptsm.pName.Substring(0, 20);
                        if (ptsm.o1Name.Length > 20)
                            ptsm.o1Name = ptsm.o1Name.Substring(0, 20);
                        if (ptsm.o2Name.Length > 20)
                            ptsm.o2Name = ptsm.o2Name.Substring(0, 20);

                        pts.matches.Add(ptsm);
                    }
                }
                schedule.Add(pts);
            }

            return schedule;
        }
    }
}