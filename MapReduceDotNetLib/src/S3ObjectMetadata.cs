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

		//private static IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1);

		public S3ObjectMetadata(string bucketName, string filename)
		{
			BucketName = bucketName;
			Filename = filename;
		}

		public void remove()
		{
			DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest()
			{
				BucketName = BucketName,
				Key = Filename
			};

			//client.DeleteObject(deleteObjectRequest);
		}

		public void upStream(Stream stream)
		{
			using (stream) {
				using (FileStream fileStream = File.Open ("/tmp/" + Filename, FileMode.Create)) {
					stream.CopyTo (fileStream);
				}
			}
			//throw new NotImplementedException();
		}

		public Stream downStream()
		{
			return File.OpenRead (Filename);
			//throw new NotImplementedException();
		}
	}
}
