using System;
using System.Collections.Generic;

namespace EntryPoint
{
	public class ResultsDtoResponse
	{
		public IList<ResultMetadata> Entities { get; set; }

		public ResultsDtoResponse(IList<ResultMetadata> entities)
		{
			Entities = entities;
		}
	}
}
