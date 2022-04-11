using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Terminal.Gui;
using NStack;
using OfficeOpenXml;
using TeamMatchGenV2;
using TeamMatchGenV2.Models;
using TeamMatchGenV2.Network;

internal class Program
{
    public static Window win;
    public static List<Event> AllEvents;
    public static List<Team> AllTeams;
    public static List<Team> ParticipatingTeams;
    public static List<Match> Matches;

    private static void Main()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        Application.Init();
        var top = Application.Top;

        win = new Window("FTC Team Match Generator V2")
        {
            X = 0,
            Y = 1, // Leave one row for the toplevel menu

            // By using Dim.Fill(), it will automatically resize without manual intervention
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        top.Add(win);

        var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("Actions", new MenuItem [] {
                new MenuItem ("_Go to start", "", () => { SetupSeasonSelectView(); }),
                new MenuItem ("_Quit app", "", () => { if (QuitDialog ()) top.Running = false; }),
                new MenuItem ("_About", "", () => { AboutDialog(); })
            })
        });
        top.Add(menu);

        static bool QuitDialog()
        {
            var n = MessageBox.Query(50, 7, "Quit app", "Are you sure you want to quit FTC Team Match Generator?", "Yes", "No");
            return n == 0;
        }

        static void AboutDialog()
        {
            var n = MessageBox.Query(50, 7, "About the author", "Hey! Thanks for stopping by!\nYou can check out my other projects at\ngithub.com/Molizo\n©2019-2022 Mihnea-Theodor Visoiu", "Bye!");
        }

        SetupSeasonSelectView();

        Application.Run();
    }

    private static async void SetupEventInfoConfirmation(int selectedSeason, Event ev)
    {
        ShowLoadingWindow();

        if (ev.code != null && ev.code != "IMPORT")
        {
            AllTeams = await Fetch.FetchAllTeams(selectedSeason, ev.code);
            Matches = Fetch.FetchMatches(selectedSeason, ev);
            ParticipatingTeams = Processing.ExtractTeamsFromMatches(Matches, AllTeams);
        }

        var eventLabel = new Label("Please confirm event info for " + ev.name + " (" + ev.code + ")") { X = 2, Y = 1 };

        var teamsListField = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Source = new ListWrapper(ParticipatingTeams.Select(e => "#" + e.teamNumber + " " + e.nameShort).ToList())
        };

        var teamsListContainer = new Window()
        {
            X = Pos.Left(eventLabel),
            Y = Pos.Bottom(eventLabel) + 1,
            Width = Dim.Percent(50) - 2,
            Height = Dim.Fill() - 1
        };
        teamsListContainer.Title = "Teams";
        teamsListContainer.Add(teamsListField);

        var matchesList = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Source = new ListWrapper(Matches.Select(e => "Q" +
                                                         e.matchNumber + " | " +
                                                         e.teams[0].teamNumber + " " +
                                                         e.teams[1].teamNumber + " vs " +
                                                         e.teams[2].teamNumber + " " +
                                                         e.teams[3].teamNumber).ToList())
        };

        var matchesListContainer = new Window()
        {
            X = Pos.Right(teamsListContainer) + 1,
            Y = Pos.Bottom(eventLabel) + 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 1
        };
        matchesListContainer.Title = "Matches";
        matchesListContainer.Add(matchesList);

        var exportButton = new Button()
        {
            X = Pos.Right(win) - 29,
            Y = Pos.Top(eventLabel),
            Text = "Continue to exporting",
            HotKey = Key.Enter
        };
        exportButton.Clicked += () =>
        {
            ShowConfigureExport(ev);
        };
        var goBackButton = new Button()
        {
            X = Pos.Right(win) - 15,
            Y = Pos.Bottom(exportButton),
            Text = "Go back"
        };
        goBackButton.Clicked += () =>
        {
            SetupSeasonSelectView();
        };

        win.RemoveAll();
        win.Title = "FTC Team Match Generator V2 - Event info - " + ev.code;
        win.Add(eventLabel, teamsListContainer, matchesListContainer, exportButton, goBackButton);
    }

    private static void ShowLoadingWindow()
    {
        var loadingLabel = new Label("Loading...")
        {
            X = 2,
            Y = 1
        };
        win.RemoveAll();
        win.Add(loadingLabel);
        Application.Refresh();
    }

    private static void ShowConfigureExport(Event ev)
    {
        var headerLabel = new Label("Please configure the schedule generation and export settings")
        {
            X = 2,
            Y = 1
        };
        var pageWidthLabel = new Label("Page width (in mm):  ")
        {
            X = Pos.Left(headerLabel),
            Y = Pos.Bottom(headerLabel) + 2
        };
        var pageHeightLabel = new Label("Page height (in mm): ")
        {
            X = Pos.Left(headerLabel),
            Y = Pos.Bottom(pageWidthLabel)
        };
        var pageSizeReferenceLabel = new Label()
        {
            X = Pos.Left(headerLabel),
            Y = Pos.Bottom(pageHeightLabel),
            Height = 2,
            Width = Dim.Width(win) - 2,
            Text = "For best results, we recommend 148.5x105 (A6 landscape) for up\nto 6 matches and 148.5x210 (A5 portrait) for up to 14 matches"
        };
        var providedByLabel = new Label("Provided by field content: ")
        {
            X = Pos.Left(headerLabel),
            Y = Pos.Bottom(pageSizeReferenceLabel) + 2
        };

        var pageWidthTextField = new TextField()
        {
            X = Pos.Right(pageWidthLabel),
            Y = Pos.Top(pageWidthLabel),
            Width = 41,
            Text = "148.5"
        };
        var pageHeightTextField = new TextField()
        {
            X = Pos.Right(pageHeightLabel),
            Y = Pos.Top(pageHeightLabel),
            Width = 41,
            Text = "105"
        };

        var providedByTextField = new TextField()
        {
            X = Pos.Left(headerLabel),
            Y = Pos.Bottom(providedByLabel),
            Width = Dim.Fill() - 2,
            Text = "Provided by Mihnea-Theodor Visoiu. Schedule subject to change."
        };

        var exportButton = new Button()
        {
            X = Pos.Left(headerLabel),
            Y = Pos.Top(providedByTextField) + 2,
            Text = "Export match schedules to PDF",
            HotKey = Key.Enter
        };
        exportButton.Clicked += () =>
        {
            ShowLoadingWindow();
            Matches = Processing.CompleteTeamInfoInMatchList(Matches, ParticipatingTeams);
            var pts = Processing.ConvertToPerTeamSchedules(Matches, ParticipatingTeams);
            Export.PDF(pts, providedByTextField.Text.ToString(), float.Parse(pageWidthTextField.Text.ToString()), float.Parse(pageHeightTextField.Text.ToString()));
            SetupSeasonSelectView();
        };
        var goBackButton = new Button()
        {
            X = Pos.Left(headerLabel),
            Y = Pos.Bottom(exportButton),
            Text = "Go to start"
        };
        goBackButton.Clicked += () =>
        {
            SetupSeasonSelectView();
        };

        win.RemoveAll();
        win.Title = "FTC Team Match Generator V2 - Configure export - " + ev.code;
        win.Add(headerLabel, pageWidthLabel, pageHeightLabel, pageSizeReferenceLabel, providedByLabel, pageWidthTextField, pageHeightTextField, providedByTextField, exportButton, goBackButton);
    }

    private static void SetupEventPickView(int selectedSeason)
    {
        ShowLoadingWindow();

        AllEvents = Fetch.FetchEvents(selectedSeason);

        var eventLabel = new Label("Please select the event. Only showing live and hybrid events.") { X = 2, Y = 1 };
        var eventListField = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Source = new ListWrapper(AllEvents.Select(e => e.name).ToList())
        };

        eventListField.OpenSelectedItem += (eventListField) =>
        {
            SetupEventInfoConfirmation(selectedSeason, AllEvents[eventListField.Item]);
        };

        var eventsListContainer = new Window()
        {
            X = Pos.Left(eventLabel),
            Y = Pos.Bottom(eventLabel) + 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 1
        };
        eventsListContainer.Title = "Events";
        eventsListContainer.Add(eventListField);

        var goBackButton = new Button()
        {
            X = Pos.Right(win) - 15,
            Y = Pos.Top(eventLabel),
            Text = "Go back",
            HotKey = Key.Enter
        };
        goBackButton.Clicked += () =>
        {
            SetupSeasonSelectView();
        };

        win.RemoveAll();
        win.Title = "FTC Team Match Generator V2 - ONLINE Event selection - " + selectedSeason + "-" + (selectedSeason + 1) + " season";
        win.Add(eventLabel, eventsListContainer, goBackButton);
    }

    private static async void SetupTeamsListView(int selectedSeason)
    {
        ShowLoadingWindow();

        AllTeams = await Fetch.FetchAllTeams(selectedSeason);

        var eventLabel = new Label("Teams participating in the " + selectedSeason + "-" + (selectedSeason + 1) + " season, registered with FIRST") { X = 2, Y = 1 };
        var eventListField = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Source = new ListWrapper(AllTeams.Select(e => "#" + e.teamNumber + " " + e.nameShort).ToList())
        };

        var eventsListContainer = new Window()
        {
            X = Pos.Left(eventLabel),
            Y = Pos.Bottom(eventLabel) + 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 1
        };
        eventsListContainer.Title = "Teams";
        eventsListContainer.Add(eventListField);

        var goBackButton = new Button()
        {
            X = Pos.Right(win) - 15,
            Y = Pos.Top(eventLabel),
            Text = "Go back",
            HotKey = Key.Enter
        };
        goBackButton.Clicked += () =>
        {
            SetupSeasonSelectView();
        };

        win.RemoveAll();
        win.Title = "FTC Team Match Generator V2 - ONLINE Teams list - Season " + selectedSeason;
        win.Add(eventLabel, eventsListContainer, goBackButton);
    }

    private static void SetupSeasonSelectView()
    {
        var seasonLabel = new Label("Please enter season starting year: ") { X = 2, Y = 1 };
        var seasonTextField = new TextField("")
        {
            X = Pos.Left(seasonLabel),
            Y = Pos.Bottom(seasonLabel),
            Width = 12,
            Text = (DateTime.Now.Month <= 8 ? DateTime.Now.Year - 1 : DateTime.Now.Year).ToString()
        };
        var continueToEventSelectionButton = new Button()
        {
            X = Pos.Left(seasonLabel),
            Y = Pos.Bottom(seasonTextField) + 2,
            Text = "Continue to event selection",
            HotKey = Key.Enter
        };
        var continueToTeamListButton = new Button()
        {
            X = Pos.Left(seasonLabel),
            Y = Pos.Bottom(continueToEventSelectionButton),
            Text = "Continue to all teams list"
        };
        var continueToOfflineImport = new Button()
        {
            X = 2,
            Y = 1,
            Text = "Continue to local XLSX import"
        };
        continueToEventSelectionButton.Clicked += () =>
        {
            SetupEventPickView(Int32.Parse(seasonTextField.Text.ToString().Substring(0, 4)));
        };
        continueToTeamListButton.Clicked += () =>
        {
            SetupTeamsListView(Int32.Parse(seasonTextField.Text.ToString().Substring(0, 4)));
        };
        continueToOfflineImport.Clicked += () =>
        {
            var fileDialog = new OpenDialog("Select offline import file", "Please select the filled-in offlineImport.xlsx file for processing", new List<string>() { "xlsx" }, OpenDialog.OpenMode.File);
            Application.Run(fileDialog);
            if (!fileDialog.Canceled)
            {
                ParticipatingTeams = Processing.ManualXLSXTeamImport(fileDialog.FilePaths.First());
                Matches = Processing.ManualXLSXMatchImport(fileDialog.FilePaths.First());
                AllTeams = ParticipatingTeams;
                SetupEventInfoConfirmation(0, new Event() { code = "IMPORT", name = "Imported event" });
            }
        };

        var onlineOperationsContainer = new Window("ONLINE Operations")
        {
            X = 2,
            Y = 1,
            Width = Dim.Percent(50),
            Height = Dim.Percent(50)
        };
        onlineOperationsContainer.Add(seasonLabel, seasonTextField, continueToEventSelectionButton, continueToTeamListButton);

        var offlineOperationsContainer = new Window("OFFLINE Operations")
        {
            X = Pos.Left(onlineOperationsContainer),
            Y = Pos.Bottom(onlineOperationsContainer) + 2,
            Width = Dim.Percent(50),
            Height = Dim.Fill() - 2
        };
        offlineOperationsContainer.Add(continueToOfflineImport);

        win.RemoveAll();
        win.Title = "FTC Team Match Generator V2 - Main menu";
        win.Add(onlineOperationsContainer, offlineOperationsContainer);
    }
}