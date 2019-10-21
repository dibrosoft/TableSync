using System;
using System.Data;

namespace TableSync
{
    public class SystemData
    {
        static SystemData()
        {
            SyncDefinition = new SyncDefinition()
            {
                Ranges = new Ranges()
                {
                    new Range()
                    {
                        Name = Constants.TableSync_Ranges,
                        Schema = Constants.TableSync,
                        TableName = Constants.Ranges,
                        Columns = new RangeColumns()
                        {
                            new RangeColumn() { Name = Constants.RangeName },
                            new RangeColumn() { Name = Constants.Schema },
                            new RangeColumn() { Name = Constants.TableName },
                            new RangeColumn() { Name = Constants.Orientation }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_RangeColumns,
                        Schema = Constants.TableSync,
                        TableName = Constants.RangeColumns,
                        Columns = new RangeColumns()
                        {
                            new RangeColumn() { Name = Constants.RangeName },
                            new RangeColumn() { Name = Constants.ColumnName },
                            new RangeColumn() { Name = Constants.Title },
                            new RangeColumn() { Name = Constants.NumberFormat },
                            new RangeColumn() { Name = Constants.CustomNumberFormat }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_RangeOrder,
                        Schema = Constants.TableSync,
                        TableName = Constants.RangeOrder,
                        Columns = new RangeColumns()
                        {
                            new RangeColumn() { Name = Constants.RangeName },
                            new RangeColumn() { Name = Constants.ColumnName },
                            new RangeColumn() { Name = Constants.Direction }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_RangeCondition,
                        Schema = Constants.TableSync,
                        TableName = Constants.RangeCondition,
                        Columns = new RangeColumns()
                        {
                            new RangeColumn() { Name = Constants.RangeName },
                            new RangeColumn() { Name = Constants.ColumnName },
                            new RangeColumn() { Name = Constants.Operator },
                            new RangeColumn() { Name = Constants.SettingName }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_Settings,
                        Schema = Constants.TableSync,
                        TableName = Constants.Settings,
                        Columns = new RangeColumns()
                        {
                            new RangeColumn() { Name = Constants.Name },
                            new RangeColumn() { Name = Constants.Value }
                        }
                    }
                }
            };
        }

        private static SyncDefinition SyncDefinition;

        public SystemData()
        {
            RangesDT = GetDataTable(Constants.TableSync_Ranges);
            RangeColumnsDT = GetDataTable(Constants.TableSync_RangeColumns);
            RangeOrderDT = GetDataTable(Constants.TableSync_RangeOrder);
            RangeConditionDT = GetDataTable(Constants.TableSync_RangeCondition);
            SettingsDT = GetDataTable(Constants.TableSync_Settings);
        }

        public Range Ranges { get { return SyncDefinition.Ranges[Constants.TableSync_Ranges]; } }
        public Range RangeColumns { get { return SyncDefinition.Ranges[Constants.TableSync_RangeColumns]; } }
        public Range RangeOrder { get { return SyncDefinition.Ranges[Constants.TableSync_RangeOrder]; } }
        public Range RangeCondition { get { return SyncDefinition.Ranges[Constants.TableSync_RangeCondition]; } }
        public Range Settings { get { return SyncDefinition.Ranges[Constants.TableSync_Settings]; } }

        public DataTable RangesDT { get; private set; }
        public DataTable RangeColumnsDT { get; private set; }
        public DataTable RangeOrderDT { get; private set; }
        public DataTable RangeConditionDT { get; private set; }
        public DataTable SettingsDT { get; private set; }


        private static DataTable GetDataTable(string rangeName)
        {
            var range = SyncDefinition.Ranges[rangeName];
            var result = new DataTable(range.Name);
            foreach(var column in range.Columns)
                result.Columns.Add(new DataColumn(column.Name, Type.GetType("System.String")));
            return result;
        }
    }
}
