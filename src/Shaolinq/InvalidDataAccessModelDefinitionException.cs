﻿// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;

namespace Shaolinq
{
	public class InvalidDataAccessModelDefinitionException
		: Exception
	{
		public InvalidDataAccessModelDefinitionException()
		{
		}

		public InvalidDataAccessModelDefinitionException(string message)
			: base(message)
		{
		}
	}
}
