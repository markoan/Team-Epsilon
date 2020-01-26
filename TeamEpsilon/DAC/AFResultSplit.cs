using System;
using PX.Data;

namespace AF
{
[Serializable]
[PXCacheName("AFResultSplit")]
public class AFResultSplit : IBqlTable
{

    #region ResultID
    [PXDBString(50, IsKey = true, InputMask = "")]
    [PXUIField(DisplayName = "Result ID")]
    public virtual string ResultID { get; set; }
    public abstract class resultID : PX.Data.BQL.BqlString.Field<resultID> { }
    #endregion

    #region ResultTstamp
    [PXDBDateAndTime(IsKey = true, PreserveTime = true)]
    [PXUIField(DisplayName = "Result Tstamp")]
    public virtual DateTime? ResultTstamp { get; set; }
    public abstract class resultTstamp : PX.Data.BQL.BqlByteArray.Field<resultTstamp> { }
        #endregion

    #region Forecast
    [PXDBDecimal()]
    [PXUIField(DisplayName = "Forecast")]
    public virtual decimal? Forecast { get; set; }
    public abstract class forecast : PX.Data.BQL.BqlDecimal.Field<forecast> { }
    #endregion

    #region ValueType
    [PXDBString()]
    [PXUIField(DisplayName = "ValueType")]
    public virtual string ValueType { get; set; }
    public abstract class valueType : PX.Data.BQL.BqlString.Field<valueType> { }
    #endregion

    }
}