﻿using System;
using System.Collections.Generic;
using Akka.Actor;

namespace MapReduceDotNetLib
{
	public abstract class Map
	{
		public Map ()
		{
			
		}

		public abstract void map (string key, LineReader lineReader);

		protected void emit (string key, string value){

		}
	}
}
