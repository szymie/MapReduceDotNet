using System;

namespace MapReduceDotNetLib
{
	public class NewWorkMessage
	{
		public NewWorkMessage (int workConfigId, WorkConfig workConfig)
		{
			this.OrderedWorkId = workConfigId;
			this.WorkConfig = workConfig;
		}
		
		public int OrderedWorkId{ get; set; }
		public WorkConfig WorkConfig { get; set; }
	}
}

