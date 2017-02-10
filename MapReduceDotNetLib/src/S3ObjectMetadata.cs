using System;
using System.IO;

namespace MapReduceDotNetLib
{
	public class S3ObjectMetadata
	{
		public string BucketName { get; private set; }
		public string Filename { get; private set; }

		public S3ObjectMetadata(string bucketName, string filename)
		{
			BucketName = bucketName;
			Filename = filename;
		}

		public void remove()
		{

		}

		public void upStream(Stream stream)
		{

		}

		public Stream downStream()
		{
			return null;
		}
}
