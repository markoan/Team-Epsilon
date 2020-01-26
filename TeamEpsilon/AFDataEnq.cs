using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Xsl;

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

namespace AF
{
    public class AFDataEnq : PXGraph<AFDataEnq>
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
            //[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? StartDate { get; set; }

            #endregion BeginDate

            #region EndDate

            public abstract class endDate : IBqlField { }

            [PXDBDate()]
            //[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? EndDate { get; set; }

            #endregion EndDate
        }


        #region DataViews

        //Declare the data views and implement the event handlers here     

        public PXSetup<AFSetup> Setup;

        //Se declaran los DataView referentes al filtro de ptocesamiento
        public PXFilter<AFDataFilter> Filter;
        public PXCancel<AFDataFilter> Cancel;

        public PXSelect<AFResult> Result;

        // GI 
        public PXSelect<GIResult, Where<GIResult.designID, Equal<Current<AFDataFilter.genericInq>>>> ResultGI;

        public PXAction<AFDataFilter> UploadData;
        public PXAction<AFDataFilter> ImportForecast;

        public AFDataEnq()
        {

        }

        #endregion DataViews

        #region Acciones

        [PXButton]
        [PXUIField(DisplayName = "Upload Data", Visible = true)]
        public virtual IEnumerable uploadData(PXAdapter adapter)
        {

            //Rastrea la consulta genérica, si existe forma la estrucutra de la tabla
            GIDesign design = PXSelectReadonly<GIDesign, Where<GIDesign.designID, Equal<Required<GIDesign.designID>>>>.Select(this, Filter.Current.GenericInq);
            if (design != null)
            {
                var start = Filter.Current.StartDate;
                var end = Filter.Current.EndDate;

                string name = design.Name+".csv";// "orders.csv";

                PXLongOperation.StartOperation(this, delegate ()
                {

                    //Obtiene la estructura de la tabla
                    DataTable tableGI = CreateTableStructure(design);

                    //Si existe más de una columna significa que recupero la estructura de la consulta generica
                    if (tableGI.Columns.Count > 1)
                    {
                        //Llena la columna con los registros de la consulta generica
                        Dictionary<string, DateTime?> parameters = new Dictionary<string, DateTime?>() {
                            { "StartDate", Filter.Current.StartDate },
                            { "EndDate", Filter.Current.EndDate}
                        };
                        tableGI = LoadRowsToTable(design, tableGI, parameters);
                    }

                    //Obtiene el csv
                    SendData(DatatableToCsv(tableGI), name);

                });
            }

            return adapter.Get();
        }

        [PXButton]
        [PXUIField(DisplayName = "Import forecast", Visible = true)]
        public virtual IEnumerable importForecast(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate ()
            {
                //Create needed graphs
                var graph = PXGraph.CreateInstance<AFResultMaint>();

                AFSetup setup = graph.Setup.Current ?? graph.Setup.Select();

                //-----------------------------------------------------------------------------
                //Change for Setup daya
                string bucketName = setup.AFBucketName;
                string s3DirectoryName = setup.AFOutDirectoryName;
                string accessKey = setup.AFAccessKey;
                string secretKey = setup.AFSecretKey;
                //-----------------------------------------------------------------------------

                PXTrace.WriteInformation($"AFAccessKey: {accessKey} AFSecretKey: {secretKey} AFBucketName: {bucketName} AFDirectoryName: {s3DirectoryName}");

                //if (data?.Count == 0) return;


                try
                {
                    AFAmazonTools myUploader = new AFAmazonTools();
                    var result = myUploader.DownloadFromS3(accessKey, secretKey, bucketName, s3DirectoryName);

                    foreach(var line in result)
                    {
                        var item = new AFResult();
                        item.ResultID = line[0];
                        item.ResultTstamp = DateTime.Parse(line[1], null, System.Globalization.DateTimeStyles.RoundtripKind);
                        item.ResultP10 = Convert.ToDecimal(line[2]);
                        item.ResultE50 = Convert.ToDecimal(line[3]);
                        item.ResultP90 = Convert.ToDecimal(line[4]);

                        graph.AFResultView.Insert(item)
                    }
                    graph.Save.Press();

                }
                catch (Exception e)
                {
                    PXProcessing<SOOrder>.SetError(e.Message);
                }


            });

            return adapter.Get();
        }
        #endregion Acciones

        #region EventHandlers 

        #endregion EventHandlers 

        //Función que crea la esturcuta de una tabla (Columnas)
        public static DataTable CreateTableStructure(GIDesign genericInq)
        {

            //Crea la instancia al graph de las consultas genericas
            PXGenericInqGrph graphGetColumns = PXGenericInqGrph.CreateInstance(genericInq.DesignID.Value);

            //Inicializa el listado de columnas
            List<string> listColumns = new List<string>();

            //Recorre, acomoda y agrega las columnas a la lista en orden
            foreach (GIResult resultMap in PXSelectReadonly<GIResult, Where<GIResult.designID, Equal<Required<GIResult.designID>>>, OrderBy<Asc<GIResult.lineNbr>>>.Select(graphGetColumns, new object[] { genericInq.DesignID.Value }))
            {
                // Solo agregamos si no está vacío
                if (!string.IsNullOrWhiteSpace(resultMap?.Caption))
                {
                    listColumns.Add(resultMap.Caption.ToString());
                }
            }

            //Crea una nueva Tabla DataTable.
            System.Data.DataTable table = new DataTable("Result");

            // Define las filas y columnas
            DataColumn column;

            // Crea la primera columna (autonumerable)
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "_id";
            column.AutoIncrement = true;
            // Agrega la columna a la tabla
            table.Columns.Add(column);

            //Genera las columnas de acuerdo a la consulta genérica
            foreach (var itemColumn in listColumns)
            {
                // Crea la segunda columna
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = itemColumn;
                column.AutoIncrement = false;

                // Agrega la columna a la tabla
                table.Columns.Add(column);
            }

            // Vuelve la columna "_id" PrimaryKey
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns["_id"];
            table.PrimaryKey = PrimaryKeyColumns;

            //Regresa el esquema base de la tabla (columnas)
            return table;
        }

        public static IEnumerable<string> DatatableToCsv(DataTable table)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName).Where(c=> c!= "_id");

            yield return string.Join(",", columnNames);

            foreach (DataRow row in table.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString()).Skip(1);
                yield return string.Join(",", fields);
            }
        }

        public static IEnumerable<string> ResultToString(GIDesign genericInq, Dictionary<string, DateTime?> parameters)
        {
            //Define la fila que se generará y la variable de autoincremento
            //DataRow row;
            int i = 0;

            //Crea la instancia al graph de las consultas genericas
            PXGenericInqGrph graphGetRows = PXGenericInqGrph.CreateInstance(genericInq.DesignID.Value);

            //Define los parametros del filtro de la consulta genérica
            foreach (var itemParameters in parameters)
            {
                graphGetRows.Caches[typeof(GenericFilter)].SetValueExt(graphGetRows.Filter.Current, itemParameters.Key, itemParameters.Value);
            }

            //Recorre los renglones recuperados
            foreach (GenericResult resultRow in graphGetRows.Views["Results"].SelectMulti())
            {
                //Genera las filas de la tabla
                //row = structureTable.NewRow();
                List<string> row = new List<string>();

                //Recorre las llaves del renglon (DAC's de los que se componen los renglones)
                foreach (string key in resultRow.Values.Keys)
                {
                    //Determina si son DAC's mapeados y genericos(formulas)
                    if (key != "GenericResult")
                    {
                        //Si son DAC´s nativos, recorre las columnas definidas en la consulta genérica
                        foreach (GIResult resultMap in PXSelectReadonly<GIResult, Where<GIResult.designID, Equal<Required<GIResult.designID>>, And<GIResult.objectName, Equal<Required<GIResult.objectName>>>>>.Select(graphGetRows, new object[] { genericInq.DesignID.Value, key }))
                        {
                            //Inicializa la variable result
                            var result = new object();
                            //Inicializa el campo como vacío
                            string fieldValue = string.Empty;

                            //Recupera el valor de esa columna en el renglón vigente
                            if (resultMap.Field.Contains("Attributes"))
                            {
                                //Recupera el NoteID del registro
                                result = graphGetRows.Caches[resultRow.Values[key].GetType()].GetValue(resultRow.Values[key], "NoteID");
                                Guid dresult = (Guid)result;

                                //Descompone la columna del atributo y recupera el nombre (llave)
                                string[] values = resultMap.Field.Split('_');
                                string attributeName = values[0];

                                //Rastrea el valor del atributo
                                CSAnswers resultAttribute = PXSelectReadonly<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>, And<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>>.Select(graphGetRows, new object[] { dresult, attributeName });

                                //Coloca la descripción del atributo en el valor del campo
                                if (resultAttribute != null)
                                {
                                    fieldValue = resultAttribute.Value;
                                }
                            }
                            else
                            {
                                result = graphGetRows.Caches[resultRow.Values[key].GetType()].GetValue(resultRow.Values[key], resultMap.Field);

                                //Si el resultado es diferente de nulo permite la conversión a texto
                                if (result != null)
                                {

                                    //Realiza la conversión del valor recuperado
                                    fieldValue = result.ToString();

                                    //Determina si es tipo Fecha
                                    if (graphGetRows.Caches[resultRow.Values[key].GetType()].GetValue(resultRow.Values[key], resultMap.Field).GetType() == typeof(DateTime))
                                    {
                                        fieldValue = ((DateTime)result).ToString("yyyy-MM-ddThh:mm:ss");
                                    }

                                    //Determina si es tipo decimal
                                    if (graphGetRows.Caches[resultRow.Values[key].GetType()].GetValue(resultRow.Values[key], resultMap.Field).GetType() == typeof(Decimal))
                                    {
                                        fieldValue = ((Decimal)result).ToString("0.00");
                                    }
                                }
                            }

                            // Agrega el valor de la fila a la columna definida por el número de línea en la definición de la tabla
                            // pero solo agregamos si el titulo no está vacío
                            if (!string.IsNullOrWhiteSpace(resultMap?.Caption))
                            {
                                row.Add(fieldValue);
                            }
                        }
                    }
                    else
                    {

                        //Recupera los registros dinámicos (fórmulas)
                        Dictionary<string, object> resGI = (Dictionary<string, object>)resultRow.Values[key];

                        //Recorre todos esos campos calculados
                        foreach (var entry in resGI)
                        {
                            //Obtiene llave y valor de cada fórmula
                            string keyGI = entry.Key;
                            var valueGI = entry.Value;

                            //La llave es un valor compuesto, se separa e identifica el RowID para saber a que columna le corresponde y lo convierte en su respectivo RowID
                            string[] words = keyGI.Split('_');
                            string objectName = words[0];
                            string rowId = words[1];
                            rowId = rowId.Replace("Formula", "");
                            Guid createRowId = Guid.ParseExact(rowId, "N");

                            //Rastrea la línea (posición de la columna)
                            GIResult resultMapRowId = PXSelectReadonly<GIResult, Where<GIResult.designID, Equal<Required<GIResult.designID>>, And<GIResult.rowID, Equal<Required<GIResult.rowID>>>>>.Select(graphGetRows, new object[] { genericInq.DesignID.Value, createRowId });
                            string fieldValueRowId = string.Empty;
                            if (fieldValueRowId != null)
                            {
                                fieldValueRowId = valueGI.ToString();
                            }

                            //Agrega el valor de la fila a la columna definida por el número de línea en la definición de la tabla
                            // pero solo agregamos si el titulo no está vacío
                            if (!string.IsNullOrWhiteSpace(resultMapRowId?.Caption))
                            {
                                row.Add(fieldValueRowId);
                            }
                        }
                    }

                }

                yield return String.Join(",", row);
            }
        }

        public static DataTable LoadRowsToTable(GIDesign genericInq, DataTable structureTable, Dictionary<string, DateTime?> parameters)
        {
            //Define la fila que se generará y la variable de autoincremento
            DataRow row;
            int i = 0;

            //Crea la instancia al graph de las consultas genericas
            PXGenericInqGrph graphGetRows = PXGenericInqGrph.CreateInstance(genericInq.DesignID.Value);

            //Define los parametros del filtro de la consulta genérica
            foreach (var itemParameters in parameters)
            {
                graphGetRows.Caches[typeof(GenericFilter)].SetValueExt(graphGetRows.Filter.Current, itemParameters.Key, itemParameters.Value);
            }

            //Recorre los renglones recuperados
            foreach (GenericResult resultRow in graphGetRows.Views["Results"].SelectMulti())
            {
                //Genera las filas de la tabla
                row = structureTable.NewRow();
                row["_id"] = i++;

                //Recorre las llaves del renglon (DAC's de los que se componen los renglones)
                foreach (string key in resultRow.Values.Keys)
                {
                    //Determina si son DAC's mapeados y genericos(formulas)
                    if (key != "GenericResult")
                    {
                        //Si son DAC´s nativos, recorre las columnas definidas en la consulta genérica
                        foreach (GIResult resultMap in PXSelectReadonly<GIResult, Where<GIResult.designID, Equal<Required<GIResult.designID>>, And<GIResult.objectName, Equal<Required<GIResult.objectName>>>>>.Select(graphGetRows, new object[] { genericInq.DesignID.Value, key }))
                        {
                            //Inicializa la variable result
                            var result = new object();
                            //Inicializa el campo como vacío
                            string fieldValue = string.Empty;

                            //Recupera el valor de esa columna en el renglón vigente
                            if (resultMap.Field.Contains("Attributes"))
                            {
                                //Recupera el NoteID del registro
                                result = graphGetRows.Caches[resultRow.Values[key].GetType()].GetValue(resultRow.Values[key], "NoteID");
                                Guid dresult = (Guid)result;

                                //Descompone la columna del atributo y recupera el nombre (llave)
                                string[] values = resultMap.Field.Split('_');
                                string attributeName = values[0];

                                //Rastrea el valor del atributo
                                CSAnswers resultAttribute = PXSelectReadonly<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>, And<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>>.Select(graphGetRows, new object[] { dresult, attributeName });

                                //Coloca la descripción del atributo en el valor del campo
                                if (resultAttribute != null)
                                {
                                    fieldValue = resultAttribute.Value;
                                }
                            }
                            else
                            {
                                result = graphGetRows.Caches[resultRow.Values[key].GetType()].GetValue(resultRow.Values[key], resultMap.Field);

                                //Si el resultado es diferente de nulo permite la conversión a texto
                                if (result != null)
                                {

                                    //Realiza la conversión del valor recuperado
                                    fieldValue = result.ToString();

                                    //Determina si es tipo Fecha
                                    if (graphGetRows.Caches[resultRow.Values[key].GetType()].GetValue(resultRow.Values[key], resultMap.Field).GetType() == typeof(DateTime))
                                    {
                                        fieldValue = ((DateTime)result).ToString("yyyy-MM-dd");
                                    }

                                    //Determina si es tipo decimal
                                    if (graphGetRows.Caches[resultRow.Values[key].GetType()].GetValue(resultRow.Values[key], resultMap.Field).GetType() == typeof(Decimal))
                                    {
                                        fieldValue = ((Decimal)result).ToString("0.00");
                                    }
                                }
                            }

                            // Agrega el valor de la fila a la columna definida por el número de línea en la definición de la tabla
                            // pero solo agregamos si el titulo no está vacío
                            if (!string.IsNullOrWhiteSpace(resultMap?.Caption))
                            {
                                row[resultMap.Caption.ToString()] = fieldValue;
                            }
                        }
                    }
                    else
                    {

                        //Recupera los registros dinámicos (fórmulas)
                        Dictionary<string, object> resGI = (Dictionary<string, object>)resultRow.Values[key];

                        //Recorre todos esos campos calculados
                        foreach (var entry in resGI)
                        {
                            //Obtiene llave y valor de cada fórmula
                            string keyGI = entry.Key;
                            var valueGI = entry.Value;

                            //La llave es un valor compuesto, se separa e identifica el RowID para saber a que columna le corresponde y lo convierte en su respectivo RowID
                            string[] words = keyGI.Split('_');
                            string objectName = words[0];
                            string rowId = words[1];
                            rowId = rowId.Replace("Formula", "");
                            Guid createRowId = Guid.ParseExact(rowId, "N");

                            //Rastrea la línea (posición de la columna)
                            GIResult resultMapRowId = PXSelectReadonly<GIResult, Where<GIResult.designID, Equal<Required<GIResult.designID>>, And<GIResult.rowID, Equal<Required<GIResult.rowID>>>>>.Select(graphGetRows, new object[] { genericInq.DesignID.Value, createRowId });
                            string fieldValueRowId = string.Empty;
                            if (fieldValueRowId != null)
                            {
                                fieldValueRowId = valueGI.ToString();
                            }

                            //Agrega el valor de la fila a la columna definida por el número de línea en la definición de la tabla
                            // pero solo agregamos si el titulo no está vacío
                            if (!string.IsNullOrWhiteSpace(resultMapRowId?.Caption))
                            {
                                row[resultMapRowId.Caption.ToString()] = fieldValueRowId;
                            }
                        }
                    }

                }

                //Agrega el renglon completo a la tabla
                structureTable.Rows.Add(row);
            }

            //Regresa la estructura completa de la tabla
            return structureTable;
        }


        public static void SendData(IEnumerable<string> data, string name = "orders.csv")
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
                        foreach (var line in data)
                        {
                            try
                            {
                                //var row = data[i];
                                //Check current item
                                //PXProcessing.SetCurrentItem(row);

                                sw.WriteLine(line);
                            }
                            catch (Exception e)
                            {
                                PXProcessing<SOOrder>.SetError(e.Message);
                            }

                        }

                        //var line = $"TEST,2020-01-25T00:10:10,1.00";

                        // WE upload to S3
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
