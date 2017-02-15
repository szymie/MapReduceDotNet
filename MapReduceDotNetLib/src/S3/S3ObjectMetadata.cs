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
	public class S3ObjectMetadata : BaseS3
	{
		public string BucketName { get; private set; }
		public string Filename { get; private set; }

		private static IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1);
		private static TransferUtility transferUtility = new TransferUtility(client);

		public S3ObjectMetadata(string bucketName, string filename)
		{
			BucketName = bucketName;
			Filename = filename;
			ServicePointManager.ServerCertificateValidationCallback = CustomRemoteCertificateValidationCallback;
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

		public void upStreamThroughLocalBuffer(Stream stream)
		{
			var tmpFileName = $"/tmp/{Filename}.tmp";

			using (var outputFileStream = File.Create(tmpFileName))
			{
				stream.CopyTo(outputFileStream);
			}

			var inputFileStream = File.OpenRead(tmpFileName);
			transferUtility.Upload(inputFileStream, BucketName, Filename);

			File.Delete(tmpFileName);
		}

		public Stream downStream()
		{
			GetObjectResponse response = requestObjectMetadata();
			return response.ResponseStream;
		}

		private GetObjectResponse requestObjectMetadata()
		{
			GetObjectRequest request = new GetObjectRequest
			{
				BucketName = BucketName,
				Key = Filename
			};

			return client.GetObject(request);
		}

		public long getSize()
		{
			GetObjectResponse response = requestObjectMetadata();
			return response.ContentLength;
		}
	}
}