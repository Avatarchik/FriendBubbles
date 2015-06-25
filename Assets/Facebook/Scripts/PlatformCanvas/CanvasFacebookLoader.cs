using UnityEngine;
using System.Collections;

namespace Facebook
{
    public class CanvasFacebookLoader : FB.CompiledFacebookLoader
    {
        protected override AbstractFacebook fb
        {
            get
            {
                return FBComponentFactory.GetComponent<CanvasFacebook>();
            }
        }
    }
}