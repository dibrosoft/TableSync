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
        public DatabaseManager(ConnectionInfo connectionInfo, SyncDefinition syncDefinition, bool startTransaction)
        {
            databaseInfo = new DatabaseInfo(connectionInfo, syncDefinition.TablesOfInterest);

            ExecutableRanges = new Ranges();
            foreach (var range in syncDefinition.Ranges)
            {
                var executableRange = range.Clone();
                if (!executableRange.HasColumns)
                {
                    var tableInfo = databaseInfo.TableInfos[executableRange.FullTableName];
                    var columns = new Columns();
                    foreach (var columnInfo in tableInfo.ColumnInfos)
                        columns.Add(new Column() { Name = columnInfo.ColumnName });

                    executableRange.Columns = columns;
                }
                ExecutableRanges.Add(executableRange);
            }

            databaseContext = new DatabaseContext(connectionInfo.ConnectionString, startTransaction);
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

        private QueryBuilder GetQuery(Range range, DownloadType downloadType, SyncDefinition syncDefinition, Settings settings)
        {
            TableInfo tableInfo = databaseInfo.TableInfos[range.FullTableName];

            var queryBuilder = new QueryBuilder();

            queryBuilder.Append("select ");

            AppendColumns(range, queryBuilder);

            queryBuilder.Append(" from ");
            queryBuilder.Append(tableInfo.FullTableName);

            if (downloadType == DownloadType.OnlyStructure)
                queryBuilder.Append(" where 1=0");
            else if (range.HasCondition)
                AppendCondition(range, syncDefinition, settings, queryBuilder);

            if (range.HasOrder)
                AppendOrder(range, queryBuilder);

            return queryBuilder;
        }

        private static void AppendColumns(Range range, QueryBuilder queryBuilder)
        {
            bool first = true;
            foreach (var item in range.Columns)
            {
                if (first)
                    first = false;
                else
                    queryBuilder.Append(",");
                queryBuilder.Append("[");
                queryBuilder.Append(item.Name);
                queryBuilder.Append("]");
            }
        }

        private void AppendCondition(Range range, SyncDefinition syncDefinition, Settings settings, QueryBuilder queryBuilder)
        {

            int parameterIndex = 0;

            var tableInfo = databaseInfo.TableInfos[range.FullTableName];

            foreach (var item in range.Condition)
            {
                parameterIndex++;
                if (parameterIndex == 1)
                    queryBuilder.Append(" where ");
                else
                    queryBuilder.Append(" and ");

                var fieldName = string.Format("[{0}]", item.Name);

                var operatorTemplate = string.Empty;
                switch (item.Operator)
                {
                    case ConditionOperator.Equal:
                        operatorTemplate = "{0}={1}";
                        break;
                    case ConditionOperator.Unequal:
                        operatorTemplate = "{0}<>{1}";
                        break;
                    case ConditionOperator.GreaterThan:
                        operatorTemplate = "{0}>{1}";
                        break;
                    case ConditionOperator.GreaterThanOrEqual:
                        operatorTemplate = "{0}>={1}";
                        break;
                    case ConditionOperator.LessThan:
                        operatorTemplate = "{0}<{1}";
                        break;
                    case ConditionOperator.LessThanOrEqual:
                        operatorTemplate = "{0}<={1}";
                        break;
                    case ConditionOperator.Like:
                        operatorTemplate = "{0} like {1}";
                        break;
                    case ConditionOperator.Template:
                        operatorTemplate = item.OperatorTemplate;
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }

                var columnInfo = tableInfo.ColumnInfos[item.Name];

                var value = item.Value;
                if (value != null && value.GetType() == typeof(string))
                {
                    var parts = ((string)item.Value).Split('$');
                    if (parts.Length == 2 && string.IsNullOrEmpty(parts[0]))
                    {
                        var settingName = parts[1];
                        var settingNameInSettings = settings != null && settings.Contains(settingName);
                        var settingNameInDefinitionSettings = syncDefinition.Settings != null && syncDefinition.Settings.Contains(settingName);

                        if (!settingNameInSettings && !settingNameInDefinitionSettings)
                            throw new MissingSettingException(settingName);

                        value = settingNameInSettings
                            ? settings[settingName].Value
                            : syncDefinition.Settings[settingName].Value;
                    }
                }

                if (string.Compare(columnInfo.ColumnType, "System.DateTime", true) == 0 && value is string)
                    value = DateTime.Parse(value.ToString());

                var parameterName = $"@value{parameterIndex}";

                queryBuilder.Append(string.Format(operatorTemplate, fieldName, parameterName));
                queryBuilder.Parameters.Add(new QueryParameter() { Name = parameterName, Value = value });
            }
        }

        private void AppendOrder(Range range, QueryBuilder queryBuilder)
        {
            queryBuilder.Append(" order by ");
            bool first = true;

            foreach (var item in range.Order)
            {
                if (first)
                    first = false;
                else
                    queryBuilder.Append(",");

                queryBuilder.Append("[");
                queryBuilder.Append(item.Name);
                queryBuilder.Append("]");

                if (item.Direction == OrderDirection.Descending)
                    queryBuilder.Append(" desc");
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

                if (!ValueComparer.AreEqual(rowValue, valueToSearchFor, column.ColumnType))
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
