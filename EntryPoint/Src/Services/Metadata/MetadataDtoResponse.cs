using System;
using System.Collections.Generic;

namespace EntryPoint
{
	public class MetadataDtoResponse<T>
	{
		public IList<T> Entities { get; set; }

		public MetadataDtoResponse(IList<T> entities)
		{
			Entities = entities;
		}
	}
}
