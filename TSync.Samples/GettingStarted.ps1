# installation of tsync
dotnet tool install --global TableSync.Cli --version 1.0.0-rc

# exploring the verbs of tsync
tsync --help

# exploring the verb download
tsync download --help

# set variables $connectionString, $workbookFileName and $tableNames
. .\SetVariables.ps1   

# a simple download
tsync download -c $connectionString -w $workbookFileName -n $tableNames

# explore the resulting workbook
Start-Process $workbookFileName

# explore the verb embed
tsync embed --help

# embed synchronisation data into the workbook
tsync embed -c $connectionString -w $workbookFileName -n $tableNames -f

# explore the workbook again
Start-Process $workbookFileName

# download using embedded synchronisation data
tsync download -c $connectionString -w $workbookFileName
