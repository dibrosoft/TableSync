namespace TableSync
{
    public static class ColumnHelpText
    {
        public const string RangeName = "Data range name.";
        public const string Schema = "Database scheme. If left empty, it means the scheme is 'dbo'.";
        public const string TableName = "Database table name. If left empty, it means the range name is also the table name.";
        public const string OrientationFormatString = "Possible values: {0}";
        public const string ColumnName = "Database column name.";
        public const string Title = "Column title. If left empty, the column name is used as title.";
        public const string NumberFormatFormatString = "Possible values: {0}, 'None' stands for 'no number format', If 'Custom' is used TableSync takes the value of CustomNumberFormat.";
        public const string CustomNumberFormat = "Number format in workbook syntax.";
        public const string DirectionFormatString = "Possible values: {0}";
        public const string OperatorFormatString = "Possible values: {0}, if 'Template' is used TableSync merges 'OperatorTemplate', 'ColumnName' and 'Value' ";
        public const string ConditionValue = "Constant or setting name prefixed with leading $ character.";
        public const string OperatorTemplate = "Sql expression. {0} is the column name placeholder and {1} is the value placeholder";
        public const string SettingName = "Setting name";
        public const string SettingValue = "Setting value";
    }
}
