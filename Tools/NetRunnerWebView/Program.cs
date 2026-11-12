using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

class Program {
 [STAThread]
 static void Main(string[] args) {
  var app=new Application();
  if(args.Length<1||!File.Exists(args[0])){MessageBox.Show("NET RUNNER 로컬 파일을 찾을 수 없습니다.");return;}
  var window=new Window{Title="NET RUNNER",Width=950,Height=850,MinWidth=480,MinHeight=500,WindowStartupLocation=WindowStartupLocation.CenterScreen,Background=System.Windows.Media.Brushes.Black};
  if(args.Length>1&&long.TryParse(args[1],out long owner)&&owner!=0)window.SourceInitialized+=(_,_)=>{try{new WindowInteropHelper(window).Owner=new IntPtr(owner);}catch{}};
  var layout=new DockPanel();
  var back=new Button{Content="← 필드로 돌아가기",Height=42,HorizontalContentAlignment=HorizontalAlignment.Left,Padding=new Thickness(18,0,0,0)};
  back.Click+=(_,_)=>window.Close();DockPanel.SetDock(back,Dock.Top);layout.Children.Add(back);
  var web=new WebView2();layout.Children.Add(web);window.Content=layout;
  window.Loaded+=async (_,_)=>{
   try{
    var environment=await CoreWebView2Environment.CreateAsync(null,Path.Combine(Path.GetTempPath(),"NetRunnerLocalWebView"));
    await web.EnsureCoreWebView2Async(environment);
    web.CoreWebView2.Settings.AreDefaultContextMenusEnabled=false;
    web.CoreWebView2.NavigationStarting+=(_,e)=>{if(!e.Uri.StartsWith("file:",StringComparison.OrdinalIgnoreCase)&&!e.Uri.StartsWith("about:",StringComparison.OrdinalIgnoreCase))e.Cancel=true;};
    web.Source=new Uri(Path.GetFullPath(args[0]));
   }catch(Exception ex){MessageBox.Show("NET RUNNER를 열 수 없습니다. "+ex.Message);window.Close();}
  };
  window.Closed+=(_,_)=>web.Dispose();
  app.Run(window);
 }
}
