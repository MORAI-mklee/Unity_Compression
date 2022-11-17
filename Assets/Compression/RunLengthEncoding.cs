using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
//Buffer.BlockCopy
//public static void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count);
src : source 버퍼
sorOffset : source buffer에 대한 byte offset (0부터 시작)
dst : destination buffer
dstOffset : destination buffer에 대한 byte offset (0부터 시작)
count : 복사할 byte 수 

*/

public class RunLengthEncoding 
{
    private int m_result = 0;

    private byte[] m_firstPix = new byte[3];
    private byte[] m_secondPix = new byte[3];
    private int count = 0;
    

    //private BitArray m_bitArray;
    public int Encode(byte[] data, int width, int height) 
    {
        for (int i = 0; 3 * i < data.Length; i++)
        {

            m_firstPix[0] = data[3 * i];
            m_firstPix[1] = data[3 * i + 1];
            m_firstPix[2] = data[3 * i + 2];

            m_secondPix[0] = data[3 * (i + 1)];
            m_secondPix[1] = data[3 * (i + 1) + 1];
            m_secondPix[2] = data[3 * (i + 1) + 2];

            if (m_firstPix.Equals(m_secondPix))
            {
                count += 1;
                byte countByte = Convert.ToByte(count);
            }
            else
            {
                count = 0;
            }
            
        }

        return m_result;
    }

    public byte[] AddByteArrayToArray(byte[] bArray, byte[] newByteArray)
    {
        byte[] newArray = new byte[bArray.Length + newByteArray.Length];
        bArray.CopyTo(newArray, 0);
        newByteArray.CopyTo(newArray, bArray.Length);
        return newArray;
    }

    public byte[] AddByteToArray(byte[] bArray, byte newByte)
    {
        byte[] newArray = new byte[bArray.Length + 1];
        bArray.CopyTo(newArray, 0);
        newArray[newArray.Length - 1] = newByte;
        return newArray;
    }

}
