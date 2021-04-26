namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	public abstract partial class DataAccessModelHookBase
	{
		public virtual Task CreateAsync(DataAccessObject dataAccessObject)
		{
			return this.CreateAsync(dataAccessObject, CancellationToken.None);
		}

		public virtual async Task CreateAsync(DataAccessObject dataAccessObject, CancellationToken cancellationToken)
		{
		}

		public virtual Task ReadAsync(DataAccessObject dataAccessObject)
		{
			return this.ReadAsync(dataAccessObject, CancellationToken.None);
		}

		public virtual async Task ReadAsync(DataAccessObject dataAccessObject, CancellationToken cancellationToken)
		{
		}

		public virtual Task BeforeSubmitAsync(DataAccessModelHookSubmitContext context)
		{
			return this.BeforeSubmitAsync(context, CancellationToken.None);
		}

		public virtual async Task BeforeSubmitAsync(DataAccessModelHookSubmitContext context, CancellationToken cancellationToken)
		{
		}

		public virtual Task AfterSubmitAsync(DataAccessModelHookSubmitContext context)
		{
			return this.AfterSubmitAsync(context, CancellationToken.None);
		}

		public virtual async Task AfterSubmitAsync(DataAccessModelHookSubmitContext context, CancellationToken cancellationToken)
		{
		}

		public virtual Task BeforeRollbackAsync(DataAccessModelHookRollbackContext context)
		{
			return this.BeforeRollbackAsync(context, CancellationToken.None);
		}

		public virtual async Task BeforeRollbackAsync(DataAccessModelHookRollbackContext context, CancellationToken cancellationToken)
		{
		}

		public virtual Task AfterRollbackAsync(DataAccessModelHookRollbackContext context)
		{
			return this.AfterRollbackAsync(context, CancellationToken.None);
		}

		public virtual async Task AfterRollbackAsync(DataAccessModelHookRollbackContext context, CancellationToken cancellationToken)
		{
		}
	}
}

namespace Shaolinq.DirectAccess.Sql
{
#pragma warning disable
	using System;
	using System.Data;
	using System.Linq;
	using System.Threading;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Linq.Expressions;
	using System.Collections.Generic;
	using Platform;
	using Shaolinq;
	using Platform.Reflection;
	using Shaolinq.Persistence;
	using Shaolinq.DirectAccess;
	using Shaolinq.DirectAccess.Sql;
	using Shaolinq.Persistence.Linq;

	public static partial class DataAccessModelExtensions
	{
		/// <summary>
		/// Opens and returns a new connection to the database for the given <see cref = "model"/>.
		/// </summary>
		/// <typeparam name = "TDataAccessModel">The type of <see cref = "DataAccessModel"/></typeparam>
		/// <param name = "model">The <see cref = "DataAccessModel"/></param>
		/// <remarks>This method does not require an existing <see cref = "DataAccessScope"/>. The new connection will be
		/// unrelated to any existing scope and it is up to the caller to dispose of the connection.</remarks>
		/// <returns>The <see cref = "IDbConnection"/></returns>
		public static Task<IDbConnection> OpenConnectionAsync<TDataAccessModel>(this TDataAccessModel model)where TDataAccessModel : DataAccessModel
		{
			return OpenConnectionAsync<TDataAccessModel>(model, CancellationToken.None);
		}

		/// <summary>
		/// Opens and returns a new connection to the database for the given <see cref = "model"/>.
		/// </summary>
		/// <typeparam name = "TDataAccessModel">The type of <see cref = "DataAccessModel"/></typeparam>
		/// <param name = "model">The <see cref = "DataAccessModel"/></param>
		/// <remarks>This method does not require an existing <see cref = "DataAccessScope"/>. The new connection will be
		/// unrelated to any existing scope and it is up to the caller to dispose of the connection.</remarks>
		/// <returns>The <see cref = "IDbConnection"/></returns>
		public static async Task<IDbConnection> OpenConnectionAsync<TDataAccessModel>(this TDataAccessModel model, CancellationToken cancellationToken)where TDataAccessModel : DataAccessModel
		{
			return (await model.GetCurrentSqlDatabaseContext().OpenConnectionAsync(cancellationToken).ConfigureAwait(false));
		}

		/// <summary>
		/// Executes a given SQL query and returns the number of rows affected.
		/// </summary>
		/// <typeparam name = "TDataAccessModel"></typeparam>
		/// <param name = "model">The <see cref = "DataAccessModel"/></param>
		/// <param name = "sql">The SQL query as a string</param>
		/// <param name = "arguments">Arguments for the SQL query</param>
		/// <remarks>
		/// This method can only be called from within a <see cref = "DataAccessScope"/>.
		/// </remarks>
		/// <returns>The number of rows affected by the query.</returns>
		public static Task<int> ExecuteNonQueryAsync<TDataAccessModel>(this TDataAccessModel model, string sql, params object[] arguments)where TDataAccessModel : DataAccessModel
		{
			return ExecuteNonQueryAsync<TDataAccessModel>(model, sql, CancellationToken.None, arguments);
		}

		/// <summary>
		/// Executes a given SQL query and returns the number of rows affected.
		/// </summary>
		/// <typeparam name = "TDataAccessModel"></typeparam>
		/// <param name = "model">The <see cref = "DataAccessModel"/></param>
		/// <param name = "sql">The SQL query as a string</param>
		/// <param name = "arguments">Arguments for the SQL query</param>
		/// <remarks>
		/// This method can only be called from within a <see cref = "DataAccessScope"/>.
		/// </remarks>
		/// <returns>The number of rows affected by the query.</returns>
		public static async Task<int> ExecuteNonQueryAsync<TDataAccessModel>(this TDataAccessModel model, string sql, CancellationToken cancellationToken, params object[] arguments)where TDataAccessModel : DataAccessModel
		{
			var args = new List<TypedValue>(arguments.Select(c => new TypedValue(c?.GetType() ?? typeof (object), c)));
			return (await model.GetCurrentCommandsContext().ExecuteNonQueryAsync(sql, args, cancellationToken).ConfigureAwait(false));
		}

		/// <summary>
		/// Returns a list of results for a given SQL query.
		/// </summary>
		/// <typeparam name = "TDataAccessModel"></typeparam>
		/// <typeparam name = "T">The type of object to return for each value in the result set. Also see <see cref = "readObject"/></typeparam>
		/// <param name = "model">The <see cref = "DataAccessModel"/></param>
		/// <param name = "readObject">A function that converts an <see cref = "IDataReader"/> into an object of type <see cref = "T"/></param>
		/// <param name = "sql">The SQL query as a string</param>
		/// <remarks>
		/// This method can only be called from within a <see cref = "DataAccessScope"/>.
		/// </remarks>
		/// <returns>An <see cref = "IAsyncEnumerable{T}"/> that presents an <see cref = "IDataReader"/> for each row in the result set.</returns>
		public static Task<List<T>> ExecuteReadAllAsync<TDataAccessModel, T>(this TDataAccessModel model, Func<IDataReader, T> readObject, string sql)where TDataAccessModel : DataAccessModel
		{
			return ExecuteReadAllAsync<TDataAccessModel, T>(model, readObject, sql, CancellationToken.None);
		}

		/// <summary>
		/// Returns a list of results for a given SQL query.
		/// </summary>
		/// <typeparam name = "TDataAccessModel"></typeparam>
		/// <typeparam name = "T">The type of object to return for each value in the result set. Also see <see cref = "readObject"/></typeparam>
		/// <param name = "model">The <see cref = "DataAccessModel"/></param>
		/// <param name = "readObject">A function that converts an <see cref = "IDataReader"/> into an object of type <see cref = "T"/></param>
		/// <param name = "sql">The SQL query as a string</param>
		/// <remarks>
		/// This method can only be called from within a <see cref = "DataAccessScope"/>.
		/// </remarks>
		/// <returns>An <see cref = "IAsyncEnumerable{T}"/> that presents an <see cref = "IDataReader"/> for each row in the result set.</returns>
		public static async Task<List<T>> ExecuteReadAllAsync<TDataAccessModel, T>(this TDataAccessModel model, Func<IDataReader, T> readObject, string sql, CancellationToken cancellationToken)where TDataAccessModel : DataAccessModel
		{
			var emptyArgs = new
			{
			}

			;
			return (await model.ExecuteReadAllAsync(readObject, sql, emptyArgs, cancellationToken).ConfigureAwait(false));
		}

		/// <summary>
		/// Returns a list of results for a given SQL query.
		/// </summary>
		/// <typeparam name = "TDataAccessModel"></typeparam>
		/// <typeparam name = "T">The type of object to return for each value in the result set. Also see <see cref = "readObject"/></typeparam>
		/// <typeparam name = "TArgs">The anonymous type containing the parameters referenced by <see cref = "sql"/></typeparam>
		/// <param name = "model">The <see cref = "DataAccessModel"/></param>
		/// <param name = "readObject">A function that converts an <see cref = "IDataReader"/> into an object of type <see cref = "T"/></param>
		/// <param name = "sql">The SQL query as a string</param>
		/// <param name = "args">An anonymous type containing the parameters referenced by <see cref = "sql"/></param>
		/// <remarks>
		/// This method can only be called from within a <see cref = "DataAccessScope"/>.
		/// </remarks>
		/// <returns>An <see cref = "IAsyncEnumerable{T}"/> that presents an <see cref = "IDataReader"/> for each row in the result set.</returns>
		public static Task<List<T>> ExecuteReadAllAsync<TDataAccessModel, TArgs, T>(this TDataAccessModel model, Func<IDataReader, T> readObject, string sql, TArgs args)where TDataAccessModel : DataAccessModel
		{
			return ExecuteReadAllAsync<TDataAccessModel, TArgs, T>(model, readObject, sql, args, CancellationToken.None);
		}

		/// <summary>
		/// Returns a list of results for a given SQL query.
		/// </summary>
		/// <typeparam name = "TDataAccessModel"></typeparam>
		/// <typeparam name = "T">The type of object to return for each value in the result set. Also see <see cref = "readObject"/></typeparam>
		/// <typeparam name = "TArgs">The anonymous type containing the parameters referenced by <see cref = "sql"/></typeparam>
		/// <param name = "model">The <see cref = "DataAccessModel"/></param>
		/// <param name = "readObject">A function that converts an <see cref = "IDataReader"/> into an object of type <see cref = "T"/></param>
		/// <param name = "sql">The SQL query as a string</param>
		/// <param name = "args">An anonymous type containing the parameters referenced by <see cref = "sql"/></param>
		/// <remarks>
		/// This method can only be called from within a <see cref = "DataAccessScope"/>.
		/// </remarks>
		/// <returns>An <see cref = "IAsyncEnumerable{T}"/> that presents an <see cref = "IDataReader"/> for each row in the result set.</returns>
		public static async Task<List<T>> ExecuteReadAllAsync<TDataAccessModel, TArgs, T>(this TDataAccessModel model, Func<IDataReader, T> readObject, string sql, TArgs args, CancellationToken cancellationToken)where TDataAccessModel : DataAccessModel
		{
			var retval = new List<T>();
			model.ExecuteReader(readObject, sql, args).WithEach(c => retval.Add(c));
			return retval;
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Transactions;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	public partial class DataAccessScope
	{
		/// <summary>
		/// Flushes the current transaction for all <see cref = "DataAccessModel"/> that have
		/// participated in the current transaction
		/// </summary>
		/// <remarks>
		/// Flushing a transaction writes any pending INSERTs, UPDATES and DELETES to the database
		/// but does not commit the transaction. To commit the transaction you must call 
		/// <see cref = "Complete(ScopeCompleteOptions)"/>.
		/// </remarks>
		public Task FlushAsync()
		{
			return this.FlushAsync(CancellationToken.None);
		}

		/// <summary>
		/// Flushes the current transaction for all <see cref = "DataAccessModel"/> that have
		/// participated in the current transaction
		/// </summary>
		/// <remarks>
		/// Flushing a transaction writes any pending INSERTs, UPDATES and DELETES to the database
		/// but does not commit the transaction. To commit the transaction you must call 
		/// <see cref = "Complete(ScopeCompleteOptions)"/>.
		/// </remarks>
		public async Task FlushAsync(CancellationToken cancellationToken)
		{
			this.transaction.CheckAborted();
			foreach (var dataAccessModel in DataAccessTransaction.Current.ParticipatingDataAccessModels)
			{
				if (!dataAccessModel.IsDisposed)
				{
					await dataAccessModel.FlushAsync(cancellationToken).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Flushes the current transaction for the given <paramref name = "dataAccessModel"/>
		/// </summary>
		/// <remarks>
		/// Flushing a transaction writes any pending INSERTs, UPDATES and DELETES to the database
		/// but does not commit the transaction. To commit the transaction you must call 
		/// <see cref = "Complete(ScopeCompleteOptions)"/>.
		/// </remarks>
		/// <param name = "dataAccessModel">
		/// The <see cref = "DataAccessModel"/> to flush if you only want to flush a single
		/// DataAccessModel
		/// </param>
		public Task FlushAsync(DataAccessModel dataAccessModel)
		{
			return this.FlushAsync(dataAccessModel, CancellationToken.None);
		}

		/// <summary>
		/// Flushes the current transaction for the given <paramref name = "dataAccessModel"/>
		/// </summary>
		/// <remarks>
		/// Flushing a transaction writes any pending INSERTs, UPDATES and DELETES to the database
		/// but does not commit the transaction. To commit the transaction you must call 
		/// <see cref = "Complete(ScopeCompleteOptions)"/>.
		/// </remarks>
		/// <param name = "dataAccessModel">
		/// The <see cref = "DataAccessModel"/> to flush if you only want to flush a single
		/// DataAccessModel
		/// </param>
		public async Task FlushAsync(DataAccessModel dataAccessModel, CancellationToken cancellationToken)
		{
			this.transaction.CheckAborted();
			if (!dataAccessModel.IsDisposed)
			{
				await dataAccessModel.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Flushes the current transaction and marks the scope as completed
		/// </summary>
		/// <remarks>
		/// <para>
		/// By default all nested scopes auto-flush without commiting the transaction. You can
		/// disable auto-flush by calling <see cref = "Complete(ScopeCompleteOptions)"/>
		/// </para>
		/// </remarks>
		public Task<T> CompleteAsync<T>(Func<T> result)
		{
			return this.CompleteAsync<T>(result, CancellationToken.None);
		}

		/// <summary>
		/// Flushes the current transaction and marks the scope as completed
		/// </summary>
		/// <remarks>
		/// <para>
		/// By default all nested scopes auto-flush without commiting the transaction. You can
		/// disable auto-flush by calling <see cref = "Complete(ScopeCompleteOptions)"/>
		/// </para>
		/// </remarks>
		public async Task<T> CompleteAsync<T>(Func<T> result, CancellationToken cancellationToken)
		{
			var retval = result();
			await CompleteAsync(ScopeCompleteOptions.Default, cancellationToken).ConfigureAwait(false);
			return retval;
		}

		/// <summary>
		/// Flushes the current transaction and marks the scope as completed
		/// </summary>
		/// <remarks>
		/// <para>
		/// By default all nested scopes auto-flush without commiting the transaction. You can
		/// disable auto-flush by calling <see cref = "Complete(ScopeCompleteOptions)"/>
		/// </para>
		/// </remarks>
		public Task<T> CompleteAsync<T>(Func<T> result, ScopeCompleteOptions options)
		{
			return this.CompleteAsync<T>(result, options, CancellationToken.None);
		}

		/// <summary>
		/// Flushes the current transaction and marks the scope as completed
		/// </summary>
		/// <remarks>
		/// <para>
		/// By default all nested scopes auto-flush without commiting the transaction. You can
		/// disable auto-flush by calling <see cref = "Complete(ScopeCompleteOptions)"/>
		/// </para>
		/// </remarks>
		public async Task<T> CompleteAsync<T>(Func<T> result, ScopeCompleteOptions options, CancellationToken cancellationToken)
		{
			var retval = result();
			await CompleteAsync(options, cancellationToken).ConfigureAwait(false);
			return retval;
		}

		/// <summary>
		/// Flushes the current transaction and marks the scope as completed
		/// </summary>
		/// <remarks>
		/// <para>
		/// By default all nested scopes auto-flush without commiting the transaction. You can
		/// disable auto-flush by calling <see cref = "Complete(ScopeCompleteOptions)"/>
		/// </para>
		/// </remarks>
		public Task CompleteAsync()
		{
			return this.CompleteAsync(CancellationToken.None);
		}

		/// <summary>
		/// Flushes the current transaction and marks the scope as completed
		/// </summary>
		/// <remarks>
		/// <para>
		/// By default all nested scopes auto-flush without commiting the transaction. You can
		/// disable auto-flush by calling <see cref = "Complete(ScopeCompleteOptions)"/>
		/// </para>
		/// </remarks>
		public async Task CompleteAsync(CancellationToken cancellationToken)
		{
			await CompleteAsync(ScopeCompleteOptions.Default, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Flushes the current transaction and marks the scope as completed
		/// </summary>
		/// <remarks>
		/// <para>
		/// Flushing a scope commits 
		/// </para>
		/// <para>
		/// A scope is considered to have aborted if Complete is not called before the scope is disposed
		/// The outer most scope flushes and commits the transaction when it is completed.
		/// </para>
		/// <para>
		/// By default all nested scopes auto-flush without commiting the transaction. You can
		/// disable auto-flush by calling <see cref = "Complete(ScopeCompleteOptions)"/>
		/// </para>
		/// </remarks>
		/// <param name = "options">Set to <a cref = "ScopeCompleteOptions.SuppressAutoFlush"/> to suppress auto-flush</param>
		public Task CompleteAsync(ScopeCompleteOptions options)
		{
			return this.CompleteAsync(options, CancellationToken.None);
		}

		/// <summary>
		/// Flushes the current transaction and marks the scope as completed
		/// </summary>
		/// <remarks>
		/// <para>
		/// Flushing a scope commits 
		/// </para>
		/// <para>
		/// A scope is considered to have aborted if Complete is not called before the scope is disposed
		/// The outer most scope flushes and commits the transaction when it is completed.
		/// </para>
		/// <para>
		/// By default all nested scopes auto-flush without commiting the transaction. You can
		/// disable auto-flush by calling <see cref = "Complete(ScopeCompleteOptions)"/>
		/// </para>
		/// </remarks>
		/// <param name = "options">Set to <a cref = "ScopeCompleteOptions.SuppressAutoFlush"/> to suppress auto-flush</param>
		public async Task CompleteAsync(ScopeCompleteOptions options, CancellationToken cancellationToken)
		{
			this.complete = true;
			this.transaction?.CheckAborted();
			if ((options & ScopeCompleteOptions.SuppressAutoFlush) != 0)
			{
				await FlushAsync(cancellationToken).ConfigureAwait(false);
			}

			if (this.transaction == null)
			{
				DataAccessTransaction.Current = this.outerTransaction;
				return;
			}

			if (!this.isRoot)
			{
				DataAccessTransaction.Current = this.outerTransaction;
				return;
			}

			if (this.transaction.HasSystemTransaction)
			{
				return;
			}

			if (this.transaction != DataAccessTransaction.Current)
			{
				throw new InvalidOperationException($"Cannot commit {GetType().Name} within another Async/Call context");
			}

			await this.transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
			this.transaction.Dispose();
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Linq;
	using System.Threading;
	using System.Transactions;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using Platform;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	public partial class DataAccessTransaction
	{
		public Task CommitAsync()
		{
			return this.CommitAsync(CancellationToken.None);
		}

		public async Task CommitAsync(CancellationToken cancellationToken)
		{
			this.isfinishing = true;
			if (this.transactionContextsByDataAccessModel != null)
			{
				foreach (var transactionContext in this.transactionContextsByDataAccessModel.Values)
				{
					await transactionContext.CommitAsync(cancellationToken).ConfigureAwait(false);
					transactionContext.Dispose();
				}
			}
		}

		public Task RollbackAsync()
		{
			return this.RollbackAsync(CancellationToken.None);
		}

		public async Task RollbackAsync(CancellationToken cancellationToken)
		{
			this.isfinishing = true;
			this.aborted = true;
			if (this.transactionContextsByDataAccessModel != null)
			{
				foreach (var transactionContext in this.transactionContextsByDataAccessModel.Values)
				{
					try
					{
						await transactionContext.RollbackAsync(cancellationToken).ConfigureAwait(false);
					}
					catch
					{
					// ignored
					}
				}
			}
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Collections;
	using System.Threading.Tasks;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	internal partial class DefaultIfEmptyEnumerator<T>
	{
		public Task<bool> MoveNextAsync()
		{
			return this.MoveNextAsync(CancellationToken.None);
		}

		public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			switch (this.state)
			{
				case 0:
					goto state0;
				case 1:
					goto state1;
				case 9:
					goto state9;
			}

			state0:
				var result = (await EnumerableExtensions.MoveNextAsync(this.enumerator, cancellationToken).ConfigureAwait(false));
			if (!result)
			{
				this.Current = this.specifiedValue;
				this.state = 9;
				return true;
			}
			else
			{
				this.state = 1;
				this.Current = this.enumerator.Current;
				return true;
			}

			state1:
				result = (await EnumerableExtensions.MoveNextAsync(this.enumerator, cancellationToken).ConfigureAwait(false));
			if (result)
			{
				this.Current = this.enumerator.Current;
				return true;
			}
			else
			{
				this.state = 9;
				return false;
			}

			state9:
				return false;
		}
	}

	internal partial class DefaultIfEmptyCoalesceSpecifiedValueEnumerator<T>
	{
		public Task<bool> MoveNextAsync()
		{
			return this.MoveNextAsync(CancellationToken.None);
		}

		public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			switch (this.state)
			{
				case 0:
					goto state0;
				case 1:
					goto state1;
				case 9:
					goto state9;
			}

			state0:
				var result = (await EnumerableExtensions.MoveNextAsync(this.enumerator, cancellationToken).ConfigureAwait(false));
			if (!result)
			{
				this.Current = this.specifiedValue ?? default (T);
				this.state = 9;
				return true;
			}
			else
			{
				this.state = 1;
				this.Current = this.enumerator.Current;
				return true;
			}

			state1:
				result = (await EnumerableExtensions.MoveNextAsync(this.enumerator, cancellationToken).ConfigureAwait(false));
			if (result)
			{
				this.Current = this.enumerator.Current;
				return true;
			}
			else
			{
				this.state = 9;
				return false;
			}

			state9:
				return false;
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Collections;
	using System.Threading.Tasks;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	internal partial class EmptyIfFirstIsNullEnumerator<T>
	{
		public Task<bool> MoveNextAsync()
		{
			return this.MoveNextAsync(CancellationToken.None);
		}

		public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			switch (this.state)
			{
				case 0:
					goto state0;
				case 1:
					goto state1;
				case 9:
					goto state9;
			}

			state0:
				var result = (await EnumerableExtensions.MoveNextAsync(this.enumerator, cancellationToken).ConfigureAwait(false));
			if (!result || this.enumerator.Current == null)
			{
				this.state = 9;
				return false;
			}
			else
			{
				this.state = 1;
				this.Current = this.enumerator.Current;
				return true;
			}

			state1:
				result = (await EnumerableExtensions.MoveNextAsync(this.enumerator, cancellationToken).ConfigureAwait(false));
			if (result)
			{
				this.Current = this.enumerator.Current;
				return true;
			}
			else
			{
				this.state = 9;
				return false;
			}

			state9:
				return false;
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Linq;
	using System.Threading;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Runtime.CompilerServices;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	public static partial class EnumerableExtensions
	{
		internal static Task<T> AlwaysReadFirstAsync<T>(this IEnumerable<T> source)
		{
			return AlwaysReadFirstAsync<T>(source, CancellationToken.None);
		}

		internal static async Task<T> AlwaysReadFirstAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
		{
			return (await source.FirstAsync(cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> CountAsync<T>(this IEnumerable<T> source)
		{
			return CountAsync<T>(source, CancellationToken.None);
		}

		public static async Task<int> CountAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
		{
			if (source is ICollection<T> list)
			{
				return list.Count;
			}

			var retval = 0;
			using (var enumerator = source.GetAsyncEnumeratorOrAdapt())
			{
				while ((await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					retval++;
				}
			}

			return retval;
		}

		public static Task<long> LongCountAsync<T>(this IEnumerable<T> source)
		{
			return LongCountAsync<T>(source, CancellationToken.None);
		}

		public static async Task<long> LongCountAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
		{
			if (source is ICollection<T> list)
			{
				return list.Count;
			}

			var retval = 0L;
			using (var enumerator = source.GetAsyncEnumeratorOrAdapt())
			{
				while ((await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					retval++;
				}
			}

			return retval;
		}

		internal static Task<T> SingleOrSpecifiedValueIfFirstIsDefaultValueAsync<T>(this IEnumerable<T> source, T specifiedValue)
		{
			return SingleOrSpecifiedValueIfFirstIsDefaultValueAsync<T>(source, specifiedValue, CancellationToken.None);
		}

		internal static async Task<T> SingleOrSpecifiedValueIfFirstIsDefaultValueAsync<T>(this IEnumerable<T> source, T specifiedValue, CancellationToken cancellationToken)
		{
			using (var enumerator = source.GetAsyncEnumeratorOrAdapt())
			{
				if (!(await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					return Enumerable.Single<T>(Enumerable.Empty<T>());
				}

				var result = enumerator.Current;
				if ((await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					return Enumerable.Single<T>(new T[2]);
				}

				if (Equals(result, default (T)))
				{
					return specifiedValue;
				}

				return result;
			}
		}

		public static Task<T> SingleAsync<T>(this IEnumerable<T> source)
		{
			return SingleAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> SingleAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
		{
			using (var enumerator = source.GetAsyncEnumeratorOrAdapt())
			{
				if (!(await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					return Enumerable.Single<T>(Enumerable.Empty<T>());
				}

				var result = enumerator.Current;
				if ((await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					return Enumerable.Single<T>(new T[2]);
				}

				return result;
			}
		}

		public static Task<T> SingleOrDefaultAsync<T>(this IEnumerable<T> source)
		{
			return SingleOrDefaultAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> SingleOrDefaultAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
		{
			using (var enumerator = source.GetAsyncEnumeratorOrAdapt())
			{
				if (!(await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					return default (T);
				}

				var result = enumerator.Current;
				if ((await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					return Enumerable.Single(new T[2]);
				}

				return result;
			}
		}

		public static Task<T> FirstAsync<T>(this IEnumerable<T> enumerable)
		{
			return FirstAsync<T>(enumerable, CancellationToken.None);
		}

		public static async Task<T> FirstAsync<T>(this IEnumerable<T> enumerable, CancellationToken cancellationToken)
		{
			using (var enumerator = enumerable.GetAsyncEnumeratorOrAdapt())
			{
				if (!(await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					return Enumerable.First(Enumerable.Empty<T>());
				}

				return enumerator.Current;
			}
		}

		public static Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> source)
		{
			return FirstOrDefaultAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
		{
			using (var enumerator = source.GetAsyncEnumeratorOrAdapt())
			{
				if (!(await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)))
				{
					return default (T);
				}

				return enumerator.Current;
			}
		}

		internal static Task<T> SingleOrExceptionIfFirstIsNullAsync<T>(this IEnumerable<T? > source)where T : struct
		{
			return SingleOrExceptionIfFirstIsNullAsync<T>(source, CancellationToken.None);
		}

		internal static async Task<T> SingleOrExceptionIfFirstIsNullAsync<T>(this IEnumerable<T? > source, CancellationToken cancellationToken)where T : struct
		{
			using (var enumerator = source.GetAsyncEnumeratorOrAdapt())
			{
				if (!(await EnumerableExtensions.MoveNextAsync(enumerator, cancellationToken).ConfigureAwait(false)) || enumerator.Current == null)
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}

				return enumerator.Current.Value;
			}
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	public partial interface IDataAccessModelHook
	{
		/// <summary>
		/// Called after a new object has been created
		/// </summary>
		Task CreateAsync(DataAccessObject dataAccessObject);
		/// <summary>
		/// Called after a new object has been created
		/// </summary>
		Task CreateAsync(DataAccessObject dataAccessObject, CancellationToken cancellationToken);
		/// <summary>
		/// Called just after an object has been read from the database
		/// </summary>
		Task ReadAsync(DataAccessObject dataAccessObject);
		/// <summary>
		/// Called just after an object has been read from the database
		/// </summary>
		Task ReadAsync(DataAccessObject dataAccessObject, CancellationToken cancellationToken);
		/// <summary>
		/// Called just before changes/updates are written to the database
		/// </summary>
		Task BeforeSubmitAsync(DataAccessModelHookSubmitContext context);
		/// <summary>
		/// Called just before changes/updates are written to the database
		/// </summary>
		Task BeforeSubmitAsync(DataAccessModelHookSubmitContext context, CancellationToken cancellationToken);
		/// <summary>
		/// Called just after changes have been written to the database
		/// </summary>
		/// <remarks>
		/// A transaction is usually committed after this call unless the call is due
		/// to a <see cref = "DataAccessModel.Flush()"/> call
		/// </remarks>
		Task AfterSubmitAsync(DataAccessModelHookSubmitContext context);
		/// <summary>
		/// Called just after changes have been written to the database
		/// </summary>
		/// <remarks>
		/// A transaction is usually committed after this call unless the call is due
		/// to a <see cref = "DataAccessModel.Flush()"/> call
		/// </remarks>
		Task AfterSubmitAsync(DataAccessModelHookSubmitContext context, CancellationToken cancellationToken);
		/// <summary>
		/// Called just before a transaction is rolled back
		/// </summary>
		Task BeforeRollbackAsync(DataAccessModelHookRollbackContext context);
		/// <summary>
		/// Called just before a transaction is rolled back
		/// </summary>
		Task BeforeRollbackAsync(DataAccessModelHookRollbackContext context, CancellationToken cancellationToken);
		/// <summary>
		/// Called just after a transaction is rolled back
		/// </summary>
		Task AfterRollbackAsync(DataAccessModelHookRollbackContext context);
		/// <summary>
		/// Called just after a transaction is rolled back
		/// </summary>
		Task AfterRollbackAsync(DataAccessModelHookRollbackContext context, CancellationToken cancellationToken);
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Shaolinq;
	using Shaolinq.Persistence;

	public partial interface IDataAccessModelInternal
	{
		Task OnHookCreateAsync(DataAccessObject obj);
		Task OnHookCreateAsync(DataAccessObject obj, CancellationToken cancellationToken);
		Task OnHookReadAsync(DataAccessObject obj);
		Task OnHookReadAsync(DataAccessObject obj, CancellationToken cancellationToken);
		Task OnHookBeforeSubmitAsync(DataAccessModelHookSubmitContext context);
		Task OnHookBeforeSubmitAsync(DataAccessModelHookSubmitContext context, CancellationToken cancellationToken);
		Task OnHookAfterSubmitAsync(DataAccessModelHookSubmitContext context);
		Task OnHookAfterSubmitAsync(DataAccessModelHookSubmitContext context, CancellationToken cancellationToken);
		Task OnHookBeforeRollbackAsync(DataAccessModelHookRollbackContext context);
		Task OnHookBeforeRollbackAsync(DataAccessModelHookRollbackContext context, CancellationToken cancellationToken);
		Task OnHookAfterRollbackAsync(DataAccessModelHookRollbackContext context);
		Task OnHookAfterRollbackAsync(DataAccessModelHookRollbackContext context, CancellationToken cancellationToken);
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	// Copyright (c) 2007-2018 Thong Nguyen (tumtumtum@gmail.com)
	using System.Data;
	using System.Threading;
	using System.Data.Common;
	using System.Threading.Tasks;
	using Shaolinq;
	using Shaolinq.Persistence;

	public static partial class DbCommandExtensions
	{
		public static Task<IDataReader> ExecuteReaderExAsync(this IDbCommand command, DataAccessModel dataAccessModel, bool suppressAnalytics = false)
		{
			return ExecuteReaderExAsync(command, dataAccessModel, CancellationToken.None, suppressAnalytics);
		}

		public static async Task<IDataReader> ExecuteReaderExAsync(this IDbCommand command, DataAccessModel dataAccessModel, CancellationToken cancellationToken, bool suppressAnalytics = false)
		{
			if (command is MarsDbCommand marsDbCommand)
			{
				if (!suppressAnalytics)
				{
					dataAccessModel.queryAnalytics.IncrementQueryCount();
				}

				return (await marsDbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false));
			}

			if (command is DbCommand dbCommand)
			{
				if (!suppressAnalytics)
				{
					dataAccessModel.queryAnalytics.IncrementQueryCount();
				}

				return (await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false));
			}

			return command.ExecuteReader();
		}

		public static Task<int> ExecuteNonQueryExAsync(this IDbCommand command, DataAccessModel dataAccessModel, bool suppressAnalytics = false)
		{
			return ExecuteNonQueryExAsync(command, dataAccessModel, CancellationToken.None, suppressAnalytics);
		}

		public static async Task<int> ExecuteNonQueryExAsync(this IDbCommand command, DataAccessModel dataAccessModel, CancellationToken cancellationToken, bool suppressAnalytics = false)
		{
			if (command is MarsDbCommand marsDbCommand)
			{
				if (!suppressAnalytics)
				{
					dataAccessModel.queryAnalytics.IncrementQueryCount();
				}

				return (await marsDbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false));
			}

			if (command is DbCommand dbCommand)
			{
				if (!suppressAnalytics)
				{
					dataAccessModel.queryAnalytics.IncrementQueryCount();
				}

				return (await dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false));
			}

			return command.ExecuteNonQuery();
		}
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	using System.Data;
	using System.Threading;
	using System.Data.Common;
	using System.Threading.Tasks;
	using Shaolinq;
	using Shaolinq.Persistence;

	public static partial class DataReaderExtensions
	{
		public static Task<bool> ReadExAsync(this IDataReader reader)
		{
			return ReadExAsync(reader, CancellationToken.None);
		}

		public static async Task<bool> ReadExAsync(this IDataReader reader, CancellationToken cancellationToken)
		{
			var dbDataReader = reader as DbDataReader;
			return (dbDataReader != null ? await dbDataReader.ReadAsync(cancellationToken).ConfigureAwait(false) : ((bool? )null)) ?? reader.Read();
		}

		public static Task<bool> NextResultExAsync(this IDataReader reader)
		{
			return NextResultExAsync(reader, CancellationToken.None);
		}

		public static async Task<bool> NextResultExAsync(this IDataReader reader, CancellationToken cancellationToken)
		{
			var dbDataReader = reader as DbDataReader;
			return (dbDataReader != null ? await dbDataReader.NextResultAsync(cancellationToken).ConfigureAwait(false) : ((bool? )null)) ?? reader.NextResult();
		}

		public static Task<bool> IsDbNullExAsync(this IDataReader reader, int ordinal)
		{
			return IsDbNullExAsync(reader, ordinal, CancellationToken.None);
		}

		public static async Task<bool> IsDbNullExAsync(this IDataReader reader, int ordinal, CancellationToken cancellationToken)
		{
			var dbDataReader = reader as DbDataReader;
			return (dbDataReader != null ? await dbDataReader.IsDBNullAsync(ordinal, cancellationToken).ConfigureAwait(false) : ((bool? )null)) ?? reader.IsDBNull(ordinal);
		}

		public static Task<T> GetFieldValueExAsync<T>(this IDataReader reader, int ordinal)
		{
			return GetFieldValueExAsync<T>(reader, ordinal, CancellationToken.None);
		}

		public static async Task<T> GetFieldValueExAsync<T>(this IDataReader reader, int ordinal, CancellationToken cancellationToken)
		{
			if (reader is DbDataReader dbDataReader)
			{
				return (await dbDataReader.GetFieldValueAsync<T>(ordinal, cancellationToken).ConfigureAwait(false));
			}

			return (T)Convert.ChangeType(reader.GetValue(ordinal), typeof (T));
		}
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Linq.Expressions;
	using System.Collections.Generic;
	using Shaolinq;
	using Shaolinq.Logging;
	using Shaolinq.Persistence;
	using Shaolinq.TypeBuilding;
	using Shaolinq.Persistence.Linq;
	using Shaolinq.Persistence.Linq.Expressions;

	public partial class DefaultSqlTransactionalCommandsContext
	{
		public override Task<int> ExecuteNonQueryAsync(string sql, IReadOnlyList<TypedValue> parameters)
		{
			return this.ExecuteNonQueryAsync(sql, parameters, CancellationToken.None);
		}

		public override async Task<int> ExecuteNonQueryAsync(string sql, IReadOnlyList<TypedValue> parameters, CancellationToken cancellationToken)
		{
			var command = CreateCommand();
			try
			{
				foreach (var value in parameters)
				{
					AddParameter(command, value.Type, value.Name, value.Value);
				}

				command.CommandText = sql;
				Logger.Info(() => FormatCommand(command));
				try
				{
					return (await command.ExecuteNonQueryExAsync(this.DataAccessModel, cancellationToken).ConfigureAwait(false));
				}
				catch (Exception e)
				{
					command?.Dispose();
					command = null;
					var decoratedException = LogAndDecorateException(e, command);
					if (decoratedException != null)
					{
						throw decoratedException;
					}

					throw;
				}
			}
			catch
			{
				command?.Dispose();
				throw;
			}
		}

		public override Task<ExecuteReaderContext> ExecuteReaderAsync(string sql, IReadOnlyList<TypedValue> parameters)
		{
			return this.ExecuteReaderAsync(sql, parameters, CancellationToken.None);
		}

		public override async Task<ExecuteReaderContext> ExecuteReaderAsync(string sql, IReadOnlyList<TypedValue> parameters, CancellationToken cancellationToken)
		{
			var command = CreateCommand();
			try
			{
				foreach (var value in parameters)
				{
					AddParameter(command, value.Type, value.Name, value.Value);
				}

				command.CommandText = sql;
				Logger.Info(() => FormatCommand(command));
				try
				{
					return new ExecuteReaderContext((await command.ExecuteReaderExAsync(this.DataAccessModel, cancellationToken).ConfigureAwait(false)), command);
				}
				catch (Exception e)
				{
					command?.Dispose();
					command = null;
					var decoratedException = LogAndDecorateException(e, command);
					if (decoratedException != null)
					{
						throw decoratedException;
					}

					throw;
				}
			}
			catch
			{
				command?.Dispose();
				throw;
			}
		}

		public override Task UpdateAsync(Type type, IEnumerable<DataAccessObject> dataAccessObjects)
		{
			return this.UpdateAsync(type, dataAccessObjects, CancellationToken.None);
		}

		public override async Task UpdateAsync(Type type, IEnumerable<DataAccessObject> dataAccessObjects, CancellationToken cancellationToken)
		{
			var typeDescriptor = this.DataAccessModel.GetTypeDescriptor(type);
			foreach (var dataAccessObject in dataAccessObjects)
			{
				if (dataAccessObject.GetAdvanced().IsCommitted)
				{
					continue;
				}

				var objectState = dataAccessObject.GetAdvanced().ObjectState;
				if ((objectState & (DataAccessObjectState.Changed | DataAccessObjectState.ServerSidePropertiesHydrated)) == 0)
				{
					continue;
				}

				using (var command = BuildUpdateCommand(typeDescriptor, dataAccessObject))
				{
					if (command == null)
					{
						Logger.ErrorFormat("Object is reported as changed but GetChangedProperties returns an empty list ({0})", dataAccessObject);
						continue;
					}

					Logger.Info(() => FormatCommand(command));
					int result;
					try
					{
						result = (await command.ExecuteNonQueryExAsync(this.DataAccessModel, cancellationToken).ConfigureAwait(false));
					}
					catch (Exception e)
					{
						var decoratedException = LogAndDecorateException(e, command);
						if (decoratedException != null)
						{
							throw decoratedException;
						}

						throw;
					}

					if (result == 0)
					{
						throw new MissingDataAccessObjectException(dataAccessObject, null, command.CommandText);
					}
				}

				dataAccessObject.ToObjectInternal().SetIsCommitted(true);
			}
		}

		public override Task<InsertResults> InsertAsync(Type type, IEnumerable<DataAccessObject> dataAccessObjects)
		{
			return this.InsertAsync(type, dataAccessObjects, CancellationToken.None);
		}

		public override async Task<InsertResults> InsertAsync(Type type, IEnumerable<DataAccessObject> dataAccessObjects, CancellationToken cancellationToken)
		{
			var listToFixup = new List<DataAccessObject>();
			var listToRetry = new List<DataAccessObject>();
			var canDefer = !this.DataAccessModel.hasAnyAutoIncrementValidators;
			foreach (var dataAccessObject in dataAccessObjects)
			{
				if (dataAccessObject.GetAdvanced().IsCommitted)
				{
					continue;
				}

				var objectState = dataAccessObject.GetAdvanced().ObjectState;
				switch (objectState & DataAccessObjectState.NewChanged)
				{
					case DataAccessObjectState.Unchanged:
						continue;
					case DataAccessObjectState.New:
					case DataAccessObjectState.NewChanged:
						break;
					case DataAccessObjectState.Changed:
						throw new NotSupportedException($"Changed state not supported {objectState}");
				}

				var primaryKeyIsComplete = (objectState & DataAccessObjectState.PrimaryKeyReferencesNewObjectWithServerSideProperties) != DataAccessObjectState.PrimaryKeyReferencesNewObjectWithServerSideProperties;
				var constraintsDeferrableOrNotReferencingNewObject = (canDefer && this.SqlDatabaseContext.SqlDialect.SupportsCapability(SqlCapability.Deferrability)) || ((objectState & DataAccessObjectState.ReferencesNewObject) == 0);
				var objectReadyToBeCommited = primaryKeyIsComplete && constraintsDeferrableOrNotReferencingNewObject;
				if (objectReadyToBeCommited)
				{
					var typeDescriptor = this.DataAccessModel.GetTypeDescriptor(type);
					using (var command = BuildInsertCommand(typeDescriptor, dataAccessObject))
					{
						retryInsert:
							Logger.Info(() => FormatCommand(command));
						try
						{
							var reader = (await command.ExecuteReaderExAsync(this.DataAccessModel, cancellationToken).ConfigureAwait(false));
							using (reader)
							{
								if (dataAccessObject.GetAdvanced().DefinesAnyDirectPropertiesGeneratedOnTheServerSide)
								{
									var dataAccessObjectInternal = dataAccessObject.ToObjectInternal();
									var result = (await reader.ReadExAsync(cancellationToken).ConfigureAwait(false));
									if (result)
									{
										ApplyPropertiesGeneratedOnServerSide(dataAccessObject, reader);
									}

									reader.Close();
									if (!dataAccessObjectInternal.ValidateServerSideGeneratedIds())
									{
										await DeleteAsync(dataAccessObject.GetType(), new[]{dataAccessObject}, cancellationToken).ConfigureAwait(false);
										goto retryInsert;
									}

									dataAccessObjectInternal.MarkServerSidePropertiesAsApplied();
									var updateRequired = dataAccessObjectInternal.ComputeServerGeneratedIdDependentComputedTextProperties();
									if (updateRequired)
									{
										await UpdateAsync(dataAccessObject.GetType(), new[]{dataAccessObject}, cancellationToken).ConfigureAwait(false);
									}
								}
							}
						}
						catch (Exception e)
						{
							var decoratedException = LogAndDecorateException(e, command);
							if (decoratedException != null)
							{
								throw decoratedException;
							}

							throw;
						}

						if ((objectState & DataAccessObjectState.ReferencesNewObjectWithServerSideProperties) == DataAccessObjectState.ReferencesNewObjectWithServerSideProperties)
						{
							dataAccessObject.ToObjectInternal().SetIsCommitted(false);
							listToFixup.Add(dataAccessObject);
						}
						else
						{
							dataAccessObject.ToObjectInternal().SetIsCommitted(true);
						}
					}
				}
				else
				{
					listToRetry.Add(dataAccessObject);
				}
			}

			return new InsertResults(listToFixup, listToRetry);
		}

		public override Task DeleteAsync(SqlDeleteExpression deleteExpression)
		{
			return this.DeleteAsync(deleteExpression, CancellationToken.None);
		}

		public override async Task DeleteAsync(SqlDeleteExpression deleteExpression, CancellationToken cancellationToken)
		{
			var formatResult = this.SqlDatabaseContext.SqlQueryFormatterManager.Format(deleteExpression, SqlQueryFormatterOptions.Default);
			using (var command = CreateCommand())
			{
				command.CommandText = formatResult.CommandText;
				foreach (var value in formatResult.ParameterValues)
				{
					AddParameter(command, value.Type, value.Name, value.Value);
				}

				Logger.Info(() => FormatCommand(command));
				try
				{
					var count = (await command.ExecuteNonQueryExAsync(this.DataAccessModel, cancellationToken).ConfigureAwait(false));
				}
				catch (Exception e)
				{
					var decoratedException = LogAndDecorateException(e, command);
					if (decoratedException != null)
					{
						throw decoratedException;
					}

					throw;
				}
			}
		}

		public override Task DeleteAsync(Type type, IEnumerable<DataAccessObject> dataAccessObjects)
		{
			return this.DeleteAsync(type, dataAccessObjects, CancellationToken.None);
		}

		public override async Task DeleteAsync(Type type, IEnumerable<DataAccessObject> dataAccessObjects, CancellationToken cancellationToken)
		{
			var provider = new SqlQueryProvider(this.DataAccessModel, this.SqlDatabaseContext);
			var expression = BuildDeleteExpression(type, dataAccessObjects);
			if (expression == null)
			{
				return;
			}

			await ((ISqlQueryProvider)provider).ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false);
			foreach (var dataAccessObject in dataAccessObjects)
			{
				dataAccessObject.ToObjectInternal().SetIsCommitted(true);
			}
		}
	}
}

namespace Shaolinq.Persistence.Linq
{
#pragma warning disable
	using System;
	using System.Data;
	using System.Threading;
	using System.Collections;
	using System.Threading.Tasks;
	using Shaolinq;
	using Shaolinq.Persistence;
	using Shaolinq.Persistence.Linq;

	internal partial class ObjectProjectionAsyncEnumerator<T, C>
	{
		public virtual Task<bool> MoveNextAsync()
		{
			return this.MoveNextAsync(CancellationToken.None);
		}

		public virtual async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			switch (this.state)
			{
				case 0:
					goto state0;
				case 1:
					goto state1;
				case 9:
					goto state9;
			}

			state0:
				this.state = 1;
			var commandsContext = (await this.transactionExecutionContextAcquisition.TransactionContext.GetSqlTransactionalCommandsContextAsync(cancellationToken).ConfigureAwait(false));
			this.executeReaderContext = (await commandsContext.ExecuteReaderAsync(this.objectProjector.CommandText, this.objectProjector.ParameterValues, cancellationToken).ConfigureAwait(false));
			this.dataReader = this.executeReaderContext.DataReader;
			this.context = this.objectProjector.CreateEnumerationContext(this.dataReader, this.transactionExecutionContextAcquisition.Version);
			state1:
				T result;
			if ((await this.dataReader.ReadExAsync(cancellationToken).ConfigureAwait(false)))
			{
				T value = this.objectProjector.objectReader(this.objectProjector, this.dataReader, this.transactionExecutionContextAcquisition.Version, this.objectProjector.placeholderValues);
				if (this.objectProjector.ProcessMoveNext(this.dataReader, value, ref this.context, out result))
				{
					this.Current = result;
					return true;
				}

				goto state1;
			}

			this.state = 9;
			if (this.objectProjector.ProcessLastMoveNext(this.dataReader, ref this.context, out result))
			{
				this.Current = result;
				Close();
				return true;
			}

			state9:
				return false;
		}
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	using System.Data;
	using System.Threading;
	using System.Data.Common;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using Shaolinq;
	using Shaolinq.Persistence;

	public partial class MarsDataReader
	{
		public Task BufferAllAsync()
		{
			return this.BufferAllAsync(CancellationToken.None);
		}

		public async Task BufferAllAsync(CancellationToken cancellationToken)
		{
			if (this.IsClosed || this.closed)
			{
				return;
			}

			this.rows = new Queue<object[]>();
			try
			{
				this.fieldCount = base.FieldCount;
				this.recordsAffected = base.RecordsAffected;
				this.ordinalByFieldName = new Dictionary<string, int>(this.fieldCount);
				this.dataTypeNames = new string[this.fieldCount];
				this.fieldTypes = new Type[this.fieldCount];
				this.names = new string[this.fieldCount];
				for (var i = 0; i < base.FieldCount; i++)
				{
					this.ordinalByFieldName[base.GetName(i)] = i;
					this.dataTypeNames[i] = base.GetDataTypeName(i);
					this.fieldTypes[i] = base.GetFieldType(i);
					this.names[i] = base.GetName(i);
				}

				while ((await this.Inner.ReadExAsync(cancellationToken).ConfigureAwait(false)))
				{
					var rowData = new object[base.FieldCount];
					base.GetValues(rowData);
					this.rows.Enqueue(rowData);
				}
			}
			finally
			{
				Dispose();
			}
		}

		public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
		{
			if (this.rows == null)
			{
				return (await this.Inner.NextResultExAsync(cancellationToken).ConfigureAwait(false));
			}

			throw new NotImplementedException();
		}

		public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
		{
			if (this.rows == null)
			{
				return (await this.Inner.ReadExAsync(cancellationToken).ConfigureAwait(false));
			}

			if (this.rows.Count == 0)
			{
				this.currentRow = null;
				return false;
			}

			this.currentRow = this.rows.Dequeue();
			return true;
		}

		public override async Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
		{
			if (this.rows == null)
			{
				return (await this.Inner.GetFieldValueExAsync<T>(ordinal, cancellationToken).ConfigureAwait(false));
			}

			return (T)Convert.ChangeType(this.currentRow[ordinal], typeof (T));
		}

		public override async Task<bool> IsDBNullAsync(int i, CancellationToken cancellationToken)
		{
			if (this.rows == null)
			{
				return (await this.Inner.IsDbNullExAsync(i, cancellationToken).ConfigureAwait(false));
			}

			return this.currentRow[i] == DBNull.Value;
		}
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	// Copyright (c) 2007-2018 Thong Nguyen (tumtumtum@gmail.com)
	using System.Data;
	using System.Threading;
	using System.Data.Common;
	using System.Threading.Tasks;
	using Shaolinq;
	using Shaolinq.Persistence;

	public partial class MarsDbCommand
	{
		public virtual Task<int> ExecuteNonQueryAsync()
		{
			return this.ExecuteNonQueryAsync(CancellationToken.None);
		}

		public async virtual Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
		{
			if (this.context.currentReader != null)
			{
				await this.context.currentReader.BufferAllAsync(cancellationToken).ConfigureAwait(false);
			}

			if (this.Inner is DbCommand dbCommand)
			{
				return (await dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false));
			}
			else
			{
				return base.ExecuteNonQuery();
			}
		}

		public virtual Task<object> ExecuteScalarAsync()
		{
			return this.ExecuteScalarAsync(CancellationToken.None);
		}

		public async virtual Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
		{
			if (this.context.currentReader != null)
			{
				await this.context.currentReader.BufferAllAsync(cancellationToken).ConfigureAwait(false);
			}

			if (this.Inner is DbCommand dbCommand)
			{
				return (await dbCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
			}
			else
			{
				return base.ExecuteScalar();
			}
		}

		public virtual Task<IDataReader> ExecuteReaderAsync()
		{
			return this.ExecuteReaderAsync(CancellationToken.None);
		}

		public async virtual Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
		{
			if (this.context.currentReader != null)
			{
				await this.context.currentReader.BufferAllAsync(cancellationToken).ConfigureAwait(false);
			}

			if (this.Inner is DbCommand dbCommand)
			{
				return new MarsDataReader(this, (await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false)));
			}
			else
			{
				return new MarsDataReader(this, base.ExecuteReader());
			}
		}

		public virtual Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior)
		{
			return this.ExecuteReaderAsync(behavior, CancellationToken.None);
		}

		public async virtual Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
		{
			if (this.context.currentReader != null)
			{
				await this.context.currentReader.BufferAllAsync(cancellationToken).ConfigureAwait(false);
			}

			if (this.Inner is DbCommand dbCommand)
			{
				return new MarsDataReader(this, (await dbCommand.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false)));
			}
			else
			{
				return new MarsDataReader(this, base.ExecuteReader(behavior));
			}
		}
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using Platform;
	using Shaolinq;
	using Shaolinq.Persistence;
	using Shaolinq.Persistence.Linq;
	using Shaolinq.Persistence.Linq.Expressions;

	public abstract partial class SqlTransactionalCommandsContext
	{
		public virtual Task CommitAsync()
		{
			return this.CommitAsync(CancellationToken.None);
		}

		public virtual async Task CommitAsync(CancellationToken cancellationToken)
		{
			try
			{
				if (this.dbTransaction != null)
				{
					await this.dbTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
					this.dbTransaction.Dispose();
					this.dbTransaction = null;
				}
			}
			catch (Exception e)
			{
				var relatedSql = this.SqlDatabaseContext.GetRelatedSql(e);
				var decoratedException = this.SqlDatabaseContext.DecorateException(e, null, relatedSql);
				if (decoratedException != e)
				{
					throw decoratedException;
				}

				throw;
			}
			finally
			{
				CloseConnection();
			}
		}

		public virtual Task RollbackAsync()
		{
			return this.RollbackAsync(CancellationToken.None);
		}

		public virtual async Task RollbackAsync(CancellationToken cancellationToken)
		{
			var context = new DataAccessModelHookRollbackContext(this.TransactionContext);
			try
			{
				await ((IDataAccessModelInternal)this.DataAccessModel).OnHookBeforeRollbackAsync(context, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
			// ignored
			}

			try
			{
				if (this.dbTransaction != null)
				{
					await this.dbTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
					this.dbTransaction.Dispose();
					this.dbTransaction = null;
				}
			}
			finally
			{
				CloseConnection();
				try
				{
					await ((IDataAccessModelInternal)this.DataAccessModel).OnHookAfterRollbackAsync(context, cancellationToken).ConfigureAwait(false);
				}
				catch
				{
				// ignored
				}
			}
		}
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Linq.Expressions;
	using Shaolinq;
	using Shaolinq.Logging;
	using Shaolinq.Persistence;
	using Shaolinq.Persistence.Linq;

	public abstract partial class SqlDatabaseSchemaManager
	{
		public virtual Task CreateDatabaseAndSchemaAsync(DatabaseCreationOptions options)
		{
			return this.CreateDatabaseAndSchemaAsync(options, CancellationToken.None);
		}

		public virtual async Task CreateDatabaseAndSchemaAsync(DatabaseCreationOptions options, CancellationToken cancellationToken)
		{
			var dataDefinitionExpressions = BuildDataDefinitonExpressions(options);
			await CreateDatabaseOnlyAsync(dataDefinitionExpressions, options, cancellationToken).ConfigureAwait(false);
			await CreateDatabaseSchemaAsync(dataDefinitionExpressions, options, cancellationToken).ConfigureAwait(false);
		}

		protected abstract Task<bool> CreateDatabaseOnlyAsync(Expression dataDefinitionExpressions, DatabaseCreationOptions options);
		protected abstract Task<bool> CreateDatabaseOnlyAsync(Expression dataDefinitionExpressions, DatabaseCreationOptions options, CancellationToken cancellationToken);
		protected virtual Task CreateDatabaseSchemaAsync(Expression dataDefinitionExpressions, DatabaseCreationOptions options)
		{
			return this.CreateDatabaseSchemaAsync(dataDefinitionExpressions, options, CancellationToken.None);
		}

		protected virtual async Task CreateDatabaseSchemaAsync(Expression dataDefinitionExpressions, DatabaseCreationOptions options, CancellationToken cancellationToken)
		{
			using (var scope = new DataAccessScope())
			{
				using (var dataTransactionContext = (await this.SqlDatabaseContext.CreateSqlTransactionalCommandsContextAsync(null, cancellationToken).ConfigureAwait(false)))
				{
					using (this.SqlDatabaseContext.AcquireDisabledForeignKeyCheckContext(dataTransactionContext))
					{
						var result = this.SqlDatabaseContext.SqlQueryFormatterManager.Format(dataDefinitionExpressions, SqlQueryFormatterOptions.Default | SqlQueryFormatterOptions.EvaluateConstants);
						using (var command = dataTransactionContext.CreateCommand(SqlCreateCommandOptions.Default | SqlCreateCommandOptions.UnpreparedExecute))
						{
							command.CommandText = result.CommandText;
							Logger.Info(command.CommandText);
							command.ExecuteNonQuery();
						}
					}

					await dataTransactionContext.CommitAsync(cancellationToken).ConfigureAwait(false);
				}

				await scope.CompleteAsync(cancellationToken).ConfigureAwait(false);
			}
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Linq;
	using System.Threading;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Linq.Expressions;
	using Platform;
	using Shaolinq.Persistence;
	using Shaolinq.TypeBuilding;
	using Shaolinq.Persistence.Linq;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;
	using global::Shaolinq.TypeBuilding;
	using global::Shaolinq.Persistence.Linq;

	public static partial class QueryableExtensions
	{
		public static Task<bool> AnyAsync<T>(this IQueryable<T> source)
		{
			return AnyAsync<T>(source, CancellationToken.None);
		}

		public static async Task<bool> AnyAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Any<T>(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<bool>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<bool> AnyAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
		{
			return AnyAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<bool> AnyAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Any<T>(default (IQueryable<T>))), Expression.Call(MethodInfoFastRef.QueryableWhereMethod.MakeGenericMethod(typeof (T)), source.Expression, Expression.Quote(predicate)));
			return (await source.Provider.ExecuteAsync<bool>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<bool> AllAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
		{
			return AllAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<bool> AllAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.All<T>(default (IQueryable<T>), default (Expression<Func<T, bool>>))), source.Expression, Expression.Quote(predicate));
			return (await source.Provider.ExecuteAsync<bool>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> FirstAsync<T>(this IQueryable<T> source)
		{
			return FirstAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> FirstAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.First<T>(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> FirstAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
		{
			return FirstAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<T> FirstAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.First<T>(default (IQueryable<T>))), Expression.Call(MethodInfoFastRef.QueryableWhereMethod.MakeGenericMethod(typeof (T)), source.Expression, Expression.Quote(predicate)));
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source)
		{
			return FirstOrDefaultAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.FirstOrDefault<T>(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
		{
			return FirstOrDefaultAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.FirstOrDefault<T>(default (IQueryable<T>))), Expression.Call(MethodInfoFastRef.QueryableWhereMethod.MakeGenericMethod(typeof (T)), source.Expression, Expression.Quote(predicate)));
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> SingleAsync<T>(this IQueryable<T> source)
		{
			return SingleAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> SingleAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Single<T>(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> SingleAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
		{
			return SingleAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<T> SingleAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Single<T>(default (IQueryable<T>))), Expression.Call(MethodInfoFastRef.QueryableWhereMethod.MakeGenericMethod(typeof (T)), source.Expression, Expression.Quote(predicate)));
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source)
		{
			return SingleOrDefaultAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.SingleOrDefault<T>(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
		{
			return SingleOrDefaultAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.SingleOrDefault<T>(default (IQueryable<T>))), Expression.Call(MethodInfoFastRef.QueryableWhereMethod.MakeGenericMethod(typeof (T)), source.Expression, Expression.Quote(predicate)));
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> DeleteAsync<T>(this IQueryable<T> source)where T : DataAccessObject
		{
			return DeleteAsync<T>(source, CancellationToken.None);
		}

		public static async Task<int> DeleteAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)where T : DataAccessObject
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Delete<T>(default (IQueryable<T>))), source.Expression);
			await ((SqlQueryProvider)source.Provider).DataAccessModel.FlushAsync(cancellationToken).ConfigureAwait(false);
			return (await source.Provider.ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> DeleteAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)where T : DataAccessObject
		{
			return DeleteAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<int> DeleteAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)where T : DataAccessObject
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Delete<T>(default (IQueryable<T>))), Expression.Call(MethodInfoFastRef.QueryableWhereMethod.MakeGenericMethod(typeof (T)), source.Expression, Expression.Quote(predicate)));
			await ((SqlQueryProvider)source.Provider).DataAccessModel.FlushAsync(cancellationToken).ConfigureAwait(false);
			return (await source.Provider.ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> CountAsync<T>(this IQueryable<T> source)
		{
			return CountAsync<T>(source, CancellationToken.None);
		}

		public static async Task<int> CountAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Count(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> CountAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
		{
			return CountAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<int> CountAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Count<T>(default (IQueryable<T>))), Expression.Call(MethodInfoFastRef.QueryableWhereMethod.MakeGenericMethod(typeof (T)), source.Expression, Expression.Quote(predicate)));
			return (await source.Provider.ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long> LongCountAsync<T>(this IQueryable<T> source)
		{
			return LongCountAsync<T>(source, CancellationToken.None);
		}

		public static async Task<long> LongCountAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.LongCount(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<long>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long> LongCountAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
		{
			return LongCountAsync<T>(source, predicate, CancellationToken.None);
		}

		public static async Task<long> LongCountAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.LongCount<T>(default (IQueryable<T>))), Expression.Call(MethodInfoFastRef.QueryableWhereMethod.MakeGenericMethod(typeof (T)), source.Expression, Expression.Quote(predicate)));
			return (await source.Provider.ExecuteAsync<long>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> MinAsync<T>(this IQueryable<T> source)
		{
			return MinAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> MinAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Min(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<T> MaxAsync<T>(this IQueryable<T> source)
		{
			return MaxAsync<T>(source, CancellationToken.None);
		}

		public static async Task<T> MaxAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Max(default (IQueryable<T>))), source.Expression);
			return (await source.Provider.ExecuteAsync<T>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<U> MinAsync<T, U>(this IQueryable<T> source, Expression<Func<T, U>> selector)
		{
			return MinAsync<T, U>(source, selector, CancellationToken.None);
		}

		public static async Task<U> MinAsync<T, U>(this IQueryable<T> source, Expression<Func<T, U>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Min(default (IQueryable<T>), c => default (U))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<U>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<U> MaxAsync<T, U>(this IQueryable<T> source, Expression<Func<T, U>> selector)
		{
			return MaxAsync<T, U>(source, selector, CancellationToken.None);
		}

		public static async Task<U> MaxAsync<T, U>(this IQueryable<T> source, Expression<Func<T, U>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Max(default (IQueryable<T>), c => default (U))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<U>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> SumAsync(this IQueryable<int> source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<int> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<int>))), source.Expression);
			return (await source.Provider.ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int? > SumAsync(this IQueryable<int? > source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<int? > SumAsync(this IQueryable<int? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<int? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<int? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long> SumAsync(this IQueryable<long> source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<long> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<long>))), source.Expression);
			return (await source.Provider.ExecuteAsync<long>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long? > SumAsync(this IQueryable<long? > source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<long? > SumAsync(this IQueryable<long? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<long? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<long? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<float> SumAsync(this IQueryable<float> source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<float>))), source.Expression);
			return (await source.Provider.ExecuteAsync<float>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<float? > SumAsync(this IQueryable<float? > source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<float? > SumAsync(this IQueryable<float? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<float? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<float? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<double> SumAsync(this IQueryable<double> source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<double> SumAsync(this IQueryable<double> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<double>))), source.Expression);
			return (await source.Provider.ExecuteAsync<double>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<double? > SumAsync(this IQueryable<double? > source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<double? > SumAsync(this IQueryable<double? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<double? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<double? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<decimal> SumAsync(this IQueryable<decimal> source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<decimal>))), source.Expression);
			return (await source.Provider.ExecuteAsync<decimal>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<decimal? > SumAsync(this IQueryable<decimal? > source)
		{
			return SumAsync(source, CancellationToken.None);
		}

		public static async Task<decimal? > SumAsync(this IQueryable<decimal? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<decimal? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<decimal? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, int>> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<int> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, int>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (int))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int? > SumAsync<T>(this IQueryable<T> source, Expression<Func<T, int? >> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<int? > SumAsync<T>(this IQueryable<T> source, Expression<Func<T, int? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (int? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<int? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, long>> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<long> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, long>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (long))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<long>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long? > SumAsync<T>(this IQueryable<T> source, Expression<Func<T, long? >> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<long? > SumAsync<T>(this IQueryable<T> source, Expression<Func<T, long? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (long? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<long? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<float> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, float>> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<float> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, float>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (float))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<float>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<float? > SumAsync<T>(this IQueryable<T> source, Expression<Func<T, float? >> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<float? > SumAsync<T>(this IQueryable<T> source, Expression<Func<T, float? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (float? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<float? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<double> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, double>> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<double> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, double>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (double))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<double>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<double? > SumAsync<T>(this IQueryable<double? > source, Expression<Func<T, double? >> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<double? > SumAsync<T>(this IQueryable<double? > source, Expression<Func<T, double? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (double? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<double? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<decimal> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, decimal>> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<decimal> SumAsync<T>(this IQueryable<T> source, Expression<Func<T, decimal>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (decimal))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<decimal>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<decimal? > SumAsync<T>(this IQueryable<decimal? > source, Expression<Func<T, decimal? >> selector)
		{
			return SumAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<decimal? > SumAsync<T>(this IQueryable<decimal? > source, Expression<Func<T, decimal? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Sum(default (IQueryable<T>), c => default (decimal? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<decimal? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> AverageAsync(this IQueryable<int> source)
		{
			return AverageAsync(source, CancellationToken.None);
		}

		public static async Task<int> AverageAsync(this IQueryable<int> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<int>))), source.Expression);
			return (await source.Provider.ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int? > AverageAsync(this IQueryable<int? > source)
		{
			return AverageAsync(source, CancellationToken.None);
		}

		public static async Task<int? > AverageAsync(this IQueryable<int? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<int? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<int? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long> AverageAsync(this IQueryable<long> source)
		{
			return AverageAsync(source, CancellationToken.None);
		}

		public static async Task<long> AverageAsync(this IQueryable<long> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<long>))), source.Expression);
			return (await source.Provider.ExecuteAsync<long>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long? > AverageAsync(this IQueryable<long? > source)
		{
			return AverageAsync(source, CancellationToken.None);
		}

		public static async Task<long? > AverageAsync(this IQueryable<long? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<long? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<long? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<float> AverageAsync(this IQueryable<float> source)
		{
			return AverageAsync(source, CancellationToken.None);
		}

		public static async Task<float> AverageAsync(this IQueryable<float> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<float>))), source.Expression);
			return (await source.Provider.ExecuteAsync<float>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<float? > AverageAsync(this IQueryable<float? > source)
		{
			return AverageAsync(source, CancellationToken.None);
		}

		public static async Task<float? > AverageAsync(this IQueryable<float? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<float? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<float? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<double> AverageAsync(this IQueryable<double> source)
		{
			return AverageAsync(source, CancellationToken.None);
		}

		public static async Task<double> AverageAsync(this IQueryable<double> source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<double>))), source.Expression);
			return (await source.Provider.ExecuteAsync<double>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<double? > AverageAsync(this IQueryable<double? > source)
		{
			return AverageAsync(source, CancellationToken.None);
		}

		public static async Task<double? > AverageAsync(this IQueryable<double? > source, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<double? >))), source.Expression);
			return (await source.Provider.ExecuteAsync<double? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int> AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, int>> selector)
		{
			return AverageAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<int> AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, int>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<T>), c => default (int))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<int>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<int? > AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, int? >> selector)
		{
			return AverageAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<int? > AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, int? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<T>), c => default (int? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<int? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long> AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, long>> selector)
		{
			return AverageAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<long> AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, long>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<T>), c => default (long))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<long>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<long? > AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, long? >> selector)
		{
			return AverageAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<long? > AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, long? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<T>), c => default (long? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<long? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<float> AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, float>> selector)
		{
			return AverageAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<float> AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, float>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<T>), c => default (float))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<float>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<float? > AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, float? >> selector)
		{
			return AverageAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<float? > AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, float? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<T>), c => default (float? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<float? >(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<double> AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, double>> selector)
		{
			return AverageAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<double> AverageAsync<T>(this IQueryable<T> source, Expression<Func<T, double>> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<T>), c => default (double))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<double>(expression, cancellationToken).ConfigureAwait(false));
		}

		public static Task<double? > AverageAsync<T>(this IQueryable<double? > source, Expression<Func<T, double? >> selector)
		{
			return AverageAsync<T>(source, selector, CancellationToken.None);
		}

		public static async Task<double? > AverageAsync<T>(this IQueryable<double? > source, Expression<Func<T, double? >> selector, CancellationToken cancellationToken)
		{
			Expression expression = Expression.Call(TypeUtils.GetMethod(() => Queryable.Average(default (IQueryable<T>), c => default (double? ))), source.Expression, Expression.Quote(selector));
			return (await source.Provider.ExecuteAsync<double? >(expression, cancellationToken).ConfigureAwait(false));
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Linq.Expressions;
	using System.Collections.Generic;
	using Platform;
	using Shaolinq.Logging;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Logging;
	using global::Shaolinq.Persistence;

	public partial class DataAccessObjectDataContext
	{
		public virtual Task CommitAsync(SqlTransactionalCommandsContext commandsContext, bool forFlush)
		{
			return this.CommitAsync(commandsContext, forFlush, CancellationToken.None);
		}

		public virtual async Task CommitAsync(SqlTransactionalCommandsContext commandsContext, bool forFlush, CancellationToken cancellationToken)
		{
			foreach (var cache in this.cachesByType)
			{
				cache.Value.AssertObjectsAreReadyForCommit();
			}

			var context = new DataAccessModelHookSubmitContext(commandsContext.TransactionContext, this, forFlush);
			try
			{
				await ((IDataAccessModelInternal)this.DataAccessModel).OnHookBeforeSubmitAsync(context, cancellationToken).ConfigureAwait(false);
				this.isCommiting = true;
				await CommitNewAsync(commandsContext, cancellationToken).ConfigureAwait(false);
				await CommitUpdatedAsync(commandsContext, cancellationToken).ConfigureAwait(false);
				await CommitDeletedAsync(commandsContext, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				context.Exception = e;
				throw;
			}
			finally
			{
				await ((IDataAccessModelInternal)this.DataAccessModel).OnHookAfterSubmitAsync(context, cancellationToken).ConfigureAwait(false);
				this.isCommiting = false;
			}

			foreach (var cache in this.cachesByType)
			{
				cache.Value.ProcessAfterCommit();
			}
		}

		private static Task CommitDeletedAsync(SqlTransactionalCommandsContext commandsContext, IObjectsByIdCache cache)
		{
			return CommitDeletedAsync(commandsContext, cache, CancellationToken.None);
		}

		private static async Task CommitDeletedAsync(SqlTransactionalCommandsContext commandsContext, IObjectsByIdCache cache, CancellationToken cancellationToken)
		{
			await commandsContext.DeleteAsync(cache.Type, cache.GetDeletedObjects(), cancellationToken).ConfigureAwait(false);
		}

		private Task CommitDeletedAsync(SqlTransactionalCommandsContext commandsContext)
		{
			return this.CommitDeletedAsync(commandsContext, CancellationToken.None);
		}

		private async Task CommitDeletedAsync(SqlTransactionalCommandsContext commandsContext, CancellationToken cancellationToken)
		{
			foreach (var cache in this.cachesByType)
			{
				await CommitDeletedAsync(commandsContext, cache.Value, cancellationToken).ConfigureAwait(false);
			}
		}

		private static Task CommitUpdatedAsync(SqlTransactionalCommandsContext commandsContext, IObjectsByIdCache cache)
		{
			return CommitUpdatedAsync(commandsContext, cache, CancellationToken.None);
		}

		private static async Task CommitUpdatedAsync(SqlTransactionalCommandsContext commandsContext, IObjectsByIdCache cache, CancellationToken cancellationToken)
		{
			await commandsContext.UpdateAsync(cache.Type, cache.GetObjectsById(), cancellationToken).ConfigureAwait(false);
			await commandsContext.UpdateAsync(cache.Type, cache.GetObjectsByPredicate(), cancellationToken).ConfigureAwait(false);
		}

		private Task CommitUpdatedAsync(SqlTransactionalCommandsContext commandsContext)
		{
			return this.CommitUpdatedAsync(commandsContext, CancellationToken.None);
		}

		private async Task CommitUpdatedAsync(SqlTransactionalCommandsContext commandsContext, CancellationToken cancellationToken)
		{
			foreach (var cache in this.cachesByType)
			{
				await CommitUpdatedAsync(commandsContext, cache.Value, cancellationToken).ConfigureAwait(false);
			}
		}

		private static Task CommitNewPhase1Async(SqlTransactionalCommandsContext commandsContext, IObjectsByIdCache cache, Dictionary<TypeAndTransactionalCommandsContext, InsertResults> insertResultsByType, Dictionary<TypeAndTransactionalCommandsContext, IReadOnlyList<DataAccessObject>> fixups)
		{
			return CommitNewPhase1Async(commandsContext, cache, insertResultsByType, fixups, CancellationToken.None);
		}

		private static async Task CommitNewPhase1Async(SqlTransactionalCommandsContext commandsContext, IObjectsByIdCache cache, Dictionary<TypeAndTransactionalCommandsContext, InsertResults> insertResultsByType, Dictionary<TypeAndTransactionalCommandsContext, IReadOnlyList<DataAccessObject>> fixups, CancellationToken cancellationToken)
		{
			var key = new TypeAndTransactionalCommandsContext(cache.Type, commandsContext);
			var currentInsertResults = (await commandsContext.InsertAsync(cache.Type, cache.GetNewObjects(), cancellationToken).ConfigureAwait(false));
			if (currentInsertResults.ToRetry.Count > 0)
			{
				insertResultsByType[key] = currentInsertResults;
			}

			if (currentInsertResults.ToFixUp.Count > 0)
			{
				fixups[key] = currentInsertResults.ToFixUp;
			}
		}

		private Task CommitNewAsync(SqlTransactionalCommandsContext commandsContext)
		{
			return this.CommitNewAsync(commandsContext, CancellationToken.None);
		}

		private async Task CommitNewAsync(SqlTransactionalCommandsContext commandsContext, CancellationToken cancellationToken)
		{
			var insertResultsByType = new Dictionary<TypeAndTransactionalCommandsContext, InsertResults>();
			var fixups = new Dictionary<TypeAndTransactionalCommandsContext, IReadOnlyList<DataAccessObject>>();
			foreach (var value in this.cachesByType.Values)
			{
				await CommitNewPhase1Async(commandsContext, value, insertResultsByType, fixups, cancellationToken).ConfigureAwait(false);
			}

			var currentInsertResultsByType = insertResultsByType;
			var newInsertResultsByType = new Dictionary<TypeAndTransactionalCommandsContext, InsertResults>();
			while (true)
			{
				var didRetry = false;
				// Perform the retry list
				foreach (var i in currentInsertResultsByType)
				{
					var type = i.Key.Type;
					var persistenceTransactionContext = i.Key.CommandsContext;
					var retryListForType = i.Value.ToRetry;
					if (retryListForType.Count == 0)
					{
						continue;
					}

					didRetry = true;
					newInsertResultsByType[new TypeAndTransactionalCommandsContext(type, persistenceTransactionContext)] = (await persistenceTransactionContext.InsertAsync(type, retryListForType, cancellationToken).ConfigureAwait(false));
				}

				if (!didRetry)
				{
					break;
				}

				MathUtils.Swap(ref currentInsertResultsByType, ref newInsertResultsByType);
				newInsertResultsByType.Clear();
			}

			// Perform fixups
			foreach (var i in fixups)
			{
				var type = i.Key.Type;
				var databaseTransactionContext = i.Key.CommandsContext;
				await databaseTransactionContext.UpdateAsync(type, i.Value, cancellationToken).ConfigureAwait(false);
			}
		}
	}
}

namespace Shaolinq.Persistence
{
#pragma warning disable
	using System;
	using System.Data;
	using System.Linq;
	using System.Threading;
	using System.Reflection;
	using System.Data.Common;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using Platform;
	using Shaolinq;
	using Shaolinq.Persistence;
	using Shaolinq.Persistence.Linq;

	public abstract partial class SqlDatabaseContext
	{
		public Task<SqlTransactionalCommandsContext> CreateSqlTransactionalCommandsContextAsync(TransactionContext transactionContext)
		{
			return this.CreateSqlTransactionalCommandsContextAsync(transactionContext, CancellationToken.None);
		}

		public async Task<SqlTransactionalCommandsContext> CreateSqlTransactionalCommandsContextAsync(TransactionContext transactionContext, CancellationToken cancellationToken)
		{
			var connection = (await OpenConnectionAsync(cancellationToken).ConfigureAwait(false));
			try
			{
				return CreateSqlTransactionalCommandsContext(connection, transactionContext);
			}
			catch
			{
				ActionUtils.IgnoreExceptions(() => connection.Dispose());
				throw;
			}
		}

		public virtual Task<IDbConnection> OpenConnectionAsync()
		{
			return this.OpenConnectionAsync(CancellationToken.None);
		}

		public virtual async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
		{
			if (this.dbProviderFactory == null)
			{
				this.dbProviderFactory = CreateDbProviderFactory();
			}

			var retval = this.dbProviderFactory.CreateConnection();
			retval.ConnectionString = this.ConnectionString;
			await retval.OpenAsync(cancellationToken).ConfigureAwait(false);
			return retval;
		}

		public virtual Task<IDbConnection> OpenServerConnectionAsync()
		{
			return this.OpenServerConnectionAsync(CancellationToken.None);
		}

		public virtual async Task<IDbConnection> OpenServerConnectionAsync(CancellationToken cancellationToken)
		{
			if (this.dbProviderFactory == null)
			{
				this.dbProviderFactory = CreateDbProviderFactory();
			}

			var retval = this.dbProviderFactory.CreateConnection();
			retval.ConnectionString = this.ServerConnectionString;
			await retval.OpenAsync(cancellationToken).ConfigureAwait(false);
			return retval;
		}

		public virtual Task BackupAsync(SqlDatabaseContext sqlDatabaseContext)
		{
			return this.BackupAsync(sqlDatabaseContext, CancellationToken.None);
		}

		public virtual async Task BackupAsync(SqlDatabaseContext sqlDatabaseContext, CancellationToken cancellationToken)
		{
			throw new NotSupportedException();
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Linq;
	using System.Threading;
	using System.Reflection;
	using System.Diagnostics;
	using System.Configuration;
	using System.Threading.Tasks;
	using System.Linq.Expressions;
	using System.Collections.Generic;
	using Platform;
	using Shaolinq.Analytics;
	using Shaolinq.Persistence;
	using Shaolinq.TypeBuilding;
	using Shaolinq.Persistence.Linq;
	using Shaolinq.Persistence.Linq.Optimizers;
	using global::Shaolinq;
	using global::Shaolinq.Analytics;
	using global::Shaolinq.Persistence;
	using global::Shaolinq.TypeBuilding;
	using global::Shaolinq.Persistence.Linq;
	using global::Shaolinq.Persistence.Linq.Optimizers;

	public partial class DataAccessModel
	{
		public virtual Task CreateAsync(DatabaseCreationOptions options)
		{
			return this.CreateAsync(options, CancellationToken.None);
		}

		public virtual async Task CreateAsync(DatabaseCreationOptions options, CancellationToken cancellationToken)
		{
			using (var scope = new DataAccessScope(DataAccessIsolationLevel.Unspecified, DataAccessScopeOptions.RequiresNew, TimeSpan.Zero))
			{
				await GetCurrentSqlDatabaseContext().SchemaManager.CreateDatabaseAndSchemaAsync(options, cancellationToken).ConfigureAwait(false);
				await scope.CompleteAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public virtual Task FlushAsync()
		{
			return this.FlushAsync(CancellationToken.None);
		}

		public virtual async Task FlushAsync(CancellationToken cancellationToken)
		{
			var transactionContext = GetCurrentContext(true);
			if (transactionContext != null)
			{
				await transactionContext.GetCurrentDataContext().CommitAsync((await transactionContext.GetSqlTransactionalCommandsContextAsync(cancellationToken).ConfigureAwait(false)), true, cancellationToken).ConfigureAwait(false);
			}
		}

		public virtual Task BackupAsync(DataAccessModel dataAccessModel)
		{
			return this.BackupAsync(dataAccessModel, CancellationToken.None);
		}

		public virtual async Task BackupAsync(DataAccessModel dataAccessModel, CancellationToken cancellationToken)
		{
			if (dataAccessModel == this)
			{
				throw new InvalidOperationException("Cannot backup to self");
			}

			await GetCurrentSqlDatabaseContext().BackupAsync(dataAccessModel.GetCurrentSqlDatabaseContext(), cancellationToken).ConfigureAwait(false);
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Transactions;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using Platform;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	public partial class TransactionContext
	{
		public virtual Task<SqlTransactionalCommandsContext> GetSqlTransactionalCommandsContextAsync()
		{
			return this.GetSqlTransactionalCommandsContextAsync(CancellationToken.None);
		}

		public virtual async Task<SqlTransactionalCommandsContext> GetSqlTransactionalCommandsContextAsync(CancellationToken cancellationToken)
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(nameof(TransactionContext));
			}

			return this.commandsContext ?? (this.commandsContext = (await GetSqlDatabaseContext().CreateSqlTransactionalCommandsContextAsync(this, cancellationToken).ConfigureAwait(false)));
		}

		public Task CommitAsync()
		{
			return this.CommitAsync(CancellationToken.None);
		}

		public async Task CommitAsync(CancellationToken cancellationToken)
		{
			if (this.disposed)
			{
				return;
			}

			try
			{
				if (this.dataAccessObjectDataContext != null)
				{
					this.commandsContext = (await GetSqlTransactionalCommandsContextAsync(cancellationToken).ConfigureAwait(false));
					await this.dataAccessObjectDataContext.CommitAsync(this.commandsContext, false, cancellationToken).ConfigureAwait(false);
					await this.commandsContext.CommitAsync(cancellationToken).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				ActionUtils.IgnoreExceptions(() => this.commandsContext?.Rollback());
				throw new DataAccessTransactionAbortedException(e);
			}
			finally
			{
				Dispose();
			}
		}

		internal Task RollbackAsync()
		{
			return this.RollbackAsync(CancellationToken.None);
		}

		internal async Task RollbackAsync(CancellationToken cancellationToken)
		{
			if (this.disposed)
			{
				return;
			}

			try
			{
				ActionUtils.IgnoreExceptions(() => this.commandsContext?.Rollback());
			}
			finally
			{
				Dispose();
			}
		}
	}
}

namespace Shaolinq
{
#pragma warning disable
	using System;
	using System.Threading;
	using System.Transactions;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using Shaolinq.Persistence;
	using global::Shaolinq;
	using global::Shaolinq.Persistence;

	public static partial class TransactionScopeExtensions
	{
		public static Task SaveAsync(this TransactionScope scope)
		{
			return SaveAsync(scope, CancellationToken.None);
		}

		public static async Task SaveAsync(this TransactionScope scope, CancellationToken cancellationToken)
		{
			if (DataAccessTransaction.Current == null)
			{
				return;
			}

			foreach (var dataAccessModel in DataAccessTransaction.Current.ParticipatingDataAccessModels)
			{
				if (!dataAccessModel.IsDisposed)
				{
					await dataAccessModel.FlushAsync(cancellationToken).ConfigureAwait(false);
				}
			}
		}

		public static Task SaveAsync(this TransactionScope scope, DataAccessModel dataAccessModel)
		{
			return SaveAsync(scope, dataAccessModel, CancellationToken.None);
		}

		public static async Task SaveAsync(this TransactionScope scope, DataAccessModel dataAccessModel, CancellationToken cancellationToken)
		{
			if (!dataAccessModel.IsDisposed)
			{
				await dataAccessModel.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public static Task FlushAsync(this TransactionScope scope)
		{
			return FlushAsync(scope, CancellationToken.None);
		}

		public static async Task FlushAsync(this TransactionScope scope, CancellationToken cancellationToken)
		{
			await scope.SaveAsync(cancellationToken).ConfigureAwait(false);
		}

		public static Task FlushAsync(this TransactionScope scope, DataAccessModel dataAccessModel)
		{
			return FlushAsync(scope, dataAccessModel, CancellationToken.None);
		}

		public static async Task FlushAsync(this TransactionScope scope, DataAccessModel dataAccessModel, CancellationToken cancellationToken)
		{
			await scope.SaveAsync(dataAccessModel, cancellationToken).ConfigureAwait(false);
		}
	}
}