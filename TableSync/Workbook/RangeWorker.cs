using OfficeOpenXml;
using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace TableSync
{
    public class RangeWorker
    {
        private ExcelPackage excelPackage;
        private Range range;
        private ExcelNamedRange namedRange;
        private ExcelWorksheet worksheet;
        public bool IsNewRange { get; set; } = false;

        public RangeWorker(ExcelPackage excelPackage, Range range)
        {
            this.excelPackage = excelPackage;
            this.range = range;

            if (excelPackage.Workbook.Names.ContainsKey(range.Name))
            {
                namedRange = excelPackage.Workbook.Names[range.Name];
                worksheet = namedRange.Worksheet;
                return;
            }

            var rangeName = this.range.Name;

            var worksheetExists = excelPackage.Workbook.Worksheets.Any(item => string.Compare(item.Name, rangeName, true) == 0);
            if (worksheetExists)
                throw new WorkSheetExistsException(rangeName);

            worksheet = excelPackage.Workbook.Worksheets.Add(rangeName);

            var newRange = this[1, 1, 1, 1];
            namedRange = excelPackage.Workbook.Names.Add(range.Name, newRange);
            IsNewRange = true;
        }

        public int FirstRowIndex
        {
            get
            { 
                return range.Orientation == RangeOrientation.Row ?
                    namedRange.Start.Row :
                    namedRange.Start.Column;
            }
        }

        public int LastRowIndex
        {
            get
            { 
                return range.Orientation == RangeOrientation.Row ?
                    namedRange.End.Row :
                    namedRange.End.Column;
            }
        }

        public int FirstColumnIndex
        {
            get { 
                return range.Orientation == RangeOrientation.Row ?
                    namedRange.Start.Column :
                    namedRange.Start.Row;
            }
        }

        public int LastColumnIndex
        {
            get
            {
                return FirstColumnIndex + range.Columns.Count - 1;
            }
        }

        public string GetColumnName(int colIndex)
        {
            if (range.Columns == null)
                return System.Convert.ToString(this[this.FirstRowIndex, colIndex]);

            var index = colIndex - FirstColumnIndex;
            var column = range.Columns[index];
            return column.Name;
        }

        public object this[int rowIndex, int colIndex, Type expectedType = null, RangeColumn rangeColumn = null, bool keepFormula = false]
        {
            get
            {
                var wsRowIndex = range.Orientation == RangeOrientation.Row ? rowIndex : colIndex;
                var wsColIndex = range.Orientation == RangeOrientation.Row ? colIndex : rowIndex;
                var workbookValue = worksheet.Cells[wsRowIndex, wsColIndex].Value;

                return ValueConverter.WorkbookToDatabase(workbookValue, expectedType);
            }
            set
            {
                var wsRowIndex = range.Orientation == RangeOrientation.Row ? rowIndex : colIndex;
                var wsColIndex = range.Orientation == RangeOrientation.Row ? colIndex : rowIndex;

                if (keepFormula && !string.IsNullOrEmpty(worksheet.Cells[wsRowIndex, wsColIndex].Formula))
                    return;

                worksheet.Cells[wsRowIndex, wsColIndex].Value = ValueConverter.DatabaseToWorkbook(value);

                if (rangeColumn != null && !string.IsNullOrEmpty(rangeColumn.DisplayNumberFormat))
                    worksheet.Cells[wsRowIndex, wsColIndex].Style.Numberformat.Format = rangeColumn.DisplayNumberFormat;
            }
        }

        public ExcelRange this[int startRow, int startCol, int endRow, int endCol]
        {
            get
            { 
                if (range.Orientation == RangeOrientation.Row)
                    return worksheet.Cells[startRow, startCol, endRow, endCol];
                else
                    return worksheet.Cells[startCol, startRow, endCol, endRow];
            }
        }

        public bool IsEmpty(int rowIndex)
        {
            for (int colIndex = FirstColumnIndex; colIndex <= LastColumnIndex; colIndex++)
                if (this[rowIndex, colIndex] != DBNull.Value)
                    return false;

            return true;
        }

        public void Clear(int rowIndex)
        {
            for (int colIndex = FirstColumnIndex; colIndex <= LastColumnIndex; colIndex++)
                this[rowIndex, colIndex] = null;
        }
    }
}
