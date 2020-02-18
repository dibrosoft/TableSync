# TableSync

TableSync is a component to synchronize data between Microsoft SQL Server and Excel files in OpenXML format (xlsx).

## Overview

Synchronizing data between SQL Server and Excel with TableSync means it works in two directions:

* You can *download* SQL data into workbooks. In this direction it is possible to use TableSync for Excel based reporting.
* For simple use cases you can *upload* workbook data into SQL databases. In this direction TableSync works as a kind of Excel based user interface.

TableSync can be used in two different ways:

* [TableSync as .NET Standard library](https://www.nuget.org/packages/TableSync) for the usage in other programs
* [TableSync as .NET Core global tool tsync](https://www.nuget.org/packages/TableSync.Cli) for the usage at the commandline

The documentation can be found on [Github](https://github.com/dibrosoft/TableSync/wiki).

## License

The project is licensed under the MIT License.