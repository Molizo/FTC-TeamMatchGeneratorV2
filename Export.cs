using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using TeamMatchGenV2.Models;

namespace TeamMatchGenV2
{
    public class Export
    {
        public static void PDF(List<PerTeamSchedule> schedule, string providedByText, float width, float height)
        {
            var document = Document.Create(container =>
            {
                foreach (var team in schedule)
                {
                    container.Page(page =>
                    {
                        //page.Size(148.5f, 105, Unit.Millimetre);
                        //page.Size(148.5f, 210, Unit.Millimetre);
                        page.Size(width, height, Unit.Millimetre);
                        page.Margin(15);
                        page.Header()
                            .AlignCenter()
                            .PaddingBottom(2)
                            .Text("#" + team.teamNumber + " " + team.teamName)
                            .FontSize(15)
                            .Weight(FontWeight.ExtraBold);
                        page.Footer()
                            .AlignRight()
                            .Text(providedByText)
                            .FontSize(8);

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().AlignCenter().Text("#").FontSize(12f).Weight(FontWeight.Bold);
                                header.Cell().AlignCenter().Text("Partner").FontSize(12f).Weight(FontWeight.Bold);
                                header.Cell().AlignCenter().Text("Opponent 1").FontSize(12f).Weight(FontWeight.Bold);
                                header.Cell().AlignCenter().Text("Opponent 2").FontSize(12f).Weight(FontWeight.Bold);

                                header.Cell().ColumnSpan(4)
                                    .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            // step 3
                            foreach (var match in team.matches)
                            {
                                table.Cell().Element(TeamNumberCellStyle).AlignCenter().Text(match.matchNumber).FontSize(12f).Weight(FontWeight.Bold).FontColor(match.isBlueTeam ? "#0000FF" : "#FF0000");
                                table.Cell().Element(TeamNumberCellStyle).AlignCenter().Text(match.pNumber).FontSize(12f).FontColor(match.isBlueTeam ? "#0000FF" : "#FF0000");
                                table.Cell().Element(TeamNumberCellStyle).AlignCenter().Text(match.o1Number).FontSize(12f).FontColor(!match.isBlueTeam ? "#0000FF" : "#FF0000");
                                table.Cell().Element(TeamNumberCellStyle).AlignCenter().Text(match.o2Number).FontSize(12f).FontColor(!match.isBlueTeam ? "#0000FF" : "#FF0000");

                                table.Cell().Element(TeamNameCellStyle).AlignCenter().Text(match.isBlueTeam ? "Blue" : "Red").FontSize(9.5f).FontColor(match.isBlueTeam ? "#0000FF" : "#FF0000");
                                table.Cell().Element(TeamNameCellStyle).AlignCenter().Text(match.pName).FontSize(9.5f).FontColor(match.isBlueTeam ? "#0000FF" : "#FF0000");
                                table.Cell().Element(TeamNameCellStyle).AlignCenter().Text(match.o1Name).FontSize(9.5f).FontColor(!match.isBlueTeam ? "#0000FF" : "#FF0000");
                                table.Cell().Element(TeamNameCellStyle).AlignCenter().Text(match.o2Name).FontSize(9.5f).FontColor(!match.isBlueTeam ? "#0000FF" : "#FF0000");
                                static IContainer TeamNumberCellStyle(IContainer container)
                                {
                                    return container.PaddingVertical(0);
                                }
                                static IContainer TeamNameCellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(0);
                                }
                            }
                        });
                    });
                }
            });

            document.GeneratePdf("output.pdf");
        }
    }
}