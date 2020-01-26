using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;


namespace AF
{
    [Serializable]
    [PXPrimaryGraph(typeof(AFSetupMaint))]
    [PXCacheName("AWS Forecast Credentials")]
    public class AFSetup : IBqlTable
    {
        public abstract class aFAccessKey : PX.Data.IBqlField { }
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Amazon Forecast Access Key")]
        public virtual string AFAccessKey { get; set; }

        public abstract class aFSecretKey : PX.Data.IBqlField { }
        [PXDBString()]
        [PXUIField(DisplayName = "Amazon Forecast Secret Key")]
        public virtual string AFSecretKey { get; set; }

        public abstract class aFBucketName : PX.Data.IBqlField { }
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Amazon Forecast Bucket Name")]
        public virtual string AFBucketName { get; set; }

        public abstract class aFDirectoryName : PX.Data.IBqlField { }
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Input Directory Name")]
        public virtual string AFDirectoryName { get; set; }

        public abstract class aFOutDirectoryName : PX.Data.IBqlField { }
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Output Directory Name")]
        public virtual string AFOutDirectoryName { get; set; }


        #region tstamp

        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
    }
}
