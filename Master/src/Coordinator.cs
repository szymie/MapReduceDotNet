using System;
using System.Collections.Generic;
using MapReduceDotNetLib;

namespace Master
{
	public class Coordinator
	{
		public Coordinator (int id)
		{
			this.Id = id;
		}

		public int Id{ get; set;}
		public Dictionary<int, Work> Works = new Dictionary<int, Work>();
		public Dictionary<int, WorkConfig> OrderedWorks = new Dictionary<int, WorkConfig>();
	}
}

