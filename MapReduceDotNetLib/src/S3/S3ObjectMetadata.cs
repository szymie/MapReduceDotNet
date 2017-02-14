using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

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
			File.Delete ("/tmp/s3/" + Filename);


			//client.DeleteObject(deleteObjectRequest);
		}

		public void upStream(Stream stream)
		{
			using (stream) {
				using (FileStream fileStream = File.Open ("/tmp/s3/" + Filename, FileMode.Create)) {
					stream.CopyTo (fileStream);
				}
			}
			//throw new NotImplementedException();
		}

		public Stream downStream()
		{
			
			return File.OpenRead ("/tmp/s3/" + Filename);
			//throw new NotImplementedException();
		}

		public long getSize()
		{
			return new FileInfo("/tmp/s3/" + Filename).Length;
		}
	}
}
