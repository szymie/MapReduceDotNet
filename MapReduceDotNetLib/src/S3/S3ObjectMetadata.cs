using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

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
			var tmpFileName = $"/tmp/{Filename}";

			using(var outputFileStream = File.Create(tmpFileName))
			{
				stream.CopyTo(outputFileStream);		
			}

			var inputFileStream = File.OpenRead(tmpFileName);
			transferUtility.Upload(inputFileStream, BucketName, Filename);

			File.Delete(tmpFileName);
		}

		public static int copyStream(Stream input, Stream output, int bytes)
		{
			byte[] buffer = new byte[32768];

			int bytesRead, totalBytesCopied = 0, bytesLeft = bytes;

			while (bytesLeft > 0 && (bytesRead = input.Read(buffer, 0, Math.Min(buffer.Length, bytesLeft))) > 0)
			{
				output.Write(buffer, 0, bytesRead);
				totalBytesCopied += bytesRead;
				bytesLeft -= bytesRead;
			}

			return totalBytesCopied;
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

		public bool CustomRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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
