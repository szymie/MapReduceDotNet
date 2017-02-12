using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.DataAnnotations;
using MapReduceDotNetLib;

namespace EntryPoint
{
	public class AssemblyMetadata : InputFileMetadata, IOwnable
	{
		public string Namespace { get; set; }
		public string MapClassName { get; set; }
		public string ReduceClassName { get; set; }

		public override bool CanDelete(List<Task> inProgressTasks)
		{
			var assemblyIds = inProgressTasks.Select(entity => entity.AssemblyId);
			return !assemblyIds.Contains(Id);
		}
	}
}
