using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

/**
 * Single user script
 * 
 * @author Yaniv Peer
 */
[DataContract]
public class FBUser : MonoBehaviour 
{
	public enum Type {DRAGGER, CLICKER};


	// ----------

	// Data structure according to Facebook's own JSON data structure
	[DataContract]
	public class FBPicture
	{
		[DataContract]
		public class Data
		{
			[JsonProperty]
			public string url;
		}

		[JsonProperty]
		public FBUser.FBPicture.Data data = new Data();
		public GameObject gameObject;

		// Clears the picture
		public void Clear()
		{
			if(this.gameObject != null)
				GameObject.Destroy(this.gameObject);

			this.data.url = "";
		}
	}

	[JsonProperty]
	private string id;
	[JsonProperty]
	private new string name
	{
		get{return this.gameObject.name;}
		set{this.gameObject.name = value;}
	}
	[JsonProperty]
	private FBUser.FBPicture picture = new FBPicture();

	private JObject jObjectData;

	public Type type;

	private bool dragging = false;
	private bool clicked = false;
	// ----------


	private void Awake()
	{
		CircleCollider2D collider2D = this.GetComponent<CircleCollider2D>();
		if(collider2D == null)
			collider2D = this.gameObject.AddComponent<CircleCollider2D>();
		collider2D.radius = 0.5f;

		EasyTouch.On_TouchStart += delegate(Gesture gesture) {
			if(gesture.pickedObject == this.gameObject)
			{
				if(this.type == Type.DRAGGER)
				{
					dragging = true;
				}
				else
				{

				}
			}
		};

		EasyTouch.On_TouchDown += delegate(Gesture gesture) {
			if(gesture.pickedObject == this.gameObject)
			{
				if(this.type == Type.DRAGGER && this.dragging)
				{
					Vector3 newPosition = Camera.main.ScreenToWorldPoint(gesture.position);
					newPosition.z = 0f;
					this.transform.parent.position = newPosition;
				}
				else
				{
					
				}
			}
		};

		EasyTouch.On_TouchUp += delegate(Gesture gesture) {
			if(gesture.pickedObject == this.gameObject)
			{
				if(this.type == Type.DRAGGER)
				{
					this.dragging = false;
				}
				else if(!this.clicked)
				{
					this.clicked = true;
					BubbleGraph.Create(this.jObjectData);
				}
			}
		};
	}

	private void Update()
	{

	}

	private void LateUpdate()
	{
		this.transform.eulerAngles = Vector3.zero;
		if(this.type == Type.DRAGGER)
			this.transform.localPosition = Vector3.zero;
	}

	// Initializes this object with a user's data
	public void Init(JObject p_jObject, FBUser.Type p_type)
	{
		// Fail safe
		if(p_jObject == null)
			return;

		this.jObjectData = p_jObject;
		this.type = p_type;

		this.Clear();

//		Debug.Log("Initializing " + p_jObject["name"]);
//		Debug.Log(p_jObject.ToString());

		JsonTools.PopulateJObjectToExistingObject(p_jObject, this);
		this.StartCoroutine(this.ReloadImage());
	}

	// Clears this object for another use
	private void Clear()
	{
		this.id = "";
		this.name = "";
		this.picture.Clear();
	}

	private void LoadImage(Texture2D p_texture)
	{
		// Create a new sprite
		Sprite newPictureSprite = Sprite.Create(
			p_texture,
			new Rect(0f, 0f, p_texture.width, p_texture.height),
			Vector2.one * 0.5f,
			128f);
		
		GameObject newPictureGameObject = new GameObject();
		newPictureGameObject.name = "Photo";
		SpriteRenderer newPictureSpriteRenderer = newPictureGameObject.AddComponent<SpriteRenderer>();
		newPictureSpriteRenderer.sprite = newPictureSprite;
		
		// Properly parent and store
		newPictureGameObject.transform.parent = this.transform;
		newPictureGameObject.transform.localPosition = Vector3.zero;
		this.picture.gameObject = newPictureGameObject;

		// Bounce in
		this.transform.localScale = Vector3.zero;
		LeanTween.scale(
			this.gameObject,
			Vector3.one,
			0.5f)
			.setDelay(UnityEngine.Random.Range(0.1f, 0.5f))
			.setEase(LeanTweenType.easeOutBounce);
	}

	private void OnFBPictureResult(FBResult p_results)
	{
		if(p_results.Texture != null)
			this.LoadImage(p_results.Texture);
		else
			Debug.LogError("Received bad image");
	}
	
	// Reloads an image
	private IEnumerator ReloadImage()
	{
		if(this.picture != null)
		{
			if(string.IsNullOrEmpty(this.picture.data.url))
			{
				// We don't have a URL, use User ID
				FB.API(
					"/" + this.id + "/picture",
					Facebook.HttpMethod.GET,
					this.OnFBPictureResult);
			}
			else
			{
				// We have a URL, bypass a Facebook API call and load directly
				while(string.IsNullOrEmpty(this.picture.data.url))
					yield return null;

				WWW www = new WWW(this.picture.data.url);
				yield return www;

				this.LoadImage(www.texture);
			}
		}
	}

	// Self destruct beautifully
	public void SelfDestruct()
	{
		// Bounce in
		LeanTween.scale(
			this.gameObject,
			Vector3.zero,
			0.5f)
			.setEase(LeanTweenType.easeOutBounce)
			.setOnComplete(delegate() {
				GameObject.Destroy(this.gameObject);
			});
	}
}
