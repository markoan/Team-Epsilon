using System;
using PX.Data;

namespace AF
{
    public class AFSetupMaint : PXGraph<AFSetupMaint>
    {

        public PXSelect<AFSetup> AFSetupView;
        public PXSave<AFSetup> Save;
        public PXCancel<AFSetup> Cancel;

    }
}