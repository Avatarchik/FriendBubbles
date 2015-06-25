using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Facebook
{
    public sealed class CanvasFacebook : AbstractFacebook
    {
        internal const string MethodAppRequests = "apprequests";
        internal const string MethodFeed = "feed";
        internal const string MethodPay = "pay";
        internal const string MethodGameGroupCreate = "game_group_create";
        internal const string MethodGameGroupJoin = "game_group_join";
        //internal const string AccessTokenKey = "access_token";
        internal const string CancelledResponse = "{\"cancelled\":true}";
        internal const string FacebookConnectURL = "https://connect.facebook.net";

        private InitDelegate onInitComplete;
        internal HideUnityDelegate OnHideUnity;

        private string integrationMethodJs = "";

        private string appId;

        private string sdkLocale = "en_US";

        private bool sdkDebug = false;

        private string deepLink;

        protected override void OnAwake()
        {
            #region Javascript stuff
            // Facebook JS Bridge lives in it's own gameobject for optimization reasons
            // see UnityObject.SendMessage()
            var bridgeObject = new GameObject("FacebookJsBridge");
            bridgeObject.AddComponent<JsBridge>();
            bridgeObject.transform.parent = gameObject.transform;
#if !UNITY_EDITOR
            integrationMethodJs = StringFromFile("JSSDKBindings");
#endif
            #endregion
        }

        #region Facebook API

        public override bool LimitEventUsage
        {
            get
            {
                return limitEventUsage;
            }
            set
            {
                limitEventUsage = value;
            }
        }

        public override void Init(
                InitDelegate onInitComplete,
                string appId,
                bool cookie = false,
                bool logging = true,
                bool status = true,
                bool xfbml = false,
                string channelUrl = "",
                string authResponse = null,
                bool frictionlessRequests = false,
                HideUnityDelegate hideUnityDelegate = null)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentException("appId cannot be null or empty!");
            }
            if (integrationMethodJs == null)
            {
#if !UNITY_EDITOR
                throw new Exception("Cannot initialize facebook javascript");
#endif
            }
            this.onInitComplete = onInitComplete;
            OnHideUnity = hideUnityDelegate;

            var parameters = new Dictionary<string, object>();

            this.appId = appId;
            parameters.Add("appId", appId);

            if (cookie != false)
            {
                parameters.Add("cookie", true);
            }
            if (logging != true)
            {
                parameters.Add("logging", false);
            }
            if (status != true)
            {
                parameters.Add("status", false);
            }
            if (xfbml != false)
            {
                parameters.Add("xfbml", true);
            }
            if (!string.IsNullOrEmpty(channelUrl))
            {
                parameters.Add("channelUrl", channelUrl);
            }
            if (!string.IsNullOrEmpty(authResponse))
            {
                parameters.Add("authResponse", authResponse);
            }
            if (frictionlessRequests != false)
            {
                parameters.Add("frictionlessRequests", true);
            }

            // Post-F8 SDK mandates specifying a version
            parameters.Add("version", "v2.2");

            var paramJson = MiniJSON.Json.Serialize(parameters);

            Application.ExternalEval(integrationMethodJs);

            bool isPlayer = true;
#if UNITY_WEBGL
            isPlayer = false;
#endif
            // use 1/0 for booleans, otherwise you'll get strings "True"/"False"
            Application.ExternalCall("FBUnity.init", isPlayer ? 1 : 0, FacebookConnectURL, sdkLocale, sdkDebug ? 1 : 0, paramJson, status ? 1 : 0);

        }

        public override void Login(string scope = "", FacebookDelegate callback = null)
        {
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }

            AddAuthDelegate(callback);
            Application.ExternalCall("FBUnity.login", scope);
        }

        public override void Logout()
        {
            accessToken = "";
            accessTokenExpiresAt = DateTime.MinValue;
            isLoggedIn = false;
            userId = "";
            Application.ExternalCall("FBUnity.logout");
        }

        public override void AppRequest(
                string message,
                OGActionType actionType,
                string objectId,
                string[] to = null,
                List<object> filters = null,
                string[] excludeIds = null,
                int? maxRecipients = null,
                string data = "",
                string title = "",
                FacebookDelegate callback = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message", "message cannot be null or empty!");
            }

            if (actionType != null && string.IsNullOrEmpty(objectId))
            {
                throw new ArgumentNullException("objectId", "You cannot provide an actionType without an objectId");
            }

            if (actionType == null && !string.IsNullOrEmpty(objectId))
            {
                throw new ArgumentNullException("actionType", "You cannot provide an objectId without an actionType");
            }

            var paramsDict = new Dictionary<string, object>();
            paramsDict["message"] = message;
            if (to != null)
            {
                paramsDict["to"] = string.Join(",", to);
            }
            if (actionType != null && !string.IsNullOrEmpty(objectId))
            {
                paramsDict["action_type"] = actionType.ToString();
                paramsDict["object_id"] = objectId;
            }
            if (filters != null)
            {
                paramsDict["filters"] = filters;
            }
            if (excludeIds != null)
            {
                paramsDict["exclude_ids"] = excludeIds;
            }
            if (maxRecipients.HasValue)
            {
                paramsDict["max_recipients"] = maxRecipients.Value;
            }
            if (!string.IsNullOrEmpty(data))
            {
                paramsDict["data"] = data;
            }
            if (!string.IsNullOrEmpty(title))
            {
                paramsDict["title"] = title;
            }

            UI(MethodAppRequests, paramsDict, callback);
        }

        public override void PublishInstall(string appId, FacebookDelegate callback = null)
        {
            FBDebug.Info("There's no reason to call this on Facebook Canvas.");
        }
        public override void ActivateApp(string appId = null)
        {
            Application.ExternalCall("FBUnity.activateApp");
        }

        public override void FeedRequest(
                string toId = "",
                string link = "",
                string linkName = "",
                string linkCaption = "",
                string linkDescription = "",
                string picture = "",
                string mediaSource = "",
                string actionName = "",
                string actionLink = "",
                string reference = "",
                Dictionary<string, string[]> properties = null,
                FacebookDelegate callback = null)
        {

            Dictionary<string, object> paramsDict = new Dictionary<string, object>();
            // Marshal all the above into the thing
            if (!string.IsNullOrEmpty(toId))
            {
                paramsDict["to"] = toId;
            }

            if (!string.IsNullOrEmpty(link))
            {
                paramsDict["link"] = link;
            }

            if (!string.IsNullOrEmpty(linkName))
            {
                paramsDict["name"] = linkName;
            }

            if (!string.IsNullOrEmpty(linkCaption))
            {
                paramsDict["caption"] = linkCaption;
            }

            if (!string.IsNullOrEmpty(linkDescription))
            {
                paramsDict["description"] = linkDescription;
            }

            if (!string.IsNullOrEmpty(picture))
            {
                paramsDict["picture"] = picture;
            }

            if (!string.IsNullOrEmpty(mediaSource))
            {
                paramsDict["source"] = mediaSource;
            }

            if (!string.IsNullOrEmpty(actionName) && !string.IsNullOrEmpty(actionLink))
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["name"] = actionName;
                dict["link"] = actionLink;

                paramsDict["actions"] = MiniJSON.Json.Serialize(new[] { dict });
            }

            if (!string.IsNullOrEmpty(reference))
            {
                paramsDict["ref"] = reference;
            }

            if (properties != null)
            {
                Dictionary<string, object> newObj = new Dictionary<string, object>();
                foreach (KeyValuePair<string, string[]> pair in properties)
                {
                    if (pair.Value.Length < 1)
                        continue;

                    if (pair.Value.Length == 1)
                    {
                        // String-string
                        newObj.Add(pair.Key, pair.Value[0]);
                    }
                    else
                    {
                        // String-Object with two parameters
                        Dictionary<string, object> innerObj = new Dictionary<string, object>();

                        innerObj.Add("text", pair.Value[0]);
                        innerObj.Add("href", pair.Value[1]);

                        newObj.Add(pair.Key, innerObj);
                    }
                }
                paramsDict.Add("properties", MiniJSON.Json.Serialize(newObj));
            }

            UI(MethodFeed, paramsDict, callback);
        }

        public override void Pay(
            string product,
            string action = "purchaseitem",
            int quantity = 1,
            int? quantityMin = null,
            int? quantityMax = null,
            string requestId = null,
            string pricepointId = null,
            string testCurrency = null,
            FacebookDelegate callback = null)
        {
            Dictionary<string, object> paramsDict = new Dictionary<string, object>();
            paramsDict["product"] = product;
            paramsDict["action"] = action;
            paramsDict["quantity"] = quantity;

            if (quantityMin.HasValue)
            {
                paramsDict["quantity_min"] = quantityMin.Value;
            }

            if (quantityMax.HasValue)
            {
                paramsDict["quantity_max"] = quantityMax.Value;
            }

            if (!string.IsNullOrEmpty(requestId))
            {
                paramsDict["request_id"] = requestId;
            }

            if (!string.IsNullOrEmpty(pricepointId))
            {
                paramsDict["pricepoint_id"] = pricepointId;
            }

            if (!string.IsNullOrEmpty(testCurrency))
            {
                paramsDict["test_currency"] = testCurrency;
            }

            UI(MethodPay, paramsDict, callback);
        }

        public override void GameGroupCreate(
            string name,
            string description,
            string privacy = "CLOSED",
            FacebookDelegate callback = null)
        {
            Dictionary<string, object> paramsDict = new Dictionary<string, object>();
            paramsDict["name"] = name;
            paramsDict["description"] = description;
            paramsDict["privacy"] = privacy;
            paramsDict["display"] = "async";

            UI(MethodGameGroupCreate, paramsDict, callback);
        }

        public override void GameGroupJoin(
            string id,
            FacebookDelegate callback = null)
        {
            Dictionary<string, object> paramsDict = new Dictionary<string, object>();
            paramsDict["id"] = id;
            paramsDict["display"] = "async";

            UI(MethodGameGroupJoin, paramsDict, callback);
        }

        internal void UI(
                string method,
                Dictionary<string, object> paramsDict,
                FacebookDelegate callback = null)
        {
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }

            var cloneParamsDict = new Dictionary<string, object>(paramsDict);
            cloneParamsDict["app_id"] = appId;
            var uniqueId = AddFacebookDelegate(callback);
            cloneParamsDict["method"] = method;
            Application.ExternalCall("FBUnity.ui", MiniJSON.Json.Serialize(cloneParamsDict), uniqueId);
        }

        public override void GetDeepLink(FacebookDelegate callback)
        {
            if (callback != null)
            {
                callback(new FBResult(deepLink));
            }
        }

        public override void AppEventsLogEvent(
            string logEvent,
            float? valueToSum = null,
            Dictionary<string, object> parameters = null)
        {
            Application.ExternalCall(
                "FBUnity.logAppEvent",
                logEvent,
                valueToSum,
                MiniJSON.Json.Serialize(parameters)
            );
        }

        public override void AppEventsLogPurchase(
            float logPurchase,
            string currency = "USD",
            Dictionary<string, object> parameters = null)
        {
            Application.ExternalCall(
                "FBUnity.logPurchase",
                logPurchase,
                currency,
                MiniJSON.Json.Serialize(parameters)
            );
        }

        #endregion

        #region Facebook JS Bridge calls
        internal void OnFacebookAuthResponse(string responseJsonData = "")
        {
            var loginStatus = MiniJSON.Json.Deserialize(responseJsonData) as Dictionary<string, object>;

            // if we don't have an authResponse that means the player
            // hit cancel
            if (loginStatus["authResponse"] == null)
            {
                OnAuthResponse(new FBResult(responseJsonData));
                return;
            }

            var authResponse = loginStatus["authResponse"] as Dictionary<string, object>;

            if (!string.IsNullOrEmpty(authResponse["accessToken"] as string))
            {
                accessToken = authResponse["accessToken"] as string;
                accessTokenExpiresAt = DateTime.Now.AddSeconds((Int64)authResponse["expiresIn"]);

                // empty string is a "Start Now" user
                userId = (!string.IsNullOrEmpty(authResponse["userID"] as string)) ? authResponse["userID"] as string : "";
                isLoggedIn = true;
            }
            else
            {
                accessToken = "";
                accessTokenExpiresAt = DateTime.MinValue;
                userId = "";
                isLoggedIn = false;
            }

            // Call all our callbacks.
            OnAuthResponse(new FBResult(responseJsonData));
        }

        // used only to refresh the access token
        internal void OnFacebookAuthResponseChange(string responseJsonData = "")
        {
            var loginStatus = MiniJSON.Json.Deserialize(responseJsonData) as Dictionary<string, object>;

            if (loginStatus["authResponse"] == null)
            {
                return;
            }

            var authResponse = loginStatus["authResponse"] as Dictionary<string, object>;
            accessToken = authResponse["accessToken"] as string;
            accessTokenExpiresAt = DateTime.Now.AddSeconds((Int64)authResponse["expiresIn"]);
        }

        internal void OnFacebookUiResponse(string responseJsonData = "")
        {
            var result = (Dictionary<string, object>)MiniJSON.Json.Deserialize(responseJsonData);
            // if the result["response"] is null then we treat it like the dialog was cancelled
            var serializedResponse = (result.ContainsKey("response") && result["response"] != null) ? MiniJSON.Json.Serialize(result["response"]) : CancelledResponse;
            OnFacebookResponse((string)result["uid"], new FBResult(serializedResponse));
        }


        internal void SetInitComplete()
        {
            this.isInitialized = true;
            if (onInitComplete != null)
            {
                onInitComplete();
            }
        }

        internal void OnUrlResponse(string url)
        {
            deepLink = url;
        }

        #endregion

        private string StringFromFile(string resourceName)
        {
            TextAsset ta = Resources.Load(resourceName) as TextAsset;
            if(ta) 
            {
                return ta.text;
            }

            return null;
        }
    }
}
