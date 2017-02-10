using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;

namespace MapReduceDotNetLib
{
	public class S3ObjectMetadata
	{
		public string BucketName { get; private set; }
		public string Filename { get; private set; }

		private static IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1);

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
