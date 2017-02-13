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

		int currentFragmentNumber;
		Dictionary<string, S3ObjectMetadata>.Enumerator inputFilesEnumerator;

		StreamWriter fragmentWriter;
		long currentFragmentSize;
		long maxFragmentSize;

		string currentFilename;

		public Divider(Dictionary<string, S3ObjectMetadata> inputFiles, int m, int taskId, string username)
		{
			InputFiles = inputFiles;
			M = m;
			TaskId = taskId;
			Username = username;

			inputFilesEnumerator = InputFiles.GetEnumerator();
			inputFilesEnumerator.MoveNext();
		}

		string getCurrentFragmentName()
		{
			return $"{Username}-{TaskId}-{currentFilename}-{currentFragmentNumber}";
		}

		string getNextFragmentName()
		{
			currentFragmentNumber++;
			inputFilesEnumerator.MoveNext();
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
			currentFragmentSize += getStringByteSize(line);
		}

		bool shouldCloseFragment()
		{
			return currentFragmentSize >= maxFragmentSize; // && currentFragmentNumber != M;
		}

		public List<Dictionary<string, S3ObjectMetadata>> divide()
		{
			long totalSize = InputFiles.Values.Aggregate(0L, (acc, file) => acc + file.getSize());
			maxFragmentSize = totalSize / M;

			Console.WriteLine("totalSize= " + totalSize);
			Console.WriteLine("maxFragmentSize= " + maxFragmentSize);

			foreach (var pair in InputFiles)
			{
				currentFilename = pair.Key;
				currentFragmentNumber = 0;

				using (var fileStreamReader = new StreamReader(pair.Value.downStream()))
				{
					string line;

					while ((line = fileStreamReader.ReadLine()) != null)
					{
						writeToCurrentFragment(line);

						if (shouldCloseFragment())
						{
							closeCurrentFragmentWriter();
							//send to S3
						}
					}
				}
			}

			closeCurrentFragmentWriter();

			return null;
		}

		int getStringByteSize(string value)
		{
			return ASCIIEncoding.Unicode.GetByteCount(value);
		}
	
	}
}
