using System;
using ServiceStack.DataAnnotations;

namespace EntryPoint
{
	public class Metadata : Entity
	{
		public string Description { get; set; }
		public bool IsUploaded { get; set; }
		public DateTime CreatedAt { get; set; }
		public int OwnerId { get; set; }
	}

	public class InputFileMetadata : Metadata
	{
	}
}
