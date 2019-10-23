﻿using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;

namespace TableSync
{
    public class DatabaseContext : IDisposable
    {
        public DatabaseContext(string connectionString, bool startTransaction)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            if (startTransaction)
                transaction = connection.BeginTransaction();
        }

        private DbConnection connection;
        private DbTransaction transaction;

        private bool transactionClosed = false;

        public void Commit()
        {
            if (transaction == null)
                throw new MissingTransactionException();

            transaction.Commit();

            transactionClosed = true;
        }

        public void Rollback()
        {
            if (transaction == null)
                throw new MissingTransactionException();

            transaction.Rollback();

            transactionClosed = true;
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }

            if (transaction != null)
            {
                if (!transactionClosed)
                    throw new MissingCommitOrRollbackException();

                transaction.Dispose();
                transaction = null;
            }
        }

        public void ExecuteNonQuery(string sql)
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        public TableContext GetTableContext(string sql)
        {
            return new TableContext(this, sql);
        }

        public DbCommand CreateCommand(string sql)
        {
            DbCommand command = new SqlCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            command.CommandText = sql;
            return command;
        }
    }

    public class TableContext : IDisposable
    {
        public TableContext(DatabaseContext databaseContext, string sql)
        {
            dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = databaseContext.CreateCommand(sql);
            
            commandBuilder = new SqlCommandBuilder();
            commandBuilder.DataAdapter = dataAdapter;

            DataTable = new DataTable();
            DataTable.Locale = CultureInfo.InvariantCulture;
            dataAdapter.Fill(DataTable);
        }

        private DbDataAdapter dataAdapter;
        private DbCommandBuilder commandBuilder;

        public DataTable DataTable { get; set; }

        public void Update()
        {
            dataAdapter.Update(DataTable);
        }

        public void Dispose()
        {
            if (dataAdapter != null)
            {
                dataAdapter.Dispose();
                dataAdapter = null;
            }

            if (commandBuilder != null)
            {
                commandBuilder.Dispose();
                commandBuilder = null;
            }
        }
    }
}