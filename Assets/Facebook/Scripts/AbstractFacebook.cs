using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Facebook
{
    public delegate void InitDelegate();
    public delegate void FacebookDelegate(FBResult result);
    public delegate void HideUnityDelegate(bool isUnityShown);

    public abstract class AbstractFacebook : MonoBehaviour
    {
        public const string GraphUrl = "https://graph.facebook.com";

        internal const string AccessTokenKey = "access_token";

        protected bool isInitialized;
        protected bool isLoggedIn;
        protected string userId;
        protected string accessToken;
        protected DateTime accessTokenExpiresAt;
        protected bool limitEventUsage;

        private List<FacebookDelegate> authDelegates;
        private int nextAsyncId;
        private Dictionary<string, FacebookDelegate> facebookDelegates;

        #region IFacebook API

        void Awake()
        {
            DontDestroyOnLoad(this);
            isInitialized = false;
            isLoggedIn = false;
            userId = "";
            accessToken = "";
            accessTokenExpiresAt = DateTime.MinValue;

            authDelegates = new List<FacebookDelegate>();
            nextAsyncId = 0;
            facebookDelegates = new Dictionary<string, FacebookDelegate>();

            // run whatever else needs to be setup
            OnAwake();
        }

        // use this to call the rest of the Awake function
        protected abstract void OnAwake();

        public bool IsInitialized { get { return isInitialized; } }

        public bool IsLoggedIn { get { return isLoggedIn; } }

        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        public virtual string AccessToken
        {
            get { return accessToken; }
            set { accessToken = value; }
        }

        public virtual DateTime AccessTokenExpiresAt { get { return accessTokenExpiresAt; } }

        public abstract bool LimitEventUsage { get; set; }

        public abstract void Init(
                InitDelegate onInitComplete,
                string appId,
                bool cookie = false,
                bool logging = true,
                bool status = true,
                bool xfbml = false,
                string channelUrl = "",
                string authResponse = null,
                bool frictionlessRequests = false,
                HideUnityDelegate hideUnityDelegate = null);

        public abstract void Login(string scope = "", FacebookDelegate callback = null);
        public abstract void Logout();
        public virtual void GetAuthResponse(FacebookDelegate callback = null)
        {
            AddAuthDelegate(callback);
        }

        [Obsolete]
        public virtual void AppRequest(
            string message,
            string[] to = null,
            List<object> filters = null,
            string[] excludeIds = null,
            int? maxRecipients = null,
            string data = "",
            string title = "",
            FacebookDelegate callback = null)
        {
            AppRequest(message, null, null, to, filters, excludeIds, maxRecipients, data, title, callback);
        }

        public abstract void AppRequest(
            string message,
            OGActionType actionType,
            string objectId,
            string[] to = null,
            List<object> filters = null,
            string[] excludeIds = null,
            int? maxRecipients = null,
            string data = "",
            string title = "",
            FacebookDelegate callback = null);

        public abstract void FeedRequest(
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
            FacebookDelegate callback = null);

        public abstract void Pay(
            string product,
            string action = "purchaseitem",
            int quantity = 1,
            int? quantityMin = null,
            int? quantityMax = null,
            string requestId = null,
            string pricepointId = null,
            string testCurrency = null,
            FacebookDelegate callback = null);

        public abstract void GameGroupCreate(
            string name,
            string description,
            string privacy = "CLOSED",
            FacebookDelegate callback = null);

        public abstract void GameGroupJoin(
            string id,
            FacebookDelegate callback = null);

        public virtual void API(
            string query,
            HttpMethod method,
            Dictionary<string, string> formData = null,
            FacebookDelegate callback = null)
        {

            Dictionary<string, string> inputFormData;
            // Copy the formData by value so it's not vulnerable to scene changes and object deletions
            inputFormData = (formData != null) ? CopyByValue(formData) : new Dictionary<string, string>();
            if (!inputFormData.ContainsKey(AccessTokenKey) && !query.Contains("access_token="))
            {
              inputFormData[AccessTokenKey] = AccessToken;
            }

            AsyncRequestString.Request(GetGraphUrl(query), method, inputFormData, callback);
        }

        public virtual void API(
            string query,
            HttpMethod method,
            WWWForm formData,
            FacebookDelegate callback = null)
        {

            if (formData == null)
            {
                formData = new WWWForm();
            }

            formData.AddField(AccessTokenKey, AccessToken);

            AsyncRequestString.Request(GetGraphUrl(query), method, formData, callback);
        }

        public abstract void PublishInstall(string appId, FacebookDelegate callback = null);

        public abstract void ActivateApp(string appId = null);

        public abstract void GetDeepLink(FacebookDelegate callback);

        #region FBAppEvents

        public abstract void AppEventsLogEvent(
            string logEvent,
            float? valueToSum = null,
            Dictionary<string, object> parameters = null);

        public abstract void AppEventsLogPurchase(
            float logPurchase,
            string currency = "USD",
            Dictionary<string, object> parameters = null);

        #endregion

        #endregion

        #region Callback Manager

        protected void AddAuthDelegate(FacebookDelegate callback)
        {
            authDelegates.Add(callback);
        }

        protected void OnAuthResponse(FBResult result)
        {
            var status = new Dictionary<string, object>();
            status["is_logged_in"] = IsLoggedIn;
            status["user_id"] = UserId;
            status["access_token"] = AccessToken;
            status["access_token_expires_at"] = AccessTokenExpiresAt;
            var formattedResult = new FBResult(Facebook.MiniJSON.Json.Serialize(status), result.Error);

            foreach (FacebookDelegate callback in authDelegates)
            {
                if (callback != null)
                {
                    callback(formattedResult);
                }
            }
            authDelegates.Clear();
        }

        protected string AddFacebookDelegate(FacebookDelegate callback)
        {
            nextAsyncId++;
            facebookDelegates.Add(nextAsyncId.ToString(), callback);
            return nextAsyncId.ToString();
        }

        protected void OnFacebookResponse(string uniqueId, FBResult result)
        {
            FacebookDelegate callback;
            if (facebookDelegates.TryGetValue(uniqueId, out callback))
            {
                if (callback != null)
                {
                    callback(result);
                }
                facebookDelegates.Remove(uniqueId);
            }
        }

        #endregion

        #region Misc Helper functions

        protected Dictionary<string, string> CopyByValue(Dictionary<string, string> data)
        {
            var newData = new Dictionary<string, string>(data.Count);
            foreach (KeyValuePair<string, string> kvp in data)
            {
                newData[kvp.Key] = String.Copy(kvp.Value);
            }
            return newData;
        }

        private string GetGraphUrl(string query)
        {
            if (!query.StartsWith("/"))
            {
                query = "/" + query;
            }
            return GraphUrl + query;
        }

        #endregion

    }
}
