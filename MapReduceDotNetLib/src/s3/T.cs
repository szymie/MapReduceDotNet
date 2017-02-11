using System;
using System.Collections.Generic;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;

namespace s3.amazon.com.docsamples
{
	class UploadFileMPULowLevelAPI
	{
		static string existingBucketName = "*** bucket name ***";
		static string keyName = "*** key name ***";
		static string filePath = "*** file path ***";

		static void Main(string[] args)
		{
			IAmazonS3 s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);

			// List to store upload part responses.
			List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

			// 1. Initialize.
			InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
			{
				BucketName = existingBucketName,
				Key = keyName
			};

			InitiateMultipartUploadResponse initResponse =
				s3Client.InitiateMultipartUpload(initiateRequest);

			// 2. Upload Parts.
			long contentLength = new FileInfo(filePath).Length;
			long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB

			try
			{
				long filePosition = 0;
				for (int i = 1; filePosition < contentLength; i++)
				{
					UploadPartRequest uploadRequest = new UploadPartRequest
					{
						BucketName = existingBucketName,
						Key = keyName,
						UploadId = initResponse.UploadId,
						PartNumber = i,
						PartSize = partSize,
						FilePosition = filePosition,
						FilePath = filePath
					};

					var x = new UploadPartRequest();



					// Upload part and add response to our list.
					uploadResponses.Add(s3Client.UploadPart(uploadRequest));

					filePosition += partSize;
				}

				// Step 3: complete.
				CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
				{
					BucketName = existingBucketName,
					Key = keyName,
					UploadId = initResponse.UploadId,
					//PartETags = new List<PartETag>(uploadResponses)

				};
				completeRequest.AddPartETags(uploadResponses);

				CompleteMultipartUploadResponse completeUploadResponse =
					s3Client.CompleteMultipartUpload(completeRequest);

			}
			catch (Exception exception)
			{
				Console.WriteLine("Exception occurred: {0}", exception.Message);
				AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
				{
					BucketName = existingBucketName,
					Key = keyName,
					UploadId = initResponse.UploadId
				};
				s3Client.AbortMultipartUpload(abortMPURequest);
			}
		}
	}
}