using System;
using ServiceStack.ServiceHost;
using System.Collections.Generic;

namespace EntryPoint
{
	[Route("/api/tasks", "GET,POST")]
	[Route("/api/tasks/{Id}", "GET")]
	public class TaskDto
	{
		public int? Id { get; set; }
		public string Status { get; set; }
		public int AssemblyId { get; set; }
		public IList<int> InputFileIds { get; set; }
		public int M { get; set; }
		public int R { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
