using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Image_Compression : MonoBehaviour
{
    //Camera Resolution
    public int m_cameraWidth = 640;
    public int m_cameraHeight = 480;
    public RunLengthEncoding RLE = new RunLengthEncoding();

    //Scene Camera를 가져오기 위한 멤머 변수
    private Camera m_Camera;

    //Camera의 RGB Data를 포함하고 있는 Texture
    private Texture2D colorBuffer;
    private RenderTexture renderINIT;
    Rect textureRect;

    //Camera의 RGB Data를 포함하고 있는 Byte buffer
    byte[] CompressionOutput;
    private float Compression_Ratio;


    void Start()
    {
        m_Camera = GetComponent<Camera>();
        m_Camera.renderingPath = RenderingPath.UsePlayerSettings;

        colorBuffer = new Texture2D(m_cameraWidth, m_cameraHeight, TextureFormat.RGB24, false);
        renderINIT = new RenderTexture(m_cameraWidth, m_cameraHeight, 24);
        renderINIT.filterMode = FilterMode.Point;

        textureRect = new Rect(0, 0, m_cameraWidth, m_cameraHeight);
    }

    // Update is called once per frame
    void Update()
    {
        m_Camera.targetTexture = renderINIT;
        RenderTexture.active = m_Camera.targetTexture;
        colorBuffer.ReadPixels(textureRect, 0, 0);
        colorBuffer.Apply();

        byte[] texture_byte = colorBuffer.GetRawTextureData();

        CompressionOutput = RLE.Encode(texture_byte);

        Compression_Ratio = ((float)CompressionOutput.Length / (float)texture_byte.Length) * 100.0f;

        UnityEngine.Debug.Log(Compression_Ratio);


    }

}