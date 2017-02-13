using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MapReduceDotNetLib;
using System.Text;

namespace Master
{
	public class Divider
	{
		public Dictionary<string, S3ObjectMetadata> InputFiles { get; private set; }
		public int M { get; private set; }
		public int TaskId { get; private set; }
		public string Username { get; private set; }
		public List<Dictionary<string, S3ObjectMetadata>> Response { get; private set; }

		int currentFragmentNumber;

		StreamWriter fragmentWriter;
		long currentFragmentSize;
		long maxFragmentSize;

		long usedSpace = 0;

		string currentFilename;

		bool closed = false;

		public Divider(Dictionary<string, S3ObjectMetadata> inputFiles, int m, int taskId, string username)
		{
			InputFiles = inputFiles;
			M = m;
			TaskId = taskId;
			Username = username;

			currentFragmentNumber = 0;
			Response = new List<Dictionary<string, S3ObjectMetadata>>();
		}

		string getCurrentFragmentName()
		{
			return $"{Username}-{TaskId}-{currentFilename}-{currentFragmentNumber}";
		}

		string getNextFragmentName()
		{
			currentFragmentNumber++;
			return getCurrentFragmentName();
		}

		string getCurrentFragmentPath()
		{
			return Path.Combine("/tmp", getCurrentFragmentName());
		}

		string getNextFragmentPath()
		{
			return Path.Combine("/tmp", getNextFragmentName());
		}

		void openNewFragmentWriter()
		{
			currentFragmentSize = 0;
			fragmentWriter = new StreamWriter(getNextFragmentPath());
		}

		StreamWriter getCurrentFragmentWriter()
		{
			if (fragmentWriter == null)
			{
				openNewFragmentWriter();
			}

			return fragmentWriter;
		}

		void closeCurrentFragmentWriter()
		{
			if (fragmentWriter != null)
			{
				fragmentWriter.Close();
				fragmentWriter = null;
			}
		}

		void writeToCurrentFragment(string line)
		{
			getCurrentFragmentWriter().WriteLine(line);

			var bytesCount = getStringByteSize(line + Environment.NewLine);
			currentFragmentSize += bytesCount;
			usedSpace += bytesCount;
		}

		bool shouldCloseFragment()
		{
			return (currentFragmentSize >= maxFragmentSize || usedSpace >= maxFragmentSize) && Response.Count < M - 1;
		}

		public List<Dictionary<string, S3ObjectMetadata>> divide()
		{
			var entry = getNewEntry();

			long totalSize = InputFiles.Values.Aggregate(0L, (acc, file) => acc + file.getSize());

			Console.WriteLine("totalSize= " + totalSize);

			maxFragmentSize = totalSize / M;

			foreach (var pair in InputFiles)
			{
				currentFilename = pair.Key;

				using (var fileStreamReader = new StreamReader(pair.Value.downStream()))
				{
					string line;

					while ((line = fileStreamReader.ReadLine()) != null)
					{
						writeToCurrentFragment(line);

						closed = false;

						if (shouldCloseFragment())
						{
							usedSpace = 0;

							closeCurrentFragmentWriter();

							var S3Object = new S3ObjectMetadata("map-reduce-dot-net", getCurrentFragmentName());

							using (var fragmentStream = File.OpenRead(getCurrentFragmentPath()))
							{
								S3Object.upStream(fragmentStream);
							}

							File.Delete(getCurrentFragmentPath());

							entry.Add(currentFilename, S3Object);
							Response.Add(entry);
							entry = getNewEntry();

							closed = true;
						}
					}

					if (!closed)
					{
						closeCurrentFragmentWriter();

						var S3Object = new S3ObjectMetadata("map-reduce-dot-net", getCurrentFragmentName());

						using (var fragmentStream = File.OpenRead(getCurrentFragmentPath()))
						{
							S3Object.upStream(fragmentStream);
						}

						File.Delete(getCurrentFragmentPath());

						entry.Add(currentFilename, S3Object);
					}
					//else
					//{
					//	usedSpace = 0;
					//}
				}
			}

			Response.Add(entry);

			return Response;
		}



		Dictionary<string, S3ObjectMetadata> getNewEntry()
		{
			return new Dictionary<string, S3ObjectMetadata>();
		}

		int getStringByteSize(string value)
		{
			return Encoding.UTF8.GetByteCount(value);
		}
	}
}
