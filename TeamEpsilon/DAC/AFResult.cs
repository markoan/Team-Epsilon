using System;
using PX.Data;

namespace AF
{
[Serializable]
[PXCacheName("AFResult")]
public class AFResult : IBqlTable
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

    #region ResultP10
    [PXDBDecimal()]
    [PXUIField(DisplayName = "Result P10")]
    public virtual decimal? ResultP10 { get; set; }
    public abstract class resultP10 : PX.Data.BQL.BqlDecimal.Field<resultP10> { }
    #endregion

    #region ResultE50
    [PXDBDecimal()]
    [PXUIField(DisplayName = "Result E50")]
    public virtual decimal? ResultE50 { get; set; }
    public abstract class resultE50 : PX.Data.BQL.BqlDecimal.Field<resultE50> { }
    #endregion

    #region ResultP90
    [PXDBDecimal()]
    [PXUIField(DisplayName = "Result P90")]
    public virtual decimal? ResultP90 { get; set; }
    public abstract class resultP90 : PX.Data.BQL.BqlDecimal.Field<resultP90> { }
    #endregion

    #region CreatedByID
    [PXDBCreatedByID()]
    public virtual Guid? CreatedByID { get; set; }
    public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
    #endregion

    #region CreatedByScreenID
    [PXDBCreatedByScreenID()]
    public virtual string CreatedByScreenID { get; set; }
    public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
    #endregion

    #region CreatedDateTime
    [PXDBCreatedDateTime()]
    public virtual DateTime? CreatedDateTime { get; set; }
    public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
    #endregion

    #region LastModifiedByID
    [PXDBLastModifiedByID()]
    public virtual Guid? LastModifiedByID { get; set; }
    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
    #endregion

    #region LastModifiedByScreenID
    [PXDBLastModifiedByScreenID()]
    public virtual string LastModifiedByScreenID { get; set; }
    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
    #endregion

    #region LastModifiedDateTime
    [PXDBLastModifiedDateTime()]
    public virtual DateTime? LastModifiedDateTime { get; set; }
    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
    #endregion

    }
}