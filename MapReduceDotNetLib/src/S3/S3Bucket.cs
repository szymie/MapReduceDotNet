using System;
using System.Collections.Generic;
using Amazon.S3;
using Amazon.S3.Model;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

namespace MapReduceDotNetLib
{
	public class S3Bucket
	{
		public static IAmazonS3 client = S3ObjectMetadata.client;
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

		protected bool CustomRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool isOk = true;
			// If there are errors in the certificate chain, look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None)
			{
				for (int i = 0; i < chain.ChainStatus.Length; i++)
				{
					if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
					{
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

						bool isChainValid = chain.Build((X509Certificate2)certificate);

						if (!isChainValid)
						{
							isOk = false;
						}
					}
				}
			}

			return isOk;
		}
	}
}
