using System;
using System.Collections.Generic;

namespace EntryPoint
{
	public class NewTaskRequestMessage
	{
		public int TaskId { get; set; }
		public AssemblyMetadata Assembly { get; set; }
		public IList<int> InputFileIds { get; set; }
		public int M { get; set; }
		public int R { get; set; }
		public string Username { get; set; }
		public int UserId { get; set; }
	}
}

