// Copyright (c) 2007-2018 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections;
using System.Data;

namespace Shaolinq.Persistence.Linq
{
	internal partial class ObjectProjectionAsyncEnumerator<T, C>
		: IAsyncEnumerator<T>
		where C : class
	{
		private int state;
		private bool disposed;
		private C context;
		private IDataReader dataReader;
		private readonly ObjectProjector<T, C> objectProjector;
		private TransactionContext.TransactionExecutionContext transactionExecutionContextAcquisition;
		private ExecuteReaderContext executeReaderContext;

		public ObjectProjectionAsyncEnumerator(ObjectProjector<T, C> objectProjector)
		{
			this.objectProjector = objectProjector;

			try
			{
				this.transactionExecutionContextAcquisition = TransactionContext
					.Acquire(this.objectProjector.DataAccessModel, false);
			}
			catch
			{
				Dispose();

				throw;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(nameof(ObjectProjectionAsyncEnumerator<T, C>));
			}

			this.disposed = true;

			Close();
		}

		private void Close()
		{
			// ReSharper disable EmptyGeneralCatchClause
			try { this.executeReaderContext?.Dispose(); } catch { }
			try { this.transactionExecutionContextAcquisition?.Dispose(); } catch { }
			this.dataReader = null;
			this.transactionExecutionContextAcquisition = null;
			// ReSharper restore EmptyGeneralCatchClause
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		object IEnumerator.Current => this.Current;
		public virtual T Current { get; private set; }

		[RewriteAsync]
		public virtual bool MoveNext()
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
			var commandsContext = this.transactionExecutionContextAcquisition.TransactionContext.GetSqlTransactionalCommandsContext();
			this.executeReaderContext = commandsContext.ExecuteReader(this.objectProjector.CommandText, this.objectProjector.ParameterValues);
			this.dataReader = this.executeReaderContext.DataReader;
			this.context = this.objectProjector.CreateEnumerationContext(this.dataReader, this.transactionExecutionContextAcquisition.Version);

state1:
			T result;

			if (this.dataReader.ReadEx())
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