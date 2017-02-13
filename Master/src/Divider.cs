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
		int currentFragmentSize;

		public Divider(Dictionary<string, S3ObjectMetadata> inputFiles, int m, int taskId)
		{
			InputFiles = inputFiles;
			M = m;
			TaskId = taskId;

			currentFragmentNumber = 0;
			inputFilesEnumerator = InputFiles.GetEnumerator();
		}

		string getCurrentFragmentName()
		{
			return $"{Username}-{TaskId}-{inputFilesEnumerator.Current.Key}-{currentFragmentNumber}";
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
			fragmentWriter.Close();
			fragmentWriter = null;
		}

		public List<Dictionary<string, S3ObjectMetadata>> divide()
		{
			long totalSize = InputFiles.Values.Aggregate(0L, (acc, file) => acc + file.getSize());

			long maxFragmentSize = totalSize / M;

			foreach (var pair in InputFiles)
			{
				var filename = pair.Key;
				var file = pair.Value;

				var fileStreamReader = new StreamReader(file.downStream());

				string line;

				while ((line = fileStreamReader.ReadLine()) != null)
				{
					var fragmentWriter = getCurrentFragmentWriter();

					fragmentWriter.WriteLine(line);
					currentFragmentSize += getStringByteSize(line);

					if (currentFragmentSize >= maxFragmentSize && currentFragmentNumber != M)
					{
						closeCurrentFragmentWriter();
						//send to S3
					}
				}

				fileStreamReader.Close();
			}

			return null;
		}

		int getStringByteSize(string value)
		{
			return ASCIIEncoding.Unicode.GetByteCount(value);
		}
	
	}
}
