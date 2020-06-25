namespace TSync
{
    class HelpText
    {
        public const string Download = "Download data from sql database into workbook.";
        public const string Upload = "Upload data from workbook into sql database.";
        public const string Embed = "Embed synchronisation definition into workbook.";
        public const string Resize = "Resize workbook ranges to fit to data.";
        public const string Info = "Get information about synchronisation or database objects.";

        public const string ConnectionStringOrName = "The database connection string or the name of a registered connection string from appsettings.json. You can query the registered connection string s with 'tsync info'.";
        public const string WorkbookFileName = "The file name of a new or existing workbook. Only the xlsx format is supported.";
        public const string TableNames = "Comma separated list of table names. You can use the underscore character to prefix table names with a database scheme.";
        public const string Json = "Output in Json format.";
        public const string SyncDefinitionFileName = "File name of a synchronisation definiton (JSON).";
        public const string SettingsFileName = "File name of additional settings (JSON).";
        public const string WorkbookOutputFileName = "Optional output workbook file name. If used, the result of the operation is saved to this file. Otherwise the original workbook will be overwritten.";
        public const string AutoResize = "Option to configure that 'resize' should always run before upload.";
        public const string RemoveMissingRows = "Option to remove the missing rows in the workbook from the database when uploading.";
        public const string KeepFormula = "Option to configure that download will not overwrite formulas in the download area.";
        public const string FullDefinition = "Option to embed a full instead of a simple synchronisation definition.";
    }
}
