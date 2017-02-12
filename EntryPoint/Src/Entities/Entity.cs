using System;
using ServiceStack.DataAnnotations;

namespace EntryPoint
{
	public class Entity
	{
		[AutoIncrement]
		public int Id { get; set; }
	}
}
