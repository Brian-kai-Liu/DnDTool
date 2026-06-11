using UnityEngine;
using TEngine;
using Log = TEngine.Log;

namespace GameLogic
{
    [Window(UILayer.UI, location : "HomeUI")]
    public partial class HomeUI
    {
        protected override void OnCreate()
        {
        }

        #region 事件

        private partial void OnClickCreateModuleBtn()
        {
            GameModule.UI.CloseUI<HomeUI>();
            GameModule.UI.ShowUIAsync<CreateModuleBasicInfoUI>();
        }

        private partial void OnClickBrowseModuleBtn()
        {
            Log.Info("主界面：浏览模组功能暂未接入。");
        }

        private partial void OnClickCharacterCardBtn()
        {
            GameModule.UI.CloseUI<HomeUI>();
            GameModule.UI.ShowUIAsync<CharacterCardManagementUI>();
        }

        private partial void OnClickItemInfoEditorBtn()
        {
            GameModule.UI.CloseUI<HomeUI>();
            GameModule.UI.ShowUIAsync<ItemInfoEditorUI>();
        }

        private partial void OnClickSettingsBtn()
        {
            Log.Info("主界面：设置功能暂未接入。");
        }

        private partial void OnClickExitBtn()
        {
            Log.Info("主界面：退出应用。");
            Application.Quit();
        }

        #endregion
    }
}
