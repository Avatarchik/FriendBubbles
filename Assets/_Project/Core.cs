using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class Core : MonoBehaviour 
{
	public SpriteRenderer goodFacebookLogo;
	public SpriteRenderer badFacebookLogo;
	public GameObject fbLoginButton;

	private string fbToken;

	// Helper function
	private void SetSpriteAlpha(SpriteRenderer p_spriteRenderer, float p_alpha)
	{
		Color color = p_spriteRenderer.color;
		color.a = p_alpha;
		p_spriteRenderer.color = color;
	}

	private void Awake()
	{
		this.fbLoginButton.gameObject.SetActive(false);

		this.SetSpriteAlpha(this.goodFacebookLogo, 0f);
//		this.SetSpriteAlpha(this.badFacebookLogo, 0f);
	}

	public void Start()
	{
		FB.Init(this.OnFBInit);
	}

	// called when facebook has been init
	private void OnFBInit()
	{
		this.fbLoginButton.SetActive(true);
	}

	// Call this to ignite login
	public void FBLogin()
	{
		// Double check login status
		if(!FB.IsLoggedIn)
			FB.Login("user_friends", this.OnFBLoggedIn);
	}

	// Called when logged in to Facebook
	private void OnFBLoggedIn(FBResult p_result)
	{
//		Debug.Log(p_result.Text);
		if(string.IsNullOrEmpty(p_result.Error))
		{
			this.fbLoginButton.SetActive(false);

			JToken resultJToken = JsonTools.ConvertStringToJToken(p_result.Text);
			if(resultJToken != null)
			{
				this.fbToken = (string)resultJToken["access_token"];
				LeanTween.alpha(
					this.goodFacebookLogo.gameObject,
					1f,
					1f)
					.setEase(LeanTweenType.easeInOutSine);

				BubbleGraph.Create((string)resultJToken["user_id"]);
			}
			else
			{
				Debug.LogError("Problem reading Facebook login result JSON");
			}
		}
		else
		{
			LeanTween.alpha(
				this.goodFacebookLogo.gameObject,
				0f,
				1f)
				.setEase(LeanTweenType.easeInOutSine);
		}
	}
}
