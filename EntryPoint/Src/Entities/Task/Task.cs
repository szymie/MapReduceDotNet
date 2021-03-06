﻿using System;
using ServiceStack.DataAnnotations;
using System.Collections.Generic;

namespace EntryPoint
{
	public class Task : Entity, Ownable
	{
		public string Status { get; set; }
		//[References(typeof(AssemblyMetadata))]	//is that necessary?
		public int AssemblyId { get; set; }
		public IList<int> InputFileIds { get; set; }
		public int M { get; set; }
		public int R { get; set; }
		public DateTime CreatedAt { get; set; }
		public int OwnerId { get; set; }
	}
}
