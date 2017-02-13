using System;

namespace Worker
{
	public class NewWorkerMessage
	{
		public NewWorkerMessage (WorkerConfig workerConfig)
		{
			this.WorkerConfig = workerConfig;
		}
		
		public WorkerConfig WorkerConfig{get;set;}
	}
}

