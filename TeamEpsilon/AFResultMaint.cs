using System;
using PX.Data;

namespace AF
{
    public class AFResultMaint : PXGraph<AFResultMaint>
    {

        public PXSelect<AFResult> AFResultView;
        public PXSave<AFResult> Save;
        public PXCancel<AFResult> Cancel;

        public PXSetup<AFSetup> Setup;


    }
}