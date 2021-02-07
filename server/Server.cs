using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;
using dataHouse;

namespace server
{
     class Server
     {


          static Socket listernerSocket;
          static List<ClientData> _clients;
          static void Main(string[] args)
          {
               Console.WriteLine("Starting Server On " + Packet.GetIP4Address());
               // starting the socket
               listernerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
               _clients = new List<ClientData>();  //making client list

               //binding
               IPEndPoint ip = new IPEndPoint(IPAddress.Parse(Packet.GetIP4Address()), 8161);
               listernerSocket.Bind(ip);

               //creating thread 

               Thread listenThread = new Thread(ListenThread);
               listenThread.Start();



          }


          //Listener -  listens for clients trying to connect

          static void ListenThread()
          {
               for (; ; )
               { 
                    listernerSocket.Listen(0);
                    _clients.Add(new ClientData(listernerSocket.Accept()));
               }
          }

          //clientdata thread - recieves data from each client individualy
          public static void Data_IN(object cSocket)
          {
               Socket clientSocket = (Socket)cSocket;

               byte[] buffer;
               int readyBytes;

               for (; ; )
               {
                try
                {
                    buffer = new byte[clientSocket.SendBufferSize];
                    readyBytes = clientSocket.Receive(buffer);

                    if (readyBytes > 0)
                    {
                        // data game here

                        Packet packet = new Packet(buffer);
                        DataManager(packet);

                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Oops! sorry client has been disconnected...");
                }


            }

          }


          // data manager takes the data from clients
          public static void DataManager(Packet p)
          {
               switch (p.packetType)
               {
                    case PacketType.Chat:
                         foreach (ClientData c in _clients)
                              c.clientSocket.Send(p.ToBytes());
                         break;
               }
          }

         
          
     }
     class ClientData
     {
          public Socket clientSocket;
          public Thread clientThread;
          public string id;

          public ClientData()
          {
               id = Guid.NewGuid().ToString();
               clientThread = new Thread(Server.Data_IN);
               clientThread.Start(clientSocket);
               SendRegPacket();
          }
          public ClientData(Socket clientSocket)
          {
               this.clientSocket = clientSocket;
               id = Guid.NewGuid().ToString();
               clientThread = new Thread(Server.Data_IN);
               clientThread.Start(clientSocket);
               SendRegPacket();
          }

          public void SendRegPacket()
          {
               Packet p = new Packet(PacketType.Registration, "server");
               p.Gdata.Add(id);

               clientSocket.Send(p.ToBytes()); // send data packet to client
          }
     }
}
