using System;
using Gtk;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Cairo;
using System.Text;
using System.Timers;

namespace SeaWar_Client
{
	class MainClass
	{
		static Window win;
		static Label la;
		static Cairo.Context c;
		static DrawingArea dr;
		static System.Net.Sockets.Socket s;
		static Button but;
		static TextView text;
		static TextBuffer buf;
		static IPAddress ipAddress ;
		static IPEndPoint remoteEP ;
		static MenuItem  NG ;
		static MenuItem  Stat ;
		static MenuItem Con ;
		public static void Main (string[] args)
		{
			Application.Init ();
			 win = new Window ("Sea War");
			win.SetDefaultSize (400, 400);
			win.SetPosition (WindowPosition.Center);
			VBox vbox = new VBox (false,2);
			HBox hbox = new HBox (false, 3);
			MenuBar menu = new MenuBar ();
			Menu slide1 = new Menu ();
			MenuItem Info = new MenuItem ("Info");
			MenuItem  start = new MenuItem ("Start");
		    NG = new MenuItem ("New Game");
			Stat = new MenuItem ("Statistic");
			Con = new MenuItem ("Connect");
			text = new TextView();
			la = new Label ("");
			buf = text.Buffer;
			dr = new DrawingArea ();
			but = new Button ("Send");
			menu.Append (start);
			menu.Append (NG);
			menu.Append (Stat);
			vbox.PackStart (menu,false,true,0);
			vbox.PackStart (dr,true,true,2);
			vbox.PackStart (la,false,true,2);
			hbox.PackEnd (but,false,false,0);
			hbox.PackEnd (text, true,true, 2);
			vbox.PackEnd (hbox,false,false,0);
			win.Add (vbox);
			win.ShowAll ();
			win.DeleteEvent += closeApp;
			start.Activated += draw;
			NG.Activated += ActNG;
			Stat.Activated += ActStat;
			NG.Sensitive = false;
			Stat.Sensitive = false;
			text.Sensitive = false;
			but.Sensitive = false;
			 ipAddress = IPAddress.Parse ("127.0.0.1");
			 remoteEP = new IPEndPoint (ipAddress, 1041);
			but.Pressed += connect;
			Application.Run ();
		}
		static void ActStat(object ob,EventArgs e){
			byte[] bytes = new byte[1024];
			s = new System.Net.Sockets.Socket (AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp);
			s.Connect (remoteEP);
			byte[] msg = Encoding.ASCII.GetBytes ("Statistics");
			int bytesSent = s.Send (msg);
			int bytesRec = s.Receive (bytes);
			string str = Encoding.ASCII.GetString (bytes, 0, bytesRec);
			StatRead (str);
			s.Shutdown (SocketShutdown.Both);
			s.Close ();
		}
		static void ActNG(object o, EventArgs a){
			Stat.Sensitive = true;
			s = new System.Net.Sockets.Socket (AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp);
			s.Connect (remoteEP);
			byte[] msg = Encoding.ASCII.GetBytes ("New Game");
			int bytesSent = s.Send (msg);
			s.Shutdown (SocketShutdown.Both);
			s.Close ();
			Cairo.Context c1 = Gdk.CairoHelper.Create (dr.GdkWindow);
			c1.SetSourceRGB (255, 255, 255);
			c1.Paint ();
			c1.GetTarget().Dispose ();
			c1.Dispose ();
			draw (o,a);

		}
		static bool IsCorrectData(string[] data){
			if (data [0] == "Who" )
				return true;
			int y = 0;
			if (data.Length % 2 == 1)
				return false;
			
			if (data.Length == 1 && data [0] == "Who")
				return true;
			for (int j = 0; j < data.Length; j++) {
				
				if (j==0 || j % 2 == 0) {
					y = conv(data [j]);
					if (y < 1 || y > 10)
						return false;
				}
				if (j % 2 == 1) {
				 	y = Convert.ToInt32 (Convert.ToChar (data [j]));
					if (y < 65 || y > 74)
						return false;
				}
			}
			return true;
		}
		static void connect(object ob,EventArgs ev){
			byte[] bytes = new byte[1024];
			
			s = new System.Net.Sockets.Socket (AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp);				
			string DATA = buf.GetText (buf.StartIter, buf.EndIter, true);
			string[] data = DATA.Split (new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);
			bool g = IsCorrectData (data);
			if (!g) {
				la.Text = "WRONG DATA!!!";
				Time (2);
				buf.Clear ();
				DATA = "";
				return;
			}	
			string response = "";
			s.Connect (remoteEP);
			byte[] msg = Encoding.ASCII.GetBytes (DATA);
			int bytesSent = s.Send (msg);
			int bytesRec = s.Receive (bytes);
			response = Encoding.ASCII.GetString (bytes, 0, bytesRec);
			responseHandler (response);
			buf.Clear ();
			text.Sensitive = false;	
			but.Sensitive = false;
			Time (1);
			s.Shutdown (SocketShutdown.Both);
			s.Close ();
		}

		static void StatRead(string s){
			Window w = new Window ("Stat");
			w.SetDefaultSize (200,200);
			w.SetPosition (WindowPosition.Center);
			Label L = new Label ();
			L.Text = s;
			w.Add (L);
			w.ShowAll ();
		}
		static void responseHandler(string Resp){
			string[] DATA = Resp.Split (new char[]{ ' ','\n','\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (DATA [DATA.Length - 1] == "1") {
				pointDraw (DATA);
				return;
			}
			if (DATA [DATA.Length - 1] == "2") {
				crossDraw (DATA);
				return;
			}		
			if (DATA [DATA.Length - 1] == "3") {
				squareDraw (DATA);
				return;
			}
			if (DATA [DATA.Length - 1] == "5") 
				endShip (DATA);		
			
			if (DATA [DATA.Length - 1] == "4") {				
				la.Text = "WRONG DATA!!!";
				Time (2);
				return;
			}

		}

		static void squareDraw(string[] DATA){
			int temp = DATA.Length-1;
			for (int i = 0; i < temp; i+=2) {
				drawSquare (c, new PointD (conv (DATA [i]), conv (DATA [i+1])));
			}

		}
		static void pointDraw(string[] DATA){
			drawPoint (c, new PointD (conv (DATA [0]), conv (DATA [1])), new PointD (conv (DATA [4]), conv (DATA [5])));
		}
		static void crossDraw(string[] DATA){
			drawCross (c, new PointD (conv (DATA [0]), conv (DATA [1])), new PointD (conv (DATA [2]), conv (DATA [3])), 
				new PointD (conv (DATA [4]), conv (DATA [5])), new PointD (conv (DATA [6]), conv (DATA [7])));
		}
		static void endShip(string[] DATA){
			drawCross (c, new PointD (conv (DATA [DATA.Length - 9]), conv (DATA [DATA.Length - 8])),
				new PointD (conv (DATA [DATA.Length - 7]), conv (DATA [DATA.Length - 6])),
				new PointD (conv (DATA [DATA.Length - 5]), conv (DATA [DATA.Length - 4])),
				new PointD (conv (DATA [DATA.Length - 3]), conv (DATA [DATA.Length - 2])));			
			int i = 0;
			for(int j=0;j<4;j++,i+=8) {
				drawLine1 (new PointD (conv (DATA [i]), conv (DATA [i+1])), new PointD (conv (DATA [i+2]), conv (DATA [i+3])), c);
				drawLine1 (new PointD (conv (DATA [i]), conv (DATA [i+1])), new PointD (conv (DATA [i+6]), conv (DATA [i+7])), c);
				drawLine1 (new PointD (conv (DATA [i+2]), conv (DATA [i+3])), new PointD (conv (DATA [i+4]), conv (DATA [i+5])), c);
				drawLine1 (new PointD (conv (DATA [i+4]), conv (DATA [i+5])), new PointD (conv (DATA [i+6]), conv (DATA [i+7])), c);
			}
		}
		static int conv(string s){
			return Convert.ToInt32 (s, 10);
		}
		static void Time(int i){
			Timer tmr = new Timer(1200);
			if(i==1)tmr.Elapsed += tmrAction1;
			if(i==2)tmr.Elapsed += tmrAction2;
			tmr.AutoReset =false;
			tmr.Start ();
		}
		static void tmrAction1(object obj,EventArgs ev){
			text.Sensitive = true;
			but.Sensitive = true;
		}
		static void tmrAction2(object obj,EventArgs ev){
			la.Text = "";
		}
		static 	void closeApp(object ob, EventArgs ev){
			Application.Quit ();
		}
		static void draw(object ob,EventArgs ev){
			c = Gdk.CairoHelper.Create (dr.GdkWindow);
			int a = 20;
			for (int i=0; i<11; i++) {
				int b = 20, d = 35;
				for (int j=0; j<10; j++) {
					drawLine (new PointD (b, a), new PointD (d, a), c);
					d += 15;
					b += 15;
				}
				a += 15;
			}
			a = 20;
			for (int g=0; g<11; g++) {
				int f = 20,h=35;
				for (int k=0; k<10; k++) {
					drawLine (new PointD(a,f),new PointD(a,h),c);
					f += 15;
					h += 15;
				}
				a+=15;
			}
			int q = 20;
			for ( int i=0; i<11; i++) {
				int s = 200, x = 215;
				for (int j=0; j<10; j++) {
					drawLine (new PointD (s,q), new PointD (x, q), c);
					x += 15;
					s += 15;
				}
				q += 15;
			}
			q = 200;
			for (int g=0; g<11; g++) {
				int v = 20,n=35;
				for (int k=0; k<10; k++) {
					drawLine (new PointD(q,v),new PointD(q,n),c);
					v += 15;
					n += 15;
				}
				q+=15;
			}
			int num = 1,X1=4,X2=185,Y=34,y=15,x1=23,x2=203;
			int hh = 65;
			for (int i=1; i<11; i++) {
				drawNumber (c, X1, Y, num);
				drawNumber (c, X2, Y, num);
				drawLetter (c, x1, y, Convert.ToChar(hh));
				drawLetter (c, x2, y, Convert.ToChar(hh));
				num++;
				Y += 15;
				x1 += 15;
				x2 += 15;
				hh++;
			}
			NG.Sensitive = true;

			text.Sensitive = true;
			but.Sensitive = true;
		}
		public static void drawSquare(Context c,PointD p1){
			c.Rectangle(p1,15,15);
			c.SetSourceRGB(40,40,0);
			c.SetFontSize( 14);
			c.Fill ();
			c.Stroke ();
		}
		public static void drawPoint(Context c,PointD p1,PointD p2){
			c.Arc ((p1.X+p2.X)/2, (p1.Y+p2.Y)/2, 4,0, Math.PI * 2);
			c.SetSourceRGB(40,0,0);
			c.Fill ();
		}
		public static void drawCross(Context c,PointD p1,PointD p2,PointD p3,PointD p4 ){
			drawLine1 (p1, p3, c);
			drawLine1 (p2, p4, c);
		}
		public static void drawLetter(Context g,int x,int y,char ch){
			g.SetSourceRGB (0, 0, 10);
			g.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Normal);
			g.SetFontSize (12);
			g.MoveTo (new PointD (x,y));
			g.ShowText (ch.ToString());
		}
		public static void drawNumber(Context g,int x,int y,int n){
			g.SetSourceRGB (0, 0, 10);
			g.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Normal);
			g.SetFontSize (12);
			g.MoveTo (new PointD (x,y));
			int u = n;
			g.ShowText (u.ToString ());
		}

		public static void drawLine(PointD v1,PointD v2,Context c)
		{
			c.SetSourceRGB (0.36, 0.25, 0.35);
			c.MoveTo (v1);
			c.LineTo (v2);
			c.Stroke ();
		}
		public static void drawLine1(PointD v1,PointD v2,Context c)
		{
			c.SetSourceRGB (0, 25, 0);
			c.MoveTo (v1);
			c.LineTo (v2);
			c.Stroke ();
		}
	}
}
