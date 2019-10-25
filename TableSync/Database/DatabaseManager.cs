using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace TableSync
{
    public enum DownloadType
    {
        OnlyStructure,
        Full
    }

    public class DatabaseManager : IDisposable
    {
        public DatabaseManager(string connectionString, SyncDefinition syncDefinition, bool startTransaction)
        {
            databaseInfo = new DatabaseInfo(connectionString, syncDefinition.TablesOfInterest);

            ExecutableRanges = new Ranges();
            foreach (var range in syncDefinition.Ranges)
            {
                var executableRange = range.Clone();
                if (!executableRange.HasColumns)
                {
                    var tableInfo = databaseInfo.TableInfos[executableRange.FullTableName];
                    var columns = new RangeColumns();
                    foreach (var columnInfo in tableInfo.ColumnInfos)
                        columns.Add(new RangeColumn() { Name = columnInfo.ColumnName });

                    executableRange.Columns = columns;
                }
                ExecutableRanges.Add(executableRange);
            }

            databaseContext = new DatabaseContext(connectionString, startTransaction);
        }

        private DatabaseInfo databaseInfo;
        private DatabaseContext databaseContext;

        public Ranges ExecutableRanges { get; set; }

        private DataTable DownloadTable(Range range, DownloadType downloadType, SyncDefinition syncDefinition, Settings settings)
        {
            var query = GetQuery(range, downloadType, syncDefinition, settings);

            using (var tableContext = databaseContext.GetTableContext(query))
                return tableContext.DataTable;
        }

        private string GetQuery(Range range, DownloadType downloadType, SyncDefinition syncDefinition, Settings settings)
        {
            TableInfo tableInfo = databaseInfo.TableInfos[range.FullTableName];

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("select ");

            AppendColumns(range, stringBuilder);

            stringBuilder.Append(" from ");
            stringBuilder.Append(tableInfo.FullTableName);

            if (downloadType == DownloadType.OnlyStructure)
                stringBuilder.Append(" where 1=0");
            else if (range.HasCondition)
            {
                stringBuilder.Append(" where ");
                stringBuilder.Append(GetCondition(range, syncDefinition, settings));
            }

            if (range.HasOrder)
                AppendOrder(range, stringBuilder);

            return stringBuilder.ToString();
        }

        private static void AppendColumns(Range range, StringBuilder stringBuilder)
        {
            bool first = true;
            foreach (var columnInfo in range.Columns)
            {
                if (first)
                    first = false;
                else
                    stringBuilder.Append(",");
                stringBuilder.Append("[");
                stringBuilder.Append(columnInfo.Name);
                stringBuilder.Append("]");
            }
        }

        private string GetCondition(Range range, SyncDefinition syncDefinition, Settings settings)
        {
            var stringBuilder = new StringBuilder();

            bool first = true;

            var tableInfo = databaseInfo.TableInfos[range.FullTableName];

            foreach (var item in range.Condition)
            {
                if (first)
                    first = false;
                else
                    stringBuilder.Append(" and ");


                var fieldName = string.Format("[{0}]", item.Name);

                var operatorFormat = string.Empty;
                switch (item.Operator)
                {
                    case RangeConditionOperator.Equal:
                        operatorFormat = "{0}={1}";
                        break;
                    case RangeConditionOperator.Unequal:
                        operatorFormat = "{0}<>{1}";
                        break;
                    case RangeConditionOperator.GreaterThan:
                        operatorFormat = "{0}>{1}";
                        break;
                    case RangeConditionOperator.GreaterThanOrEqual:
                        operatorFormat = "{0}>={1}";
                        break;
                    case RangeConditionOperator.LessThan:
                        operatorFormat = "{0}<{1}";
                        break;
                    case RangeConditionOperator.LessThanOrEqual:
                        operatorFormat = "{0}<={1}";
                        break;
                    case RangeConditionOperator.Like:
                        operatorFormat = "{0} like {1}";
                        break;
                    case RangeConditionOperator.Custom:
                        operatorFormat = item.CustomOperatorFormat;
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }

                var columnInfo = tableInfo.ColumnInfos[item.Name];

                var settingNameInSettings = settings != null && settings.Contains(item.SettingName);
                var settingNameInDefinitionSettings = syncDefinition.Settings != null && syncDefinition.Settings.Contains(item.SettingName);

                if (!settingNameInSettings && !settingNameInDefinitionSettings)
                    throw new MissingSettingException(item.SettingName);

                var value = settingNameInSettings
                    ? settings[item.SettingName].Value
                    : syncDefinition.Settings[item.SettingName].Value;

                if (value == null)
                    throw new MissingSettingException(item.SettingName);

                string formattedValue;

                switch (columnInfo.ColumnType)
                {
                    case "System.String":
                        formattedValue = string.Format("'{0}'", value.ToString().Replace("'", "''"));
                        break;
                    case "System.DateTime":
                        var typedValue = value.GetType() == typeof(string) ? DateTime.Parse(value.ToString()) : (DateTime)value;

                        formattedValue = string.Format("'{0}'", typedValue.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        break;
                    default:
                        formattedValue = value.ToString();
                        break;
                }

                stringBuilder.Append(string.Format(operatorFormat, fieldName, formattedValue));
            }

            return stringBuilder.ToString();
        }

        private void AppendOrder(Range range, StringBuilder stringBuilder)
        {
            stringBuilder.Append(" order by ");
            bool first = true;

            foreach (var item in range.Order)
            {
                if (first)
                    first = false;
                else
                    stringBuilder.Append(",");

                stringBuilder.Append("[");
                stringBuilder.Append(item.Name);
                stringBuilder.Append("]");

                if (item.Direction == RangeOrderDirection.Descending)
                    stringBuilder.Append(" desc");
            }
        }

        public IEnumerable<string> FullTableNamesIndependantToDependant(Ranges ranges)
        {
            var result = new List<string>();
            var fullTableNames = ranges.Select(item => item.FullTableName).ToList();
            var fullTableNamesToInspect = ranges.Select(item => item.FullTableName).ToList();

            do
            {
                var dependants = new List<string>();

                foreach (var tableName in fullTableNamesToInspect)
                {
                    var tableInfo = databaseInfo.TableInfos[tableName];
                    // Are there dependencies of tableName with other inspected tables?
                    var tableDependencies = tableInfo.DependsOn.Intersect(fullTableNames, StringComparer.InvariantCultureIgnoreCase);

                    // Remove tables that are already part of the result from the dependencies
                    tableDependencies = tableDependencies.Where(item => !result.Contains(item, StringComparer.InvariantCultureIgnoreCase)).ToList();

                    if (tableDependencies.Any())
                        dependants.Add(tableName);
                    else
                        result.Add(tableName);
                }

                if (fullTableNamesToInspect.Count() == dependants.Count())
                    throw new CyclicDependenciesException(dependants);

                fullTableNamesToInspect = dependants;
            } while (fullTableNamesToInspect.Any());

            return result;
        }

        public IEnumerable<string> FullTableNamesDependantToIndependant(Ranges ranges)
        {
            return FullTableNamesIndependantToDependant(ranges).Reverse();
        }

        public void UploadChecks(Range range)
        {
            var tableInfo = databaseInfo.TableInfos[range.FullTableName];
            
            foreach(var columnInfo in tableInfo.ColumnInfos)
            { 
                if (columnInfo.IsRequired && !range.Columns.Contains(columnInfo.ColumnName))
                    throw new MissingRequiredColumnException(columnInfo.ColumnName);
            }
        }

        public DataTable DownloadTable(Range range, SyncDefinition syncDefinition, Settings settings)
        {
            return DownloadTable(range, DownloadType.Full, syncDefinition, settings);
        }

        public DataTable DownloadEmptyTableStructure(Range range, SyncDefinition syncDefinition)
        {
            return DownloadTable(range, DownloadType.OnlyStructure, syncDefinition, null);
        }

        private DataRow SearchRow(DataTable dataTable, IEnumerable<ColumnInfo> columnsToSearchFor, DataRow valuesToSearchFor)
        {
            foreach (DataRow row in dataTable.Rows)
                if (CompareColumns(row, columnsToSearchFor, valuesToSearchFor))
                    return row;

            return null;
        }

        private bool CompareColumns(DataRow row, IEnumerable<ColumnInfo> columnsToSearchFor, DataRow valuesToSearchFor)
        {
            foreach(var column in columnsToSearchFor)
            { 
                var rowValue = row[column.ColumnName];
                var valueToSearchFor = valuesToSearchFor[column.ColumnName];

                bool equal;
                switch (column.ColumnType)
                {
                    case "System.String":
                        equal = string.Compare(Convert.ToString(rowValue), Convert.ToString(valueToSearchFor)) == 0;
                        break;
                    default:
                        if (rowValue.GetType().FullName != valueToSearchFor.GetType().FullName)
                            equal = false;
                        else
                            equal = (dynamic)rowValue == (dynamic)valueToSearchFor;
                        break;
                }

                if (!equal)
                    return false;
            }

            return true;
        }

        private void CopyRow(DataRow sourceRow, DataRow targetRow, TableInfo tableInfo)
        {
            foreach (ColumnInfo columnInfo in tableInfo.ColumnInfos)
                targetRow[columnInfo.ColumnName] = sourceRow[columnInfo.ColumnName];
        }

        public void RemoveUnusedRows(Range range, DataTable workbookTable, SyncDefinition syncDefinition, Settings settings)
        {
            var query = GetQuery(range, DownloadType.Full, syncDefinition, settings);
            var tableInfo = databaseInfo.TableInfos[range.FullTableName];
            var primaryColumnInfos = tableInfo.PrimaryColumnInfos();

            using (var tableContext = databaseContext.GetTableContext(query))
            {
                var databaseTable = tableContext.DataTable;

                for (var rowIndex = databaseTable.Rows.Count - 1; rowIndex >= 0; rowIndex--)
                {
                    var databaseRow = databaseTable.Rows[rowIndex];
                    var workbookRow = SearchRow(workbookTable, primaryColumnInfos, databaseRow);
                    if (workbookRow == null)
                        databaseRow.Delete();
                }

                tableContext.Update();
            }
        }

        public void InsertOrUpdateRows(Range range, DataTable workbookTable, SyncDefinition syncDefinition, Settings settings)
        {
            if (workbookTable.Rows.Count < 1)
                return;

            var query = GetQuery(range, DownloadType.Full, syncDefinition, settings);
            var tableInfo = databaseInfo.TableInfos[range.FullTableName];
            var primaryColumnInfos = tableInfo.PrimaryColumnInfos();

            using (var tableContext = databaseContext.GetTableContext(query))
            {
                foreach (DataRow sourceRow in workbookTable.Rows)
                {
                    DataRow TargetRow = SearchRow(tableContext.DataTable, primaryColumnInfos, sourceRow);
                    if (TargetRow == null)
                    {
                        TargetRow = tableContext.DataTable.NewRow();

                        CopyRow(sourceRow, TargetRow, tableInfo);

                        tableContext.DataTable.Rows.Add(TargetRow);
                    }
                    else
                        CopyRow(sourceRow, TargetRow, tableInfo);
                }

                tableContext.Update();
            }
        }

        public void Commit()
        {
            databaseContext.Commit();
        }
        public void Rollback()
        {
            databaseContext.Rollback();
        }

        public void Dispose()
        {
            if (databaseContext != null)
            {
                databaseContext.Dispose();
                databaseContext = null;
            }
        }
    }
}
