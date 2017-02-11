using System;

namespace MapReduceDotNetLib
{
	public class RegisterCoordinatorAckMessage
	{
		public int CoordinatorId{get;set;}

		public RegisterCoordinatorAckMessage (int coordinatorId)
		{
			this.CoordinatorId = coordinatorId;
		}
	}
}

