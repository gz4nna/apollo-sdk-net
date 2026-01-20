using System.Numerics;

namespace Apollo.SDK.NET.Algorithms;

/// <summary>
/// MurmurHash3 实现
/// </summary>
public static class MurmurHash3
{
    /// <summary>
    /// 计算字符串的 MurmurHash3 哈希值
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="seed">种子</param>
    /// <returns></returns>
    public static uint Hash(Span<char> input, uint seed = 0)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(input.ToArray());
        uint c1 = 0xcc9e2d51;
        uint c2 = 0x1b873593;
        int r1 = 15;
        int r2 = 13;
        uint m = 5;
        uint n = 0xe6546b64;
        uint hash = seed;
        int length = data.Length;
        int nBlocks = length / 4;

        // 处理 4 字节块
        for (int i = 0; i < nBlocks; i++)
        {
            uint k1 = BitConverter.ToUInt32(data, i * 4);
            k1 *= c1;
            k1 = BitOperations.RotateLeft(k1, r1);
            k1 *= c2;

            hash ^= k1;
            hash = BitOperations.RotateLeft(hash, r2);
            hash = hash * m + n;
        }

        // 处理剩余字节
        int tailIndex = nBlocks * 4;
        uint remainingBytes = 0;
        switch (length & 3)
        {
            case 3: remainingBytes ^= (uint)data[tailIndex + 2] << 16; goto case 2;
            case 2: remainingBytes ^= (uint)data[tailIndex + 1] << 8; goto case 1;
            case 1:
                remainingBytes ^= (uint)data[tailIndex];
                remainingBytes *= c1;
                remainingBytes = BitOperations.RotateLeft(remainingBytes, r1);
                remainingBytes *= c2;
                hash ^= remainingBytes;
                break;
        }

        // 最终化
        hash ^= (uint)length;
        hash ^= hash >> 16;
        hash *= 0x85ebca6b;
        hash ^= hash >> 13;
        hash *= 0xc2b2ae35;
        hash ^= hash >> 16;

        return hash;
    }
}