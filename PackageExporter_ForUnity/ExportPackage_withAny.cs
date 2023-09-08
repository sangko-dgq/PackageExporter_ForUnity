/*
 * Copyright (c) sangko
 * All rights reserved.
 *
 * This script is part of the Unity Custom Package Exporter project.
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * Proprietary and confidential.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class ExportPackage_withAny : EditorWindow
{
    #region Configuration Section
	static List<string> filterArr = new List<string>
	{
		"Assets",
    };
	static string targetPackageName = "UltimateTemplate.unitypackage";
	static ExportPackageOptions flags = ExportPackageOptions.Interactive;
    #endregion

	static private bool exportAssets = true;
	static private bool exportProjectSettings = true;
	static private bool exportLibrary = true;        // Added Library option
	static private bool exportUserSettings = true;    // Added UserSettings option
	static private bool exportPackages = true;        // Added Packages option

	static string exportPath = "";
	static string packageName = "";

	[MenuItem("CustomTools/ExPackage_Any_1.0")]
	static void ShowWindow()
	{
		var window = EditorWindow.GetWindow<ExportPackage_withAny>(false, "ExPackage_Any_1.0", true);
		window.minSize = new Vector2(400, 250);
		window.maxSize = new Vector2(800, 600);

		// Set the default export path and package name
		string projectFolderPath = Application.dataPath;
		projectFolderPath = Path.GetFullPath(Path.Combine(projectFolderPath, ".."));
		exportPath = projectFolderPath;
		packageName = GeneratePackageName();
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));

		GUILayout.Space(10);

		GUILayout.Label("导出配置", EditorStyles.boldLabel);

		exportAssets = EditorGUILayout.Toggle("Assets", exportAssets);
		if (exportAssets)
		{
			if (!filterArr.Contains("Assets"))
			{
				filterArr.Add("Assets");
			}
		}
		else
		{
			filterArr.Remove("Assets");
		}

		exportProjectSettings = EditorGUILayout.Toggle("ProjectSettings", exportProjectSettings);
		if (exportProjectSettings)
		{
			if (!filterArr.Contains("ProjectSettings"))
			{
				filterArr.Add("ProjectSettings");
			}
		}
		else
		{
			filterArr.Remove("ProjectSettings");
		}

		// Add Library and UserSettings options
		exportLibrary = EditorGUILayout.Toggle("Library", exportLibrary);
		if (exportLibrary)
		{
			if (!filterArr.Contains("Library"))
			{
				filterArr.Add("Library");
			}
		}
		else
		{
			filterArr.Remove("Library");
		}

		exportUserSettings = EditorGUILayout.Toggle("UserSettings", exportUserSettings);
		if (exportUserSettings)
		{
			if (!filterArr.Contains("UserSettings"))
			{
				filterArr.Add("UserSettings");
			}
		}
		else
		{
			filterArr.Remove("UserSettings");
		}

		// Add Packages option
		exportPackages = EditorGUILayout.Toggle("Packages", exportPackages);
		if (exportPackages)
		{
			if (!filterArr.Contains("Packages"))
			{
				filterArr.Add("Packages");
			}
		}
		else
		{
			filterArr.Remove("Packages");
		}

		// Update the package name to reflect the user's selection
		packageName = GeneratePackageName();

		GUILayout.Space(10);

		GUILayout.Label("导出选项", EditorStyles.boldLabel);

		GUILayout.BeginHorizontal();
		GUILayout.Label("导出路径:", GUILayout.Width(80));
		exportPath = EditorGUILayout.TextField(exportPath);
		if (GUILayout.Button("浏览", GUILayout.Width(80)))
		{
			exportPath = EditorUtility.OpenFolderPanel("选择导出路径", exportPath, "");
			GUI.FocusControl(null);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("包名称:", GUILayout.Width(80));
		GUILayout.BeginHorizontal(); // 新增水平组用于包名称输入框和“重置”按钮

		packageName = EditorGUILayout.TextField(packageName);

		// 添加“重置”按钮，宽度和“浏览”按钮相同
		if (GUILayout.Button("重置", GUILayout.Width(80)))
		{
			packageName = GeneratePackageName();
			GUI.FocusControl(null); // 清除输入焦点以使文本框的值更新
		}
		GUILayout.EndHorizontal(); // 结束新的水平组
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("项目文件夹", GUILayout.Width(Screen.width / 2 - 100), GUILayout.Height(30)))
		{
			string projectFolderPath = Application.dataPath;
			projectFolderPath = Path.GetFullPath(Path.Combine(projectFolderPath, ".."));
			EditorUtility.RevealInFinder(projectFolderPath);
		}

		GUILayout.Space(10);

		GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
		if (GUILayout.Button("导出", GUILayout.Width(Screen.width / 2 + 80), GUILayout.Height(30)))
		{
			if (string.IsNullOrEmpty(exportPath) || !Directory.Exists(exportPath))
			{
				EditorUtility.DisplayDialog("错误", "导出路径无效或为空。", "确定");
				return;
			}

			string resourcesToExport = "";
			if (exportAssets)
			{
				resourcesToExport += "Assets\n";
			}
			if (exportProjectSettings)
			{
				resourcesToExport += "ProjectSettings\n";
			}
			if (exportLibrary)             // Include Library in the export
			{
				resourcesToExport += "Library\n";
			}
			if (exportUserSettings)         // Include UserSettings in the export
			{
				resourcesToExport += "UserSettings\n";
			}
			if (exportPackages)             // Include Packages in the export
			{
				resourcesToExport += "Packages\n";
			}

			bool userConfirmed = EditorUtility.DisplayDialog("导出资源", "确定要导出以下资源吗？\n\n" + resourcesToExport, "确认", "取消");

			if (userConfirmed)
			{
				ExportAllProject();
			}
		}
		GUI.backgroundColor = Color.white;

		GUILayout.Space(10);

		GUILayout.EndHorizontal();

		GUILayout.Space(10);

		GUILayout.EndArea();
	}

	static private string GeneratePackageName()
	{
		string timeStamp = DateTime.Now.ToString("yyMMdd_HHmm");
		string projectName = Application.productName;
		string exportOptions = "";

		if (exportAssets)
		{
			exportOptions += "Asset";
		}
		if (exportProjectSettings)
		{
			exportOptions += "ProjectSetting";
		}
		if (exportLibrary)
		{
			exportOptions += "Library";
		}
		if (exportUserSettings)
		{
			exportOptions += "UserSettings";
		}
		if (exportPackages)
		{
			exportOptions += "Packages";
		}

		return projectName + "_" + exportOptions + "_" + timeStamp;
	}

	static void ExportAllProject()
	{
		var projectContent = AssetDatabase.GetAllAssetPaths();
		var filteredPathLst = new List<string>();
		var index = -1;
		EditorApplication.update = () =>
		{
			index++;
			if (index >= projectContent.Length)
			{
				EditorApplication.update = null;
				EditorUtility.ClearProgressBar();

				if (string.IsNullOrEmpty(exportPath) || string.IsNullOrEmpty(packageName))
				{
					EditorUtility.DisplayDialog("错误", "导出路径和包名称不能为空。", "确定");
					return;
				}

				string packagePath = Path.Combine(exportPath, packageName + ".unitypackage");

				AssetDatabase.ExportPackage(filteredPathLst.ToArray(), packagePath, flags);
				Debug.Log("项目已导出到: " + packagePath);
			}
			else
			{
				var path = projectContent[index];
				if (FilterAsset(path))
				{
					filteredPathLst.Add(path);
				}

				if (EditorUtility.DisplayCancelableProgressBar("导出资源", "正在导出资源，请稍候...", index / (float)projectContent.Length))
				{
					EditorApplication.update = null;
					EditorUtility.ClearProgressBar();
					Debug.LogWarning("用户取消了导出操作。");
				}
			}
		};
	}

	static bool FilterAsset(string path)
	{
		foreach (var filter in filterArr)
		{
			if (path.StartsWith(filter))
			{
				return true;
			}
		}
		return false;
	}
}
