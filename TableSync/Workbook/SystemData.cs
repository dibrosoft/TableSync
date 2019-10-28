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
                        Columns = new Columns()
                        {
                            new Column() { Name = Constants.RangeName },
                            new Column() { Name = Constants.Schema },
                            new Column() { Name = Constants.TableName },
                            new Column() { Name = Constants.Orientation }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_Column,
                        Schema = Constants.TableSync,
                        TableName = Constants.Columns,
                        Columns = new Columns()
                        {
                            new Column() { Name = Constants.RangeName },
                            new Column() { Name = Constants.ColumnName },
                            new Column() { Name = Constants.Title },
                            new Column() { Name = Constants.NumberFormat },
                            new Column() { Name = Constants.CustomNumberFormat }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_Order,
                        Schema = Constants.TableSync,
                        TableName = Constants.Order,
                        Columns = new Columns()
                        {
                            new Column() { Name = Constants.RangeName },
                            new Column() { Name = Constants.ColumnName },
                            new Column() { Name = Constants.Direction }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_Condition,
                        Schema = Constants.TableSync,
                        TableName = Constants.Condition,
                        Columns = new Columns()
                        {
                            new Column() { Name = Constants.RangeName },
                            new Column() { Name = Constants.ColumnName },
                            new Column() { Name = Constants.Operator },
                            new Column() { Name = Constants.Value },
                            new Column() { Name = Constants.OperatorTemplate }
                        }
                    },
                    new Range()
                    {
                        Name = Constants.TableSync_Setting,
                        Schema = Constants.TableSync,
                        TableName = Constants.Settings,
                        Columns = new Columns()
                        {
                            new Column() { Name = Constants.Name },
                            new Column() { Name = Constants.Value }
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
