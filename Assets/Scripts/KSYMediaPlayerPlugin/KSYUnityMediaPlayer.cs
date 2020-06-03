using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KSYUnityMediaPlayer : RawImage
{
    public enum VideoDataFormat
    {
        YUV420,
        RGBA8888
    }

    private List<byte[]> videoFrames = new List<byte[]>();

    private VideoDataFormat videoDataFormat;
    private Texture2D texARGB8888;
    private Texture2D texY;
    private Texture2D texU;
    private Texture2D texV;
    private byte[] bufY;
    private byte[] bufU;
    private byte[] bufV;
    private int yIndex;
    private int uIndex;
    private int vIndex;

    protected override void Start()
    {
    }

    protected void Update()
    {
        UpdateVideo();
    }

    void UpdateVideo()
    {
        if (videoFrames.Count == 0) return;
        byte[] frameAt0 = videoFrames[0];
        videoFrames.RemoveAt(0);

        switch (videoDataFormat)
        {
            case VideoDataFormat.YUV420:
                SeperateYUVChanels(frameAt0);

                texY.LoadRawTextureData(bufY);
                texU.LoadRawTextureData(bufU);
                texV.LoadRawTextureData(bufV);
                texY.Apply(false);
                texU.Apply(false);
                texV.Apply(false);

                material.SetTexture("_MainTex", texY);
                material.SetTexture("_UTex", texU);
                material.SetTexture("_VTex", texV);
                SetMaterialDirty();
                break;
            case VideoDataFormat.RGBA8888:
                texARGB8888.LoadRawTextureData(frameAt0);
                texARGB8888.Apply(false);
                SetMaterialDirty();
                break;
        }
    }

    private void SeperateYUVChanels(byte[] buff)
    {
        for (int i = 0; i < vIndex; i++)
        {
            if (i < yIndex)
            {
                bufY[i] = buff[i];
            }
            else if (i < uIndex)
            {
                bufU[i - yIndex] = buff[i];
            }
            else
            {
                bufV[i - uIndex] = buff[i];
            }
        }
    }

#if UNITY_ANDROID
    private AndroidJavaObject context;
    private AndroidJavaObject pluginInst; //原生侧 KSMedia4Unity 的实例
    private AndroidJavaObject nativeMediaPlayer; //原生侧的 KSYMediaPlayer 实例
    private Native2UnityNotifier notifier;

    public void InitPlugin()
    {
        context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        if(context != null)
        {
            //实例化插件
            pluginInst = new AndroidJavaObject("com.yqunity.ksymediaplayer4unity.KSMedia4Unity", context);
            //设置回调
            notifier = new Native2UnityNotifier();
            notifier.onVideoDataPrepared += OnVideoDataPrepared;
            notifier.onVideoDataArrive += OnVideoDataArrive;
            notifier.onNativeMediaPlayerInfo += OnNativeMediaPlayerInfo;
            pluginInst.Call("setUnityNotifier", notifier);
            videoDataFormat = VideoDataFormat.RGBA8888;
            pluginInst.Call("setOutputDataFormat", (int)videoDataFormat);
            nativeMediaPlayer = pluginInst.Call<AndroidJavaObject>("getRawMediaPlayerInstance");
        }
    }

    public void Play(string videoUrl)
    {
        if (pluginInst != null)
        {
            pluginInst.Call("play", videoUrl);
        }
    }

    public void SoftReset(string videoUrl = null)
    {
        if (pluginInst != null)
        {
            if(videoUrl == null)
                pluginInst.Call("softReset");
            else
                pluginInst.Call("softReset", videoUrl);
        }
    }

    public void Reload(string videoUrl = null)
    {
        if (pluginInst != null)
        {
            if (videoUrl == null)
                pluginInst.Call("reload");
            else
                pluginInst.Call("reload", videoUrl);
        }
    }

    public void SetBufferTimeMax(float time)
    {
        if(pluginInst != null)
        {
            pluginInst.Call("setBufferTimeMax", time);
        }
    }

    private void OnNativeMediaPlayerInfo(int info)
    {
        //Debug.Log("AAAAAAAA" + Time.time + "   " + info);

        switch (info)
        {
            case 701:
                ///** 开始缓存数据,可认为是一次卡顿 */
                ///在这里可以显示 loading 圈
                break;
            case 702:
                /** 播放器缓存结束,开始播放音视频 */
                ///在这里可以隐藏 loading 圈
                break;
            case 3:
                /** 视频开始渲染 */
                break;
            case 10002:
                /** 音频开始播放 */
                break;
            case 1:
            case 40020:
                /** 建议使用者调用reload接口 */
                Reload();
                break;
            case 50001:
                /** reload成功 */
                break;


            case -1004:
                //【读超时】和【链接超时】均能导致此错误的出现，用户可以选择重连
                //SoftReset();
                Reload();
                break;
        }
    }

    private void OnVideoDataPrepared(int width, int height)
    {
        switch(videoDataFormat)
        {
            case VideoDataFormat.YUV420:
                int firstFrameEndIndex = width * height * 3 / 2;

                yIndex = firstFrameEndIndex * 4 / 6;
                uIndex = firstFrameEndIndex * 5 / 6;
                vIndex = firstFrameEndIndex;

                bufY = new byte[width * height];
                bufU = new byte[width * height >> 2];
                bufV = new byte[width * height >> 2];

                texY = new Texture2D(width, height, TextureFormat.Alpha8, false);
                texU = new Texture2D(width >> 1, height >> 1, TextureFormat.Alpha8, false);
                texV = new Texture2D(width >> 1, height >> 1, TextureFormat.Alpha8, false);
                break;
            case VideoDataFormat.RGBA8888:
                texARGB8888 = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture = texARGB8888;
                break;
        }
        
    }

    private void OnVideoDataArrive(AndroidJavaObject videoFrameData)
    {
        byte[] buffer = videoFrameData.Get<byte[]>("buffer");

        //int size = videoFrameData.Get<int>("bufferSize");
        //int width = videoFrameData.Get<int>("videoWidth");
        //int height = videoFrameData.Get<int>("videoHeight");
        //Debug.Log(string.Format("buf length:{0}, size:{1}, width:{2}, height:{3}。", buffer.Length, size, width, height));
        videoFrames.Add(buffer);

        //回收android侧的 帧数据 对象
        if(pluginInst != null)
        {
            pluginInst.Call("RecycleFrameData", videoFrameData);
        }
    }

    protected override void OnDestroy()
    {
        if(notifier != null)
        {
            notifier.onVideoDataPrepared -= OnVideoDataPrepared;
            notifier.onVideoDataArrive -= OnVideoDataArrive;
            notifier.onNativeMediaPlayerInfo -= OnNativeMediaPlayerInfo;
        }

        texARGB8888 = null;
        texU = null;
        texV = null;
        texY = null;

        bufU = null;
        bufV = null;
        bufY = null;

        videoFrames.Clear();
        videoFrames = null;

        if(pluginInst != null)
        {
            pluginInst.Call("Destroy");
            pluginInst = null;
        }
    }

#elif UNITY_EDITOR

#elif UNITY_IOS
#endif
}
