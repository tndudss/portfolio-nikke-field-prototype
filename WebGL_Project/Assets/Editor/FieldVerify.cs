using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Reflection;
using System.Collections;
public static class FieldVerify {
 static double started; static int stage; static EventField field;
 public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/EventField.unity");EditorApplication.playModeStateChanged+=Changed;EditorApplication.EnterPlaymode();}
 static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}}
 static object Get(string name)=>typeof(EventField).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(field);
 static void Tick(){try{
  if(EditorApplication.timeSinceStartup-started<2)return;
  field=UnityEngine.Object.FindFirstObjectByType<EventField>();if(field==null)throw new Exception("Field missing");
  if(stage==0){if(GameObject.Find("SD Player - Rapi")==null)throw new Exception("Player missing");var list=(ICollection)Get("beacons");if(list.Count!=7)throw new Exception("Expected 7 beacons");field.StartCoroutine((IEnumerator)typeof(EventField).GetMethod("Transition",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(field,new object[]{"상점"}));stage++;started=EditorApplication.timeSinceStartup;}
  else if(stage==1){if((string)Get("screen")!="상점")throw new Exception("Transition failed");field.StartCoroutine((IEnumerator)typeof(EventField).GetMethod("Transition",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(field,new object[]{""}));stage++;started=EditorApplication.timeSinceStartup;}
  else if(stage==2){if((string)Get("screen")!="")throw new Exception("Back failed");field.StartCoroutine((IEnumerator)typeof(EventField).GetMethod("Portal",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(field,null));stage++;started=EditorApplication.timeSinceStartup;}
  else{if(!(bool)Get("lobby"))throw new Exception("Lobby failed");Debug.Log("FIELD_SMOKE_PASS: player, 7 beacons, transition, back, lobby portal");EditorApplication.update-=Tick;EditorApplication.Exit(0);}
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}

