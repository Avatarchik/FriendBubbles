using UnityEngine;
using System.Collections.Generic;

namespace Facebook
{
    internal class JsBridge : MonoBehaviour
    {

        private CanvasFacebook facebook;

        void Start()
        {
            facebook = FBComponentFactory.GetComponent<CanvasFacebook>(IfNotExist.ReturnNull);
        }

        void OnFacebookAuthResponse(string responseJsonData = "")
        {
            facebook.OnFacebookAuthResponse(responseJsonData);
        }

        void OnFacebookAuthResponseChange(string responseJsonData = "")
        {
            facebook.OnFacebookAuthResponseChange(responseJsonData);
        }

        void OnFacebookUiResponse(string responseJsonData = "")
        {
            facebook.OnFacebookUiResponse(responseJsonData);
        }

        void OnFacebookFocus(string state)
        {
            if (facebook.OnHideUnity != null)
            {
                facebook.OnHideUnity((state != "hide"));
            }
        }

        void OnInit(string responseJsonData = "")
        {
            if (!string.IsNullOrEmpty(responseJsonData))
            {
                OnFacebookAuthResponse(responseJsonData);
            }
            facebook.SetInitComplete();
        }

        void OnUrlResponse(string url = "")
        {
            facebook.OnUrlResponse(url);
        }
    }
}