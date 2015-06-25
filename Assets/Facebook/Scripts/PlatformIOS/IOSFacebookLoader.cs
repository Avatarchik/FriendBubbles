using UnityEngine;
using System.Collections;

namespace Facebook
{
    public class IOSFacebookLoader : FB.CompiledFacebookLoader
    {

        protected override AbstractFacebook fb
        {
            get
            {
                return FBComponentFactory.GetComponent<IOSFacebook>();
            }
        }
    }
}