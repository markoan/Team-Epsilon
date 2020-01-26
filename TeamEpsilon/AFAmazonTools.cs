using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using System.IO;

namespace AF
{

    #region Clase Amazon S3

    // Amazon S3 utility class
    public class AFAmazonTools
    {
        // Uploads to S3
        public bool UploadToS3(string accessKey, string secretKey, byte[] content, string bucketName, string subDirectoryInBucket, string fileNameInS3)
        {
            // Initialize client
            IAmazonS3 client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.USEast1);
            TransferUtility utility = new TransferUtility(client);
            TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();

            // Initialize stream and write bytes
            var ms = new System.IO.MemoryStream();
            ms.Write(content, 0, content.Length);
            ms.Position = 0;

            // We create directory if it's needed
            if (subDirectoryInBucket == "" || subDirectoryInBucket == null)
            {
                request.BucketName = bucketName;
            }
            else
            {   // Directory name
                request.BucketName = bucketName + @"/" + subDirectoryInBucket;
            }

            // Filename, content and upload
            request.Key = fileNameInS3;
            request.InputStream = ms;
            request.CannedACL = S3CannedACL.BucketOwnerFullControl;
            utility.Upload(request);

            return true;
        }

        public IEnumerable<string[]> DownloadFromS3(string accessKey, string secretKey, string bucketName, string subDirectoryInBucket)
        {
            char[] separator = new[] { ',' };
            string currentLine;

            // Initialize client
            IAmazonS3 client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.USEast1);
            TransferUtility utility = new TransferUtility(client);

            ListObjectsRequest request = new ListObjectsRequest();

            request.BucketName = bucketName;
            request.Prefix = subDirectoryInBucket;
            request.Delimiter = "/";
            request.MaxKeys = 1000;

            ListObjectsResponse response = client.ListObjects(request);
            var csvFiles = response.S3Objects.Where(s => s.Key.Contains("csv")).OrderBy(s => s.Key);

            foreach (var fileInfo in csvFiles)
            {
                GetObjectRequest objRequest = new GetObjectRequest();
                objRequest.BucketName = bucketName;
                objRequest.Key = fileInfo.Key;
                GetObjectResponse file = client.GetObject(objRequest);

                using (Stream responseStream = file.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    //responseBody = reader.ReadToEnd(); // Now you process the response body.

                    while ((currentLine = reader.ReadLine()) != null)
                    {
                        yield return currentLine.Split(separator, StringSplitOptions.None);
                    }
                }

            }
        }
    }

    #endregion

}
