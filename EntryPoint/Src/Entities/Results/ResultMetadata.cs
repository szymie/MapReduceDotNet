using System;
using System.Collections.Generic;

namespace EntryPoint
{
	public class ResultMetadata : Entity
	{
		public List<string> Keys { get; set; }
		public string Name { get; set; }
		public int TaskId { get; set; }
	}
}
