using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EventField : MonoBehaviour {
 class Beacon { public string title; public Vector3 pos; public Color color; public Transform marker; public bool portal; }
 static readonly Vector2[] walkStarts={new Vector2(0,-58),Vector2.zero,Vector2.zero,Vector2.zero};
 static readonly Vector2[] walkEnds={Vector2.zero,new Vector2(-22,20),new Vector2(22,20),new Vector2(0,31.5f)};
 static readonly string[] outdoorMenu={"스토리 1부","스토리 2부","미니게임","상점","챌린지","메인타워 입구"};
 static readonly string[] lobbyMenu={"스토리 1부","스토리 2부","미니게임","상점","챌린지","메인타워 출구"};
 readonly Collider[] routeColliders=new Collider[64];
 readonly List<Beacon> beacons=new List<Beacon>();
 readonly List<GameObject> topViewLights=new List<GameObject>();
 readonly Queue<Vector3> route=new Queue<Vector3>();
 readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
 class ForegroundBuilding { public Transform root; public Renderer[] renderers; public Material[] original; public bool softened; public float z; }
 readonly List<ForegroundBuilding> foreground=new List<ForegroundBuilding>();
 readonly Dictionary<Material,Material> softMaterials=new Dictionary<Material,Material>();
 Transform towerGroup;
 class CharacterView { public Transform root; public Renderer[] faces=new Renderer[4]; public int visible=-1; }
 readonly List<CharacterView> characterViews=new List<CharacterView>();
 Transform player; Transform[] companions = new Transform[2]; Camera cam; CharacterController motor; Font font;
 Vector3 destination; bool moving, lobby, busy, topView; float fade, dwell; float fieldZoom=1f,zoomResistance,zoomInputTime,topOverlayAlpha,topCameraFastUntil; bool closeCamera; string screen="", hint="", area="진입로", autoStory="";
 Vector3 walkGoal, lastWalkPosition; float stuckTime; Beacon nearest; GUIStyle titleStyle,bodyStyle,labelStyle,buttonStyle,locationStyle,viewButtonStyle,storyOneMenuStyle,storyTwoMenuStyle,storyOneTitleStyle,storyTwoTitleStyle; bool styles;

 Texture2D netRunnerMainCapture;
 readonly Color cyan=new Color(.10f,.48f,.56f),pink=new Color(.58f,.12f,.30f),storyTwo=new Color(.42f,.28f,.63f);
 Vector3 V(float x,float z,float y=0)=>new Vector3(x,y,z);
 void Start(){
  Application.targetFrameRate=60;

  netRunnerMainCapture=Resources.Load<Texture2D>("NetRunnerMainCapture");
  font=Resources.Load<Font>("FieldFont");
  RenderSettings.ambientLight=new Color(.28f,.34f,.46f); RenderSettings.fog=true; RenderSettings.fogColor=new Color(.025f,.035f,.075f); RenderSettings.fogDensity=.004f;
  var light=new GameObject("Moon").AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.3f; light.transform.rotation=Quaternion.Euler(45,-35,0);
  cam=new GameObject("Camera").AddComponent<Camera>(); cam.clearFlags=CameraClearFlags.Skybox; cam.backgroundColor=RenderSettings.fogColor; SetupSky(); cam.farClipPlane=300; cam.fieldOfView=52;
  Outdoor(); Lobby();
  player=GameObject.CreatePrimitive(PrimitiveType.Cube).transform; player.name="SD Player - Rapi"; player.position=V(0,-34,1); player.localScale=new Vector3(1.1f,1.8f,1.1f); Destroy(player.GetComponent<Collider>());
  player.GetComponent<Renderer>().enabled=false; AddCharacterFaces(player,"Rapi"); motor=player.gameObject.AddComponent<CharacterController>(); motor.height=1; motor.radius=.5f; motor.stepOffset=.55f; motor.center=Vector3.zero;
  
  CreateCompanions(); destination=player.position; cam.transform.position=player.position+new Vector3(0,2.3f,-7.5f); cam.transform.LookAt(player.position+Vector3.up*1.7f);
 }
 void SetupSky(){
  var image=Resources.Load<Texture2D>("CitySkyPanorama");var shader=Shader.Find("Skybox/Panoramic");
  if(image==null||shader==null){cam.clearFlags=CameraClearFlags.SolidColor;return;}
  image.wrapMode=TextureWrapMode.Repeat;var sky=new Material(shader);sky.SetTexture("_MainTex",image);sky.SetFloat("_Exposure",.85f);sky.SetFloat("_Rotation",0);RenderSettings.skybox=sky;
 }
 void DistantCity(){
  // Behind the destination: a layered city silhouette, lower than the landmark tower.
  for(int row=0;row<3;row++)for(int col=-3;col<=3;col++){
   float x=col*14+(row%2)*6;float z=59+row*19;
   float h=10+((col*col*7+row*11+21)%14);float width=8+((col*col+row)%4);
   Tower("City skyline",V(x,z),new Vector3(width,h,7),new Color(.12f,.25f,.4f));
  }
  // Close-set blocks continue the entrance street behind the spawn point.
  for(int row=0;row<4;row++)foreach(int side in new[]{-1,1}){
   float z=-47-row*9;float x=side*(9+(row%2)*1.2f);
   Tower("City skyline",V(x,z),new Vector3(7.5f,17+(row%3)*3,7.5f),new Color(.12f,.25f,.4f));
  }
  // Extend the skyline behind the entrance so the spawn area is framed from both directions.
  for(int row=0;row<3;row++)for(int col=-4;col<=4;col++){
   float x=col*12+(row%2)*5;float z=-55-row*15;if(Mathf.Abs(x)<7)continue;float height=11+((col*col*5+row*9+17)%14);
   Tower("City skyline",V(x,z),new Vector3(8+(Mathf.Abs(col)+row)%3,height,7),new Color(.12f,.25f,.4f));
  }
 }
 void AddCharacterFaces(Transform character, string characterName){
  string[] views={"Front","Right","Back","Left"}; var characterView=new CharacterView{root=character}; characterViews.Add(characterView);
  for(int i=0;i<4;i++){
   var texture=Resources.Load<Texture2D>(characterName+"_"+views[i]);
   if(texture==null){Debug.LogWarning("Missing Rapi view: "+views[i]);continue;}
   var material=new Material(Shader.Find("Prototype/RapiCapture"));material.mainTexture=texture;material.SetFloat("_Front",i==0?1f:0f); if(characterName!="Rapi"){float width=i==2?.31f:i==0?.36f:.5f;material.SetVector("_Crop",new Vector4((1-width)*.5f,.13f,width,.84f));}
   var face=GameObject.CreatePrimitive(PrimitiveType.Quad);face.name="Rapi "+views[i];
   face.transform.SetParent(character,false);
   float angle=i*90;var direction=Quaternion.Euler(0,angle,0)*Vector3.forward;
   face.transform.localPosition=direction*.506f+Vector3.down*.05f;
   face.transform.localRotation=Quaternion.Euler(0,angle+180,0);
   face.transform.localScale=new Vector3(.99f,.99f,1);
   Destroy(face.GetComponent<Collider>());var faceRenderer=face.GetComponent<Renderer>();faceRenderer.sharedMaterial=material;characterView.faces[i]=faceRenderer;
  }
 }
 void UpdateCharacterViews(){
  foreach(var view in characterViews){
   Vector3 direction=view.root.InverseTransformDirection(cam.transform.position-view.root.position);
   int selected=Mathf.Abs(direction.z)>=Mathf.Abs(direction.x)?(direction.z>=0?0:2):(direction.x>=0?1:3);
   if(view.visible==selected)continue;
   for(int i=0;i<4;i++)if(view.faces[i]!=null)view.faces[i].enabled=i==selected;
   view.visible=selected;
  }
 }
 void CreateCompanions(){
  string[] names={"Neon","Anis"};
  for(int i=0;i<2;i++){
   var body=GameObject.CreatePrimitive(PrimitiveType.Cube);body.name="SD Companion - "+names[i];
   body.transform.localScale=new Vector3(1.1f,1.8f,1.1f);body.GetComponent<Collider>().enabled=false;
   body.GetComponent<Renderer>().enabled=false;
   companions[i]=body.transform;AddCharacterFaces(companions[i],names[i]);
  }
  PositionCompanions(true);
 }
 void PositionCompanions(bool snap){
  for(int i=0;i<companions.Length;i++){
   if(companions[i]==null)continue;
   Vector3 target=player.position+new Vector3(i==0?-2.0f:2.0f,0,-.55f);
   if(!lobby)target=ClampWalkable(target,.55f);
   Vector3 next=snap?target:Vector3.Lerp(companions[i].position,target,1-Mathf.Exp(-Time.deltaTime*9));
   companions[i].position=lobby?next:ClampWalkable(next,.55f);
   companions[i].rotation=snap?player.rotation:Quaternion.Slerp(companions[i].rotation,player.rotation,1-Mathf.Exp(-Time.deltaTime*9));
  }
 }
 Material Mat(Color c,bool glow=false){string key=ColorUtility.ToHtmlStringRGBA(c)+(glow?"e":"n");if(materials.TryGetValue(key,out var cached))return cached;var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;m.SetFloat("_Smoothness",.32f);if(glow){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*.28f);}materials[key]=m;return m;}
 void ApplySurface(GameObject obj,string resource,Vector2 tiling,Color tint){
  string key=resource+":"+tiling.x.ToString("F2")+":"+tiling.y.ToString("F2")+":"+ColorUtility.ToHtmlStringRGBA(tint);
  if(!materials.TryGetValue(key,out var m)){
   var tex=Resources.Load<Texture2D>(resource);
   if(tex==null){Debug.LogWarning("Missing surface texture: "+resource);return;}
   tex.wrapMode=TextureWrapMode.Repeat;tex.filterMode=FilterMode.Trilinear;
   m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.mainTexture=tex;m.mainTextureScale=tiling;m.color=tint;
   float smooth=resource=="BrushedMetal"?.53f:resource=="PlazaConcrete"?.08f:resource=="ParkFoliage"?.12f:.28f;
   m.SetFloat("_Smoothness",smooth);if(resource=="BrushedMetal")m.SetFloat("_Metallic",.38f);materials[key]=m;
  }
  obj.GetComponent<Renderer>().sharedMaterial=m;
 }
 GameObject Box(string n,Vector3 p,Vector3 s,Color c,bool glow=false){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.position=p;g.transform.localScale=s;if(towerGroup!=null)g.transform.SetParent(towerGroup,true);g.GetComponent<Renderer>().sharedMaterial=Mat(c,glow);return g;}
 GameObject Detail(string n,Vector3 p,Vector3 s,Color c,bool glow=false){var g=Box(n,p,s,c,glow);g.GetComponent<Collider>().enabled=false;
  if(n=="Planter"||n=="Park bench"||n=="Memorial plaque"||n=="Memorial sculpture base")ApplySurface(g,"PlazaConcrete",Vector2.one,new Color(.67f,.72f,.74f));
  else if(n=="Park bench seat"||n=="Memorial sculpture wing"||n=="Memorial sculpture center"||n=="Streetlight pole"||n=="Streetlight arm"||n=="Street bollard")ApplySurface(g,"BrushedMetal",Vector2.one,new Color(.72f,.77f,.8f));
  return g;}
 void Disc(string n,Vector3 p,float radius,Color c){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.position=p;g.transform.localScale=new Vector3(radius*2,.12f,radius*2);g.GetComponent<Renderer>().sharedMaterial=Mat(c);if(n=="Memorial Park"||n=="Central dais")ApplySurface(g,"PlazaConcrete",new Vector2(3,3),new Color(.46f,.53f,.55f));g.GetComponent<Collider>().enabled=false;if(n=="Memorial Park"){var floor=g.AddComponent<BoxCollider>();floor.size=new Vector3(1,2,1);}}
 void Road(Vector3 a,Vector3 b,float width){var d=b-a;var g=Box("Raised avenue",(a+b)/2+Vector3.down*.2f,new Vector3(width,.35f,d.magnitude),new Color(.085f,.105f,.14f));g.transform.rotation=Quaternion.LookRotation(d);ApplySurface(g,"UrbanAsphalt",new Vector2(Mathf.Max(1,width/7),Mathf.Max(1,d.magnitude/7)),new Color(.8f,.84f,.9f));
  foreach(float side in new[]{-1f,1f}){
   Vector3 outward=Vector3.Cross(d.normalized,Vector3.up)*side;
   var sidewalk=Detail("Avenue sidewalk",(a+b)/2+outward*(width/2+.7f)+Vector3.down*.06f,new Vector3(1.4f,.10f,d.magnitude),new Color(.2f,.23f,.25f));sidewalk.transform.rotation=g.transform.rotation;ApplySurface(sidewalk,"PlazaConcrete",new Vector2(1,Mathf.Max(1,d.magnitude/5)),new Color(.43f,.47f,.49f));
   var gutter=Detail("Avenue drainage channel",(a+b)/2+outward*(width/2+.07f)+Vector3.up*.01f,new Vector3(.16f,.025f,d.magnitude),new Color(.09f,.12f,.14f));gutter.transform.rotation=g.transform.rotation;
   var edge=Detail("Inset road trim",(a+b)/2+outward*width*.47f+Vector3.up*.005f,new Vector3(.055f,.035f,d.magnitude),cyan*.5f,true);edge.transform.rotation=g.transform.rotation;
  }
  for(float t=3;t<d.magnitude-2;t+=4){var dash=Detail("Road center marking",a+d.normalized*t+Vector3.up*.015f,new Vector3(.08f,.025f,1.35f),new Color(.28f,.34f,.36f));dash.transform.rotation=g.transform.rotation;}
 }
 Vector2 RoadClamp(Vector2 point,Vector2 a,Vector2 b,float halfWidth){
  Vector2 axis=(b-a).normalized,side=new Vector2(-axis.y,axis.x);Vector2 delta=point-a;
  return a+axis*Mathf.Clamp(Vector2.Dot(delta,axis),0,Vector2.Distance(a,b))+side*Mathf.Clamp(Vector2.Dot(delta,side),-halfWidth,halfWidth);
 }
 Vector3 ClampWalkable(Vector3 position,float margin){
  Vector2 p=new Vector2(position.x,position.z);Vector2 best=p.magnitude<=10-margin?p:p.normalized*(10-margin);float distance=(p-best).sqrMagnitude;


  for(int i=0;i<walkStarts.Length;i++){
   float width=(i==0?4f:3.5f)-margin;Vector2 candidate=RoadClamp(p,walkStarts[i],walkEnds[i],width);float d=(p-candidate).sqrMagnitude;
   if(d<distance){distance=d;best=candidate;}
  }
  return new Vector3(best.x,position.y,best.y);
 }
 void Tower(string name,Vector3 p,Vector3 size,Color color){
  bool isForeground=name=="Entrance skyscraper",isSub=name=="SUB TOWER 1"||name=="SUB TOWER 2";
  if(isForeground||isSub){towerGroup=new GameObject(isSub?name+" aligned group":"Foreground building group").transform;towerGroup.position=p;}
  bool skyline=name=="City skyline";bool entrance=name=="Entrance skyscraper";
  var shell=new Color(.055f,.073f,.105f);var frame=new Color(.10f,.13f,.17f);
  var building=Box(name,p+Vector3.up*size.y/2,size,shell);
  if(name!="Lobby column")ApplySurface(building,"CorporateFacade",new Vector2(Mathf.Max(1,size.x/8),Mathf.Max(1,size.y/11)),name=="SUB TOWER 1"?new Color(.89f,.77f,.80f):name=="SUB TOWER 2"?new Color(.70f,.85f,.90f):name=="MAIN TOWER"?new Color(.69f,.75f,.84f):new Color(.8f,.86f,.9f));
  else ApplySurface(building,"BrushedMetal",new Vector2(1,2),new Color(.55f,.59f,.64f));
  Detail(name+" podium",p+Vector3.up*.8f,new Vector3(size.x+.45f,1.6f,size.z+.45f),frame);
  Detail(name+" upper setback",p+Vector3.up*(size.y+1),new Vector3(size.x*.7f,2,size.z*.7f),shell);
  Detail(name+" roof frame",p+Vector3.up*(size.y+.1f),new Vector3(size.x+.2f,.22f,size.z+.2f),frame);
  if(!skyline&&name!="Lobby column"){
   var roof=Detail(name+" rooftop service panel",p+new Vector3(0,size.y+2.1f,0),new Vector3(size.x*.32f,.25f,size.z*.32f),frame);ApplySurface(roof,"BrushedMetal",Vector2.one,new Color(.52f,.56f,.59f));
   for(int j=-1;j<=1;j++)Detail(name+" rooftop vent",p+new Vector3(j*size.x*.09f,size.y+2.28f,0),new Vector3(size.x*.05f,.16f,size.z*.25f),shell);
  }
  if(!skyline){
   for(float y=3;y<size.y-1;y+=2.7f)
    foreach(int face in new[]{-1,1})Detail(name+" facade band",p+new Vector3(0,y-1.1f,face*(size.z/2+.055f)),new Vector3(size.x+.08f,.07f,.08f),frame);
  }
  if(entrance){
   int side=p.x<0?1:-1;float x=p.x+side*(size.x/2+.035f);
   Detail(name+" vertical accent",new Vector3(x,p.y+size.y*.5f,p.z-size.z*.4f),new Vector3(.07f,size.y*.72f,.08f),color*.32f,true);
  } else if(!skyline){
   foreach(int face in new[]{-1,1})for(int i=-1;i<=1;i++)Detail(name+" vertical fin",p+new Vector3(i*size.x*.27f,size.y*.53f,face*(size.z/2+.1f)),new Vector3(.16f,size.y*.85f,.16f),frame);
  }
  if(name=="SUB TOWER 1"||name=="SUB TOWER 2"){
   bool left=name=="SUB TOWER 1";float front=p.z-size.z/2-.15f;Color accent=left?pink*.65f:cyan*.65f;
   var forecourt=Detail(name+" forecourt",p+new Vector3(0,.02f,-size.z/2-1.7f),new Vector3(size.x+2,.05f,3.2f),frame);ApplySurface(forecourt,"PlazaConcrete",new Vector2(2,1),new Color(.43f,.47f,.49f));
   Detail(name+" entrance canopy",new Vector3(p.x,3.8f,front-.65f),new Vector3(7,.3f,1.5f),frame);
   Detail(name+" canopy light",new Vector3(p.x,3.62f,front-1.25f),new Vector3(6,.06f,.08f),accent,true);
   if(left){for(int i=0;i<3;i++)Detail(name+" horizontal identity",new Vector3(p.x,6+i*1.25f,front),new Vector3(7,.12f,.10f),accent,true);}
   else foreach(int side in new[]{-1,1})Detail(name+" vertical identity",new Vector3(p.x+side*3.8f,8,front),new Vector3(.13f,10,.1f),accent,true);
  }
  if(isForeground){
   var r=towerGroup.GetComponentsInChildren<Renderer>();var originals=new Material[r.Length];
   for(int i=0;i<r.Length;i++)originals[i]=r[i].sharedMaterial;
   foreground.Add(new ForegroundBuilding{root=towerGroup,renderers=r,original=originals,z=p.z});towerGroup=null;
  }
  if(isSub){towerGroup.rotation=Quaternion.LookRotation(new Vector3(p.x,0,p.z));towerGroup=null;}
 }
 Material SoftMaterial(Material original){
  if(softMaterials.TryGetValue(original,out var result))return result;
  var shader=Shader.Find("Prototype/OccluderSoft");if(shader==null)return original;
  result=new Material(shader);result.mainTexture=original.mainTexture;result.mainTextureScale=original.mainTextureScale;result.mainTextureOffset=original.mainTextureOffset;result.color=original.color;result.SetFloat("_Opacity",.22f);softMaterials[original]=result;return result;
 }
 void UpdateForeground(){
  bool plaza=!lobby&&player.position.z>=-11;
  foreach(var b in foreground){
   bool shouldSoften=plaza&&b.z<player.position.z-2;
   if(shouldSoften==b.softened)continue;
   for(int i=0;i<b.renderers.Length;i++)b.renderers[i].sharedMaterial=shouldSoften?SoftMaterial(b.original[i]):b.original[i];
   b.softened=shouldSoften;
  }
 }
 void MainTowerIdentity(){
  // The identifying facade sits on the near side of the tower, facing the entrance avenue.
  var dark=new Color(.038f,.048f,.065f);var red=new Color(.55f,.11f,.13f);var silver=new Color(.38f,.43f,.46f);
  var spine=Detail("Main tower central spine",V(0,31.91f,23),new Vector3(5.5f,43,.18f),dark);
  ApplySurface(spine,"BrushedMetal",new Vector2(1,4),new Color(.25f,.29f,.34f));
  var trim=new Color(.21f,.25f,.29f);var softRed=new Color(.38f,.085f,.10f);var ice=new Color(.30f,.43f,.46f);
  foreach(int side in new[]{-1,1}){
   Detail("Main tower spine outer rail",V(side*2.78f,31.76f,23),new Vector3(.22f,42,.14f),trim);
   Detail("Main tower illuminated spine",V(side*2.54f,31.66f,23),new Vector3(.18f,40,.10f),softRed,true);
   Detail("Main tower inset light",V(side*1.65f,31.65f,26),new Vector3(.10f,31,.08f),ice*.70f,true);
   Detail("Main tower structural flank",V(side*8.7f,31.85f,22),new Vector3(.48f,42,.25f),dark);
   Detail("Main tower portal jamb",V(side*3.2f,31.55f,3.7f),new Vector3(.35f,7.4f,.42f),silver);
  }
  // Repeating restrained modules make the central facade readable at a distance.
  for(float h=13.5f;h<45f;h+=6.0f){
   Detail("Main tower inset divider",V(0,31.64f,h),new Vector3(3.25f,.075f,.09f),trim);
   foreach(int side in new[]{-1,1}){
    Detail("Main tower accent segment",V(side*2.18f,31.62f,h+.8f),new Vector3(.12f,1.2f,.08f),softRed,true);
    Detail("Main tower cross marker",V(side*.85f,31.62f,h),new Vector3(.48f,.09f,.08f),ice*.65f,true);
   }
  }
  Detail("Main tower gateway",V(0,31.55f,7.25f),new Vector3(6.8f,.3f,.42f),silver);
  Detail("Main tower name panel",V(0,31.45f,10.5f),new Vector3(10.0f,2.2f,.22f),dark);
  Detail("Main tower name underline",V(0,31.30f,9.48f),new Vector3(10.0f,.12f,.12f),red,true);
  var entryLight=new GameObject("Main tower entrance soft light").AddComponent<Light>();entryLight.type=LightType.Point;entryLight.transform.position=V(0,29.5f,5.8f);entryLight.range=12;entryLight.intensity=.7f;entryLight.color=new Color(.80f,.70f,.67f);entryLight.shadows=LightShadows.None;
  Detail("Main tower crown cap",V(0,38,49.5f),new Vector3(13,1.1f,9),dark);
  foreach(int side in new[]{-1,1})Detail("Main tower crown light",V(side*6.2f,33.43f,49.5f),new Vector3(.12f,.8f,.08f),red*.65f,true);
  var label=new GameObject("Main tower identity text").AddComponent<TextMesh>();
  label.text="MAIN TOWER";label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;
  label.fontSize=64;label.characterSize=.17f;label.color=new Color(.8f,.84f,.84f);
  label.transform.position=V(0,31.27f,10.45f);
  // Roof marks make the corporate landmark legible from directly above.
  foreach(var point in new[]{new Vector2(0,40),new Vector2(-2.2f,36.5f),new Vector2(2.2f,36.5f)})
   Disc("Tower roof corporate emblem",V(point.x,point.y,47.08f),1.55f,new Color(.38f,.07f,.08f));
 }
 void MainTowerForecourt(){
  // Bridge the avenue to the tower podium with the same surface and collision height.
  var join=Box("Main tower avenue connection",V(0,31.73f,-.2f),new Vector3(7,.35f,.55f),new Color(.085f,.105f,.14f));
  ApplySurface(join,"UrbanAsphalt",new Vector2(1,.1f),new Color(.8f,.84f,.9f));
  var threshold=Detail("Main tower entrance threshold",V(0,31.39f,-.005f),new Vector3(7,.025f,.18f),new Color(.19f,.23f,.26f));
  ApplySurface(threshold,"BrushedMetal",new Vector2(3,1),new Color(.46f,.50f,.54f));
  Color frame=new Color(.10f,.14f,.18f),metal=new Color(.24f,.29f,.32f),warm=new Color(.38f,.09f,.105f);
  foreach(int side in new[]{-1,1}){
   float x=side*4.45f;
   var pad=Detail("Main tower entry side pad",V(x,30.0f,-.015f),new Vector3(1.5f,.05f,3.7f),frame);
   ApplySurface(pad,"PlazaConcrete",new Vector2(1,2),new Color(.42f,.46f,.49f));
   var pedestal=Detail("Main tower entry pylon",V(x,30.45f,.7f),new Vector3(.55f,1.4f,.55f),metal);
   ApplySurface(pedestal,"BrushedMetal",Vector2.one,new Color(.51f,.55f,.58f));
   Detail("Main tower entry pylon light",V(x,30.10f,1.1f),new Vector3(.32f,.07f,.07f),warm,true);
   Detail("Main tower forecourt edge",V(side*3.65f,30.1f,.025f),new Vector3(.07f,.035f,2.6f),warm,true);
  }
 }
 void Tree(Vector3 p){
  Detail("Planter",p+Vector3.up*.25f,new Vector3(2.1f,.5f,2.1f),new Color(.14f,.17f,.18f));
  Detail("Planter soil",p+Vector3.up*.52f,new Vector3(1.8f,.06f,1.8f),new Color(.11f,.12f,.12f));
  var trunk=Detail("Tree trunk",p+Vector3.up*1.7f,new Vector3(.25f,1.5f,.25f),new Color(.20f,.18f,.17f));ApplySurface(trunk,"ParkBark",new Vector2(1,1),new Color(.7f,.72f,.72f));
  var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.name="Park tree crown";crown.transform.position=p+Vector3.up*2.65f;crown.transform.localScale=new Vector3(1.9f,1.55f,1.9f);crown.GetComponent<Collider>().enabled=false;ApplySurface(crown,"ParkFoliage",new Vector2(1,1),new Color(.68f,.78f,.74f));
 }
 void Sign(string label,Vector3 p,int side){
  var frame=Detail("Wayfinding sign",p+Vector3.up*3.8f,new Vector3(.16f,2.7f,3.8f),new Color(.055f,.085f,.105f));
  Detail("Sign edge",p+Vector3.up*5.12f,new Vector3(.18f,.07f,3.8f),cyan*.45f,true);
  var t=new GameObject("Sign label "+label).AddComponent<TextMesh>();t.text=label;t.fontSize=40;t.characterSize=.05f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.65f,.74f,.75f);t.transform.position=p+Vector3.up*4+Vector3.right*side*.1f;t.transform.rotation=Quaternion.Euler(0,side>0?-90:90,0);
 }
 void PlazaDetails(){
  for(int i=0;i<16;i++){float a=i*Mathf.PI*2/16;Vector3 p=V(Mathf.Cos(a)*6.8f,Mathf.Sin(a)*6.8f,.145f);var strip=Detail("Plaza paving joint",p,new Vector3(.035f,.01f,3.5f),new Color(.19f,.25f,.25f));strip.transform.rotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0);}
  foreach(var v in new[]{V(-15,1),V(15,1),V(-7,-4),V(7,-4),V(-20,8),V(20,8),V(-25,12),V(25,12)})Tree(v);
  foreach(int side in new[]{-1,1})for(int z=-32;z<=-14;z+=9){
   float x=side*4.6f;Detail("Street bollard",V(x,z,.6f),new Vector3(.18f,1.2f,.18f),new Color(.18f,.23f,.25f));Detail("Bollard marker",V(x,z,1.18f),new Vector3(.21f,.07f,.21f),cyan*.45f,true);
  }
  for(int i=-3;i<=3;i++)Detail("Crosswalk",V(i*.85f,-11,.145f),new Vector3(.48f,.02f,2.0f),new Color(.34f,.39f,.38f));
  Sign("NIGHT /  CITY",V(-5.15f,-24),1);Sign("CORPORATE / PLAZA",V(5.15f,-18),-1);
 }
 void MemorialDetails(){
  var stone=new Color(.19f,.23f,.24f);var metal=new Color(.28f,.32f,.33f);var warm=new Color(.43f,.30f,.24f);
  // A low memorial ring and plaques frame the central beacon without obstructing it.
  for(int i=0;i<32;i++){
   float a=i*Mathf.PI*2/32;var p=V(Mathf.Cos(a)*4.35f,Mathf.Sin(a)*4.35f,.19f);
   var tile=Detail("Memorial ring",p,new Vector3(.75f,.05f,.18f),stone);tile.transform.rotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0);
  }
  for(int i=0;i<5;i++){
   float a=(i-2)*.3f;var p=V(Mathf.Sin(a)*5.1f,Mathf.Cos(a)*5.1f,.30f);
   var plaque=Detail("Memorial plaque",p,new Vector3(.95f,.36f,.34f),stone);plaque.transform.rotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
   var inset=Detail("Memorial plaque inset",p+Vector3.up*.21f,new Vector3(.7f,.025f,.24f),warm*.7f);inset.transform.rotation=plaque.transform.rotation;
  }
  // Landmark sculpture deliberately sits behind the central story point.
  Detail("Memorial sculpture base",V(0,6,.45f),new Vector3(2.8f,.55f,1.4f),stone);
  foreach(int side in new[]{-1,1}){
   var blade=Detail("Memorial sculpture wing",V(side*.8f,6,2.4f),new Vector3(.32f,3.7f,.24f),metal);blade.transform.rotation=Quaternion.Euler(0,0,side*-12);
   Detail("Memorial sculpture light",V(side*.8f,5.84f,2.4f),new Vector3(.07f,2.3f,.06f),cyan*.4f,true);
  }
  Detail("Memorial sculpture center",V(0,6,2.45f),new Vector3(.38f,3.6f,.38f),warm);
  var monumentLight=new GameObject("Memorial soft light").AddComponent<Light>();monumentLight.type=LightType.Point;monumentLight.transform.position=V(0,6,3.8f);monumentLight.range=7;monumentLight.intensity=.5f;monumentLight.color=new Color(.68f,.78f,.86f);monumentLight.shadows=LightShadows.None;
  for(int i=0;i<3;i++){
   float a=(i-1)*.6f+Mathf.PI;var pos=V(Mathf.Sin(a)*7.5f,Mathf.Cos(a)*7.5f,.4f);
   var bench=Detail("Park bench",pos,new Vector3(2.0f,.42f,.75f),stone);bench.transform.rotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
   var seat=Detail("Park bench seat",pos+Vector3.up*.25f,new Vector3(1.95f,.09f,.7f),metal);seat.transform.rotation=bench.transform.rotation;
  }
  foreach(int side in new[]{-1,1})for(int z=-34;z<=-16;z+=9){
   float x=side*4.8f;
   Detail("Streetlight pole",V(x,z,3),new Vector3(.12f,6,.12f),metal);
   Detail("Streetlight arm",V(x-side*.55f,z,5.9f),new Vector3(1.2f,.12f,.12f),metal);
   Detail("Streetlight fixture",V(x-side*1.0f,z,5.78f),new Vector3(.38f,.08f,.3f),new Color(.4f,.42f,.39f),true);
   var lamp=new GameObject("Soft street light").AddComponent<Light>();lamp.type=LightType.Point;lamp.transform.position=V(x-side*.8f,z,5.4f);lamp.range=8;lamp.intensity=.45f;lamp.color=new Color(.67f,.77f,.83f);lamp.shadows=LightShadows.None;
  }
 }
 void AddBeacon(string title,Vector3 p,Color color,bool portal=false){
  Disc(title+" platform",p+Vector3.up*.15f,1.3f,color*.45f);
  Transform marker;
  if(portal){
   // A thin, non-colliding gateway makes the two travel points distinct from content beacons.
   Vector3 center=p+Vector3.up*1.55f;float radius=1.25f;int segments=40;
   for(int i=0;i<segments;i++){
    float a=i*Mathf.PI*2/segments;var point=center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0);
    var frame=Detail(title+" portal frame",point,new Vector3(2*Mathf.PI*radius/segments+.02f,.15f,.12f),new Color(.12f,.19f,.23f));frame.transform.rotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg+90);
    var lens=Detail(title+" portal light",point+new Vector3(0,0,-.075f),new Vector3(2*Mathf.PI*radius/segments+.02f,.055f,.025f),color*.70f,true);lens.transform.rotation=frame.transform.rotation;
   }
   var surface=GameObject.CreatePrimitive(PrimitiveType.Cylinder);surface.name=title+" portal surface";surface.transform.position=center;surface.transform.rotation=Quaternion.Euler(90,0,0);surface.transform.localScale=new Vector3(2.35f,.006f,2.35f);surface.GetComponent<Collider>().enabled=false;
   var mat=new Material(Shader.Find("Prototype/Transparent"));mat.color=new Color(.18f,.65f,.72f,.14f);surface.GetComponent<Renderer>().sharedMaterial=mat;surface.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
   marker=new GameObject(title+" portal marker").transform;marker.position=p+Vector3.up*2;
  }else{
   var beacon=Box(title+" beacon",p+Vector3.up*2,new Vector3(.65f,.65f,.65f),color,true);beacon.transform.rotation=Quaternion.Euler(0,45,45);Destroy(beacon.GetComponent<Collider>());marker=beacon.transform;
  }
  beacons.Add(new Beacon{title=title,pos=p,color=color,marker=marker,portal=portal});
 }
 void TowerGroundRing(Vector3 center,float radius){
  int count=80;float length=2*Mathf.PI*radius/count+.02f;
  for(int i=0;i<count;i++){
   float a=i*Mathf.PI*2/count;Vector3 p=center+new Vector3(Mathf.Cos(a)*radius,-.035f,Mathf.Sin(a)*radius);
   var basePart=Detail("Tower ground ring frame",p,new Vector3(length,.02f,.36f),new Color(.18f,.21f,.22f));
   basePart.transform.rotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0);ApplySurface(basePart,"BrushedMetal",Vector2.one,new Color(.45f,.48f,.50f));
   var strip=Detail("Tower ground ring light",p+Vector3.up*.014f,new Vector3(length,.008f,.23f),new Color(.65f,.12f,.13f),true);strip.transform.rotation=basePart.transform.rotation;
   basePart.SetActive(false);strip.SetActive(false);topViewLights.Add(basePart);topViewLights.Add(strip);
  }
 }
 void TopViewRoute(Vector3 from,Vector3 to){
  Vector3 delta=to-from;delta.y=0;
  var strip=Detail("Top view corporate route",((from+to)*.5f)+Vector3.up*.16f,new Vector3(.3f,.015f,delta.magnitude),new Color(.65f,.12f,.13f),true);
  strip.transform.rotation=Quaternion.LookRotation(delta);strip.SetActive(false);topViewLights.Add(strip);
 }
 float SegmentDistance(Vector2 p,Vector2 a,Vector2 b){Vector2 d=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);return Vector2.Distance(p,a+d*t);}
 void TopViewEmblem(){
  // Combine every shape in one mask so intersections never accumulate opacity.
  const int resolution=1024;const float extent=80f;var pixels=new Color32[resolution*resolution];
  Vector2 center=new Vector2(0,19),junction=Vector2.zero;
  Vector2[] ends={new Vector2(-25,23),new Vector2(25,23),new Vector2(0,38)};
  for(int y=0;y<resolution;y++)for(int x=0;x<resolution;x++){
   Vector2 p=new Vector2((x+.5f)/resolution*extent-extent/2,extent/2-(y+.5f)/resolution*extent+19);
   float distance=Mathf.Abs(Vector2.Distance(p,center)-37f)-1.65f;
   for(int i=0;i<ends.Length;i++)distance=Mathf.Min(distance,SegmentDistance(p,junction,ends[i])-2.1f);
   float stem=Mathf.Max(Mathf.Abs(p.x)-2.1f,Mathf.Max(-15f-p.y,p.y));distance=Mathf.Min(distance,stem);
   for(int i=0;i<3;i++)distance=Mathf.Min(distance,Vector2.Distance(p,ends[i])-6f);
   byte alpha=(byte)Mathf.RoundToInt(107*Mathf.Clamp01(.5f-distance*resolution/extent));
   pixels[y*resolution+x]=new Color32(237,247,255,alpha);
  }
  var texture=new Texture2D(resolution,resolution,TextureFormat.RGBA32,false);texture.name="Top view emblem mask";texture.wrapMode=TextureWrapMode.Clamp;texture.SetPixels32(pixels);texture.Apply();
  var ink=new Material(Shader.Find("Prototype/Transparent"));ink.mainTexture=texture;ink.color=Color.white;
  var overlay=GameObject.CreatePrimitive(PrimitiveType.Plane);overlay.name="Top view emblem unified light";overlay.transform.position=V(0,19,55);overlay.transform.localScale=new Vector3(extent/10,1,extent/10);
  overlay.GetComponent<Collider>().enabled=false;overlay.GetComponent<Renderer>().sharedMaterial=ink;overlay.SetActive(false);topViewLights.Add(overlay);
 }
 void Outdoor(){
  var cityBase=Box("City base",V(0,0,-.65f),new Vector3(130,1,180),new Color(.025f,.04f,.065f));ApplySurface(cityBase,"NightGrass",new Vector2(24,24),new Color(.72f,.77f,.74f));
  Road(V(0,-78),V(0,0),8);Road(V(0,0),V(-22,20),7);Road(V(0,0),V(22,20),7);Road(V(0,0),V(0,31.5f),7);
  var rearWall=new GameObject("Rear street invisible boundary").AddComponent<BoxCollider>();rearWall.transform.position=V(0,-59,3);rearWall.size=new Vector3(10,6,1);
  Disc("Memorial Park",V(0,0),10,new Color(.12f,.2f,.21f));Disc("Central dais",V(0,0,.18f),3,new Color(.16f,.22f,.3f));
  for(int i=0;i<24;i++){
   float a=i*Mathf.PI*2/24;var p=V(Mathf.Cos(a)*9,Mathf.Sin(a)*9,.13f);var rotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0);
   var bezel=Detail("Recessed park light bezel",p,new Vector3(1.3f,.025f,.38f),new Color(.19f,.23f,.24f));bezel.transform.rotation=rotation;
   ApplySurface(bezel,"BrushedMetal",Vector2.one,new Color(.55f,.62f,.63f));
   var lens=Detail("Recessed park light lens",p+Vector3.up*.016f,new Vector3(1.1f,.01f,.16f),new Color(.12f,.38f,.40f),true);lens.transform.rotation=rotation;
  }
  for(int i=-1;i<3;i++)foreach(int side in new[]{-1,1})Tower("Entrance skyscraper",V(side*9,-31+i*9),new Vector3(7,17+i*3,6),i%2==0?cyan:pink);
  Tower("SUB TOWER 1",V(-25,23),new Vector3(10,28,10),pink);Tower("SUB TOWER 2",V(25,23),new Vector3(10,28,10),cyan);Tower("MAIN TOWER",V(0,38),new Vector3(19,47,12),pink);MainTowerIdentity();MainTowerForecourt();
  TopViewEmblem();DistantCity();
  for(int i=0;i<14;i++){float x=(i%2==0?-1:1)*(38+(i%3)*7);Tower("City skyline",V(x,-35+i*6),new Vector3(7,10+(i*7%15),7),new Color(.12f,.25f,.4f));}
  PlazaDetails();
  MemorialDetails();
  AddBeacon("스토리 1부",V(0,0,.3f),pink);AddBeacon("미니게임",V(6,5,.2f),new Color(.62f,.47f,.10f));AddBeacon("상점",V(-19,17),pink);AddBeacon("챌린지",V(19,17),cyan);AddBeacon("메인타워 입구",V(0,28),cyan,true);
 }
 void Lobby(){
  Color stone=new Color(.12f,.15f,.19f),metal=new Color(.18f,.23f,.27f),red=new Color(.42f,.08f,.11f);
  var surround=Box("Lobby black surround",V(150,0,-1.2f),new Vector3(160,.6f,160),Color.black);surround.GetComponent<Renderer>().sharedMaterial=new Material(Shader.Find("Unlit/Color")){color=Color.black};
  var floor=Box("Lobby floor",V(150,0,-.25f),new Vector3(30,.5f,30),stone);ApplySurface(floor,"PlazaConcrete",new Vector2(6,6),new Color(.38f,.42f,.48f));
  var rear=Box("Lobby rear wall",V(150,15,6),new Vector3(30,12,1),new Color(.05f,.07f,.12f));ApplySurface(rear,"PlazaConcrete",new Vector2(5,2),new Color(.29f,.33f,.39f));
  foreach(int side in new[]{-1,1}){
   var sideWall=Box("Lobby side wall",V(150+side*15,0,4),new Vector3(1,8,30),new Color(.05f,.07f,.12f));ApplySurface(sideWall,"PlazaConcrete",new Vector2(5,2),new Color(.29f,.33f,.39f));
   for(int i=0;i<3;i++)Tower("Lobby column",V(150+side*11,-7+i*8),new Vector3(1.5f,8,1.5f),red);
   for(int i=0;i<4;i++){
    float z=-9+i*6;
    var wallPanel=Detail("Lobby wall panel",V(150+side*14.35f,z,3.8f),new Vector3(.12f,5.4f,4.9f),metal);ApplySurface(wallPanel,"BrushedMetal",Vector2.one,new Color(.50f,.55f,.59f));
    Detail("Lobby vertical light",V(150+side*14.2f,z-2.25f,3.8f),new Vector3(.08f,5.4f,.07f),red*.6f,true);
   }
   Detail("Lobby floor guide",V(150+side*4.2f,0,.02f),new Vector3(.08f,.03f,26),red*.5f,true);
   for(int i=0;i<3;i++){
    float z=-5+i*7;
    var bench=Detail("Lobby bench base",V(150+side*8,z,.42f),new Vector3(2.7f,.55f,1.0f),metal);ApplySurface(bench,"PlazaConcrete",Vector2.one,new Color(.50f,.55f,.58f));
    var seat=Detail("Lobby bench top",V(150+side*8,z,.72f),new Vector3(2.7f,.14f,1.0f),new Color(.22f,.19f,.20f));ApplySurface(seat,"BrushedMetal",Vector2.one,new Color(.58f,.56f,.57f));
   }
  }
  var desk=Detail("Lobby reception desk",V(143,10,.65f),new Vector3(4,1.3f,1.2f),metal);ApplySurface(desk,"BrushedMetal",Vector2.one,new Color(.55f,.6f,.63f));
  Detail("Lobby reception trim",V(143,9.36f,1.05f),new Vector3(3.6f,.06f,.03f),red*.45f,true);
  foreach(int side in new[]{-1,1}){Detail("Lobby access gate",V(150+side*2,-8,.6f),new Vector3(.28f,1.2f,1.5f),metal);Detail("Lobby gate indicator",V(150+side*2,-8,1.22f),new Vector3(.16f,.04f,.5f),cyan*.4f,true);}
  for(int i=-5;i<=5;i++)Detail("Lobby floor inlay",V(150+i*2.3f,0,.012f),new Vector3(.035f,.02f,28),new Color(.24f,.28f,.30f));
  for(int i=-5;i<=5;i++)Detail("Lobby cross joint",V(150,i*2.3f,.015f),new Vector3(27,.025f,.035f),new Color(.21f,.25f,.27f));
  Detail("Lobby rear feature wall",V(150,14.35f,5.7f),new Vector3(12,9,.22f),new Color(.09f,.105f,.13f));
  foreach(int side in new[]{-1,1}){
   var panel=Detail("Lobby information panel",V(150+side*9,14.34f,4.0f),new Vector3(3.3f,3.6f,.1f),metal);ApplySurface(panel,"BrushedMetal",Vector2.one,new Color(.42f,.47f,.51f));
   for(int line=0;line<3;line++)Detail("Lobby information line",V(150+side*9,14.25f,4.8f-line*.55f),new Vector3(2.3f,.05f,.03f),cyan*.45f,true);
   var lamp=new GameObject("Lobby warm pool light").AddComponent<Light>();lamp.type=LightType.Point;lamp.transform.position=V(150+side*7,5,6);lamp.range=11;lamp.intensity=.6f;lamp.color=new Color(.84f,.77f,.69f);lamp.shadows=LightShadows.None;
  }
  foreach(int side in new[]{-1,1})Detail("Lobby feature light",V(150+side*5.6f,14.2f,5.7f),new Vector3(.12f,8.8f,.1f),red,true);
  var title=new GameObject("Lobby wall title").AddComponent<TextMesh>();title.text="MAIN TOWER";title.anchor=TextAnchor.MiddleCenter;title.alignment=TextAlignment.Center;title.fontSize=64;title.characterSize=.17f;title.color=new Color(.68f,.73f,.75f);title.transform.position=V(150,14.0f,6.2f);
  Disc("Lobby central stage",V(150,5,.04f),3.1f,new Color(.17f,.20f,.23f));
  AddBeacon("스토리 2부",V(150,5),storyTwo);AddBeacon("메인타워 출구",V(150,-11),cyan,true);
 }
 void Update(){
  if(player==null)return;
  cam.clearFlags=(lobby||topView)?CameraClearFlags.SolidColor:(RenderSettings.skybox!=null?CameraClearFlags.Skybox:CameraClearFlags.SolidColor);cam.backgroundColor=lobby?Color.black:RenderSettings.fogColor;
  foreach(var b in beacons){b.marker.Rotate(0,35*Time.deltaTime,0,Space.World);var p=b.marker.position;p.y=2+Mathf.Sin(Time.time*2)*.2f;b.marker.position=p;}
  if(Input.GetKeyDown(KeyCode.Escape)&&!busy){if(screen!="")StartCoroutine(Transition(""));else {
   #if UNITY_STANDALONE
   Application.Quit();
   #endif
  }}
  if(screen==""&&!busy){
   Vector3 input=new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));
   if(input.sqrMagnitude>0){moving=false;route.Clear();autoStory="";Move(input.normalized);}
   else if(moving){var delta=destination-player.position;delta.y=0;if(delta.magnitude<.55f){if(route.Count>0)destination=route.Dequeue();else{moving=false;if(autoStory!=""){if((autoStory=="스토리 2부"&&!lobby)||(autoStory!="스토리 2부"&&lobby)){if(autoStory=="메인타워 출구")autoStory="";StartCoroutine(Portal());}else{Beacon targetBeacon=beacons.Find(b=>b.title==autoStory);if(targetBeacon!=null){if(targetBeacon.portal){autoStory="";StartCoroutine(Portal());}else Interact(targetBeacon);}else autoStory="";}}}}else Move(delta.normalized);}
   if(moving){if((player.position-lastWalkPosition).sqrMagnitude<.0004f)stuckTime+=Time.deltaTime;else stuckTime=0;lastWalkPosition=player.position;if(stuckTime>.65f){stuckTime=0;SetWalkDestination(walkGoal);}}else stuckTime=0;
   if(Input.GetMouseButtonDown(0)&&!PointerOverFieldUI()){RaycastHit hit;if(Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition),out hit,400)&&hit.normal.y>.7f){autoStory="";SetWalkDestination(lobby?hit.point:ClampWalkable(hit.point,.55f));}}
   nearest=null;float best=2.5f;foreach(var b in beacons){float distance=Vector3.Distance(player.position,b.pos+Vector3.up);if(distance<best){best=distance;nearest=b;}}
   if(nearest!=null&&nearest.portal&&new Vector2(player.position.x-nearest.pos.x,player.position.z-nearest.pos.z).magnitude<1.25f){dwell+=Time.deltaTime;if(dwell>=.5f)StartCoroutine(Portal());}else dwell=0;
   if(nearest!=null&&!nearest.portal&&Input.GetKeyDown(KeyCode.E))Interact(nearest);
  }
  PositionCompanions(false);
  UpdateForeground();
  area=lobby?"메인 타워 · 로비":player.position.z<-11?"빌딩 숲 · 진입로":"코퍼레이트 플라자 · 원형 공원";
  bool back=!lobby&&player.position.z<-11;
  if(!topView&&!back&&screen==""&&!busy){
   float scroll=Input.GetAxis("Mouse ScrollWheel");
   if(Mathf.Abs(scroll)>.001f){
    if(Time.time-zoomInputTime>1.5f)zoomResistance=0;zoomInputTime=Time.time;
    if(closeCamera){if(scroll<0){zoomResistance+=-scroll;if(zoomResistance>=.20f){closeCamera=false;fieldZoom=.50f;zoomResistance=0;}}else zoomResistance=0;}
    else if(scroll>0&&fieldZoom<=.501f){zoomResistance+=scroll;if(zoomResistance>=.20f){closeCamera=true;zoomResistance=0;}}
    else{fieldZoom=Mathf.Clamp(fieldZoom-scroll*1.5f,.50f,1f);zoomResistance=0;}
   }
  }
  bool close=!topView&&!back&&closeCamera;
  cam.fieldOfView=Mathf.Lerp(cam.fieldOfView,topView?60f:back||close?64f:52f,1-Mathf.Exp(-Time.deltaTime*4));
  Vector3 target=topView?(lobby?player.position+new Vector3(0,70,0):new Vector3(0,110,19)):player.position+(back?new Vector3(0,2.3f,-7.5f):close?new Vector3(0,2.8f,-8f):new Vector3(0,24,-20)*fieldZoom);
  bool quickView=Time.time<topCameraFastUntil;
  cam.transform.position=Vector3.Lerp(cam.transform.position,target,1-Mathf.Exp(-Time.deltaTime*(quickView?12f:4f)));
  Quaternion rot=topView?Quaternion.Euler(90,0,0):Quaternion.LookRotation(player.position+Vector3.up*(back||close?1.5f:0)-cam.transform.position);cam.transform.rotation=Quaternion.Slerp(cam.transform.rotation,rot,1-Mathf.Exp(-Time.deltaTime*(quickView?14f:5f)));UpdateCharacterViews();
  bool showOverlay=topView&&!lobby&&Vector3.Distance(cam.transform.position,target)<2f&&Quaternion.Angle(cam.transform.rotation,rot)<2f;
  topOverlayAlpha=showOverlay?Mathf.MoveTowards(topOverlayAlpha,1f,Time.deltaTime*3f):0f;
  foreach(var line in topViewLights){bool active=topView&&!lobby&&topOverlayAlpha>0;if(line.activeSelf!=active)line.SetActive(active);if(active){var renderer=line.GetComponent<Renderer>();if(renderer!=null)renderer.sharedMaterial.color=new Color(1,1,1,topOverlayAlpha);}}
 }
 void Move(Vector3 dir){Vector3 step=dir*7*Time.deltaTime;if(!lobby)step=ClampWalkable(player.position+step,.55f)-player.position;motor.Move(step+Vector3.down*2*Time.deltaTime);player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(dir),Time.deltaTime*12);}
 void Interact(Beacon b){if(b==null||busy)return;autoStory="";if(b.portal)StartCoroutine(Portal());else StartCoroutine(Transition(b.title));}
 Rect InteractionRect(Beacon b,float scale){var sp=cam.WorldToScreenPoint(b.pos+Vector3.up*3);return new Rect(sp.x/scale-78,(Screen.height-sp.y)/scale-78,156,48);}
 bool PointerOverFieldUI(){
  float scale=Screen.width/1440f,h=Screen.height/scale;Vector2 p=new Vector2(Input.mousePosition.x/scale,(Screen.height-Input.mousePosition.y)/scale);
  if(new Rect(150,h-94,1140,84).Contains(p))return true;
  if(new Rect(1200,20,220,55).Contains(p))return true;
  return nearest!=null&&!nearest.portal&&cam.WorldToScreenPoint(nearest.pos+Vector3.up*3).z>0&&InteractionRect(nearest,scale).Contains(p);
 }
 bool RouteCellOpen(Vector3 p){
  if(!lobby&&(ClampWalkable(p,.65f)-p).sqrMagnitude>.04f)return false;
  var a=p+Vector3.up*.45f;var b=p+Vector3.up*1.25f;
  int count=Physics.OverlapCapsuleNonAlloc(a,b,.48f,routeColliders);
  // Preserve exact behavior in the rare case that the reusable buffer fills up.
  var hits=count==routeColliders.Length?Physics.OverlapCapsule(a,b,.48f):routeColliders;
  int hitCount=count==routeColliders.Length?hits.Length:count;
  for(int i=0;i<hitCount;i++){
   var c=hits[i];if(c==motor||c.isTrigger||c.transform.IsChildOf(player)||c.bounds.max.y<.35f)continue;
   return false;
  }
  return true;
 }
 void SetWalkDestination(Vector3 goal){
  route.Clear();goal.y=player.position.y;walkGoal=goal;lastWalkPosition=player.position;stuckTime=0;
  int sx=Mathf.RoundToInt(player.position.x),sz=Mathf.RoundToInt(player.position.z),gx=Mathf.RoundToInt(goal.x),gz=Mathf.RoundToInt(goal.z);
  int minX=Mathf.Min(sx,gx)-20,maxX=Mathf.Max(sx,gx)+20,minZ=Mathf.Min(sz,gz)-20,maxZ=Mathf.Max(sz,gz)+20;
  var start=new Vector2Int(sx,sz);var finish=new Vector2Int(gx,gz);
  var open=new List<Vector2Int>{start};var cost=new Dictionary<Vector2Int,float>{{start,0}};var parent=new Dictionary<Vector2Int,Vector2Int>();var closed=new HashSet<Vector2Int>();
  bool found=false;int iterations=0;
  while(open.Count>0&&iterations++<12000){
   int best=0;float score=float.MaxValue;for(int i=0;i<open.Count;i++){var n=open[i];float f=cost[n]+Vector2.Distance(n,finish);if(f<score){score=f;best=i;}}
   var current=open[best];open.RemoveAt(best);if(current==finish){found=true;break;}closed.Add(current);
   for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){
    if(x==0&&z==0)continue;var next=current+new Vector2Int(x,z);if(next.x<minX||next.x>maxX||next.y<minZ||next.y>maxZ||closed.Contains(next))continue;
    Vector3 point=new Vector3(next.x,player.position.y-1f,next.y);if(next!=finish&&!RouteCellOpen(point))continue;
    if(x!=0&&z!=0&&(!RouteCellOpen(new Vector3(current.x+x,player.position.y-1f,current.y))||!RouteCellOpen(new Vector3(current.x,player.position.y-1f,current.y+z))))continue;
    float nextCost=cost[current]+(x!=0&&z!=0?1.4142f:1f);if(cost.TryGetValue(next,out float previous)&&previous<=nextCost)continue;
    cost[next]=nextCost;parent[next]=current;if(!open.Contains(next))open.Add(next);
   }
  }
  if(found){var path=new List<Vector3>();var node=finish;while(node!=start){path.Add(new Vector3(node.x,player.position.y,node.y));node=parent[node];}path.Reverse();foreach(var point in path)route.Enqueue(point);}
  if(route.Count>0){destination=route.Dequeue();}else destination=goal;
  moving=true;
 }
 void SetDestination(string title){foreach(var b in beacons)if(b.title==title){SetWalkDestination(b.pos);return;}}
 void NavigateTo(string title){
  autoStory="";
  if(title=="스토리 2부"&&!lobby){autoStory=title;SetDestination("메인타워 입구");return;}
  if(lobby&&title!="스토리 2부"){autoStory=title=="메인타워 출구"?"메인타워 출구":title;SetDestination("메인타워 출구");return;}
  autoStory=title;SetDestination(title);
 }
 IEnumerator Transition(string next){busy=true;moving=false;for(float t=0;t<.28f;t+=Time.unscaledDeltaTime){fade=t/.28f;yield return null;}fade=1;screen=next;yield return new WaitForSecondsRealtime(.12f);for(float t=0;t<.28f;t+=Time.unscaledDeltaTime){fade=1-t/.28f;yield return null;}fade=0;busy=false;}
 IEnumerator Portal(){busy=true;moving=false;dwell=0;for(float t=0;t<.3f;t+=Time.deltaTime){fade=t/.3f;yield return null;}fade=1;motor.enabled=false;lobby=!lobby;if(lobby&&topView){topView=false;cam.orthographic=false;}player.position=lobby?V(150,-7,1):V(0,24,1);motor.enabled=true;PositionCompanions(true);nearest=null;cam.transform.position=player.position+new Vector3(0,24,-20);yield return new WaitForSeconds(.15f);for(float t=0;t<.3f;t+=Time.deltaTime){fade=1-t/.3f;yield return null;}fade=0;busy=false;if(autoStory=="메인타워 출구")autoStory="";else if(autoStory!="")SetDestination(autoStory);}
 void InitStyles(){if(styles)return;styles=true;titleStyle=new GUIStyle(GUI.skin.label){font=font,fontSize=30,fontStyle=FontStyle.Bold,normal={textColor=Color.white}};bodyStyle=new GUIStyle(GUI.skin.label){font=font,fontSize=14,normal={textColor=new Color(.65f,.78f,.85f)}};labelStyle=new GUIStyle(bodyStyle){fontSize=13,alignment=TextAnchor.MiddleCenter,normal={textColor=Color.white}};buttonStyle=new GUIStyle(GUI.skin.button){font=font,fontSize=15,alignment=TextAnchor.MiddleCenter};locationStyle=new GUIStyle(bodyStyle){fontSize=11};viewButtonStyle=new GUIStyle(labelStyle){fontSize=13};storyOneMenuStyle=new GUIStyle(labelStyle){normal={textColor=new Color(1f,.67f,.78f)}};storyTwoMenuStyle=new GUIStyle(labelStyle){normal={textColor=new Color(.79f,.71f,1f)}};storyOneTitleStyle=new GUIStyle(titleStyle){normal={textColor=new Color(1f,.67f,.78f)}};storyTwoTitleStyle=new GUIStyle(titleStyle){normal={textColor=new Color(.79f,.71f,1f)}};}
 void Panel(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 void OnGUI(){InitStyles();float scale=Screen.width/1440f;GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(scale,scale,1));float h=Screen.height/scale;
  if(screen=="미니게임"){
   Panel(new Rect(0,0,1440,h),new Color(.025f,.035f,.045f));
   if(netRunnerMainCapture!=null)GUI.DrawTexture(new Rect(0,0,1440,h),netRunnerMainCapture,ScaleMode.ScaleToFit);
   else GUI.Label(new Rect(500,h/2-25,500,50),"미니게임 화면을 찾을 수 없습니다.",bodyStyle);
   if(!busy&&GUI.Button(new Rect(1200,20,220,46),"필드로 돌아가기",buttonStyle))StartCoroutine(Transition(""));
   if(fade>0)Panel(new Rect(0,0,1440,h),new Color(0,0,0,fade));
   return;
  }
  if(screen!=""){
   Panel(new Rect(0,0,1440,h),new Color(.025f,.035f,.065f));Color screenAccent=screen=="스토리 1부"?pink:screen=="스토리 2부"?storyTwo:cyan;GUIStyle screenTitle=screen=="스토리 1부"?storyOneTitleStyle:screen=="스토리 2부"?storyTwoTitleStyle:titleStyle;Panel(new Rect(530,h/2-95,70,4),screenAccent);GUI.Label(new Rect(530,h/2-65,600,55),screen,screenTitle);
   if(!busy&&GUI.Button(new Rect(530,h/2+20,380,60),"뒤로가기",buttonStyle))StartCoroutine(Transition(""));
  }else{
   Panel(new Rect(22,20,230,42),new Color(.02f,.03f,.06f,.78f));Panel(new Rect(22,20,3,42),cyan);GUI.Label(new Rect(32,21,215,20),"NIGHT CITY  /  EVENT FIELD",locationStyle);GUI.Label(new Rect(32,39,215,20),area,locationStyle);
   if(!lobby){Panel(new Rect(1280,20,140,38),new Color(.02f,.03f,.06f,.78f));if(GUI.Button(new Rect(1280,20,140,38),topView?"쿼터뷰 / 복귀":"탑뷰",viewButtonStyle)){topView=!topView;topCameraFastUntil=Time.time+1f;cam.orthographic=topView;cam.orthographicSize=52;moving=false;}}
   if(topView){var point=cam.WorldToScreenPoint(player.position);if(point.z>0){float px=point.x/scale,py=(Screen.height-point.y)/scale;Panel(new Rect(px-5,py-5,10,10),Color.white);GUI.Label(new Rect(px+8,py-13,90,24),"현재 위치",bodyStyle);}}
   if(!topView){
   foreach(var b in beacons){if((b.pos.x>100)!=lobby||Vector3.Distance(player.position,b.pos)>24)continue;var sp=cam.WorldToScreenPoint(b.pos+Vector3.up*3);if(sp.z<=0)continue;var rect=new Rect(sp.x/scale-80,(Screen.height-sp.y)/scale-18,160,32);if(b!=nearest){Panel(rect,new Color(.02f,.03f,.07f,.6f));GUI.Label(rect,b.title,labelStyle);}}
   if(nearest!=null&&!busy&&cam.WorldToScreenPoint(nearest.pos+Vector3.up*3).z>0){Rect r=InteractionRect(nearest,scale);if(nearest.portal){Panel(r,new Color(.02f,.03f,.07f,.6f));GUI.Label(r,nearest.title,labelStyle);}else if(GUI.Button(r,"◇  "+nearest.title,buttonStyle))Interact(nearest);}
   string[] names=lobby?lobbyMenu:outdoorMenu;
   float width=165,gap=8,total=names.Length*width+(names.Length-1)*gap,start=(1440-total)/2;
   for(int i=0;i<names.Length;i++){Rect r=new Rect(start+i*(width+gap),h-72,width,52);Panel(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),new Color(.03f,.05f,.07f,.82f));Panel(new Rect(r.x,r.y,3,r.height),i==0?pink:i==1?storyTwo:cyan*.65f);if(GUI.Button(r,"◇  "+names[i],i==0?storyOneMenuStyle:i==1?storyTwoMenuStyle:labelStyle)){NavigateTo(names[i]);}}
   }
  }
  if(fade>0)Panel(new Rect(0,0,1440,h),new Color(0,0,0,fade));
 }
 void DrawMap(Rect r){Panel(r,new Color(.02f,.03f,.06f,.92f));GUI.Label(new Rect(r.x,r.y+7,r.width,25),"FIELD MAP",labelStyle);Vector2 center=new Vector2(r.center.x,r.y+112);MapLine(center+new Vector2(0,65),center);MapLine(center,center+new Vector2(-55,-40));MapLine(center,center+new Vector2(55,-40));MapLine(center,center+new Vector2(0,-65));Panel(new Rect(center.x-15,center.y-15,30,30),new Color(.15f,.4f,.38f));foreach(var b in beacons){if(b.pos.x>100)continue;Vector2 p=center+new Vector2(b.pos.x*2,-b.pos.z*2);Panel(new Rect(p.x-3,p.y-3,6,6),b.color);}var pp=center+new Vector2(player.position.x*2,-player.position.z*2);Panel(new Rect(pp.x-4,pp.y-4,8,8),Color.white);}
 void MapLine(Vector2 a,Vector2 b){var old=GUI.matrix;GUIUtility.RotateAroundPivot(Vector2.SignedAngle(Vector2.right,b-a),a);Panel(new Rect(a.x,a.y-2,(b-a).magnitude,4),new Color(.2f,.38f,.48f));GUI.matrix=old;}
}













