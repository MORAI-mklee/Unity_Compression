using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class Image_Compression : MonoBehaviour
{
    //Camera Resolution
    public int m_cameraWidth = 640;
    public int m_cameraHeight = 480;
    CancellationTokenSource cancellationTokenSource;

    //Scene Camera를 가져오기 위한 멤머 변수
    private Camera m_Camera;

    //Camera의 RGB Data를 포함하고 있는 Texture
    private Texture2D colorBuffer;
    private RenderTexture renderINIT;
    Rect textureRect;

    //Camera의 RGB Data를 포함하고 있는 Byte buffer

    public byte[] m_firstPix = new byte[3];
    public byte[] m_secondPix = new byte[3];
    public byte[] s_countByte = new byte[2];
    byte[] m_outputByte = new byte[5]; // 640*480*3
    int m_loopCounter = 0;
    int count = 0;
    int count_Equal = 0;
    int count_nEqual = 0;
    public int second_count = 0;
    Thread thread;
    byte[] _rawTextureBuffer;
    public Texture2D tex;
    
    public class ImageData
    {
        public byte[] imgBytes;


        public ImageData(byte[] imgBytes)
        {
            this.imgBytes = imgBytes;

        }
    }
    void Start()
    {

        m_Camera = GetComponent<Camera>();
        m_Camera.renderingPath = RenderingPath.UsePlayerSettings;

        
        renderINIT = new RenderTexture(m_cameraWidth, m_cameraHeight, 24);
        renderINIT.filterMode = FilterMode.Point;
        

        textureRect = new Rect(0, 0, m_cameraWidth, m_cameraHeight);

        m_Camera.targetTexture = renderINIT;

        _rawTextureBuffer = new byte[m_cameraWidth * m_cameraHeight * 3];

    }

    // Update is called once per frame
    void Update()
    {
        tex = GetRTPixels(m_Camera.targetTexture);
        
        //var tex_byte = tex.GetRawTextureData();
        //tex.ReadPixels(textureRect, 0, 0);
        tex.Apply();

        var request = AsyncGPUReadback.Request(tex, 0, OnCompleteReadBack);
        request.WaitForCompletion();
        //Encode(tex_byte);

        //CopyNativeArrayToByteArray(ref req, ref _rawTextureBuffer);
        UnityEngine.Debug.Log("Count_Equal : " + count_Equal);
        UnityEngine.Debug.Log("Count_nEqual : " + count_nEqual);
        UnityEngine.Debug.Log("m_firstPixLength : " + m_firstPix.Length);
        UnityEngine.Debug.Log("m_outputByte : " + m_outputByte.Length);
        //UnityEngine.Debug.Log(
        UnityEngine.Debug.Log((float)(m_outputByte.Length) * 100.0f / (float)(640*480*3));

        Destroy(tex);

    }

    private void OnDestroy()
    {
        AsyncGPUReadback.WaitAllRequests();


        cancellationTokenSource.Dispose();
        
    }

    Texture2D GetRTPixels(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D colorBuffer = new Texture2D(m_cameraWidth, m_cameraHeight, TextureFormat.RGB24, false);
        colorBuffer.ReadPixels(new Rect(0, 0, m_cameraWidth, m_cameraHeight), 0, 0);
        
        RenderTexture.active = null;
        return colorBuffer;
    }

    public void Encode(object obj)
    {
        m_outputByte = new byte[5];
        m_firstPix = new byte[3];
        m_secondPix = new byte[3];
        ImageData data = (ImageData)obj;
        m_loopCounter = 0;
        count = 0;
        count_Equal = 0;
        count_nEqual = 0;


        if(data.imgBytes != null)
        {
            for (int i = 0; i < (data.imgBytes.Length / 3) - 1; i++)
            {
                Buffer.BlockCopy(data.imgBytes, 3 * i, m_firstPix, 0, 3);
                Buffer.BlockCopy(data.imgBytes, 3 * (i + 1) , m_secondPix, 0, 3);

                //Debug.Log(data.imgBytes.Length);

                if (m_firstPix.SequenceEqual(m_secondPix))
                {
                    count += 1;
                    count_Equal += 1;
                    var countByte = BitConverter.GetBytes(count);
                    Buffer.BlockCopy(countByte, 0, m_outputByte, 8 * count_nEqual, sizeof(int));
                }
                else
                {

                    count_nEqual += 1;
                    var countByte_nEqual = BitConverter.GetBytes(1);

                    Buffer.BlockCopy(countByte_nEqual, 0, m_outputByte, 8 * count_nEqual, sizeof(int));
                    //m_outputByte = AddByteArrayToArray(m_outputByte, countByte_nEqual);
                    //Buffer.BlockCopy(m_secondPix, 0, m_outputByte, 8 * count_nEqual + 4, 3);
                    //m_outputByte = AddByteArrayToArray(m_outputByte, m_secondPix);
                }
            }

        }
        
    }

    void OnCompleteReadBack(AsyncGPUReadbackRequest request)
    {
        if (thread != null)
        {
            thread.Abort();
        }

        thread = new Thread(Encode);
        thread.IsBackground = true;
        thread.Start(new ImageData(_rawTextureBuffer));
        thread.Join();

        var req = request.GetData<byte>();
        CopyNativeArrayToByteArray(ref req, ref _rawTextureBuffer);


    }

    public static unsafe void CopyNativeArrayToByteArray<T>(ref NativeArray<T> src, ref byte[] dst) where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(src));
        if (dst == null)
            throw new ArgumentNullException(nameof(dst));
#endif

        var size = UnsafeUtility.SizeOf<T>() * src.Length;
        if (dst.Length != size)
            dst = new byte[size];

        var srcAddr = (byte*)src.GetUnsafeReadOnlyPtr();

        fixed (byte* dstAddr = dst)
        {
            UnsafeUtility.MemCpy(&dstAddr[0], &srcAddr[0], size);
        }
    }

    public static unsafe void CopyNativeArrayToManagedArray<T>(ref NativeArray<T> src, ref T[] dst) where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(src));
        if (dst == null)
            throw new ArgumentNullException(nameof(dst));
#endif

        var size = src.Length;
        if (size != dst.Length)
        {
            dst = new T[size];
        }

        var srcAddr = (T*)src.GetUnsafeReadOnlyPtr();

        fixed (T* dstAddr = dst)
        {
            UnsafeUtility.MemCpy(&dstAddr[0], &srcAddr[0], size);
        }
    }

    public static unsafe void CopyManagedArrayToNativeArray<T>(ref T[] src, ref NativeArray<T> dst) where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(dst));
        if (src == null)
            throw new ArgumentNullException(nameof(src));
#endif

        var size = src.Length;
        if (size != dst.Length)
        {
            dst.Dispose();
            dst = new NativeArray<T>(size, Allocator.Persistent);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(dst));
#endif
        }

        var dstAddr = (T*)dst.GetUnsafeReadOnlyPtr();
        fixed (T* srcAddr = src)
        {
            UnsafeUtility.MemCpy(&dstAddr[0], &srcAddr[0], src.Length);
        }
    }

    public static unsafe void ClearNativeArray<T>(NativeArray<T> src, int length) where T : unmanaged
    {
        UnsafeUtility.MemClear(
            NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(src),
            UnsafeUtility.SizeOf<T>() * length);
    }

    public byte[] AddByteArrayToArray(byte[] bArray, byte[] newByteArray)
    {
        byte[] newArray = new byte[bArray.Length + newByteArray.Length];
        bArray.CopyTo(newArray, 0);
        newByteArray.CopyTo(newArray, bArray.Length);
        return newArray;
    }

}