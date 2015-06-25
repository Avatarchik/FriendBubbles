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

	// ----------

	// Initializes this object with a user's data
	public void Init(JObject p_jObject)
	{
		// Fail safe
		if(p_jObject == null)
			return;

		this.Clear();

		Debug.Log("Initializing " + p_jObject["name"]);
		Debug.Log(p_jObject.ToString());

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

	// Reloads an image
	private IEnumerator ReloadImage()
	{
		if(this.picture != null)
		{
			// Make sure the image is cleared
			this.picture.Clear();

			WWW www = new WWW(this.picture.data.url);
			yield return www;

			// Create a new sprite
			Sprite newPictureSprite = Sprite.Create(
				www.texture,
				new Rect(0f, 0f, www.texture.width, www.texture.height),
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
		}
	}

	// Self destruct beautifully
	public void SelfDestruct()
	{
		// TODO Pretty animation
		GameObject.Destroy(this.gameObject);
	}
}
