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
        private static extern ImageInfo Battlehub_LoadImage_GetInfo( string path);

        [DllImport(k_LoadImage, CharSet = k_charSet)]
        private static extern void Battlehub_LoadImage_Load(string path, byte[] data, int channels, int mipLevels);

        private static int CalculateMipmapArraySize(int width, int height, int channels, int mipmapLevels)
        {
            int totalSize = 0;
            int currentWidth = width;
            int currentHeight = height;

            for (int i = 0; i < mipmapLevels; i++)
            {
                totalSize += currentWidth * currentHeight * channels;
                currentWidth = currentWidth / 2;
                currentHeight = currentHeight / 2;
            }

            return totalSize;
        }

        public static async Task<bool> LoadImageAsync(this Texture2D texture, string path, bool mipChain = true)
        {
            try
            {
                ImageInfo info = Battlehub_LoadImage_GetInfo(path);
                if(info.status != 1)
                {
                    return false;
                }

                TextureFormat format = info.channels == 4 ? TextureFormat.ARGB32 : TextureFormat.RGB24;
                texture.Reinitialize(info.width, info.height, format, mipChain);

                int mipmapCount = texture.mipmapCount;
                int size = CalculateMipmapArraySize(info.width, info.height, info.channels, mipmapCount);

                byte[] data = new byte[size];
                await Task.Run(() => Battlehub_LoadImage_Load(path, data, info.channels, mipmapCount));

                texture.LoadRawTextureData(data);
                texture.Apply(false);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

    }

}
