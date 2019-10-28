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
            excelPackage.Workbook.Calculate();
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
                var ranges = databaseManager.ExecutableRanges;

                try
                {
                    var workbookTables = new Dictionary<string, DataTable>();
                    foreach (var range in ranges)
                    {
                        databaseManager.UploadChecks(range);

                        var workbookTable = databaseManager.DownloadEmptyTableStructure(range, syncDefinition);

                        CopyRangeToTable(range, workbookTable);

                        workbookTables.Add(range.FullTableName, workbookTable);
                    }

                    foreach (var fullTableName in databaseManager.FullTableNamesDependantToIndependant(ranges))
                    {
                        var workbookTable = workbookTables[fullTableName];
                        var range = ranges.SearchByFullTableName(fullTableName);

                        databaseManager.RemoveUnusedRows(range, workbookTable, syncDefinition, settings);
                    }

                    foreach (var fullTableName in databaseManager.FullTableNamesIndependantToDependant(ranges))
                    {
                        var workbookTable = workbookTables[fullTableName];
                        var range = ranges.SearchByFullTableName(fullTableName);

                        databaseManager.InsertOrUpdateRows(range, workbookTable, syncDefinition, settings);
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

        public bool Resize(string connectionStringOrName, SyncDefinition syncDefinition = null)
        {
            var hasChanged = false;

            if (syncDefinition == null)
            {
                hasChanged = ResizeDefinition();

                syncDefinition = GetDefinition();
            }

            var connectionString = connections != null ? connections.GetConnectionString(connectionStringOrName) : connectionStringOrName;

            using (var databaseManager = new DatabaseManager(connectionString, syncDefinition, false))
                foreach (var range in databaseManager.ExecutableRanges)
                {
                    var rangeChanges = ResizeRange(range);
                    hasChanged = hasChanged || rangeChanges;
                }

            return hasChanged;
        }

        private bool ResizeDefinition()
        {
            var hasChanged = false;

            var systemData = new SystemData();

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Range))
            {
                var rangeChanges = ResizeRange(systemData.Range);
                hasChanged = hasChanged || rangeChanges;
            }

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Column))
            {
                var rangeChanges = ResizeRange(systemData.Column);
                hasChanged = hasChanged || rangeChanges;
            }

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Order))
            {
                var rangeChanges = ResizeRange(systemData.Order);
                hasChanged = hasChanged || rangeChanges;
            }

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Condition))
            {
                var rangeChanges = ResizeRange(systemData.Condition);
                hasChanged = hasChanged || rangeChanges;
            }

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Setting))
            {
                var rangeChanges = ResizeRange(systemData.Setting);
                hasChanged = hasChanged || rangeChanges;
            }

            return hasChanged;
        }

        public SyncDefinition GetDefinition()
        {
            if (!excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Range))
                throw new MissingSyncDefinitionException();

            var systemData = new SystemData();

            CopyRangeToTable(systemData.Range, systemData.RangeDT);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Column))
                CopyRangeToTable(systemData.Column, systemData.ColumnsDT);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Order))
                CopyRangeToTable(systemData.Order, systemData.OrderDT);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Condition))
                CopyRangeToTable(systemData.Condition, systemData.ConditionDT);

            if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Setting))
                CopyRangeToTable(systemData.Setting, systemData.SettingDT);

            var result = new SyncDefinition() { Ranges = new Ranges() };

            foreach (DataRow row in systemData.RangeDT.Rows)
                result.Ranges.Add(new Range()
                {
                    Name = row[Constants.RangeName].ToString(),
                    Schema = row[Constants.Schema] == DBNull.Value ? null : row[Constants.Schema].ToString(),
                    TableName = row[Constants.TableName] == DBNull.Value ? null : row[Constants.TableName].ToString(),
                    Orientation = (RangeOrientation)Enum.Parse(typeof(RangeOrientation), row[Constants.Orientation].ToString())
                });

            foreach (DataRow row in systemData.ColumnsDT.Rows)
            {
                var rangeName = row[Constants.RangeName].ToString();
                var range = result.Ranges[rangeName];
                if (!range.HasColumns)
                    range.Columns = new RangeColumns();

                range.Columns.Add(new RangeColumn()
                {
                    Name = row[Constants.ColumnName].ToString(),
                    Title = row[Constants.Title] == DBNull.Value ? null : row[Constants.Title].ToString(),
                    NumberFormat = (NumberFormat)Enum.Parse(typeof(NumberFormat), row[Constants.NumberFormat].ToString()),
                    CustomNumberFormat = row[Constants.CustomNumberFormat] == DBNull.Value ? null : row[Constants.CustomNumberFormat].ToString(),
                });
            }

            foreach (DataRow row in systemData.OrderDT.Rows)
            {
                var rangeName = row[Constants.RangeName].ToString();
                var range = result.Ranges[rangeName];
                if (!range.HasOrder)
                    range.Order = new RangeOrder();

                range.Order.Add(new RangeOrderItem()
                {
                    Name = row[Constants.ColumnName].ToString(),
                    Direction = (RangeOrderDirection)Enum.Parse(typeof(RangeOrderDirection), row[Constants.Direction].ToString()),
                });
            }

            foreach (DataRow row in systemData.ConditionDT.Rows)
            {
                var rangeName = row[Constants.RangeName].ToString();
                var range = result.Ranges[rangeName];
                if (!range.HasCondition)
                    range.Condition = new RangeCondition();

                range.Condition.Add(new RangeConditionItem()
                {
                    Name = row[Constants.ColumnName].ToString(),
                    Operator = (RangeConditionOperator)Enum.Parse(typeof(RangeConditionOperator), row[Constants.Operator].ToString()),
                    Value = row[Constants.Value] == DBNull.Value ? null : row[Constants.Value],
                    OperatorTemplate = row[Constants.OperatorTemplate] == DBNull.Value ? null : row[Constants.OperatorTemplate].ToString(),
                });
            }

            foreach (DataRow row in systemData.SettingDT.Rows)
            {
                if (result.Settings == null)
                    result.Settings = new Settings();

                result.Settings.Add(new Setting()
                {
                    Name = row[Constants.Name].ToString(),
                    Value = row[Constants.Value] == DBNull.Value ? null : row[Constants.Value],
                });
            }
            return result;
        }

        public void EmbedDefinition(SyncDefinition syncDefinition, bool insertFullDefinition = false)
        {
            var systemData = new SystemData();

            DataRow newRow;

            foreach (var range in syncDefinition.Ranges)
            {
                newRow = systemData.RangeDT.NewRow();

                newRow[Constants.RangeName] = range.Name;
                newRow[Constants.Schema] = range.Schema;
                newRow[Constants.TableName] = range.TableName;
                newRow[Constants.Orientation] = range.Orientation;

                systemData.RangeDT.Rows.Add(newRow);

                if (range.HasColumns)
                    foreach (var item in range.Columns)
                    {
                        newRow = systemData.ColumnsDT.NewRow();

                        newRow[Constants.RangeName] = range.Name;
                        newRow[Constants.ColumnName] = item.Name;
                        newRow[Constants.Title] = item.Title;
                        newRow[Constants.NumberFormat] = item.NumberFormat.ToString();
                        newRow[Constants.CustomNumberFormat] = item.CustomNumberFormat;

                        systemData.ColumnsDT.Rows.Add(newRow);
                    }

                if (range.HasOrder)
                    foreach (var item in range.Order)
                    {
                        newRow = systemData.OrderDT.NewRow();

                        newRow[Constants.RangeName] = range.Name;
                        newRow[Constants.ColumnName] = item.Name;
                        newRow[Constants.Direction] = item.Direction.ToString();

                        systemData.OrderDT.Rows.Add(newRow);
                    }

                if (range.HasCondition)
                    foreach (var item in range.Condition)
                    {
                        newRow = systemData.ConditionDT.NewRow();

                        newRow[Constants.RangeName] = range.Name;
                        newRow[Constants.ColumnName] = item.Name;
                        newRow[Constants.Operator] = item.Operator.ToString();
                        newRow[Constants.Value] = item.Value;
                        newRow[Constants.OperatorTemplate] = item.OperatorTemplate;

                        systemData.ConditionDT.Rows.Add(newRow);
                    }
            }

            if (syncDefinition.HasSettings)
                foreach (var item in syncDefinition.Settings)
                {
                    newRow = systemData.SettingDT.NewRow();

                    newRow[Constants.Name] = item.Name;
                    newRow[Constants.Value] = item.Value;

                    systemData.SettingDT.Rows.Add(newRow);
                }


            if (systemData.RangeDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.Range, systemData.RangeDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Range))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_Range);

            if (systemData.ColumnsDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.Column, systemData.ColumnsDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Column))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_Column);

            if (systemData.OrderDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.Order, systemData.OrderDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Order))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_Order);

            if (systemData.ConditionDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.Condition, systemData.ConditionDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Condition))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_Condition);

            if (systemData.SettingDT.Rows.Count > 0 || insertFullDefinition)
                CopyTableToRange(systemData.Setting, systemData.SettingDT);
            else if (excelPackage.Workbook.Names.ContainsKey(Constants.TableSync_Setting))
                excelPackage.Workbook.Names.Remove(Constants.TableSync_Setting);
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

        private bool ResizeRange(Range range)
        {
            var rangeWorker = new RangeWorker(excelPackage, range);

            var newLastRowIndex = rangeWorker.LastRowIndex;

            while (rangeWorker.IsEmpty(newLastRowIndex) && newLastRowIndex > rangeWorker.FirstRowIndex)
                newLastRowIndex--;

            while (!rangeWorker.IsEmpty(newLastRowIndex + 1))
                newLastRowIndex++;

            if (newLastRowIndex == rangeWorker.LastRowIndex)
                return rangeWorker.IsNewRange;

            excelPackage.Workbook.Names.Remove(range.Name);

            var newRange = rangeWorker[rangeWorker.FirstRowIndex, rangeWorker.FirstColumnIndex, newLastRowIndex, rangeWorker.LastColumnIndex];

            excelPackage.Workbook.Names.Add(range.Name, newRange);

            return true;
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
                var newRow = systemData.SettingDT.NewRow();

                newRow[Constants.Name] = setting.Name;
                newRow[Constants.Value] = setting.Value;

                systemData.SettingDT.Rows.Add(newRow);
            }

            CopyTableToRange(systemData.Setting, systemData.SettingDT);
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
