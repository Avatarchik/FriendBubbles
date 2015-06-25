using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/**
 * Bubble graph of multiple users
 * 
 * @author Yaniv Peer
 */
public class BubbleGraph : MonoBehaviour 
{
	private List<FBUser> friends = new List<FBUser>();
	private FBUser me;

	public int maxFriends = 5;

	public void Init(JObject p_userDataJToken)
	{
		Dictionary<string, string> fbFormData = new Dictionary<string, string>();
		fbFormData.Add("limit", this.maxFriends.ToString());

		// Clear this person's friends
		this.Clear();

		FB.API(
			"/" + p_userDataJToken["user_id"] + "/",
			Facebook.HttpMethod.GET,
			this.OnMeResults,
			fbFormData);

		FB.API(
			"/" + p_userDataJToken["user_id"] + "/taggable_friends",
			Facebook.HttpMethod.GET,
			this.OnFriendsResults,
			fbFormData);
	}

	private void OnMeResults(FBResult p_results)
	{
		if(string.IsNullOrEmpty(p_results.Error))
		{
//			Debug.Log(p_results.Text);
			JToken fbResultJToken = JsonTools.ConvertStringToJToken(p_results.Text);
			if(fbResultJToken != null)
			{
				// Create a new FBUser object inside a new GameObject
				GameObject newFBUserGameObject = new GameObject();
				newFBUserGameObject.transform.parent = this.transform;
				newFBUserGameObject.transform.localPosition = Vector3.zero;
				FBUser newFBUser = newFBUserGameObject.AddComponent<FBUser>();
				newFBUser.Init((JObject)fbResultJToken);
			}
		}
	}

	private void OnFriendsResults(FBResult p_results)
	{
		if(string.IsNullOrEmpty(p_results.Error))
		{
			int count = 0;

			JToken fbResultJToken = JsonTools.ConvertStringToJToken(p_results.Text);
			if(fbResultJToken != null)
			{
				foreach(JToken userJToken in (JArray)(fbResultJToken["data"]))
				{
					// Create a new FBUser object inside a new GameObject
					GameObject newFBUserGameObject = new GameObject();
					newFBUserGameObject.transform.parent = this.transform;

					FBUser newFBUser = newFBUserGameObject.AddComponent<FBUser>();
					newFBUser.Init((JObject)userJToken);
				}
			}
		}
		else
		{
			Debug.Log(p_results.Error);
		}
	}

	private void Clear()
	{
		if(this.me != null)
			this.me.SelfDestruct();

		// Destroy all users
		foreach(FBUser fbUser in this.friends)
			if(fbUser != null)
				fbUser.SelfDestruct();

		// Clear the list for next use
		this.friends.Clear();
	}
}
