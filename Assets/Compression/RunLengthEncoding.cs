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
    public byte[] m_firstPix = new byte[3];
    public byte[] m_secondPix = new byte[3];
    public byte[] m_outputByte = new byte[5];
    public ushort count = 1;
    public int second_count = 0;
    public int m_loopCounter = 0;

    public byte[] Encode(byte[] data) 
    {
        var f_countByte = BitConverter.GetBytes(count);
        f_countByte.CopyTo(m_outputByte, 0);

        m_firstPix[0] = data[0];
        m_firstPix[1] = data[1];
        m_firstPix[2] = data[2];

        m_firstPix.CopyTo(m_outputByte, 2);

        for (int i = 0; 3 * i < data.Length - 3; i++)
        {

            m_firstPix[0] = data[3 * i];
            m_firstPix[1] = data[3 * i + 1];
            m_firstPix[2] = data[3 * i + 2];

            m_secondPix[0] = data[3 * (i + 1)];
            m_secondPix[1] = data[3 * (i + 1) + 1];
            m_secondPix[2] = data[3 * (i + 1) + 2];

            if (m_firstPix[0] == m_secondPix[0] && m_firstPix[1] == m_secondPix[1] && m_firstPix[2] == m_secondPix[2])
            {
                count += 1;
                
                //just count increase
                var s_countByte = BitConverter.GetBytes(count);

                s_countByte.CopyTo(m_outputByte, second_count * (m_outputByte.Length-5));

            }
            else
            {
                count = 1;
                second_count += 1;

                var s_countByte = BitConverter.GetBytes(count);

                //s_countByte.CopyTo(m_outputByte, second_count * m_outputByte.Length);

                m_outputByte = AddByteArrayToArray(m_outputByte, s_countByte);

                m_outputByte = AddByteArrayToArray(m_outputByte, m_secondPix);

                //m_outputByte = new byte[firstoutput.Length + m_secondPix.Length];

                //firstoutput.CopyTo(m_outputByte, 0);
                //m_secondPix.CopyTo(m_outputByte, firstoutput.Length);

            }

            m_loopCounter += 1;

        }

        return m_outputByte;
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
