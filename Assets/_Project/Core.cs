using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Core : MonoBehaviour 
{
	public GameObject goodFacebookLogo;
	public GameObject badFacebookLogo;
	public GameObject fbLoginButton;

	private string fbToken;

	private void Awake()
	{
		this.goodFacebookLogo.SetActive(false);
		this.badFacebookLogo.SetActive(false);
		this.fbLoginButton.gameObject.SetActive(false);
	}

	public void Start()
	{
		FB.Init(this.OnFBInit);
	}

	private void OnFBInit()
	{
		this.fbLoginButton.SetActive(true);
	}

	// Called when Facebook API initialized
	public void FBLogin()
	{
		// Double check login status
		if(!FB.IsLoggedIn)
			FB.Login("", this.OnFBLoggedIn);
	}

	// Called when logged in to Facebook
	private void OnFBLoggedIn(FBResult p_result)
	{
		Debug.Log(p_result.Text);
		Debug.Log(p_result.Error);
		
		if(string.IsNullOrEmpty(p_result.Error))
		{
			JToken resultJToken = JsonTools.ConvertStringToJToken(p_result.Text);
			if(resultJToken != null)
			{
				this.fbToken = (string)resultJToken["access_token"];
				this.goodFacebookLogo.SetActive(true);
				this.badFacebookLogo.SetActive(false);

				Debug.Log("FB Token :" + this.fbToken);

//				FB.API("me/photos", Facebook.HttpMethod.GET, delegate(FBResult result2) {
//					Debug.Log(result2.Text);
//				});
			}
			else
			{
				Debug.LogError("Problem reading Facebook login result JSON");
			}
		}
		else
		{
			this.goodFacebookLogo.SetActive(false);
			this.badFacebookLogo.SetActive(true);
		}
	}
}
