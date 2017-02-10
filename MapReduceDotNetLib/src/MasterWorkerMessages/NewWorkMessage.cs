using System;

namespace MapReduceDotNetLib
{
	public class NewWorkMessage
	{
		public NewWorkMessage (int workConfigId, WorkConfig workConfig)
		{
			this.WorkConfigId = workConfigId;
			this.WorkConfig = workConfig;
		}
		
		public int WorkConfigId{ get; set; }
		public WorkConfig WorkConfig { get; set; }
	}
}

