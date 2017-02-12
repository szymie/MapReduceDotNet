using System;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	[Route("/api/tasks/{TaskId}/status", "GET")]
	public class StatusDto : IReturn<StatusDtoResponse>
	{
		public int TaskId { get; set; }
	}
}