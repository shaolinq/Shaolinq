// Copyright (c) 2007-2018 Thong Nguyen (tumtumtum@gmail.com)

using System.Reflection;
using Platform;

namespace Shaolinq.TypeBuilding
{
	public static class PropertyInfoFastRef
	{
		public static readonly PropertyInfo DataAccessObjectObjectState = TypeUtils.GetProperty<IDataAccessObjectAdvanced>(c => c.ObjectState);
		public static readonly PropertyInfo DataAccessObjectIsCommitted = TypeUtils.GetProperty<IDataAccessObjectAdvanced>(c => c.IsCommitted);
		public static readonly PropertyInfo DataAccessObjectIsNewProperty = TypeUtils.GetProperty<IDataAccessObjectAdvanced>(c => c.IsNew);
		public static readonly PropertyInfo DataAccessObjectIsMissingAnyDirectOrIndirectServerSideGeneratedPrimaryKeys = TypeUtils.GetProperty<IDataAccessObjectAdvanced>(c => c.IsMissingAnyDirectOrIndirectServerSideGeneratedPrimaryKeys);
		public static readonly PropertyInfo ObjectPropertyValueValueProperty = TypeUtils.GetProperty<ObjectPropertyValue>(c => c.Value);
	}
}
