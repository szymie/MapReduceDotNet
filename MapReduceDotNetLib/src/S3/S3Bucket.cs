using System;
using System.Collections.Generic;
using Amazon.S3;
using Amazon.S3.Model;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

namespace MapReduceDotNetLib
{
	public class S3Bucket : BaseS3
	{
		private static IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.EUCentral1);
		public string BucketName { get; private set; }

		public S3Bucket(string bucketName)
		{
			BucketName = bucketName;
			ServicePointManager.ServerCertificateValidationCallback = CustomRemoteCertificateValidationCallback;
		}

		private ListObjectsV2Request request;
		private ListObjectsV2Response response;
		private IEnumerator<S3Object> enumerator;

		public void fetchKeys()
		{
			request = new ListObjectsV2Request
			{
				BucketName = BucketName,
				MaxKeys = 5
			};

			response = client.ListObjectsV2(request);
			enumerator = response.S3Objects.GetEnumerator();
		}

		public string getCurrentKey()
		{

			var currentKey = enumerator.Current.Key;
			return currentKey;
		}

		public bool moveNext()
		{
			var moved = enumerator.MoveNext();

			if (!moved)
			{
				if (response.IsTruncated)
				{
					request.ContinuationToken = response.NextContinuationToken;
					response = client.ListObjectsV2(request);
					enumerator = response.S3Objects.GetEnumerator();
					enumerator.MoveNext();
				}
				else
				{
					return false;
				}
			}

			return true;
		}
	}
}
