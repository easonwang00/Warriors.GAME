﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net.Sockets;




namespace BasicWeb
{
    internal class Server
    {


        public static HttpListener listener;
        static HttpClient client= new HttpClient();
        public static int pageViews = 0;
        public static int requestCount = 0;

       public static Dictionary<string, Pachetto> dataset = new Dictionary<string, Pachetto>();
       public static Pachetto player;
       public static string url;

        public static bool Spawn=true;
        public static float Timer = 10;
        public static float[] Postionx= new float[100];
        public static float[] Postiony = new float[100];
        public static int MeleCount=0;



        public Server()
        {
            Start();
        }

        public static void SetRandom()
        {
            Random random = new Random();
            Timer = (float)random.NextDouble() * (float)10;
            Timer += 10;
            
        }
        public static void SpawnApple()
        {

            if (dataset.Count>0&& MeleCount <= 5 && Spawn)
            {
                Random random = new Random();

                dataset["1"].applex = (float)random.NextDouble() * (float)5.5;
                dataset["1"].applex -= (float)2.8;
                dataset["1"].appley = (float)random.NextDouble() * (float)2.35;
                dataset["1"].appley -= (float)1.25;
                dataset["1"].apple = true;
                Postionx[MeleCount] = dataset["1"].applex;
                Postiony[MeleCount] = dataset["1"].appley;
                MeleCount++;
                Console.WriteLine("pozione cambiata x{0},y{1},Mele: {2}", dataset["1"].appley, dataset["1"].applex, MeleCount);
                Spawn = false;

            }
            else if (dataset.Count>0&&!Spawn)
            {
                dataset["1"].apple = false;
                Console.WriteLine(dataset["1"].apple);
                Spawn = true;
            }
        }
        public static async Task CheckDeath()
        {

            foreach (KeyValuePair<string, Pachetto> kvp in dataset)
            {
                float x = kvp.Value.posizione[0];
                float y = kvp.Value.posizione[1];
                float vx = kvp.Value.velocity[0];
                float vy = kvp.Value.velocity[1];
                if (Postiony != null)
                {
                    for(int i=0; i<Postiony.Length; i++)
                    {
                        float disMela = (float)Math.Sqrt(Math.Pow(x - Postionx[i], 2) + Math.Pow(y - Postiony[i], 2));
                        if (disMela < 0.1)
                        {
                            if (kvp.Value.vivo < 3)
                            {
                                kvp.Value.vivo++;
                            }
                        }

                    }

                }

                foreach (KeyValuePair<string, Pachetto> kvp2 in dataset)
                   {
                    if (kvp.Key != kvp2.Key)
                    {
                        float x2=kvp2.Value.posizione[0];
                        float y2=kvp2.Value.posizione[1];
                        float vx2=kvp2.Value.velocity[0];
                        float vy2=kvp2.Value.velocity[1];


                        float dis;
                        dis = (float)Math.Sqrt(Math.Pow(x - x2, 2) + Math.Pow(y - y2, 2));
                            if (dis < 0.1 && ((Math.Abs(vx) > Math.Abs(vx2) || Math.Abs(vy) > Math.Abs(vy2)))&&kvp2.Value.Colpito)
                            {
                                Console.WriteLine("mortoooooooooooooooooooooooooooooooooooooooooooooooooooooo");
                                kvp2.Value.vivo=kvp2.Value.vivo-1;
                                kvp.Value.Colpito = false;
                            }       
                            //else if (dis < 0.1 && ((Math.Abs(vx) < Math.Abs(vx2) || Math.Abs(vy) < Math.Abs(vy2)))&&kvp2.Value.Colpito)
                            //{
                            //    kvp.Value.vivo=kvp.Value.vivo - 1;
                            //    kvp2.Value.Colpito = false;
                            //    Console.WriteLine("mortoooooooooooooooooooooooooooooooooooooooooooooooooooooo");

                            //}

                        if (!kvp.Value.Colpito)
                        {
                            kvp.Value.Time-=(float)0.01;
                            if(kvp.Value.Time < 0)
                            {
                                kvp.Value.Colpito=true;
                                kvp.Value.Time=5;

                            }
                        }


                    }

                }
            }
        }
        public static async Task inattivita()
        {
            foreach(KeyValuePair<string, Pachetto> kvp in dataset)
            {
                float x = kvp.Value.posizione[0];
                float y = kvp.Value.posizione[1];
                Thread.Sleep(20000);
                float newx=kvp.Value.posizione[0];
                float newy = kvp.Value.posizione[1];

                if(x == newx && y == newy)
                {
                    dataset.Remove(kvp.Key);
                }

            }

        }

        public static void calcolo()
        {
            foreach (KeyValuePair<string, Pachetto> kvp in dataset)
            {
                kvp.Value.persone = pageViews;

            }
            if (Timer <= 0)
            {
                SpawnApple();
                SetRandom();
            }
            else Timer -= (float)0.1;
            CheckDeath();




            foreach (KeyValuePair<string, Pachetto> kvp in dataset)
            {
                Console.WriteLine("Key: {0}, Value x: {1}, Value y:{2}, vita:{3},player online: {4},apple x{5},y{6},{7}", kvp.Key, kvp.Value.posizione[0], kvp.Value.posizione[1], kvp.Value.vivo, kvp.Value.persone, kvp.Value.applex, kvp.Value.appley, kvp.Value.apple);

            }
        }
        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;
            SetRandom();
            while (runServer)
            {
                //aspetta il pachetto 
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                var request = new HttpRequestMessage(HttpMethod.Post,url);



                //permesso per inviare il pachetto
                resp.AddHeader("Access-Control-Allow-Origin", "*");
                resp.AddHeader("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
                resp.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
                resp.AddHeader("Acesss-Control-Max-Age", "86400");
                resp.AddHeader("Access-Control-Allow-Headers", "Access-Control-Allow-Headers, Origin,Accept, X-Requested-With, Content-Type, Access-Control-Request-Method, Access-Control-Request-Headers");


                //lettura pachetto
                System.IO.Stream body = req.InputStream;
                System.Text.Encoding encoding = req.ContentEncoding;
                System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                string ris = reader.ReadToEnd();




                if (ris != null)
                {
                player = JsonConvert.DeserializeObject<Pachetto>(ris);
                    if (player != null)
                    {
                        if (player.id > 100)
                        {
                            //Console.WriteLine(player.persone);
                            player.persone = pageViews;
                            pageViews++;

                        }
                        if (pageViews > 0) dataset.Remove("200");



                        dataset[player.id.ToString()] = player;
                        await Task.Run(() => {
                            calcolo();
                          });
                        await Task.Run(() => {
                            //inattivita();
                        });

                        string data = System.Text.Json.JsonSerializer.Serialize(dataset);
                        request.Content = new StringContent(data,Encoding.UTF8,"application/json");




                        //invio risposta
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
                        resp.ContentType = "application/json";
                        resp.ContentEncoding = Encoding.UTF8;
;


                        await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                    resp.Close();

            }
        }


        public string GetIp()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            localIP = "http://" + localIP + ":8000/";
            return localIP;
        }

        public void Start()
        {
            url=GetIp();
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("server partito in {0}", url);

            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

 
            listener.Close();
        }

    }
}

