using UnityEngine;
using System.Collections;

namespace Facebook
{
    public class AndroidFacebookLoader : FB.CompiledFacebookLoader
    {

        protected override AbstractFacebook fb
        {
            get
            {
                return FBComponentFactory.GetComponent<AndroidFacebook>();
            }
        }
    }
}