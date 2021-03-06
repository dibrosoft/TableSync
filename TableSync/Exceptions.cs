﻿using System;
using System.Collections.Generic;
using System.IO;

namespace TableSync
{
    public abstract class ExceptionBase : Exception
    {
        public ExceptionBase(string message) : base(message) { }
    }

    public class CyclicDependenciesException : ExceptionBase
    {
        public CyclicDependenciesException(IEnumerable<string> dependants) : base($"There are cyclic dependencies between the following tables: {string.Join(",", dependants)}") { }
    }

    public class IllegalFileExtensionException : ExceptionBase
    {
        public IllegalFileExtensionException(string fileName, string extension) : base($"The file {fileName} has the wrong extension. Extension {extension} is expected.") { }
    }

    public class IllegalRangeNameException : ExceptionBase
    {
        public IllegalRangeNameException() : base("The range name is illegal.") { }
    }

    public class MissingCommitOrRollbackException : ExceptionBase
    {
        public MissingCommitOrRollbackException() : base("Dispose of transactional database context without a preceding commit or rollback.") { }
    }

    public class MissingConnectionStringException : ExceptionBase
    {
        public MissingConnectionStringException() : base("The connection string isn't configured. You can pass connection strings or names with the -c|--ConnectionStringOrName option. Use 'tsync info' to list the available connection string names.") { }
    }

    public class MissingRequiredColumnException : ExceptionBase
    {
        public MissingRequiredColumnException(string columnName) : base($"The required column {columnName} is missing in the range definition.") { }
    }

    public class MissingSyncDefinitionException : ExceptionBase
    {
        public MissingSyncDefinitionException() : base("The synchronisation definition is missing. You can use workbooks with embedded synchronisation definitions or you can pass definitions with -n|--TableNames or -d|--DefinitionFileName. Use 'tsync info -c' to query connections for available tables. Use 'tsync embed' to create workbooks with embedded synchronisation definitions.") { }
    }

    public class MissingSettingException : ExceptionBase
    {
        public MissingSettingException(string settingName) : base($"The value for setting {settingName} is missing") { }
    }
    public class MissingTableException : ExceptionBase
    {
        public MissingTableException(string tableName) : base($"Table {tableName} is missing in database") { }
    }
    public class MissingTransactionException : ExceptionBase
    {
        public MissingTransactionException() : base("The database context wasn't started as a transaction.") { }
    }

    public class WorkSheetExistsException : ExceptionBase
    {
        public WorkSheetExistsException(string worksheetName) : base($"Worksheet {worksheetName} already exists") { }
    }
}

