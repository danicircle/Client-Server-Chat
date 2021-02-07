using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dataHouse;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace client
{
     class Client
     {
          public static Socket grand;
          public static string name;
          public static string id;


          static void Main(string[] args)
          {

               Console.WriteLine("Enter Your Name: ");
               name = Console.ReadLine();

               A:Console.Clear();
               Console.WriteLine("Enter host IP Address: ");
               string ip = Console.ReadLine();

               grand = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

               IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), 8161);

               try
               {
                    grand.Connect(ipep);
               }
               catch
               {
                    Console.WriteLine("Could not connect to host :/ ");
                    Thread.Sleep(1000);
                    goto A;
               }

               Thread t = new Thread(Data_IN);
               t.Start();

               for (; ; )
               {
                    Console.Write("::>");
                    string input = Console.ReadLine();

                    Packet p = new Packet(PacketType.Chat, id);
                    p.Gdata.Add(name);
                    p.Gdata.Add(input);
                    grand.Send(p.ToBytes());
               }

          }
          
          //client listener
          static void Data_IN()
          {
               byte[] buffer;
               int readBytes;

               for (; ; )
               {
                try
                {

                    buffer = new byte[grand.SendBufferSize];
                    readBytes = grand.Receive(buffer);

                    if (readBytes > 0)
                    {
                        DataManager(new Packet(buffer));
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Oops! sorry server has been disconnected...");
                }

               }
          }

          static void DataManager(Packet p)
          {
               switch (p.packetType)
               {
                    case PacketType.Registration:
                         
                         id = p.Gdata[0];
                         break;

                    case PacketType.Chat:
                         ConsoleColor c = Console.ForegroundColor;
                         Console.ForegroundColor = ConsoleColor.Cyan;

                         Console.WriteLine(p.Gdata[0] + ": " + p.Gdata[1]);
                         Console.ForegroundColor = c;
                         break;
               }
          }
     }
}
