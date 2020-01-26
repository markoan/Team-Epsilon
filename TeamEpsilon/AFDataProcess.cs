using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;


using PX.Data;
using PX.Data.Reports;
using PX.Data.Maintenance.GI;
using PX.Objects;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.TM;
using PX.Web.UI;
using PX.Reports;

using System.Configuration;
using System.Collections.Specialized;

namespace AF
{
    [Serializable]
    [PXCacheName("Data Filter")]
    public class AFDataFilter : IBqlTable
    {

        // GenericInq
        #region GenericInq
        public abstract class genericInq : IBqlField { }
        [PXDBGuid(false)]
        [PXDefault()]
        [PXUIField(DisplayName = "Generic Inquiry", Visible = true, Enabled = true, Required = true)]
        [PXSelector(typeof(Search3<GIDesign.designID, OrderBy<Asc<GIDesign.name>>>),
            SubstituteKey = typeof(GIDesign.name))]
        public virtual Guid? GenericInq { get; set; }
        #endregion GenericInq

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

    /// <summary>
    ///To declare a new graph for a new custom page, 
    ///derive the new graph class from PXGraph<T> or PXGraph<T, A> where
    ///T is the type of the new graph (required), 
    ///A is the type of the data access class of the primary view of the graph (optional).
    /// </summary>
    public class AFDataProcess : PXGraph<AFDataProcess>
    {
        #region DataViews

        //Declare the data views and implement the event handlers here     

        public PXSetup<AFSetup> Setup;

        //Se declaran los DataView referentes al filtro de ptocesamiento
        public PXFilter<AFDataFilter> Filter;

        [PXFilterable]
        public PXFilteredProcessing<SOOrder, AFDataFilter> Orders;

        public PXSelect<ARRegister> Register;


        // GI 
        public PXSelect<GIResult, Where<GIResult.designID, Equal<Current<AFDataFilter.genericInq>>>> ResultGI;

        [PXFilterable]
        public PXSelectOrderBy<CCProcTran, OrderBy<Desc<CCProcTran.refNbr>>> PaymentTrans;

        public PXAction<AFDataFilter> UploadData;

        #endregion DataViews

        public AFDataProcess()
        {
            Orders.SetProcessEnabled(false);
            Orders.SetProcessVisible(false);

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

        public static void SendData(IEnumerable<SOOrder> data, string name = "orders.csv")
        {

            //Create needed graphs
            var graph = PXGraph.CreateInstance<AFSetupMaint>();

            AFSetup setup = graph.AFSetupView.Current ?? graph.AFSetupView.Select();

            //-----------------------------------------------------------------------------
            //Change for Setup daya
            string bucketName = setup.AFBucketName; //"acumatica-forecast";
            string s3DirectoryName = setup.AFDirectoryName; //"dynamics/facturas";
            string accessKey = setup.AFAccessKey;
            string secretKey = setup.AFSecretKey;
            //-----------------------------------------------------------------------------

            PXTrace.WriteInformation($"AFAccessKey: {accessKey} AFSecretKey: {secretKey} AFBucketName: {bucketName} AFDirectoryName: {s3DirectoryName}");

            //if (data?.Count == 0) return;

            using (var stream = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    try
                    {
                        // Convert to csv and upload
                        foreach(var i in data)
                        {
                            try
                            {
                                //var row = data[i];
                                //Check current item
                                //PXProcessing.SetCurrentItem(row);

                                //sw.WriteLine(i);
                            }
                            catch (Exception e)
                            {
                                PXProcessing<SOOrder>.SetError(e.Message);
                            }

                        }

                        var line = $"TEST,2020-01-25T00:10:10,1.00";
                        sw.WriteLine(line);

                        //Si existe el CFDI permite la subida del XML y PDF
                        //Se crea la instancia a S3 
                        AFAmazonTools myUploader = new AFAmazonTools();
                        var result = myUploader.UploadToS3(accessKey, secretKey, stream.ToArray(), bucketName, s3DirectoryName, name);

                    }
                    catch (Exception e)
                    {
                        PXProcessing<SOOrder>.SetError(e.Message);
                    }
                }
            }

        }


    }
}
