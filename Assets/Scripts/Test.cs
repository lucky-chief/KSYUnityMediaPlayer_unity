using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public KSYUnityMediaPlayer player;

    //public Slider R;
    //public Slider G;
    //public Slider B;

    // Start is called before the first frame update
    void Start()
    {
        //R.onValueChanged.AddListener((float value) =>
        //{
        //    player.material.SetFloat("_RMutiplier", value);
        //    player.SetMaterialDirty();
        //});

        //G.onValueChanged.AddListener((float value) =>
        //{
        //    player.material.SetFloat("_GMutiplier", value);
        //    player.SetMaterialDirty();
        //});

        //B.onValueChanged.AddListener((float value) =>
        //{
        //    player.material.SetFloat("_BMutiplier", value);
        //    player.SetMaterialDirty();
        //});
    }

    // Update is called once per frame
    void Update()
    {
        //player.OnVideoDataArrive(b, 0, 0, 0);
    }

    string videoUrl = "rtmp://202.69.69.180:443/webcast/bshdlive-pc";
    //string videoUrl = "rtmp://aliplay.17zuoye.cn/17live/5ed4673baa95fb0300b28c75";
    //string videoUrl = "rtmp://fms.105.net/live/rmc1";
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(100);
        videoUrl = GUILayout.TextField(videoUrl, GUILayout.Width(500), GUILayout.Height(25));
        if(GUILayout.Button("测试"))
        {
            player.InitPlugin();
            player.SetBufferTimeMax(1.0f);
            player.Play(videoUrl);

            //player.OnVideoDataPrepared(176, 144);
            
            //player.OnVideoDataArrive(b, 0, 0, 0);

            //var tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            //tex.LoadRawTextureData(b);
            //tex.Apply();
            //video.texture = tex;
            Debug.Log("Click Test!");
        }
        GUILayout.Space(100);
        GUILayout.EndHorizontal();

    }
}
