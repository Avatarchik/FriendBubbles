using Facebook;
using UnityEngine;
using System;
using System.Collections;

public class FBResult : IDisposable
{
    private bool isWWWWrapper = false;
    private object data;
    private string error;

    public FBResult(WWW www)
    {
        isWWWWrapper = true;
        data = www;
    }

    public FBResult(string data, string error = null)
    {
        this.data = data;
        this.error = error;
    }

    public Texture2D Texture
    {
        get
        {
            return (isWWWWrapper) ? ((WWW) data).texture : data as Texture2D;
        }
    }

    public string Text
    {
        get
        {
            return (isWWWWrapper) ? ((WWW)data).text : data as string;
        }
    }

    public string Error
    {
        get
        {
            return (isWWWWrapper) ? ((WWW)data).error : error;
        }
    }

    public void Dispose()
    {
        if (isWWWWrapper && data != null)
        {
            ((WWW)data).Dispose();
        }
    }

    // Destructor
    ~FBResult()
    {
        Dispose();
    }
}