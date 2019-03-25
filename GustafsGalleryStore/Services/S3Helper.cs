using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Amazon.Extensions.NETCore.Setup;
using System.Net.Http.Headers;
using GustafsGalleryStore.Helpers;

namespace GustafsGalleryStore.Services
{
    public class S3Helper
    {

        private IHostingEnvironment _hostingEnvironment;
        private static AmazonS3Client _s3Client = new AmazonS3Client(awsAccessKeyId: MasterStrings.AWSAccessKeyId,
                                                              awsSecretAccessKey: MasterStrings.AWSSecretAccessKey,
                                                              region: Amazon.RegionEndpoint.USEast1);
        public static readonly string bucketName = "gustafsgallerystore-images";//this is my Amazon Bucket name
        public static readonly string tempBucketName = "gustafsgallerystore-tempimages";//this is my Amazon Bucket name


        public S3Helper(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        public static void UploadToS3(string filePath, string bucketName, string bucketSubdirectory = null)
        {
            try
            {
                TransferUtility fileTransferUtility = new TransferUtility(_s3Client);

                if (!string.IsNullOrWhiteSpace(bucketSubdirectory))
                {   // subdirectory and bucket name  
                    bucketName = bucketName + @"/" + bucketSubdirectory;
                }

                // 1. Upload a file, file name is used as the object key name.

                fileTransferUtility.Upload(filePath, bucketName);

                 //2. Make file public
                _s3Client.PutACLAsync(new PutACLRequest
                {
                    BucketName = bucketName,
                    Key = filePath,
                    CannedACL = S3CannedACL.PublicRead
                });

            }
            catch (AmazonS3Exception s3Exception)
            {
                throw new Exception(s3Exception.Message);
            }
        }

        public static async void DeleteFromS3Async(string filePath, string bucketName, string bucketSubdirectory = null)
        {
            try
            {

                string fileName = filePath.Replace("https://s3.amazonaws.com/gustafsgallerystore-images/", "").Replace("?Authorization","");

                if (!string.IsNullOrWhiteSpace(bucketSubdirectory))
                {   // subdirectory and bucket name  
                    bucketName = bucketName + @"/" + bucketSubdirectory;
                }

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName
                };

                // 1. Upload a file, file name is used as the object key name.

                await _s3Client.DeleteObjectAsync(deleteRequest);

            }
            catch (AmazonS3Exception s3Exception)
            {
                throw new Exception(s3Exception.Message);
            }
        }
    }
}
