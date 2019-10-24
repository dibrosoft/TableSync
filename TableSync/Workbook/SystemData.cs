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
                        Name = Constants.TableSync_Range,
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
                        Name = Constants.TableSync_Column,
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
                        Name = Constants.TableSync_Order,
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
                        Name = Constants.TableSync_Condition,
                        Schema = Constants.TableSync,
                        TableName = Constants.RangeCondition,
                        Columns = new RangeColumns()
                        {
                            new RangeColumn() { Name = Constants.RangeName },
                            new RangeColumn() { Name = Constants.ColumnName },
                            new RangeColumn() { Name = Constants.Operator },
                            new RangeColumn() { Name = Constants.SettingName },
                            new RangeColumn() { Name = Constants.CustomOperatorFormat }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_Setting,
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
            RangeDT = GetDataTable(Constants.TableSync_Range);
            ColumnsDT = GetDataTable(Constants.TableSync_Column);
            OrderDT = GetDataTable(Constants.TableSync_Order);
            ConditionDT = GetDataTable(Constants.TableSync_Condition);
            SettingDT = GetDataTable(Constants.TableSync_Setting);
        }

        public Range Range { get { return SyncDefinition.Ranges[Constants.TableSync_Range]; } }
        public Range Column { get { return SyncDefinition.Ranges[Constants.TableSync_Column]; } }
        public Range Order { get { return SyncDefinition.Ranges[Constants.TableSync_Order]; } }
        public Range Condition { get { return SyncDefinition.Ranges[Constants.TableSync_Condition]; } }
        public Range Setting { get { return SyncDefinition.Ranges[Constants.TableSync_Setting]; } }

        public DataTable RangeDT { get; private set; }
        public DataTable ColumnsDT { get; private set; }
        public DataTable OrderDT { get; private set; }
        public DataTable ConditionDT { get; private set; }
        public DataTable SettingDT { get; private set; }


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
