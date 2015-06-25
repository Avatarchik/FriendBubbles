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
[RequireComponent(typeof(Rigidbody2D))]
public class BubbleGraph : MonoBehaviour 
{
	private List<FBUser> friends = new List<FBUser>();
	private FBUser me;

	public int maxFriends = 20;
	private float friendsDistance = 1f;

	public void Awake()
	{
		Rigidbody2D rigidBody2D = this.GetComponent<Rigidbody2D>();
		rigidBody2D.isKinematic = true;
		
		this.gameObject.name = "BubbleGraph";
	}

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

				this.me = newFBUser;
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
//					newFBUserGameObject.transform.parent = this.transform;

					float eulerAngle = Mathf.Deg2Rad * (((float)count / (float)this.maxFriends) * 360f);
					newFBUserGameObject.transform.localPosition = 
						new Vector3(
							this.friendsDistance * Mathf.Cos (eulerAngle),
							this.friendsDistance * Mathf.Sin (eulerAngle),
							0f);

					SpringJoint2D joint2D = newFBUserGameObject.GetComponent<SpringJoint2D>();
					if(joint2D == null)
						joint2D = newFBUserGameObject.AddComponent<SpringJoint2D>();
					joint2D.connectedBody = this.GetComponent<Rigidbody2D>();
					joint2D.distance = 2f;
					joint2D.frequency = 3f;
					joint2D.dampingRatio = 0f;

					Rigidbody2D rigidBody2D = newFBUserGameObject.GetComponent<Rigidbody2D>();
					rigidBody2D.drag = 10f;

					FBUser newFBUser = newFBUserGameObject.AddComponent<FBUser>();
					newFBUser.Init((JObject)userJToken);

					this.friends.Add(newFBUser);

					count++;
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

	public void SelfDestruct()
	{
		this.Clear();
		this.StartCoroutine(this.CoSelfDestruct());
	}

	private IEnumerator CoSelfDestruct()
	{
		FBUser[] fbUsers = GameObject.FindObjectsOfType<FBUser>();
		while(fbUsers.Length > 0)
		{
			yield return null;
			fbUsers = GameObject.FindObjectsOfType<FBUser>();
		}

		GameObject.Destroy(this.gameObject);
	}
}
