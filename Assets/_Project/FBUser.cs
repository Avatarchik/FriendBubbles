﻿using UnityEngine;
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
	public enum Type {SELF, FRIEND};


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
	public string id;

	[JsonProperty]
	private new string name
	{
		get{return this.gameObject.name;}
		set{this.gameObject.name = value;}
	}
	[JsonProperty]
	private FBUser.FBPicture picture = new FBPicture();

	private JObject jObjectData;

	private Type _type;
	public Type type
	{
		set
		{
			this._type = value;

			CircleCollider2D collider2D = this.GetComponent<CircleCollider2D>();
			if(this._type == Type.FRIEND)
			{
				if(collider2D == null)
					collider2D = this.gameObject.AddComponent<CircleCollider2D>();
				collider2D.radius = 0.5f;
			}
			else
			{
				if(collider2D != null)
					GameObject.Destroy(collider2D);
			}
		}
		get
		{
			return this._type;
		}
	}
	
	private bool clicked = false;
	// ----------
	
	private void LateUpdate()
	{
		this.transform.eulerAngles = Vector3.zero;
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
			this.type == Type.SELF ? Vector3.one * 3f : Vector3.one * Random.Range(0.7f, 1.3f),
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
