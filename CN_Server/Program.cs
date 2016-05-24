using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;

public class SynchronousSocketListener {
	static string data = null;
	static List <Square> FieldServer;
	static List <Ship> shipsServer;
	static string[] DATA;
	static int NumberOfShots,NumberOfHit,NumberOfMiss,NumberOfGames;
	public static void StartListening() {
		File.WriteAllText ("C:\\Log.txt","");
		File.WriteAllText ("C:\\Statistics.txt","");
		NumberOfShots = 0;NumberOfHit = 0;NumberOfMiss = 0;NumberOfGames=1;
		FieldServer = new List<Square>();
		shipsServer = new List<Ship> ();
		byte[] bytes = new Byte[1024];
		fillListSquares ();
		IPAddress ipAddress=null;
		IPEndPoint localEndPoint = null;
		ipAddress = IPAddress.Parse ("127.0.0.1");
		localEndPoint = new IPEndPoint(ipAddress, 1041);
		Socket listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp );
		try {
			listener.Bind(localEndPoint);
			listener.Listen(1);
			do {
				Console.WriteLine("Waiting for data...");
				Socket handler = listener.Accept();
				data = null;
				string response="";
				bytes = new byte[1024];
				int bytesRec = handler.Receive(bytes);
				data += Encoding.ASCII.GetString(bytes,0,bytesRec);
				Console.WriteLine( "Data received : {0}", data);
				string z = DateTime.Now+"   Data received :  " + data +"\r\n";
				File.AppendAllText("C:\\Log.txt",z);
				DATA = data.Split (new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if(DATA[0]=="New" && DATA[1]=="Game"){ 
					Statistics(NumberOfGames,NumberOfShots,NumberOfMiss,NumberOfHit);
					FillShips();
					NumberOfGames++;NumberOfShots = 0;NumberOfHit = 0;NumberOfMiss = 0;
					continue;
				}
					if(DATA[0]=="Statistics") response = ReadStat();
					else{
						response = resp(convChar(DATA[0], DATA[1]),shipsServer,FieldServer,
						WhatShip(convChar(DATA[0], DATA[1]),shipsServer,FieldServer));
					}
					if(response[response.Length-1] == '1'){ 
						NumberOfMiss ++;
						NumberOfShots++;
				}
				if(response[response.Length-1] == '2' || response[response.Length-1] == '5'){
					NumberOfHit ++;
					NumberOfShots++;
					}
				byte[] msg = Encoding.ASCII.GetBytes(response);
				handler.Send(msg);
				Console.WriteLine( "Data send : {0}", response);
				string h = DateTime.Now+"   Data send :  " + response +"\r\n";
				File.AppendAllText("C:\\Log.txt",h);

			}while(data !="END ");

		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
		Console.WriteLine("\nPress ENTER to continue...");
		Console.Read();
	}
	static string ReadStat(){
		FileStream file = new FileStream ("C:\\Statistics.txt", FileMode.Open, FileAccess.Read);
		StreamReader read = new StreamReader (file);
		string str = "";
			while(!read.EndOfStream) str+=read.ReadLine ()+"\r\n";
		file.Close ();
		return str;
	}
	static void Statistics(int GameNum,int Shots,int Miss,int Hit){
		string text = "Game #"+GameNum+":::\tNumber of shots:"+Shots+"\tNumber of miss:"+Miss+"\tNumber of hit:"+Hit+"\n";
		File.AppendAllText("C:\\Statistics.txt",text);
	}
		
	static string resp(int k,List <Ship> ships, List <Square> Field,int NumOfShip){		
		if (NumOfShip == -1) {
			if (!Field [k].isEntered && !Field [k].HasShip) {
				Field [k].isEntered = true;
				return	
					(Field [k].A.X.ToString () + " " + Field [k].A.Y.ToString () + " " +
						Field [k].B.X.ToString () + " " + Field [k].B.Y.ToString () + " " +
						Field [k].C.X.ToString () + " " + Field [k].C.Y.ToString () + " " +
						Field [k].D.X.ToString () + " " + Field [k].D.Y.ToString () + " 1");
			}	
			if (!Field [k].isEntered && Field [k].HasShip) {
				Field [k].isEntered = true;
				return( 	
					Field [k].A.X.ToString () + " " + Field [k].A.Y.ToString () + " " +
					Field [k].B.X.ToString () + " " + Field [k].B.Y.ToString () + " " +
					Field [k].C.X.ToString () + " " + Field [k].C.Y.ToString () + " " +
					Field [k].D.X.ToString () + " " + Field [k].D.Y.ToString () + " 2");
			}
			if (Field [k].isEntered)
				return("4");
		}
		if (NumOfShip >= 0) {
			Field [k].isEntered = true;
			ships [NumOfShip].AmountOfDecks --;
			if (shipsServer [NumOfShip].AmountOfDecks == 0) {
				return(
					ships [NumOfShip].Deck1.A.X.ToString () + " " + ships [NumOfShip].Deck1.A.Y.ToString () + " " +
					ships [NumOfShip].Deck1.B.X.ToString () + " " + ships [NumOfShip].Deck1.B.Y.ToString () + " " +
					ships [NumOfShip].Deck1.C.X.ToString () + " " + ships [NumOfShip].Deck1.C.Y.ToString () + " " +
					ships [NumOfShip].Deck1.D.X.ToString () + " " + ships [NumOfShip].Deck1.D.Y.ToString () + " " +
					ships [NumOfShip].Deck2.A.X.ToString () + " " + ships [NumOfShip].Deck2.A.Y.ToString () + " " +
					ships [NumOfShip].Deck2.B.X.ToString () + " " + ships [NumOfShip].Deck2.B.Y.ToString () + " " +
					ships [NumOfShip].Deck2.C.X.ToString () + " " + ships [NumOfShip].Deck2.C.Y.ToString () + " " +
					ships [NumOfShip].Deck2.D.X.ToString () + " " + ships [NumOfShip].Deck2.D.Y.ToString () + " " +
					ships [NumOfShip].Deck3.A.X.ToString () + " " + ships [NumOfShip].Deck3.A.Y.ToString () + " " +
					ships [NumOfShip].Deck3.B.X.ToString () + " " + ships [NumOfShip].Deck3.B.Y.ToString () + " " +
					ships [NumOfShip].Deck3.C.X.ToString () + " " + ships [NumOfShip].Deck3.C.Y.ToString () + " " +
					ships [NumOfShip].Deck3.D.X.ToString () + " " + ships [NumOfShip].Deck3.D.Y.ToString () + " " +
					ships [NumOfShip].Deck4.A.X.ToString () + " " + ships [NumOfShip].Deck4.A.Y.ToString () + " " +
					ships [NumOfShip].Deck4.B.X.ToString () + " " + ships [NumOfShip].Deck4.B.Y.ToString () + " " +
					ships [NumOfShip].Deck4.C.X.ToString () + " " + ships [NumOfShip].Deck4.C.Y.ToString () + " " +
					ships [NumOfShip].Deck4.D.X.ToString () + " " + ships [NumOfShip].Deck4.D.Y.ToString () + " | " +
					Field [k].A.X.ToString () + " " + Field [k].A.Y.ToString () + " " +
					Field [k].B.X.ToString () + " " + Field [k].B.Y.ToString () + " " +
					Field [k].C.X.ToString () + " " + Field [k].C.Y.ToString () + " " +
					Field[k].D.X.ToString () + " " + Field [k].D.Y.ToString () + " 5");

			} else {
				return( Field [k].A.X.ToString () + " " + Field [k].A.Y.ToString () + " " +
					Field [k].B.X.ToString () + " " + Field [k].B.Y.ToString () + " " +
					Field[k].C.X.ToString () + " " + Field [k].C.Y.ToString () + " " +
					Field [k].D.X.ToString () + " " + Field [k].D.Y.ToString () + " 2");
			}
		}
		else return("4");
	}

	static int WhatShip(int k,List <Ship> ships, List<Square> Field){
		for (int i = 0; i < ships.Count; i++) {			
			if (ships [i].Deck1.A.X ==  Field [k].A.X &&
				ships [i].Deck1.A.Y ==  Field [k].A.Y &&
				ships [i].Deck1.B.X ==  Field [k].B.X &&
				ships [i].Deck1.B.Y ==  Field [k].B.Y &&
				ships [i].Deck1.C.X ==  Field [k].C.X &&
				ships [i].Deck1.C.Y ==  Field [k].C.Y &&
				ships [i].Deck1.D.X ==  Field [k].D.X &&
				ships [i].Deck1.D.Y ==  Field [k].D.Y)
				return i;
			if (ships [i].Deck2.A.X ==  Field [k].A.X &&
				ships [i].Deck2.A.Y ==  Field [k].A.Y &&
				ships [i].Deck2.B.X ==  Field [k].B.X &&
				ships [i].Deck2.B.Y ==  Field [k].B.Y &&
				ships [i].Deck2.C.X ==  Field [k].C.X &&
				ships [i].Deck2.C.Y ==  Field [k].C.Y &&
				ships [i].Deck2.D.X ==  Field [k].D.X &&
				ships [i].Deck2.D.Y ==  Field [k].D.Y)
				return i;
			if (ships [i].Deck3.A.X ==  Field [k].A.X &&
				ships [i].Deck3.A.Y ==  Field [k].A.Y &&
				ships [i].Deck3.B.X ==  Field [k].B.X &&
				ships [i].Deck3.B.Y ==  Field [k].B.Y &&
				ships [i].Deck3.C.X ==  Field [k].C.X &&
				ships [i].Deck3.C.Y ==  Field [k].C.Y &&
				ships [i].Deck3.D.X ==  Field [k].D.X &&
				ships [i].Deck3.D.Y ==  Field [k].D.Y)
				return i;
			if (ships [i].Deck4.A.X ==  Field [k].A.X &&
				ships [i].Deck4.A.Y ==  Field [k].A.Y &&
				ships [i].Deck4.B.X ==  Field [k].B.X &&
				ships [i].Deck4.B.Y ==  Field [k].B.Y &&
				ships [i].Deck4.C.X ==  Field [k].C.X &&
				ships [i].Deck4.C.Y ==  Field [k].C.Y &&
				ships [i].Deck4.D.X ==  Field [k].D.X &&
				ships [i].Deck4.D.Y ==  Field [k].D.Y)
				return i;
		}
		return -1;
	}
		

	static int convChar(string a1,string a2){
		return (conv (a1) - 1) * 10 + (Convert.ToInt32 (Convert.ToChar (a2))) - 65;
	}
	static int conv(string s){
		return Convert.ToInt32 (s,10);
	}
	static void fillListSquares(){
		int a=20, b= 35; int c=0, d=0;
		for (int f = 0; f < 10; f++) {	
			c = 200;
			d = 215;
			for (int g = 0; g < 10; g++) {
				FieldServer.Add (new Square (new PointD (c, a), new PointD (d, a), new PointD (d, b), new PointD (c, b)));
				c += 15;
				d += 15;
			}
			a += 15;
			b += 15;
		}
		FillShips ();
	}

	static void FillShips(){
		shipsServer.Clear ();
		FileStream file = new FileStream ("C:\\Ship1.txt", FileMode.Open, FileAccess.Read);
		StreamReader read = new StreamReader (file);
		string str = read.ReadToEnd ();
		int[] data = new int[20];
		string[] Data = str.Split (new char[]{ ' ', '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < Data.Length; i++) {
			data [i] = conv (Data [i]);
		}
		shipsServer.Add (new Ship (FieldServer [data [0]], FieldServer [data [1]], FieldServer [data [2]], FieldServer [data [3]]));
		shipsServer.Add (new Ship (FieldServer [data [4]], FieldServer [data [5]], FieldServer [data [6]]));
		shipsServer.Add (new Ship (FieldServer [data [7]], FieldServer [data [8]], FieldServer [data [9]]));
		shipsServer.Add (new Ship (FieldServer [data [10]], FieldServer [data [11]]));
		shipsServer.Add (new Ship (FieldServer [data [12]], FieldServer [data [13]]));
		shipsServer.Add (new Ship (FieldServer [data [14]], FieldServer [data [15]]));
		shipsServer.Add (new Ship (FieldServer [data [16]]));
		shipsServer.Add (new Ship (FieldServer [data [17]]));
		shipsServer.Add (new Ship (FieldServer [data [18]]));
		shipsServer.Add (new Ship (FieldServer [data [19]]));
		for (int i = 0; i < data.Length; i++)
			FieldServer [data [i]].HasShip = true;
		file.Close ();
	}
	

	public static int Main(String[] args) {
		StartListening();
		return 0;
	}
}


class Ship{
	public Ship (Square a1,Square a2,Square a3,Square a4){
		Deck1 = a1;
		Deck2 = a2;
		Deck3 = a3;
		Deck4 = a4;
		AmountOfDecks = 4;
	}
	public Ship (Square a1,Square a2,Square a3){
		Deck1 = a1;
		Deck2 = a2;
		Deck3 = a3;
		Deck4 = new Square (new PointD (0, 0), new PointD (0, 0), new PointD (0, 0), new PointD (0, 0));
		AmountOfDecks = 3;
	}
	public Ship (Square a1,Square a2){
		Deck1 = a1;
		Deck2 = a2;
		Deck3 = new Square (new PointD (0, 0), new PointD (0, 0), new PointD (0, 0), new PointD (0, 0));
		Deck4 = new Square (new PointD (0, 0), new PointD (0, 0), new PointD (0, 0), new PointD (0, 0));
		AmountOfDecks = 2;
	}
	public Ship (Square a1){
		Deck1 = a1;
		Deck2 = new Square (new PointD (0, 0), new PointD (0, 0), new PointD (0, 0), new PointD (0, 0));
		Deck3 = new Square (new PointD (0, 0), new PointD (0, 0), new PointD (0, 0), new PointD (0, 0));
		Deck4 = new Square (new PointD (0, 0), new PointD (0, 0), new PointD (0, 0), new PointD (0, 0));
		AmountOfDecks = 1;
	}
	public int AmountOfDecks;
	public Square Deck1;
	public Square Deck2;
	public Square Deck3;
	public Square Deck4;
}

class Square{
	public PointD A;
	public PointD B;
	public PointD C;
	public PointD D;
	public int num;
	public Square (PointD a1,PointD a2, PointD a3,PointD a4){
		A = a1;
		B = a2;
		C = a3;
		D = a4;
		HasShip = false;
		isEntered = false;
		isBlocked = false;
		num = 0;
	}
	public bool isBlocked;
	public bool HasShip;
	public bool isEntered;
}
class PointD{
	public int X;
	public int Y;

	public PointD (int x, int y){
		X = x;
		Y = y;
	}
}