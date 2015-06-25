//This file is a shared enum between objective-c and c#
//That's a little bit weird, but its better than keeping two enums in sync
namespace NativeDialogModes
{
#if !__cplusplus    
    public
#endif

    enum DialogMode {
      WEBVIEW_DIALOG_MODE = 0,
      FAST_APP_SWITCH_SHARE_DIALOG,
    };

}
