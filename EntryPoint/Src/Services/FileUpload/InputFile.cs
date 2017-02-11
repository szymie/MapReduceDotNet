using System;
using ServiceStack.DataAnnotations;

namespace EntryPoint
{
	public class InputFile
	{
		[AutoIncrement]
		public int Id { get; set; }
		public string Description { get; set; }
	}
}
