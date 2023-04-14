 using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageInfo
    {
        public int status;
        public int width;
        public int height;
        public int channels;
    }

    public static class Texture2DExtension
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private const string k_LoadImage = "Battlehub.LoadImage";
        private const CharSet k_charSet = CharSet.Unicode;
#elif UNITY_ANDROID
        private const string k_LoadImage = "Battlehub_LoadImage_Droid";
        private const CharSet k_charSet = CharSet.Ansi;
#else 
        private const string k_LoadImage = "__Internal";
        private const CharSet k_charSet = CharSet.Ansi;
#endif
        [DllImport(k_LoadImage, CharSet = k_charSet)]
        private static extern ImageInfo Battlehub_LoadImage_GetInfo(string path);


        [DllImport(k_LoadImage, CharSet = k_charSet)]
        private static extern void Battlehub_LoadImage_Load(string path, byte[] data, int channels = 0, int mipmapCount = 1, int width = 0, int height = 0);

        [DllImport(k_LoadImage)]
        private static extern ImageInfo Battlehub_LoadImage_Load_From_Memory(byte[] bytes, int size, bool mipChain, out IntPtr outBytes);

        [DllImport(k_LoadImage)]
        private static extern void Battlehub_LoadImage_Free_Memory(IntPtr bytes);

        private static int CalculateMipmapArraySize(int width, int height, int channels, int mipmapCount)
        {
            int totalSize = 0;
            int currentWidth = width;
            int currentHeight = height;

            for (int i = 0; i < mipmapCount; i++)
            {
                totalSize += currentWidth * currentHeight * channels;
                currentWidth = Mathf.Max(1, currentWidth / 2);
                currentHeight = Mathf.Max(1, currentHeight / 2);
            }

            return totalSize;
        }

        private static int CalculateMipmapCount(int width, int height)
        {
            int maxDimension = Mathf.Max(width, height);
            int mipLevels = Mathf.FloorToInt(Mathf.Log(maxDimension, 2)) + 1;
            return mipLevels;
        }

        public static async Task<bool> LoadImageAsync(this Texture2D texture, string path, bool mipChain = true, int width = 0, int height = 0)
        {
            ImageInfo info = Battlehub_LoadImage_GetInfo(path);
            if (info.status != 1)
            {
                return false;
            }

            if (width <= 0)
            {
                width = info.width;
            }

            if (height <= 0)
            {
                height = info.height;
            }

            TextureFormat format = info.channels == 4 ? TextureFormat.RGBA32 : TextureFormat.RGB24;
#if UNITY_2021_2_OR_NEWER
            texture.Reinitialize(width, height, format, mipChain);
#else
            texture.Resize(width, height, format, mipChain);
#endif
            int mipmapCount = texture.mipmapCount;
            int size = CalculateMipmapArraySize(width, height, info.channels, mipmapCount);

            byte[] data = new byte[size];
            await Task.Run(() => Battlehub_LoadImage_Load(path, data, info.channels, mipmapCount, width, height));

            texture.LoadRawTextureData(data);
            texture.Apply(false);

            return true;
        }

        public static async Task<bool> LoadImageAsync(this Texture2D texture, byte[] data, bool mipChain = true)
        {
            IntPtr rawDataPtr = IntPtr.Zero;
            ImageInfo info = await Task.Run(() => Battlehub_LoadImage_Load_From_Memory(data, data.Length, mipChain, out rawDataPtr));
            if (info.status != 1)
            {
                return false;
            }

            try
            {
                int mipmapCount = CalculateMipmapCount(info.width, info.height);
                int size = CalculateMipmapArraySize(info.width, info.height, info.channels, mipmapCount);

                byte[] rawData = new byte[size];
                Marshal.Copy(rawDataPtr, rawData, 0, size);

                TextureFormat format = info.channels == 4 ? TextureFormat.RGBA32 : TextureFormat.RGB24;
#if UNITY_2021_2_OR_NEWER
                texture.Reinitialize(info.width, info.height, format, mipChain);
#else
                texture.Resize(info.width, info.height, format, mipChain);
#endif
                texture.LoadRawTextureData(rawData);
                texture.Apply(false);

                return true;
            }
            finally
            {
                Battlehub_LoadImage_Free_Memory(rawDataPtr);
            }   
        }
    }
}
