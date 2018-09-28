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
namespace GustafsGalleryStore.Services
{
    public class S3Helper
    {

        private IHostingEnvironment _hostingEnvironment;
        private static AmazonS3Client _s3Client = new AmazonS3Client(awsAccessKeyId: "AKIAIMGF2ETCLPVRVHAA",
                                                              awsSecretAccessKey: "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",
                                                              region: Amazon.RegionEndpoint.USEast1);
        private static readonly string _bucketName = "sbt-solutions.imagestore";//this is my Amazon Bucket name
        private static string _bucketSubdirectory = String.Empty;

        public S3Helper(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        public static void UploadToS3(string filePath)
        {
            try
            {
                TransferUtility fileTransferUtility = new
                    TransferUtility(_s3Client);

                string bucketName;

                if (_bucketSubdirectory == "" || _bucketSubdirectory == null)
                {
                    bucketName = _bucketName; //no subdirectory just bucket name  
                }
                else
                {   // subdirectory and bucket name  
                    bucketName = _bucketName + @"/" + _bucketSubdirectory;
                }

                // 1. Upload a file, file name is used as the object key name.

                fileTransferUtility.Upload(filePath, _bucketName);

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

        public static async void DeleteFromS3Async(string filePath)
        {
            try
            {

                string bucketName;
                string fileName = filePath.Replace("https://s3.amazonaws.com/sbt-solutions.imagestore/", "").Replace("?Authorization","");


                if (_bucketSubdirectory == "" || _bucketSubdirectory == null)
                {
                    bucketName = _bucketName; //no subdirectory just bucket name  
                }
                else
                {   // subdirectory and bucket name  
                    bucketName = _bucketName + @"/" + _bucketSubdirectory;
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
