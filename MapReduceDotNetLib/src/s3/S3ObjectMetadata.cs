using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Collections.Generic;

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

		public void upStream2(Stream stream)
		{
			List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

			InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
			{
				BucketName = BucketName,
				Key = Filename
			};

			Console.WriteLine("1-0");


			InitiateMultipartUploadResponse initiateResponse = client.InitiateMultipartUpload(initiateRequest);

			Console.WriteLine("1-00");

			int partSize = 5 * (int) Math.Pow(2, 20); // 5 MB

			try
			{
				int partNumber = 0, bytesCopied;

				Console.WriteLine("1-1");

				do
				{
					var memoryStream = new MemoryStream();

					bytesCopied = copyStream(stream, memoryStream, partSize);

					UploadPartRequest uploadRequest = new UploadPartRequest
					{
						BucketName = BucketName,
						Key = Filename,
						UploadId = initiateResponse.UploadId,
						PartNumber = partNumber++,
						PartSize = partSize,
						InputStream = memoryStream,
					};

					uploadResponses.Add(client.UploadPart(uploadRequest));
				} while(bytesCopied > 0);

				Console.WriteLine("1-2");

				CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
				{
					BucketName = BucketName,
					Key = Filename,
					UploadId = initiateResponse.UploadId,
				};
					
				completeRequest.AddPartETags(uploadResponses);

				CompleteMultipartUploadResponse completeUploadResponse =
					client.CompleteMultipartUpload(completeRequest);
			}
			catch (Exception exception)
			{
				Console.WriteLine("Exception occurred: {0}", exception.Message);
				AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
				{
					BucketName = BucketName,
					Key = Filename,
					UploadId = initiateResponse.UploadId
				};

				client.AbortMultipartUpload(abortMPURequest);
			}

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
	}
}
