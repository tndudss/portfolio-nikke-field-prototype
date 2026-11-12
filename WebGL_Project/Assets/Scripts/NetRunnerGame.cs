using System.Collections.Generic;
using UnityEngine;

// Unity adaptation of the NET RUNNER HTML prototype.
public sealed class NetRunnerGame {
 readonly Color[] colors={new Color(.84f,.88f,.23f),new Color(.33f,.83f,.74f),new Color(.32f,.62f,.91f),new Color(.83f,.58f,.86f)};
 readonly List<int[]> pairs=new List<int[]>();
 readonly List<List<int>> paths=new List<List<int>>();
 readonly HashSet<int> awarded=new HashSet<int>();
 string phase="home"; int score,best,round,drag=-1; float endTime,countdownEnd,remaining=90;
 GUIStyle text,large,small,button; Font activeFont;
 static bool Adjacent(int a,int b)=>Mathf.Abs(a%5-b%5)+Mathf.Abs(a/5-b/5)==1;
 void Shuffle(List<int> list){for(int i=list.Count-1;i>0;i--){int j=Random.Range(0,i+1);int v=list[i];list[i]=list[j];list[j]=v;}}
 bool Walk(List<int> path,HashSet<int> used,int length){
  if(path.Count==length)return true;
  int tail=path[path.Count-1];var next=new List<int>{tail-5,tail+5,tail-1,tail+1};
  next.RemoveAll(n=>n<0||n>=25||!Adjacent(tail,n)||used.Contains(n)||path.Contains(n));Shuffle(next);
  foreach(int n in next){path.Add(n);if(Walk(path,used,length))return true;path.RemoveAt(path.Count-1);}return false;
 }
 void Puzzle(){
  pairs.Clear();paths.Clear();awarded.Clear();drag=-1;int count=Mathf.Min(round,4);
  for(int attempt=0;attempt<150;attempt++){
   var used=new HashSet<int>();var solutions=new List<List<int>>();
   for(int c=0;c<count;c++){
    var starts=new List<int>();for(int i=0;i<25;i++)if(!used.Contains(i))starts.Add(i);Shuffle(starts);
    List<int> found=null;
    foreach(int start in starts){var p=new List<int>{start};if(Walk(p,used,Random.Range(3,7))){found=p;break;}}
    if(found==null)break;foreach(int n in found)used.Add(n);solutions.Add(found);
   }
   if(solutions.Count!=count)continue;
   foreach(var p in solutions){pairs.Add(new[]{p[0],p[p.Count-1]});paths.Add(new List<int>());}return;
  }
  int[][] fallback={new[]{0,18},new[]{15,22},new[]{16,20},new[]{3,23}};
  for(int i=0;i<count;i++){pairs.Add(fallback[i]);paths.Add(new List<int>());}
 }
 void Start(){score=0;round=1;remaining=90;phase="countdown";countdownEnd=Time.realtimeSinceStartup+3;Puzzle();}
 public void Close(){phase="home";drag=-1;}
 bool Connected(int c){var p=paths[c];return p.Count>1&&p[0]!=p[p.Count-1]&&(p[0]==pairs[c][0]||p[0]==pairs[c][1])&&(p[p.Count-1]==pairs[c][0]||p[p.Count-1]==pairs[c][1]);}
 void Begin(int n){
  for(int i=0;i<pairs.Count;i++)if(pairs[i][0]==n||pairs[i][1]==n){paths[i].Clear();paths[i].Add(n);drag=i;return;}
  for(int i=0;i<paths.Count;i++){int index=paths[i].IndexOf(n);if(index>=0){paths[i].RemoveRange(index+1,paths[i].Count-index-1);drag=i;return;}}
 }
 void Move(int n){
  if(drag<0)return;int c=drag;var p=paths[c];int tail=p[p.Count-1];if(n==tail||!Adjacent(tail,n))return;
  for(int i=0;i<pairs.Count;i++)if(i!=c&&(pairs[i][0]==n||pairs[i][1]==n||paths[i].Contains(n)))return;
  int previous=p.IndexOf(n);if(previous>=0){p.RemoveRange(previous+1,p.Count-previous-1);return;}
  p.Add(n);if(!Connected(c))return;drag=-1;if(awarded.Add(c))score=Mathf.Min(999999,score+1000);best=Mathf.Max(best,score);
  for(int i=0;i<pairs.Count;i++)if(!Connected(i))return;
  score=Mathf.Min(999999,score+5000);best=Mathf.Max(best,score);round=Mathf.Min(99,round+1);Puzzle();
 }
 void Styles(Font font){
  if(text!=null&&activeFont==font)return;activeFont=font;
  text=new GUIStyle(GUI.skin.label){font=font,fontSize=20,normal={textColor=Color.white}};
  small=new GUIStyle(text){fontSize=15,normal={textColor=new Color(.65f,.75f,.75f)}};
  large=new GUIStyle(text){fontSize=38,fontStyle=FontStyle.Bold};
  button=new GUIStyle(GUI.skin.button){font=font,fontSize=18};
 }
 void Box(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 void Line(Vector2 a,Vector2 b,float width,Color color){var old=GUI.matrix;GUIUtility.RotateAroundPivot(Vector2.SignedAngle(Vector2.right,b-a),a);Box(new Rect(a.x,a.y-width/2,(b-a).magnitude,width),color);GUI.matrix=old;}
 public bool Draw(float h,Font font){
  Styles(font);float now=Time.realtimeSinceStartup;
  if(phase=="countdown"&&now>=countdownEnd){phase="playing";endTime=countdownEnd+90;}
  if(phase=="playing"){remaining=Mathf.Max(0,endTime-now);if(remaining<=0){phase="result";drag=-1;}}
  Box(new Rect(0,0,1440,h),new Color(.025f,.04f,.05f));
  GUI.Label(new Rect(45,22,700,55),"NET RUNNER  /  미니게임",large);
  if(GUI.Button(new Rect(1200,25,190,45),"필드로 돌아가기",button))return true;
  float size=Mathf.Min(480,h-230);size=Mathf.Max(220,size);float x=(1440-size)/2,y=(h-size)/2+30,unit=size/5;
  if(phase=="home"){
   GUI.Label(new Rect(x,y+25,600,55),"회로 연결",large);
   GUI.Label(new Rect(x,y+100,600,35),"같은 색의 점을 드래그해 연결하세요.",text);
   GUI.Label(new Rect(x,y+140,650,70),"90초 동안 최대한 많은 라운드를 완료하세요.\n다른 선과 교차할 수 없습니다.",small);
   GUI.Label(new Rect(x,y+220,500,35),"최고 기록  "+best.ToString("N0"),text);
   if(GUI.Button(new Rect(x,y+280,230,55),"게임 시작",button))Start();
   return false;
  }
  GUI.Label(new Rect(x,y-95,800,40),"SCORE  "+score.ToString("D6")+"     BEST  "+best.ToString("D6")+"     ROUND  "+round.ToString("D2"),text);
  GUI.Label(new Rect(x,y-55,350,35),"TIME  "+Mathf.CeilToInt(remaining).ToString("D2")+"초",text);
  if(phase=="playing"&&GUI.Button(new Rect(x+size-180,y-65,85,40),"RESET",button)){foreach(var p in paths)p.Clear();drag=-1;}
  if((phase=="playing"||phase=="paused")&&GUI.Button(new Rect(x+size-90,y-65,90,40),phase=="playing"?"일시정지":"계속",button)){
   if(phase=="playing"){remaining=Mathf.Max(0,endTime-now);phase="paused";drag=-1;}else{endTime=now+remaining;phase="playing";}
  }
  Box(new Rect(x,y,size,size),new Color(.045f,.075f,.085f));
  for(int i=0;i<=5;i++){Box(new Rect(x+i*unit,y,1,size),new Color(.18f,.24f,.25f));Box(new Rect(x,y+i*unit,size,1),new Color(.18f,.24f,.25f));}
  for(int i=0;i<paths.Count;i++)for(int j=1;j<paths[i].Count;j++){
   int a=paths[i][j-1],b=paths[i][j];Line(new Vector2(x+(a%5+.5f)*unit,y+(a/5+.5f)*unit),new Vector2(x+(b%5+.5f)*unit,y+(b/5+.5f)*unit),unit*.13f,colors[i]);
  }
  for(int i=0;i<pairs.Count;i++)foreach(int n in pairs[i]){
   float px=x+(n%5+.5f)*unit,py=y+(n/5+.5f)*unit;Box(new Rect(px-unit*.18f,py-unit*.18f,unit*.36f,unit*.36f),colors[i]);
   if(Connected(i))Box(new Rect(px-unit*.055f,py-unit*.055f,unit*.11f,unit*.11f),new Color(.04f,.07f,.08f));
  }
  Event e=Event.current;int cx=Mathf.FloorToInt((e.mousePosition.x-x)/unit),cy=Mathf.FloorToInt((e.mousePosition.y-y)/unit);
  if(phase=="playing"&&cx>=0&&cx<5&&cy>=0&&cy<5){
   if(e.type==EventType.MouseDown&&e.button==0){Begin(cy*5+cx);e.Use();}
   else if(e.type==EventType.MouseDrag&&drag>=0){Move(cy*5+cx);e.Use();}
  }
  if(e.rawType==EventType.MouseUp)drag=-1;
  if(phase=="countdown"||phase=="paused"||phase=="result"){
   Box(new Rect(x,y,size,size),new Color(0,0,0,.8f));
   string title=phase=="countdown"?Mathf.CeilToInt(countdownEnd-now).ToString():phase=="paused"?"일시정지":"TIME OUT";
   GUI.Label(new Rect(x+35,y+size*.3f,size-70,60),title,large);
   if(phase=="result"){
    GUI.Label(new Rect(x+35,y+size*.45f,size-70,40),"최종 점수  "+score.ToString("N0"),text);
    if(GUI.Button(new Rect(x+35,y+size*.6f,170,48),"다시 시작",button))Start();
    if(GUI.Button(new Rect(x+220,y+size*.6f,170,48),"메뉴로",button))Close();
   }
  }
  return false;
 }
}
