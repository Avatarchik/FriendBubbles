using UnityEngine;
using System.Collections;

namespace Facebook
{
    public class EditorFacebookLoader : FB.CompiledFacebookLoader
    {

        protected override AbstractFacebook fb
        {
            get
            {
                return FBComponentFactory.GetComponent<EditorFacebook>();
            }
        }
    }
}