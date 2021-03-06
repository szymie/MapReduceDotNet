﻿using System;
using System.Collections.Generic;

namespace Master
{
	public class NextCoordinatorInBulkGetter
	{
		private List<Coordinator> Coordinators{ get; set;}
		private int coordinatorIndex = 0;

		public NextCoordinatorInBulkGetter (List<Coordinator> coordinators)
		{
			if (coordinators.Count == 0) {
				throw new Exception ("No coordinators");
			}

			this.Coordinators = coordinators;
		}

		public Coordinator next(){
			coordinatorIndex %= Coordinators.Count;
			var coordinator = Coordinators[coordinatorIndex];
			coordinatorIndex++;
			return coordinator;
		}
	}
}

