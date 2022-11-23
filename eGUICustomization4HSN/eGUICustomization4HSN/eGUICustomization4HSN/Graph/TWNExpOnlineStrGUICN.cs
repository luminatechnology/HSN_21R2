using PX.Data;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;

namespace eGUICustomization4HSN.Graph
{
    public class TWNExpOnlineStrGUICN : PXGraph<TWNExpOnlineStrGUICN>
    {
        #region Features
        public PXCancel<TWNGUITrans> Cancel;
        public PXProcessing<TWNGUITrans,
                            Where<TWNGUITrans.eGUIExcluded, NotEqual<True>,
                                  And<TWNGUITrans.gUIFormatcode, Equal<PX.Objects.AR.ARRegisterExt.VATOut33Att>,
                                      And2<Where<TWNGUITrans.eGUIExported, Equal<False>,
                                                 Or<TWNGUITrans.eGUIExported, IsNull>>,
                                            And<TWNGUITrans.isOnlineStore, Equal<True>>>>>> GUITranProc;
        #endregion

        #region Ctor
        public TWNExpOnlineStrGUICN()
        {
            GUITranProc.SetProcessCaption(ActionsMessages.Upload);
            GUITranProc.SetProcessAllCaption(TWMessages.UploadAll);
            GUITranProc.SetProcessDelegate(TWNExpOnlineStrGUIInv.Upload);
        }
        #endregion 
    }
}