// Copyright (c) 2007-2018 Thong Nguyen (tumtumtum@gmail.com)

using System.Collections.Generic;
using Shaolinq.Persistence;

namespace Shaolinq.Postgres
{
	public static class PostgresConfiguration
	{
		public static DataAccessModelConfiguration Create(string connectionString)
		{
			return new DataAccessModelConfiguration
			{
				SqlDatabaseContextInfos = new List<SqlDatabaseContextInfo>
				{
					new PostgresSqlDatabaseContextInfo
					{
						ConnectionString = connectionString
					},
				}
			};
		}

		public static DataAccessModelConfiguration Create(string databaseName, string serverName, string userId, string password, bool poolConnections = PostgresSqlDatabaseContextInfo.DefaultPooling, string categories = null, int port = PostgresSqlDatabaseContextInfo.DefaultPostgresPort, int commandTimeout = SqlDatabaseContextInfo.DefaultCommandTimeout, int connectionTimeout = SqlDatabaseContextInfo.DefaultConnectionTimeout)
		{
			return Create(new PostgresSqlDatabaseContextInfo
			{
				DatabaseName = databaseName,
				Categories = categories,
				ServerName = serverName,
				Port = port,
				Pooling = poolConnections,
				UserId = userId,
				Password = password,
				ConnectionCommandTimeout = commandTimeout,
				ConnectionTimeout = connectionTimeout
			});
		}

		public static DataAccessModelConfiguration Create(PostgresSqlDatabaseContextInfo contextInfo)
		{
			return new DataAccessModelConfiguration
			{
				SqlDatabaseContextInfos = new List<SqlDatabaseContextInfo>
				{
					contextInfo
				}
			};
		}
	}
}
