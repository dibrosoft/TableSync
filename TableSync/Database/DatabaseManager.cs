using System;
using System.Collections.Generic;
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

            using (var tableContext = databaseContext.GetTableContext(stringBuilder.ToString()))
                return tableContext.DataTable;
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

                stringBuilder.Append("[");
                stringBuilder.Append(item.Name);
                stringBuilder.Append("]");

                switch (item.Operator)
                {
                    case RangeConditionOperator.Equal:
                        stringBuilder.Append(" = ");
                        break;
                    case RangeConditionOperator.Unequal:
                        stringBuilder.Append(" <> ");
                        break;
                    case RangeConditionOperator.GreaterThan:
                        stringBuilder.Append(" > ");
                        break;
                    case RangeConditionOperator.GreaterThanOrEqual:
                        stringBuilder.Append(" >= ");
                        break;
                    case RangeConditionOperator.LessThan:
                        stringBuilder.Append(" < ");
                        break;
                    case RangeConditionOperator.LessThanOrEqual:
                        stringBuilder.Append(" <= ");
                        break;
                    case RangeConditionOperator.Like:
                        stringBuilder.Append(" like ");
                        break;
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

                switch (columnInfo.ColumnType)
                {
                    case "System.String":
                        stringBuilder.Append("'");
                        stringBuilder.Append(value.ToString().Replace("'", "''"));
                        stringBuilder.Append("'");
                        break;
                    case "System.DateTime":
                        var typedValue = value.GetType() == typeof(string) ? DateTime.Parse(value.ToString()) : (DateTime)value;

                        stringBuilder.Append("'");
                        stringBuilder.Append(typedValue.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        stringBuilder.Append("'");
                        break;
                    default:
                        stringBuilder.Append(value);
                        break;
                }

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

        public IEnumerable<string> SortIndependantToDependant(Ranges ranges)
        {
            var result = new List<string>();
            var tableNames = ranges.Select(item => item.FullTableName).ToList();
            var tableNamesToInspect = ranges.Select(item => item.FullTableName).ToList();

            do
            {
                var dependants = new List<string>();

                foreach (var tableName in tableNamesToInspect)
                {
                    var tableInfo = databaseInfo.TableInfos[tableName];
                    // Are there dependencies of tableName with other inspected tables?
                    var tableDependencies = tableInfo.DependsOn.Intersect(tableNames, StringComparer.InvariantCultureIgnoreCase);

                    // Remove tables that are already part of the result from the dependencies
                    tableDependencies = tableDependencies.Where(item => !result.Contains(item, StringComparer.InvariantCultureIgnoreCase)).ToList();

                    if (tableDependencies.Any())
                        dependants.Add(tableName);
                    else
                        result.Add(tableName);
                }

                if (tableNamesToInspect.Count() == dependants.Count())
                    throw new CyclicDependenciesException(dependants);

                tableNamesToInspect = dependants;
            } while (tableNamesToInspect.Any());

            return result;
        }

        public IEnumerable<string> SortDependantToIndependant(Ranges ranges)
        {
            return SortIndependantToDependant(ranges).Reverse();
        }

        public void UploadChecks(Range range)
        {
            var tableInfo = databaseInfo.TableInfos[range.FullTableName];
            var primaryColumnCount = tableInfo.ColumnInfos.Where(item => item.IsPrimary).Count();
            if (primaryColumnCount != 1)
                throw new UploadMultipleKeyException();
            
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

        private DataRow SearchRow(DataTable dataTable, ColumnInfo columnInfo, object value)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                var rowValue = row[columnInfo.ColumnName];

                bool equal;
                switch (columnInfo.ColumnType)
                {
                    case "System.String":
                        equal = string.Compare(Convert.ToString(rowValue), Convert.ToString(value)) == 0;
                        break;
                    default:
                        if (rowValue.GetType().FullName != value.GetType().FullName)
                            equal = false;
                        else
                            equal = (dynamic)rowValue == (dynamic)value;
                        break;
                }

                if (equal)
                    return row;
            }

            return null;
        }

        private void CopyRow(DataRow sourceRow, DataRow targetRow, TableInfo tableInfo)
        {
            foreach (ColumnInfo columnInfo in tableInfo.ColumnInfos)
                targetRow[columnInfo.ColumnName] = sourceRow[columnInfo.ColumnName];
        }

        private string GetPrimaryCondition(ColumnInfo primaryColumnInfo, DataTable dataTable, bool negateCondition)
        {
            var stringBuilder = new StringBuilder();

            if (dataTable.Rows.Count == 0)
            {
                if (negateCondition)
                    stringBuilder.Append("1=1");
                else
                    stringBuilder.Append("1=0");
            }
            else
            {
                stringBuilder.Append("[");
                stringBuilder.Append(primaryColumnInfo.ColumnName);
                stringBuilder.Append("] ");

                if (negateCondition)
                    stringBuilder.Append("not ");
                stringBuilder.Append("in (");

                var first = true;

                foreach (DataRow Row in dataTable.Rows)
                {
                    var value = Row[primaryColumnInfo.ColumnName];

                    if (value != DBNull.Value)
                    {
                        if (first)
                            first = false;
                        else
                            stringBuilder.Append(",");

                        switch (primaryColumnInfo.ColumnType)
                        {
                            case "System.String":
                            case "System.Guid":
                                {
                                    stringBuilder.Append("'");
                                    stringBuilder.Append(value);
                                    stringBuilder.Append("'");
                                    break;
                                }

                            default:
                                {
                                    stringBuilder.Append(value);
                                    break;
                                }
                        }
                    }
                }
                stringBuilder.Append(")");
            }
            
            return stringBuilder.ToString();
        }

        public void UploadTableDelete(Range range, string tableName, DataTable dataTable, SyncDefinition syncDefinition, Settings settings)
        {
            TableInfo tableInfo = databaseInfo.TableInfos[tableName];
            ColumnInfo primaryColumnInfo = tableInfo.PrimaryColumnInfo();

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("delete from ");
            stringBuilder.Append(tableInfo.FullTableName);

            stringBuilder.Append(" where (");
            stringBuilder.Append(GetPrimaryCondition(primaryColumnInfo, dataTable, true));
            stringBuilder.Append(")");

            if (range.HasCondition)
            {
                stringBuilder.Append(" and (");
                stringBuilder.Append(GetCondition(range, syncDefinition, settings));
                stringBuilder.Append(")");
            }

            var query = stringBuilder.ToString();

            databaseContext.ExecuteNonQuery(query);
        }

        public void UploadTableInsertAndUpdate(string tableName, DataTable dataTable)
        {
            if (dataTable.Rows.Count < 1)
                return;

            TableInfo tableInfo = databaseInfo.TableInfos[tableName];
            ColumnInfo primaryColumnInfo = tableInfo.PrimaryColumnInfo();

            string query;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("select * from ");
            stringBuilder.Append(tableName);
            stringBuilder.Append("where ");
            stringBuilder.Append(GetPrimaryCondition(primaryColumnInfo, dataTable, false));

            query = stringBuilder.ToString();

            using (var tableContext = databaseContext.GetTableContext(query))
            {
                foreach (DataRow sourceRow in dataTable.Rows)
                {
                    DataRow TargetRow = SearchRow(tableContext.DataTable, primaryColumnInfo, sourceRow[primaryColumnInfo.ColumnName]);
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
