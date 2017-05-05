﻿using System;

namespace Shaolinq.Tests.SqlServerClusteredIndexes
{
	[DataAccessObject]
	public class Directory : DataAccessObject<Guid>
	{
		[OrganizationIndex(CompositeOrder = 2)]
		public override Guid Id { get; set; }

		[PersistedMember]
		[OrganizationIndex(CompositeOrder = 1)]
		public virtual string Name { get; set; }
	}
}