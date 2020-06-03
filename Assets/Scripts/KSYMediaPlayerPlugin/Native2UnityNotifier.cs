using UnityEngine;
using System;

#if UNITY_ANDROID
public delegate void OnVideoPrepared(int width, int height);
public delegate void OnVideoFrameDataArrive(AndroidJavaObject videoFrameData);
public delegate void OnMediaPlayerInfo(int info);
public class Native2UnityNotifier : AndroidJavaProxy
{
    public event OnVideoPrepared onVideoDataPrepared;
    public event OnVideoFrameDataArrive onVideoDataArrive;
    public event OnMediaPlayerInfo onNativeMediaPlayerInfo;

    public Native2UnityNotifier() : base("com.yqunity.ksymediaplayer4unity.INative2UnityNotifier"){}

    public void onVideoPrepared(int width, int height)
    {
        if(onVideoDataPrepared != null)
        {
            onVideoDataPrepared(width, height);
        }
    }

    public void onVideoFrameDataArrive(AndroidJavaObject videoFrameData)
    {
        if (onVideoDataArrive != null)
        {
            onVideoDataArrive(videoFrameData);
        }

    }

    public void onMediaPlayerInfo(int info)
    {
        if(onNativeMediaPlayerInfo != null)
        {
            onNativeMediaPlayerInfo(info);
        }
    }

    public void onMsg(string msg)
    {
        Debug.Log(msg);
    }
}
#endif