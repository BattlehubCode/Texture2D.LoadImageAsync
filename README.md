# Texture2D.LoadImageAsync
[![openupm](https://img.shields.io/npm/v/net.battlehub.loadimageasync?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/net.battlehub.loadimageasync/)
[![GitHub issues](https://img.shields.io/github/issues/Battlehub0x/Texture2D.LoadImageAsync)](https://github.com/Battlehub0x/Texture2D.LoadImageAsync/issues)
[![GitHub license](https://img.shields.io/github/license/Battlehub0x/Texture2D.LoadImageAsync?label=license)](https://github.com/Battlehub0x/Texture2D.LoadImageAsync/blob/main/LICENSE)

The Texture2D.LoadImageAsync package is an extension for loading images asynchronously in Unity. It improves performance and responsiveness by reducing the impact of image loading on the main thread. It supports various image formats, including JPEG, PNG. It uses this [plugin](https://github.com/Battlehub0x/LoadImageAsyncPlugin) built on top of stb_image.h, stb_image_resize.h and compiled for Windows, macOS and Android.

## Installation

The easiest way to install is to download and open the [Installer Package](https://package-installer.glitch.me/v1/installer/OpenUPM/net.battlehub.loadimageasync?registry=https%3A%2F%2Fpackage.openupm.com&scope=net.battlehub)

It runs a script that installs Load Image Async via a scoped registry.

Afterwards Load Image Async is listed in the Package Manager and can be installed and updated from there.

## Usage

```
using Battlehub.Utils;
using UnityEngine;

public class LoadImageAsyncSample : MonoBehaviour
{
    [SerializeField]
    private string m_path;
    
    async void  Start()
    {
        Texture2D texture = new Texture2D(1, 1);
        await texture.LoadImageAsync(m_path);

        Debug.Log($"{texture.width} {texture.height} {texture.format} {texture.mipmapCount}");
    }
}
```


