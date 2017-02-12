using System;
using ServiceStack.DataAnnotations;
using System.Collections.Generic;
using System.Linq;


namespace EntryPoint
{
	public class Metadata : Entity, IOwnable
	{
		public string Description { get; set; }
		public bool IsUploaded { get; set; }
		public DateTime CreatedAt { get; set; }
		public int OwnerId { get; set; }

		public virtual bool CanDelete(List<Task> inProgressTasks)
		{
			return false;
		}
	}

	public class InputFileMetadata : Metadata
	{
		public override bool CanDelete(List<Task> inProgressTasks)
		{
			var inputFileIds = inProgressTasks.SelectMany(entity => entity.InputFileIds);
			return !inputFileIds.Contains(Id);
		}
	}
}
