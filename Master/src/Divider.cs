using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MapReduceDotNetLib;

namespace Master
{
	public class Divider
	{
		public Dictionary<string, S3ObjectMetadata> Files { get; private set; }
		public int M { get; private set; }
		public int TaskId { get; private set; }

		public Divider(Dictionary<string, S3ObjectMetadata> files, int m, int taskId)
		{
			Files = files;
			M = m;
			TaskId = taskId;
		}

		public List<Dictionary<string, S3ObjectMetadata>> divide()
		{
			
			return null;
		}

		private long sumFilesSize()
		{
			long totalSize = Files.Values.Aggregate(0L, (acc, file) => acc + file.getSize());

			long partSize = totalSize / M;

			return 0;
		}
	}
}
