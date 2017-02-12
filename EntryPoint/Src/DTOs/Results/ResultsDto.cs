using System;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	[Route("/api/tasks/{TaskId}/results", "GET")]
	public class ResultsDto : IReturn<ResultsDtoResponse>
	{
		public int TaskId { get; set; }
	}
}
