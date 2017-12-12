using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Timers;

namespace ConsoleWeb
{
    class Client
    {
        static void Main(string[] args)
        {        


        }

        public static void ysapi()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(TimeEvent);
            // 设置引发时间的时间间隔 此处设置为１秒（１０００毫秒）
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
            Console.WriteLine("按回车键结束程序");
            Console.WriteLine(" 等待程序的执行．．．．．．");
            Console.ReadLine();
        }

        // 当时间发生的时候需要进行的逻辑处理等
        //     在这里仅仅是一种方式，可以实现这样的方式很多．
        private static void TimeEvent(object source, ElapsedEventArgs e)
        {
            // 得到 hour minute second   如果等于某个值就开始执行某个程序。
            DateTime date1 = e.SignalTime; //.Date.ToString()+" "+e.SignalTime.Hour.ToString()+":"+e.SignalTime.Minute.ToString()+":"+e.SignalTime.Second.ToString());
            int intHour = e.SignalTime.Hour;
            int intMinute = e.SignalTime.Minute;
            int intSecond = e.SignalTime.Second;
            // 定制时间； 比如 在10：30 ：00 的时候执行某个函数
            int iHour = 10;
            int iMinute = 21;
            int iSecond = 00;
            // 设置 每秒钟的开始执行一次
            if (intSecond == iSecond)
            {
                Console.WriteLine("每秒钟的开始执行一次！");
            }
            // 设置 每个小时的３０分钟开始执行
            if (intMinute == iMinute && intSecond == iSecond)
            {
                Console.WriteLine("每个小时的３０分钟开始执行一次！");
            }

            // 设置 每天的１０：３０：００开始执行程序
            if (intHour == iHour && intMinute == iMinute && intSecond == iSecond)
            {
                Console.WriteLine("在每天１０点３０分开始执行！");
            }
        }

        #region 测试
        public void StartReceivingShow()
        {
            var client = new ClientWebSocket();
            client.ConnectAsync(new Uri("ws://echo.websocket.org"), CancellationToken.None).Wait();
            //client.ConnectAsync(new Uri("ws://localhost:4567/ws/"), CancellationToken.None).Wait();
            StartReceiving(client);

            string line;
            while ((line = Console.ReadLine()) != "exit")
            {
                var array = new ArraySegment<byte>(Encoding.UTF8.GetBytes(line));
                client.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        static async void StartReceiving(ClientWebSocket client)
        {
            while (true)
            {
                var array = new byte[4096];
                var result = await client.ReceiveAsync(new ArraySegment<byte>(array), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string msg = Encoding.UTF8.GetString(array, 0, result.Count);
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine("--> {0}", msg);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
            }
        }
        #endregion

    }
}

 
