using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;

using PX.Data;
using PX.Objects;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.TM;
using PX.Web.UI;
using PX.Reports;
using PX.Data.Reports;

using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using System.Configuration;
using System.Collections.Specialized;

namespace AF
{
    [Serializable]
    [PXCacheName("Data Filter")]
    public class AFDataFilter : IBqlTable
    {

        #region BeginDate

        public abstract class startDate : IBqlField { }

        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
        public virtual DateTime? StartDate { get; set; }

        #endregion BeginDate

        #region EndDate

        public abstract class endDate : IBqlField { }

        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
        public virtual DateTime? EndDate { get; set; }

        #endregion EndDate
    }

    #region Declaring a new graph
    /// <summary>
    ///To declare a new graph for a new custom page, 
    ///derive the new graph class from PXGraph<T> or PXGraph<T, A> where
    ///T is the type of the new graph (required), 
    ///A is the type of the data access class of the primary view of the graph (optional).
    /// </summary>
    public class AFDataProcess : PXGraph<AFDataProcess>
    {
        //Declare the data views and implement the event handlers here     

        public PXSetup<AFSetup> Setup;

        //Se declaran los DataView referentes al filtro de ptocesamiento
        public PXFilter<AFDataFilter> Filter;

        [PXFilterable]
        public PXFilteredProcessing<SOOrder, AFDataFilter> Orders;

        public PXSelect<ARRegister> Register;

        public AFDataProcess()
        {
            Orders.SetProcessDelegate(
                delegate (List<SOOrder> list)
                {
                    SendData(list);
                });
        }

        public virtual IEnumerable orders(PXAdapter adapter)
        {
            PXSelectBase<SOOrder> query = new PXSelect<SOOrder>(this);

            if (Filter.Current.StartDate != null)
            {
                query.WhereAnd<Where<SOOrder.orderDate, GreaterEqual<Current<AFDataFilter.startDate>>>>();
            }

            if (Filter.Current.EndDate != null)
            {
                query.WhereAnd<Where<SOOrder.orderDate, LessEqual<Current<AFDataFilter.endDate>>>>();
            }

            return query.Select();
        }

        public static void SendData(List<SOOrder> data)
        {

            //Delaración inicial del archivo
            string name = "orders.csv";

            //Create needed graphs
            var graph = PXGraph.CreateInstance<AFSetupMaint>();

            //-----------------------------------------------------------------------------
            //Change for Setup daya
            string bucketName = graph.AFSetupView.Current.AFBucketName; //"acumatica-forecast";
            string s3DirectoryName = graph.AFSetupView.Current.AFDirectoryName; //"dynamics/facturas";
            string accessKey = graph.AFSetupView.Current.AFAccessKey;
            string secretKey = graph.AFSetupView.Current.AFSecretKey;
            //-----------------------------------------------------------------------------

            PXTrace.WriteInformation($"AFAccessKey: {accessKey} AFSecretKey: {secretKey} AFBucketName: {bucketName} AFDirectoryName: {s3DirectoryName}");

            //if (data?.Count == 0) return;

            using (var stream = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    try
                    {
                        //Convert to csv and upload
                        //for (int i = 0; i < data.Count; i++)
                        //{
                        //    try
                        //    {
                        //        var row = data[i];
                        //        //Check current item
                        //        PXProcessing.SetCurrentItem(row);

                        //        var line = $"{row.OrderNbr},{row.OrderDate:yyyy-MM-ddTHH:mm:ss},{row.OrderQty}";

                        //        sw.WriteLine(line);
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        PXProcessing<SOOrder>.SetError(i, e.Message);
                        //    }

                        //}

                        var line = $"TEST,2020-01-25T00:10:10,1.00";
                        sw.WriteLine(line);

                        //Si existe el CFDI permite la subida del XML y PDF
                        //Se crea la instancia a S3 
                        AmazonUploader myUploader = new AmazonUploader();
                        var result = myUploader.UploadToS3(accessKey, secretKey, stream.ToArray(), bucketName, s3DirectoryName, name);

                    }
                    catch (Exception e)
                    {
                        PXProcessing<SOOrder>.SetError(e.Message);
                    }
                }
            }

        }

        #region Clase Amazon S3

        // Amazon S3 utility class
        public class AmazonUploader
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
        }

        #endregion

    }
    #endregion
}
