using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public static class ConfigureURP {
 public static void Run(){
  const string rendererPath="Assets/FieldUniversalRenderer.asset", assetPath="Assets/FieldUniversalPipeline.asset";
  var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
  if(renderer==null){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,rendererPath);}
  var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
  if(pipeline==null){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,assetPath);}
  GraphicsSettings.defaultRenderPipeline=pipeline;
  for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;}
  AssetDatabase.SaveAssets();
  Debug.Log("URP configured: "+GraphicsSettings.defaultRenderPipeline.name);
 }
}
