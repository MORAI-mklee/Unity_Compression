using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Image_Compression : MonoBehaviour
{
    //Camera Resolution
    public int m_cameraWidth = 640;
    public int m_cameraHeight = 480;
    RunLengthEncoding RLE;

    //Scene Camera�� �������� ���� ��� ����
    private Camera m_Camera;

    //Camera�� RGB Data�� �����ϰ� �ִ� Texture
    private Texture2D colorBuffer;
    private RenderTexture renderINIT;
    Rect textureRect;

    //Camera�� RGB Data�� �����ϰ� �ִ� Byte buffer
    private byte[] texture_byte;

    private int CompressionRatio;


    void Start()
    {
        m_Camera = GetComponent<Camera>();
        m_Camera.renderingPath = RenderingPath.UsePlayerSettings;

        colorBuffer = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
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

        texture_byte = colorBuffer.GetRawTextureData();

        CompressionRatio = RLE.Encode(texture_byte,colorBuffer.width,colorBuffer.width);

    }

}