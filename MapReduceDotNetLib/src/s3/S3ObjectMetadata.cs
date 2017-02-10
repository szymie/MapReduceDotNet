using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace MapReduceDotNetLib
{
	public class S3ObjectMetadata
	{
		public string BucketName { get; private set; }
		public string Filename { get; private set; }

		private static IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1);
		private static TransferUtility transferUtility = new TransferUtility(client);

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

			client.DeleteObject(deleteObjectRequest);
		}

		public void upStream(Stream stream)
		{
			transferUtility.Upload(stream, BucketName, Filename);
		}

		public Stream downStream()
		{
			GetObjectRequest request = new GetObjectRequest
			{
				BucketName = BucketName,
				Key = Filename
			};

			GetObjectResponse response = client.GetObject(request);

			return response.ResponseStream;
		}
	}
}
