﻿//
// AssetsMenuItem.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.IO;
using UnityEditor;
using UnityEngine;
using ColaFramework;

namespace Plugins.XAsset.Editor
{
    public static class AssetsMenuItem
    {
        private const string MARK_ASSET_WITH_DIR = "Assets/AssetBundles/按目录标记";
        private const string MARK_ASSET_WITH_FILE = "Assets/AssetBundles/按文件标记";
        private const string MARK_ASSET_WITH_NAME = "Assets/AssetBundles/按名称标记";
        private const string CLEAR_ABNAME = "Assets/AssetBundles/清除所有AssetBundle的标记";
        private const string BUILD_MANIFEST = "Assets/AssetBundles/生成配置";
        private const string BUILD_ASSETBUNDLES = "Assets/AssetBundles/生成资源包";
        private const string BUILD_PLAYER = "Assets/AssetBundles/生成播放器";
        private const string COPY_PATH = "Assets/复制路径";
        private const string MARK_ASSETS = "正在标记资源";
        private const string COPY_TO_STREAMINGASSETS = "Assets/AssetBundles/拷贝到StreamingAssets";
        private const string CLEAR_SANDBOX = "Assets/AssetBundles/清除沙盒目录下的内容";

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            EditorUtility.ClearProgressBar();
            if (AppConst.isLocalServer)
            {
                bool isRunning = LaunchLocalServer.IsRunning();
                if (!isRunning)
                {
                    LaunchLocalServer.Run();
                }
            }
            else
            {
                bool isRunning = LaunchLocalServer.IsRunning();
                if (isRunning)
                {
                    LaunchLocalServer.KillRunningAssetBundleServer();
                }
            }
            Utility.dataPath = System.Environment.CurrentDirectory;
            Utility.downloadURL = BuildScript.GetManifest().downloadURL;
            Utility.assetBundleMode = AppConst.SimulateMode;
            Utility.getPlatformDelegate = BuildScript.GetPlatformName;
            Utility.loadDelegate = AssetDatabase.LoadAssetAtPath;
        }

        public static string TrimedAssetBundleName(string assetBundleName)
        {
            return assetBundleName.Replace(Constants.GameAssetBasePath, "");
        }

        [MenuItem(COPY_TO_STREAMINGASSETS)]
        private static void CopyAssetBundles()
        {
            BuildScript.CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundles));
            AssetDatabase.Refresh();
        }

        [MenuItem(MARK_ASSET_WITH_DIR)]
        private static void MarkAssetsWithDir()
        {
            var assetsManifest = BuildScript.GetManifest();
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(MARK_ASSETS, path, i * 1f / assets.Length))
                    break;
                var assetBundleName = TrimedAssetBundleName(Path.GetDirectoryName(path).Replace("\\", "/")) + "_g";
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null);
            }
            EditorUtility.SetDirty(assetsManifest);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem(MARK_ASSET_WITH_FILE)]
        private static void MarkAssetsWithFile()
        {
            var assetsManifest = BuildScript.GetManifest();
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(MARK_ASSETS, path, i * 1f / assets.Length))
                    break;

                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path);
                if (dir == null)
                    continue;
                dir = dir.Replace("\\", "/") + "/";
                if (name == null)
                    continue;

                var assetBundleName = TrimedAssetBundleName(Path.Combine(dir, name));
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null);
            }
            EditorUtility.SetDirty(assetsManifest);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem(MARK_ASSET_WITH_NAME)]
        private static void MarkAssetsWithName()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            var assetsManifest = BuildScript.GetManifest();
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(MARK_ASSETS, path, i * 1f / assets.Length))
                    break;
                var assetBundleName = Path.GetFileNameWithoutExtension(path);
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null);
            }
            EditorUtility.SetDirty(assetsManifest);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem(CLEAR_ABNAME)]
        private static void ClearAllABName()
        {
            BuildScript.ClearAllAssetBundleName();
        }

        [MenuItem(BUILD_MANIFEST)]
        private static void BuildManifest()
        {
            BuildScript.BuildManifest();
        }

        [MenuItem(BUILD_ASSETBUNDLES)]
        private static void BuildAssetBundles()
        {
            BuildScript.BuildManifest();
            BuildScript.BuildAssetBundles();
        }

        [MenuItem(BUILD_PLAYER)]
        private static void BuildStandalonePlayer()
        {
            BuildScript.BuildStandalonePlayer();
        }

        [MenuItem(COPY_PATH)]
        private static void CopyPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            EditorGUIUtility.systemCopyBuffer = assetPath;
            Debug.Log(assetPath);
        }

        [MenuItem(CLEAR_SANDBOX)]
        private static void ClearSandBox()
        {
            var dir = Path.GetDirectoryName(Utility.UpdatePath);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);             
            }
        }
    }
}