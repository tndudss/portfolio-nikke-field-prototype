using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
public static class DistributionBuild {
 static void Build(BuildTarget target,string path){
  var graphics=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
  var settings=new SerializedObject(graphics);var shaders=settings.FindProperty("m_AlwaysIncludedShaders");
  foreach(var name in new[]{"Universal Render Pipeline/Lit","Prototype/Transparent","Skybox/Panoramic","Unlit/Color","Prototype/RapiCapture","Prototype/OccluderSoft"}){var shader=UnityEngine.Shader.Find(name);if(shader==null)throw new Exception("Missing shader: "+name);bool exists=false;for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader)exists=true;if(!exists){shaders.InsertArrayElementAtIndex(shaders.arraySize);shaders.GetArrayElementAtIndex(shaders.arraySize-1).objectReferenceValue=shader;}}
  settings.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssets();
  Directory.CreateDirectory(Path.GetDirectoryName(path));
  PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
  PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;
  PlayerSettings.WebGL.template="PROJECT:NightCity";
  PlayerSettings.stripEngineCode=false;
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
  PlayerSettings.WebGL.dataCaching=false;
  var r=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/EventField.unity"},path,target,BuildOptions.None);
  UnityEngine.Debug.Log("Distribution build: "+r.summary.result+", errors: "+r.summary.totalErrors+", path: "+path);
  if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+r.summary.result);
 }
 [MenuItem("Prototype/Build Windows Distribution")]
 public static void Windows(){Build(BuildTarget.StandaloneWindows64,Path.GetFullPath("../Windows/NightCityEventField.exe"));}
 [MenuItem("Prototype/Build Web Distribution")]
 public static void Web(){Build(BuildTarget.WebGL,Path.GetFullPath("../WebGL"));}
}


