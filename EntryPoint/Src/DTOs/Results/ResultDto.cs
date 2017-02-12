using System;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	[Route("/api/tasks/{TaskId}/results/{Id}", "GET")]
	public class ResultDto : IReturn<ResultsDtoResponse>
	{
		public int Id { get; set; }
		public int TaskId { get; set; }
	}
}
