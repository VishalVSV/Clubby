using Clubby.Club;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Clubby.GoogleServices
{
    /// <summary>
    /// Schedule sheet handler.
    /// </summary>
    public class ScheduleSheetsHandler
    {
        /// <summary>
        /// The service used to make requests.
        /// </summary>
        private readonly SheetsService service;
        /// <summary>
        /// The register id.
        /// </summary>
        private readonly string spreadSheetId = null;
        /// <summary>
        /// The column to search for the Cursor.
        /// </summary>
        /// TODO: Make this configurable
        private readonly string CursorColumn = "J";
        /// <summary>
        /// Private copy of the SheetName used
        /// </summary>
        private readonly string SheetName;

        public ScheduleSheetsHandler(string spreadSheetId, string SheetName)
        {
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Program.config.GoogleUserCreds,
                ApplicationName = "Clubby v2"
            });

            this.spreadSheetId = spreadSheetId;
            this.SheetName = SheetName;
        }

        // Add an event from raw data. Pretty much only used as a poor man's destructuring
        private async Task AddEvent(DateTime date, string prop, string opp, string context, string motion, string judges, string remarks, char start_column = 'B')
        {
            // Get the row that the cursor is in
            int row = GetNextRange();

            if (row == -1)
                throw new Exception("Cursor not found in sheets file!");

            // Range of actual values to add
            ValueRange range = new ValueRange();
            var values = new List<IList<object>>
            {
                new List<object> { date.ToString("dd/MM/yyyy"), date.DayOfWeek.ToString(), prop, opp, context, motion, judges, remarks }
            };

            range.MajorDimension = "ROWS";
            range.Range = $"'{SheetName}'!{start_column}{row}:{(char)(start_column + 9)}{row}";
            range.Values = values;

            // Range to move the cursor down
            ValueRange cursorMover = new ValueRange();
            var cVals = new List<IList<object>>
            {
                new List<object> { Program.config.RegisterClubbyCursor }
            };

            cursorMover.MajorDimension = "ROWS";
            cursorMover.Range = $"'{SheetName}'!{CursorColumn}{row + 1}:{CursorColumn}{row + 1}";
            cursorMover.Values = cVals;

            // Range to delete the cursor from previous location
            ValueRange cursor_deletor = new ValueRange();
            var cDVals = new List<IList<object>>
            {
                new List<object> { "" }
            };

            cursor_deletor.MajorDimension = "ROWS";
            cursor_deletor.Range = $"'{SheetName}'!{CursorColumn}{row}:{CursorColumn}{row}";
            cursor_deletor.Values = cDVals;

            BatchUpdateValuesRequest req = new BatchUpdateValuesRequest
            {
                Data = new List<ValueRange>() { range, cursorMover, cursor_deletor },
                ValueInputOption = "USER_ENTERED"
            };

            SpreadsheetsResource.ValuesResource.BatchUpdateRequest request = service.Spreadsheets.Values.BatchUpdate(req, spreadSheetId);

            await request.ExecuteAsync();
        }

        /// <summary>
        /// Add an event to the register.
        /// </summary>
        /// <param name="debate">The debate to add to the register</param>
        /// <param name="start_column">Column to start storing from. (Currently unused)</param>
        public async Task AddEvent(Debate debate, char start_column = 'B')
        {
            await AddEvent(debate.date, debate.Proposition, debate.Opposition, debate.Context, debate.Motion, debate.Judges, debate.Remarks, start_column);
        }

        /// <summary>
        /// Get where to input the next row.
        /// </summary>
        /// <returns>The row to insert the next record</returns>
        private int GetNextRange()
        {
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadSheetId, $"'{SheetName}'!{CursorColumn}1:{CursorColumn}150");

            request.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS;

            ValueRange res = request.Execute();

            int i = 1;
            foreach (List<object> _cell in res.Values)
            {
                if (_cell.Count > 0)
                {
                    string cell = _cell[0].ToString();
                    if (cell == Program.config.RegisterClubbyCursor)
                    {
                        return i;
                    }
                }
                i++;
            }

            return -1;
        }
    }
}
