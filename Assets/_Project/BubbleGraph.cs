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

	public int maxFriends = 30;
	private float friendsDistance = 1f;
	private string userId = "";
	private string nextPage = "";
	private bool dragging = false;

	public void Awake()
	{
		Rigidbody2D rigidBody2D = this.GetComponent<Rigidbody2D>();
		rigidBody2D.isKinematic = true;
		
		this.gameObject.name = "BubbleGraph";

		CircleCollider2D collider = this.GetComponent<CircleCollider2D>();
		if(collider == null)
			collider = this.gameObject.AddComponent<CircleCollider2D>();

		collider.radius = 4f;

		EasyTouch.On_SimpleTap += this.OnTap;
		EasyTouch.On_TouchStart += this.OnTouchStart;
		EasyTouch.On_TouchDown += this.OnTouchDown;
		EasyTouch.On_TouchUp += this.OnTouchUp;
	}

	private void OnDestroy()
	{
		EasyTouch.On_SimpleTap -= this.OnTap;
		EasyTouch.On_TouchStart -= this.OnTouchStart;
		EasyTouch.On_TouchDown -= this.OnTouchDown;
		EasyTouch.On_TouchUp -= this.OnTouchUp;
	}

	
	private void OnTouchStart(Gesture p_gesture)
	{
		if(p_gesture.pickedObject == this.gameObject)
		{
			dragging = true;
		}
	}
	
	private void OnTouchDown(Gesture p_gesture)
	{
		if(p_gesture.pickedObject == this.gameObject)
		{
			if(this.dragging)
			{
				Vector3 newPosition = Camera.main.ScreenToWorldPoint(p_gesture.position);
				newPosition.z = 0f;
				this.transform.position = newPosition;
			}
		}
	}
	
	private void OnTouchUp(Gesture p_gesture)
	{
		this.dragging = false;
	}

	private void OnTap(Gesture p_gesture)
	{
		this.Refresh();
	}
	
	public static BubbleGraph Create(string p_userId)
	{
		GameObject newBubbleGraphGameObject = new GameObject();
		newBubbleGraphGameObject.transform.position = Vector3.zero;
		BubbleGraph bubbleGraph = newBubbleGraphGameObject.AddComponent<BubbleGraph>();
		bubbleGraph.Init(p_userId);

		return bubbleGraph;
	}

	public void Init(string p_userId)
	{
		this.userId = p_userId;
		
		// Clear this person's friends
		this.Clear();

		if(this.me == null)
		{
			FB.API(
				"/" + p_userId + "/",
				Facebook.HttpMethod.GET,
				this.OnMeResults);
		}

		Dictionary<string, string> fbFormData = new Dictionary<string, string>();
		fbFormData.Add("limit", this.maxFriends.ToString());
		if(!string.IsNullOrEmpty(this.nextPage))
		{
			Debug.Log(this.nextPage);
			this.StartCoroutine(this.LoadFBURL(this.nextPage, this.OnFriendsResults));
//			FB.API(
//				this.nextPage,
//				Facebook.HttpMethod.GET,
//				this.OnFriendsResults,
//				fbFormData);
		}
		else
		{
			FB.API(
				"/" + p_userId + "/taggable_friends",
				Facebook.HttpMethod.GET,
				this.OnFriendsResults,
				fbFormData);
		}
	}

	// Helper function
	public IEnumerator LoadFBURL(string p_fbURL, Facebook.FacebookDelegate p_callback)
	{
		WWW www = new WWW(p_fbURL);
		yield return www;
		p_callback(new FBResult(www));
	}
	
	private void OnMeResults(FBResult p_results)
	{
		if(string.IsNullOrEmpty(p_results.Error))
		{
			JToken fbResultJToken = JsonTools.ConvertStringToJToken(p_results.Text);
			if(fbResultJToken != null)
			{
				// Create a new FBUser object inside a new GameObject
				GameObject newFBUserGameObject = new GameObject();
				newFBUserGameObject.transform.parent = this.transform;
				newFBUserGameObject.transform.localPosition = Vector3.zero;
				FBUser newFBUser = newFBUserGameObject.AddComponent<FBUser>();
				newFBUser.Init((JObject)fbResultJToken, FBUser.Type.SELF);

				this.me = newFBUser;
			}
		}
	}

	private void OnFriendsResults(FBResult p_results)
	{
		Debug.Log(p_results.Text);

		if(string.IsNullOrEmpty(p_results.Error))
		{
			int count = 0;

			JToken fbResultJToken = JsonTools.ConvertStringToJToken(p_results.Text);
			if(fbResultJToken != null)
			{
				this.nextPage = (string)fbResultJToken["paging"]["next"];
				foreach(JToken userJToken in (JArray)(fbResultJToken["data"]))
				{
					// Create a new FBUser object inside a new GameObject
					GameObject newFBUserGameObject = new GameObject();
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
					newFBUser.Init((JObject)userJToken, FBUser.Type.FRIEND);

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
		// Destroy all users
		foreach(FBUser fbUser in this.friends)
			if(fbUser != null)
				fbUser.SelfDestruct();
	}

	public void SelfDestruct()
	{
		this.Clear();
		if(this.me != null)
			this.me.SelfDestruct();
		this.StartCoroutine(this.CoSelfDestruct());
	}

	public void Refresh()
	{
		this.Init(this.userId);
	}

	private IEnumerator CoSelfDestruct()
	{
		bool foundActive;
		do
		{
			foundActive = false;
			foreach(FBUser fbUser in this.friends)
			{
				if(fbUser != null)
					foundActive = true;
			}
			yield return null;
		}while(foundActive);

//		FBUser[] fbUsers = GameObject.FindObjectsOfType<FBUser>();
//		while(fbUsers.Length > 0)
//		{
//			yield return null;
//			fbUsers = GameObject.FindObjectsOfType<FBUser>();
//		}

		GameObject.Destroy(this.gameObject);
	}
}
