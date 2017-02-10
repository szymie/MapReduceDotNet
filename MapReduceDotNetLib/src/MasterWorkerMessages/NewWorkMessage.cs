using System;

namespace MapReduceDotNetLib
{
	public class NewWorkMessage
	{
		public NewWorkMessage (int workConfigId, WorkerConfig workerConfig)
		{
			this.WorkConfigId = workConfigId;
			this.WorkerConfig = workerConfig;
		}
		
		public int WorkConfigId{ get; set; }
		public WorkerConfig WorkerConfig { get; set; }
	}
}

