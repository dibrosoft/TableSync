using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace TableSync
{
    public class Workbook : IDisposable
    {
        private readonly Connections connections;
        private ExcelPackage excelPackage;

        internal Workbook(Connections connections, string filename)
        {
            this.connections = connections;
            var fileInfo = new FileInfo(filename);
            excelPackage = new ExcelPackage(fileInfo);
        }

        public void Download(string connectionStringOrName, bool keepFormula = false, SyncDefinition syncDefinition = null, Settings settings = null)
        {
            var connectionString = connections != null ? connections.GetConnectionString(connectionStringOrName) : connectionStringOrName;

            if (syncDefinition == null)
                syncDefinition = GetDefinition();

            using (var databaseManager = new DatabaseManager(connectionString, syncDefinition, false))
            {
                foreach (var range in databaseManager.ExecutableRanges)
                {
                    var dataTable = databaseManager.DownloadTable(range, syncDefinition, settings);

                    CopyTableToRange(range, dataTable, keepFormula);
                }
            }

            if (settings != null)
                RefreshSettings(syncDefinition, settings);
        }

        public void Upload(string connectionStringOrName, SyncDefinition syncDefinition = null, Settings settings = null)
        {
            var connectionString = connections != null ? connections.GetConnectionString(connectionStringOrName) : connectionStringOrName;

            if (syncDefinition == null)
                syncDefinition = GetDefinition();

            using (var databaseManager = new DatabaseManager(connectionString, syncDefinition, true))
            {
                try
                {
                    var dataTables = new Dictionary<string, DataTable>();
                    foreach (var range in databaseManager.ExecutableRanges)
                    {
                        databaseManager.UploadChecks(range);

                        var dataTable = databaseManager.DownloadEmptyTableStructure(range, syncDefinition);

                        CopyRangeToTable(range, dataTable);

                        dataTables.Add(range.FullTableName, dataTable);
                    }

                    foreach (var tableName in databaseManager.SortDependantToIndependant(syncDefinition.Ranges))
                    {
                        var dataTable = dataTables[tableName];

                        var range = syncDefinition.Ranges.Where(item => string.Compare(item.FullTableName, tableName, true) == 0).Single();

                        databaseManager.UploadTableDelete(range, tableName, dataTable, syncDefinition, settings);
                    }

                    foreach (var tableName in databaseManager.SortIndependantToDependant(syncDefinition.Ranges))
                    {
                        var dataTable = dataTables[tableName];

                        databaseManager.UploadTableInsertAndUpdate(tableName, dataTable);
                    }

                    databaseManager.Commit();
                }
                catch
                {
                    databaseManager.Rollback();
                    throw;
                }
            }
        }

        public void Resize(string connectionStringOrName, SyncDefinition syncDefinition = null)
        {
            if (syncDefinition == null)
            {
                ResizeDefinition();

                syncDefinition = GetDefinition();
            }

            var connectionString = connections != null ? connections.GetConnectionString(connectionStringOrName) : connectionStringOrName;

            using (var databaseManager = new DatabaseManager(connectionString, syncDefinition, false))
                foreach (var range in databaseManager.ExecutableRanges)
                    ResizeRange(range);
        }

        private void ResizeDefinition()
        {
            var systemData = new SystemData();

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Ranges))
                ResizeRange(systemData.Ranges);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeColumns))
                ResizeRange(systemData.RangeColumns);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeOrder))
                ResizeRange(systemData.RangeOrder);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeCondition))
                ResizeRange(systemData.RangeCondition);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Settings))
                ResizeRange(systemData.Settings);
        }

        public SyncDefinition GetDefinition()
        {
            if (!excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Ranges))
                throw new MissingSyncDefinitionException();

            var systemData = new SystemData();

            CopyRangeToTable(systemData.Ranges, systemData.RangesDT);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeColumns))
                CopyRangeToTable(systemData.RangeColumns, systemData.RangeColumnsDT);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeOrder))
                CopyRangeToTable(systemData.RangeOrder, systemData.RangeOrderDT);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeCondition))
                CopyRangeToTable(systemData.RangeCondition, systemData.RangeConditionDT);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Settings))
                CopyRangeToTable(systemData.Settings, systemData.SettingsDT);

            var result = new SyncDefinition() { Ranges = new Ranges() };

            foreach (DataRow row in systemData.RangesDT.Rows)
                result.Ranges.Add(new Range()
                {
                    Name = row[Constants.RangeName].ToString(),
                    Schema = row[Constants.Schema] == DBNull.Value ? null : row[Constants.Schema].ToString(),
                    TableName = row[Constants.TableName] == DBNull.Value ? null : row[Constants.TableName].ToString(),
                    Orientation = Enum.Parse<RangeOrientation>(row[Constants.Orientation].ToString())
                });

            foreach (DataRow row in systemData.RangeColumnsDT.Rows)
            {
                var rangeName = row[Constants.RangeName].ToString();
                var range = result.Ranges[rangeName];
                if (!range.HasColumns)
                    range.Columns = new RangeColumns();

                range.Columns.Add(new RangeColumn()
                {
                    Name = row[Constants.ColumnName].ToString(),
                    Title = row[Constants.Title] == DBNull.Value ? null : row[Constants.Title].ToString(),
                    NumberFormat = Enum.Parse<NumberFormat>(row[Constants.NumberFormat].ToString()),
                    CustomNumberFormat = row[Constants.CustomNumberFormat] == DBNull.Value ? null : row[Constants.CustomNumberFormat].ToString(),
                });
            }

            foreach (DataRow row in systemData.RangeOrderDT.Rows)
            {
                var rangeName = row[Constants.RangeName].ToString();
                var range = result.Ranges[rangeName];
                if (!range.HasOrder)
                    range.Order = new RangeOrder();

                range.Order.Add(new RangeOrderItem()
                {
                    Name = row[Constants.ColumnName].ToString(),
                    Direction = Enum.Parse<RangeOrderDirection>(row[Constants.Direction].ToString()),
                });
            }

            foreach (DataRow row in systemData.RangeConditionDT.Rows)
            {
                var rangeName = row[Constants.RangeName].ToString();
                var range = result.Ranges[rangeName];
                if (!range.HasCondition)
                    range.Condition = new RangeCondition();

                range.Condition.Add(new RangeConditionItem()
                {
                    Name = row[Constants.ColumnName].ToString(),
                    Operator = Enum.Parse<RangeConditionOperator>(row[Constants.Operator].ToString()),
                    SettingName = row[Constants.SettingName].ToString()
                });
            }

            foreach (DataRow row in systemData.SettingsDT.Rows)
            {
                if (result.Settings == null)
                    result.Settings = new Settings();

                result.Settings.Add(new Setting()
                {
                    Name = row[Constants.Name].ToString(),
                    Value = row[Constants.Value] == DBNull.Value ? null : row[Constants.Value].ToString(),
                });
            }
            return result;
        }

        public void UpdateDefinition(SyncDefinition syncDefinition, bool insertFullDefinition = false)
        {
            var systemData = new SystemData();

            DataRow newRow;

            foreach (var range in syncDefinition.Ranges)
            {
                newRow = systemData.RangesDT.NewRow();

                newRow[Constants.RangeName] = range.Name;
                newRow[Constants.Schema] = range.Schema;
                newRow[Constants.TableName] = range.TableName;
                newRow[Constants.Orientation] = range.Orientation;

                systemData.RangesDT.Rows.Add(newRow);

                if (range.HasColumns)
                    foreach (var column in range.Columns)
                    {
                        newRow = systemData.RangeColumnsDT.NewRow();

                        newRow[Constants.RangeName] = range.Name;
                        newRow[Constants.ColumnName] = column.Name;
                        newRow[Constants.Title] = column.Title;
                        newRow[Constants.NumberFormat] = column.NumberFormat.ToString();
                        newRow[Constants.CustomNumberFormat] = column.CustomNumberFormat;

                        systemData.RangeColumnsDT.Rows.Add(newRow);
                    }

                if (range.HasOrder)
                    foreach (var item in range.Order)
                    {
                        newRow = systemData.RangeOrderDT.NewRow();

                        newRow[Constants.RangeName] = range.Name;
                        newRow[Constants.ColumnName] = item.Name;
                        newRow[Constants.Direction] = item.Direction.ToString();

                        systemData.RangeOrderDT.Rows.Add(newRow);
                    }

                if (range.HasCondition)
                    foreach (var item in range.Condition)
                    {
                        newRow = systemData.RangeConditionDT.NewRow();

                        newRow[Constants.RangeName] = range.Name;
                        newRow[Constants.ColumnName] = item.Name;
                        newRow[Constants.Operator] = item.Operator.ToString();
                        newRow[Constants.SettingName] = item.SettingName;

                        systemData.RangeConditionDT.Rows.Add(newRow);
                    }
            }

            if (syncDefinition.HasSettings)
                foreach (var item in syncDefinition.Settings)
                {
                    newRow = systemData.SettingsDT.NewRow();

                    newRow[Constants.Name] = item.Name;
                    newRow[Constants.Value] = item.Value;

                    systemData.SettingsDT.Rows.Add(newRow);
                }


            if (systemData.RangesDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.Ranges, systemData.RangesDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Ranges))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_Ranges);

            if (systemData.RangeColumnsDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.RangeColumns, systemData.RangeColumnsDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeColumns))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_RangeColumns);

            if (systemData.RangeOrderDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.RangeOrder, systemData.RangeOrderDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeOrder))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_RangeOrder);

            if (systemData.RangeConditionDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.RangeCondition, systemData.RangeConditionDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_RangeCondition))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_RangeCondition);

            if (systemData.SettingsDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.Settings, systemData.SettingsDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Settings))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_Settings);
        }

        public void Save()
        {
            excelPackage.Save();
        }

        public void SaveAs(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            excelPackage.SaveAs(fileInfo);
        }

        private void CopyRangeToTable(Range range, DataTable dataTable)
        {
            var rangeWorker = new RangeWorker(excelPackage, range);

            for (var rowIndex = rangeWorker.FirstRowIndex + 1; rowIndex <= rangeWorker.LastRowIndex; rowIndex++)
            {
                DataRow newRow = dataTable.NewRow();

                for (int colIndex = rangeWorker.FirstColumnIndex; colIndex <= rangeWorker.LastColumnIndex; colIndex++)
                {
                    var columnName = rangeWorker.GetColumnName(colIndex);
                    var expectedType = dataTable.Columns[columnName].DataType;
                    var value = rangeWorker[rowIndex, colIndex, expectedType];

                    newRow[columnName] = value;
                }

                dataTable.Rows.Add(newRow);
            }
        }

        private void CopyTableToRange(Range range, DataTable dataTable, bool keepFormula = false)
        {
            var rangeWorker = new RangeWorker(excelPackage, range);

            for (int colIndex = rangeWorker.FirstColumnIndex; colIndex <= rangeWorker.LastColumnIndex; colIndex++)
            {
                var columnName = rangeWorker.GetColumnName(colIndex);
                var rangeColumn = range.Columns?[columnName];

                rangeWorker[rangeWorker.FirstRowIndex, colIndex, rangeColumn: rangeColumn] = rangeColumn.DisplayTitle;
            }

            var rowIndex = rangeWorker.FirstRowIndex + 1;
            foreach (DataRow row in dataTable.Rows)
            {
                for (int colIndex = rangeWorker.FirstColumnIndex; colIndex <= rangeWorker.LastColumnIndex; colIndex++)
                {
                    var columnName = rangeWorker.GetColumnName(colIndex);
                    var rangeColumn = range.Columns?[columnName];

                    rangeWorker[rowIndex, colIndex, rangeColumn : rangeColumn, keepFormula : keepFormula] = row[columnName];
                }

                rowIndex++;
            }

            var newLastRowIndex = rowIndex - 1;

            for (var emptyRowIndex = newLastRowIndex + 1; emptyRowIndex <= rangeWorker.LastRowIndex; emptyRowIndex++)
                rangeWorker.Clear(emptyRowIndex);

            excelPackage.Workbook.Names.Remove(range.Name);

            var newRange = rangeWorker[rangeWorker.FirstRowIndex, rangeWorker.FirstColumnIndex, newLastRowIndex, rangeWorker.LastColumnIndex];

            excelPackage.Workbook.Names.Add(range.Name, newRange);
        }

        private void ResizeRange(Range range)
        {
            var rangeWorker = new RangeWorker(excelPackage, range);

            var newLastRowIndex = rangeWorker.LastRowIndex;

            while (rangeWorker.IsEmpty(newLastRowIndex) && newLastRowIndex > rangeWorker.FirstRowIndex)
                newLastRowIndex--;

            while (!rangeWorker.IsEmpty(newLastRowIndex + 1))
                newLastRowIndex++;

            excelPackage.Workbook.Names.Remove(range.Name);

            var newRange = rangeWorker[rangeWorker.FirstRowIndex, rangeWorker.FirstColumnIndex, newLastRowIndex, rangeWorker.LastColumnIndex];

            excelPackage.Workbook.Names.Add(range.Name, newRange);
        }

        private void RefreshSettings(SyncDefinition syncDefinition, Settings settings)
        {
            var systemData = new SystemData();

            var consolidatedSettings = syncDefinition.Settings == null
                ? new Settings() : syncDefinition.Settings.Clone();

            foreach (var setting in settings)
                if (consolidatedSettings.Contains(setting.Name))
                    consolidatedSettings[setting.Name].Value = setting.Value;
                else
                    consolidatedSettings.Add(setting.Clone());

            foreach (var setting in consolidatedSettings)
            {
                var newRow = systemData.SettingsDT.NewRow();

                newRow[Constants.Name] = setting.Name;
                newRow[Constants.Value] = setting.Value;

                systemData.SettingsDT.Rows.Add(newRow);
            }

            CopyTableToRange(systemData.Settings, systemData.SettingsDT);
        }

        public void Dispose()
        {
            if (excelPackage != null)
            {
                excelPackage.Dispose();
                excelPackage = null;
            }
        }
    }
}
