using System;
using ServiceStack.DataAnnotations;

namespace EntryPoint
{
	public class InputFileMetadata
	{
		[AutoIncrement]
		public int Id { get; set; }
		public string Description { get; set; }

		public DateTime CreatedAt { get; set; }
		public int OwnerId { get; set; }
	}
}
