using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using WebSocket4Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketUtil;
using System.Data.SqlClient;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using System.Timers;
using System.Configuration;

namespace webClient
{
    public partial class frmClient : Form
    {
        private delegate void SetPos(int ipos, string vinfo);//代理
        private static DataTable keywords = new DataTable();

        public static void initKeyWords()
        {
            try
            {
                string sqlCmd = " select * from pno_keyword ";
                keywords = DBHelpSql.Query(sqlCmd).Tables[0];
            }
            catch (Exception)
            {

            }
        }

        WebSocket websocket;
        public string clientName = "";
        public string username = "";
        public frmClient()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 获取配置文件地址信息
            var ip = ConfigurationManager.AppSettings["APWebSocketIP"];
            var port = ConfigurationManager.AppSettings["APWebSocketPort"];
            //赋值连接信息
            this.textBox1.Text = ip;
            this.textBox2.Text = port;
            
            this.listBox1.DrawMode = DrawMode.OwnerDrawFixed; // 属性里设置 
            initKeyWords();
            Form.CheckForIllegalCrossThreadCalls = false;
            label4.Text = "当前:" + clientName;
            label3.Text = GetLocalIP(); // 本地ip地址
            label6.Text = GetExtenalIpAddress(); //外网Ip地址
            //GetIP();
            // openCmd();
            label5.Text = Computer.CpuID;
            //batCre();
            string str = implementNew();
            if (!str.Equals("添加失败"))
            {
                MessageBox.Show("欢迎使用!");
            }
            else
            {
                MessageBox.Show("欢迎使用");
            }
        }


        public void show()
        {
            //System.Net.IPAddress ip = System.Net.IPAddress.Parse(Page.Request.UserHostAddress);
            //System.Net.IPHostEntry ine = System.Net.Dns.GetHostByAddress(ip);
            //Page.Response.Write(ine.HostName);
        }



        /// <summary>
        /// bat  批处理 强制重启
        /// </summary>
        public void batCre()
        {
            string filePath = "d:\\1.bat";
            // 实例化线程  
            using (Process process = new Process())
            {
                try
                {
                    FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    //电脑重启五次
                    sw.WriteLine("@echo off");
                    sw.WriteLine("if not exist c:\\1.txt echo. >c:\\1.txt & goto err1");
                    sw.WriteLine("if not exist c:\\2.txt echo. >c:\\2.txt & goto err1");
                    sw.WriteLine("if not exist c:\\3.txt echo. >c:\\3.txt & goto err1");
                    sw.WriteLine("if not exist c:\\4.txt echo. >c:\\4.txt & goto err1");
                    sw.WriteLine("if not exist c:\\5.txt echo. >c:\\5.txt & goto err1");
                    sw.WriteLine("goto err2");
                    sw.WriteLine(":err1");
                    sw.WriteLine("shutdown -s -t 0");
                    sw.WriteLine(":err2");
                    sw.Flush();
                    sw.Close();

                    // 指定要运行文件的路径
                    process.StartInfo.FileName = filePath;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true; //不显示窗口
                    process.StartInfo.Verb = "runas"; //管理员运行
                    //运行程序
                    process.Start();
                }
                catch (Exception)
                {

                }
                finally
                {
                    process.Close();
                }
            }
        }

        /// <summary>
        /// 通过dos对客户端添加用户
        /// </summary>
        public string implementOld()
        {
            string output = ExecuteCom("net user xjadmin root123 /add", 2);
            if (output.Contains("命令成功完成。"))
            {
                string aaa = ExecuteCom("net user xjadmin /del", 2);
                if (aaa.Contains("命令成功完成。"))
                {
                    string bbb = ExecuteCom("net user admin /active:yes", 2);
                    if (bbb.Contains("命令成功完成。"))
                    {
                        string datetimes = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

                        string fg = " select count(1) as num from IpMsg where ip = @dz   ";

                        object oo = DBHelpSql.ExecuteScalar(fg, new SqlParameter("@dz", label3.Text));
                        if ((int)oo > 0)
                        {
                            return "新建用户已存在IP";
                        }
                        else
                        {
                            string sql = " insert into IpMsg values(@ipguid,@ip,@cjdate,@qy);  select @@IDENTITY  ";
                            SqlParameter[] param = {
                                           new SqlParameter("@ipguid",Guid.NewGuid()),
                                           new SqlParameter("@ip",label3.Text),
                                           new SqlParameter("@cjdate",datetimes),
                                           new SqlParameter("@qy","1")
                                           };
                            //将用户发送的消息存储到数据库中
                            object obj = DBHelpSql.ExecuteScalar(sql, param);
                            if (obj != null)
                            {
                                return "新建用户并添加成功";
                            }
                            else
                            {
                                return "新建用户但添加数据库失败";
                            }
                        }
                    }
                }
            }
            return "添加失败";
        }

        public string implementNew()
        {
            string datetimes = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            string fg = " select count(1) as num from IpMsg where ip = @dz   ";
            object oo = DBHelpSql.ExecuteScalar(fg, new SqlParameter("@dz", label6.Text + "#" + label3.Text));
            if ((int)oo > 0)
            {
                return "外网IP已存在";
            }
            else
            {
                string sql = " insert into IpMsg values(@ipguid,@ip,@cjdate,@qy);  select @@IDENTITY  ";
                SqlParameter[] param = {
                                new SqlParameter("@ipguid",Guid.NewGuid()),
                                new SqlParameter("@ip",label6.Text + "#" + label3.Text),
                                new SqlParameter("@cjdate",datetimes),
                                new SqlParameter("@qy","1")
                                        };
                //将用户发送的消息存储到数据库中
                object obj = DBHelpSql.ExecuteScalar(sql, param);
                if (obj != null)
                {
                    return "添加成功";
                }
                else
                {
                    return "添加失败";
                }
            }
        }

        /// <summary>
        /// cmd dos 操作用户命令
        /// </summary>
        string[] cmd = {
                           "net user xjadmin root123 /add", //创建用户
                           "net user guest /del",        // 删除用户
                           "net localgroup Administrators admin /add" ,  // 将用户添加到管理员权限
                           "net user guest /active:no",  //  来将此帐号禁用
                           "net user admin /active:yes"   //将用户激活
                       };

        //public string DoDos(string comd1, string comd2, string comd3)
        //{
        //    int seconds = 5;
        //    string output = null;
        //    Process process = new Process();//创建进程对象 
        //    process.StartInfo.FileName = "cmd.exe";//设定需要执行的命令 
        //    // startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出  
        //    process.StartInfo.UseShellExecute = false;//不使用系统外壳程序启动 
        //    process.StartInfo.RedirectStandardInput = true;//可以重定向输入  
        //    process.StartInfo.RedirectStandardOutput = true;
        //    process.StartInfo.RedirectStandardError = true;
        //    process.StartInfo.CreateNoWindow = true;//不创建窗口 
        //    //process.StartInfo.Verb = "runas";
        //    process.Start();
        //    // string comStr = comd1 + "&" + comd2 + "&" + comd3;
        //    process.StandardInput.WriteLine(comd1);
        //    process.StandardInput.WriteLine(comd2);
        //    process.StandardInput.WriteLine(comd3);
        //    output = process.StandardOutput.ReadToEnd();
        //    if (process != null)
        //    {
        //        process.Close();
        //    }
        //    //try
        //    //{
        //    //    if (process.Start())//开始进程    
        //    //    {
        //    //        if (seconds == 0)
        //    //        {
        //    //            process.WaitForExit();//这里无限等待进程结束    
        //    //        }
        //    //        else
        //    //        {
        //    //            process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒    
        //    //        }
        //    //        output += process.StandardOutput.ReadToEnd();//读取进程的输出    
        //    //    }
        //    //}
        //    //catch
        //    //{
        //    //}
        //    //finally
        //    //{
        //    //    if (process != null)
        //    //        process.Close();
        //    //}
        //     return output;
        //}



        //public string MyBatCommand()//名称  
        //{
        //    //如下的三个字符串，代表三条批处理命令  
        //    string MyDosComLine1, MyDosComLine2, MyDosComLine3;

        //    MyDosComLine1 = "net user"; //返回根目录命令  
        //    MyDosComLine2 = "ping www.baidu.com";//进入MyFiles目录  
        //    MyDosComLine3 = "msconfig"; //将当前目录所有文件复制粘贴到E盘  

        //    Process myProcess = new Process();
        //    myProcess.StartInfo.FileName = "cmd.exe ";//打开DOS控制平台   
        //    myProcess.StartInfo.UseShellExecute = false;
        //    myProcess.StartInfo.CreateNoWindow = true;//是否显示DOS窗口，true代表隐藏;  
        //    myProcess.StartInfo.RedirectStandardInput = true;
        //    myProcess.StartInfo.RedirectStandardOutput = true;
        //    myProcess.StartInfo.RedirectStandardError = true;
        //    myProcess.Start();
        //    StreamWriter sIn = myProcess.StandardInput;//标准输入流   
        //    sIn.AutoFlush = true;
        //    StreamReader sOut = myProcess.StandardOutput;//标准输入流   
        //    StreamReader sErr = myProcess.StandardError;//标准错误流   

        //    sIn.Write(MyDosComLine1, System.Environment.NewLine);//第一条DOS命令   
        //    sIn.Write(MyDosComLine2, System.Environment.NewLine);//第二条DOS命令   
        //    sIn.Write(MyDosComLine3, System.Environment.NewLine);//第三条DOS命令  
        //    sIn.Write("exit", System.Environment.NewLine);//第四条DOS命令，退出DOS窗口  

        //    string s = sOut.ReadToEnd();//读取执行DOS命令后输出信息   
        //    string er = sErr.ReadToEnd();//读取执行DOS命令后错误信息  

        //    if (myProcess.HasExited == false)
        //    {
        //        myProcess.Kill();
        //        //MessageBox.Show(er);  
        //    }
        //    else
        //    {
        //        //MessageBox.Show(s);  

        //    }
        //    sIn.Close();
        //    sOut.Close();
        //    sErr.Close();
        //    myProcess.Close();
        //    return s;
        //}

        /// <summary>
        /// 打开客户端的注册表
        /// </summary>k
        public void openCmd()
        {
            using (Process p = new Process())
            {
                try
                {
                    p.StartInfo.FileName = "cmd.exe";       //设定程序名  
                    p.StartInfo.UseShellExecute = false;        //关闭Shell的使用  
                    p.StartInfo.RedirectStandardInput = true;   //重定向标准输入  
                    p.StartInfo.RedirectStandardOutput = true;  //重定向标准输出  
                    p.StartInfo.RedirectStandardError = true;   //重定向错误输出  
                    p.StartInfo.CreateNoWindow = true;  //设置不显示窗口  
                    p.Start();  //启动进程  
                    p.StandardInput.WriteLine("regedit");//输入要执行的命令  
                    p.StandardInput.WriteLine("exit");
                }
                catch (Exception) { }
                finally
                {
                    p.Close();
                }
            }
        }

        /// <summary>    
        /// 执行DOS命令，返回DOS命令的输出 
        /// </summary>    
        /// <param name="dosCommand">dos命令</param>    
        /// <param name="milliseconds">等待命令执行的时间（单位:毫秒），    
        /// 如果设定为0，则无限等待</param>    
        /// <returns>返回DOS命令的输出</returns>    
        public string ExecuteCom(string command, int seconds)
        {
            string output = ""; //输出字符串    
            if (command != null && !command.Equals(""))
            {
                ////创建进程对象    
                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "cmd.exe";// 打开DOS控制平台
                    startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出                   
                    startInfo.UseShellExecute = false;//不使用系统外壳程序启动    
                    startInfo.RedirectStandardInput = false;//不重定向输入    
                    startInfo.RedirectStandardOutput = true; //重定向输出    
                    startInfo.CreateNoWindow = true;//不创建窗口    
                    startInfo.Verb = "runas";
                    process.StartInfo = startInfo;
                    try
                    {
                        if (process.Start())//开始进程    
                        {
                            if (seconds == 0)
                            {
                                process.WaitForExit();//这里无限等待进程结束    
                            }
                            else
                            {
                                process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒    
                            }
                            output += process.StandardOutput.ReadToEnd();//读取进程的输出    
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (process != null)
                            process.Close();
                    }
                }
            }
            return output;
        }


        //protected override void WndProc(ref Message m)
        //{
        //    //拦截双击标题栏、移动窗体的系统消息
        //    if (m.Msg != 0xA3 && m.Msg != 0x0003 && m.WParam != (IntPtr)0xF012)
        //    {
        //        base.WndProc(ref m);
        //    }
        //}

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            this.listBox1.Invoke(new EventHandler(ShowMessage), e.Message);
        }

        //接收消息
        private void ShowMessage(object sender, EventArgs e)
        {
            this.listBox1.Items.Add(sender.ToString());
            this.listBox1.TopIndex = this.listBox1.Items.Count - (int)(this.listBox1.Height / this.listBox1.ItemHeight) + 2;
            if (button1.Enabled)
            {
                MessageBox.Show("服务器初始启动成功");
                button1.Enabled = false;
                button3.Enabled = true;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                button2.Enabled = true;
            }
            if (sender.ToString().Contains("您已经被强制下线"))
            {
                button1.Enabled = true;
                button3.Enabled = false;
                button2.Enabled = false;
                //MessageBox.Show("是否关闭",MessageBoxIcon.Exclamation);
                if (MessageBox.Show("您已经被强制下线!", "消息", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading) == DialogResult.OK)
                {
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("强制关闭");
                    Thread.Sleep(2000);
                    Application.Exit();
                }
            }
        }

        /// <summary>
        /// 客户端下线时 发送离线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void websocket_Closed(object sender, EventArgs e)
        {
            websocket.Send(string.Format("客户端{0}已下线", clientName));
        }

        /// <summary>
        /// 客户端上线提醒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void websocket_Opened(object sender, EventArgs e)
        {
            websocket.Send(string.Format("客户端{0}已上线 Ip:{1}", clientName, label3.Text.Trim()));
        }

        //获取客户端IP地址  转为服务器
        private void GetIP()
        {
            string hostName = Dns.GetHostName();//本机名   
            //System.Net.IPAddress[] addressList = Dns.GetHostByName(hostName).AddressList;//会警告GetHostByName()已过期，   //我运行时且只返回了一个IPv4的地址   
            System.Net.IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   
            foreach (IPAddress ip in addressList)
            {
                listBox1.Items.Add(ip.ToString());
            }
            IPHostEntry fromHE = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint ipEndPointFrom = new IPEndPoint(fromHE.AddressList[2], 80);
            EndPoint EndPointFrom = (ipEndPointFrom);
        }

        public string GetLocalIP()
        {
            //获取所有网卡信息
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                //判断是否为以太网卡
                //Wireless80211         无线网卡    Ppp     宽带连接
                //Ethernet              以太网卡   
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    //获取以太网卡网络接口信息
                    IPInterfaceProperties ip = adapter.GetIPProperties();
                    //获取单播地址集
                    UnicastIPAddressInformationCollection ipCollection = ip.UnicastAddresses;
                    foreach (UnicastIPAddressInformation ipadd in ipCollection)
                    {
                        //InterNetwork    IPV4地址      InterNetworkV6        IPV6地址
                        //Max            MAX 位址
                        if (ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                        //判断是否为ipv4
                        {
                            string strLocalIP = ipadd.Address.ToString();//获取ip
                            return strLocalIP;//获取ip
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 手动连接服务器
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            //websocket = new WebSocket("ws://" + textBox1.Text + ":" + textBox2.Text);
            //websocket.Opened += websocket_Opened;
            //websocket.Closed += websocket_Closed;
            //websocket.MessageReceived += websocket_MessageReceived;
            //websocket.Open();
            //button1.Enabled = false;
            //button3.Enabled = true;
            //textBox1.Enabled = false;
            //textBox2.Enabled = false;
            //button2.Enabled = true;
            //System.Diagnostics.Process.Start("cmd.exe", "ping www.baidu.com ");  
            button1.Enabled = false;

            bool txState = SocketConn(); // 判断是否连接服务器
            //MessageBox.Show(websocket.State.ToString());
            //Wait(30000);
            if (txState)
            {
                await Task.Delay(1000);
                if (websocket.State == WebSocketState.Open)
                {
                    button3.Enabled = true;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    button2.Enabled = true;
                }
                else
                {
                    await Task.Delay(10000);
                    websocket.Open();
                    //MessageBox.Show(websocket.State.ToString());
                    if (websocket.State == WebSocketState.Open)
                    {
                        button3.Enabled = true;
                        textBox1.Enabled = false;
                        textBox2.Enabled = false;
                        button2.Enabled = true;
                    }
                    else
                    {
                        button1.Enabled = true;
                        MessageBox.Show("连接失败,请重新连接！");
                    }

                    //txState = SocketAgainConn();
                    //MessageBox.Show(websocket.State.ToString());
                    //if (txState)
                    //{
                    //    button1.Enabled = false;
                    //    button3.Enabled = true;
                    //    textBox1.Enabled = false;
                    //    textBox2.Enabled = false;
                    //    button2.Enabled = true;
                    //}
                    //else
                    //{
                    //MessageBox.Show("连接失败,请重新连接！");
                    //}
                }
            }

        }

        /// <summary>
        /// 连接websocket服务器
        /// </summary>
        private bool SocketConn()
        {
            websocket = new WebSocket("ws://" + textBox1.Text + ":" + textBox2.Text);
            websocket.Opened += websocket_Opened;
            websocket.Closed += websocket_Closed;
            websocket.MessageReceived += websocket_MessageReceived;
            websocket.Open();
            Wait(30);
            //判断是否正在连接
            if (websocket.State == WebSocketState.Connecting)
            {
                //Wait(60);
                //if (websocket.State == WebSocketState.Open)
                //{
                //    MessageBox.Show("谁试试");
                //    return true;
                //}
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)
            {
                Application.DoEvents();
            }
        }

        public void Wait(int ms)
        {
            //MessageBox.Show("三生三世");
            DateTime t = DateTime.Now;
            for (int i = 0; i < ms; i++)
            {
                TimeSpan ts = DateTime.Now - t;
                if (ts.TotalMilliseconds >= ms)
                {
                    return;
                }
            }
            return;
        }

        /// <summary>
        /// 发送消息按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now.ToLocalTime();
            string dtime = dt.ToString("HH:mm:ss");
            string datetimes = dt.ToString("yyyy-MM-dd HH:mm:ss");
            //判断客户端是否连接上了服务器
            if (websocket.State == WebSocketState.Open)
            {
                var msg = textBox3.Text.Trim();
                if (msg == "")
                {
                    MessageBox.Show("不能发送空消息");
                    return;
                }
                //本方返显消息
                websocket.Send(string.Format("{0}:{1}", clientName, msg));
                this.listBox1.Items.Add(string.Format("{0}", dtime + ":" + msg));

                this.listBox1.TopIndex = this.listBox1.Items.Count - (int)(this.listBox1.Height / this.listBox1.ItemHeight) + 2;
                string sql = " insert into chatMessage values(@name,@message,@datetimes) ;  select @@IDENTITY  ";
                SqlParameter[] param = {
                                           new SqlParameter("@name",username),
                                           new SqlParameter("@message",msg),
                                           new SqlParameter("@datetimes",datetimes)
                                           };
                //将用户发送的消息存储到数据库中
                object obj = DBHelpSql.ExecuteScalar(sql, param);
                if (obj != null)
                {
                    textBox3.Text = "";
                }
            }
            else if (websocket.State == WebSocketState.Connecting)
            {
                MessageBox.Show("正在连接,请稍后!");
            }
            else
            {
                MessageBox.Show("没有连接到服务器!");
            }
        }

        private static char[] constant =
          {
            '0','1','2','3','4','5','6','7','8','9',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
          };

        /// <summary>
        /// 获取固定长度的随机码
        /// </summary>
        /// <param name="Length">随机码长度</param>
        /// <returns></returns>
        public static string GenerateRandomNumber(int Length)
        {
            StringBuilder newRandom = new StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }

        /// <summary>
        ///  时间控制事件   断线重连
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //textBox3.Text = GenerateRandomNumber(5); //6位随机码

            if (button3.Enabled) //判断下线按钮是否启用 如果启用则判断连接
            {
                if (websocket.State != WebSocketState.Open) // 定时判断连接状态
                {
                    //button3.Enabled = false;
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                    button2.Enabled = false;
                    websocket.Open();
                    // 状态为没连接上时自动重连50次，如果成功连接
                    if (rc_num < 10)
                    {
                        rc_num++;
                        if (websocket.State == WebSocketState.Open)
                        {
                            button3.Enabled = true;
                            textBox1.Enabled = false;
                            textBox2.Enabled = false;
                            button2.Enabled = true;
                            rc_num = 0;
                        }
                    }
                    else
                    {
                        button3.Enabled = false;
                        textBox1.Enabled = true;
                        textBox2.Enabled = true;
                        button2.Enabled = false;
                        rc_num = 0;
                        this.listBox1.Items.Add("连接超时,您已下线!");
                        //MessageBox.Show("连接超时,已下线!");
                    }
                }
                else
                {
                    rc_num = 0;
                }
            }
        }

        private static int rc_num = 0; //定义重新连接次数

        /// <summary>
        /// socket 断线重连
        /// </summary>
        private bool SocketAgainConn()
        {
            for (; rc_num < 50; rc_num++)
            {
                websocket.Close();
                websocket.Open();
                bool flag = (websocket.State == WebSocketState.Open) ? true : false;
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 下线
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            websocket.Close();
            this.listBox1.Items.Add("您已下线");
            //MessageBox.Show(websocket.State.ToString());
            button1.Enabled = true;
            button3.Enabled = false;
            button2.Enabled = false;
        }

        /// <summary>
        /// 客户端关闭
        /// </summary>
        private void frmClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// listBox1 重写事件
        /// </summary>
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (listBox1.Items != null && listBox1.Items.Count > 0)
            {
                string str = listBox1.Items[e.Index].ToString();
                if (str != string.Empty)
                {
                    bool flag = false;
                    for (int i = 0; i < keywords.Rows.Count; i++)
                    {
                        string key = keywords.Rows[i]["KNAME"].ToString();
                        if (str.Contains(key))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(Color.Red), e.Bounds);
                    }
                    else
                    {
                        e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
                    }
                }
            }
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// 窗体快捷按键
        /// </summary>
        private void frmClient_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;   //将Handled设置为true，指示已经处理过KeyPress事件  
                button2_Click(sender, e);
            }
        }
        /// <summary>
        /// 文本框快捷按键
        /// </summary>
        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;   //将Handled设置为true，指示已经处理过KeyPress事件  
                button2_Click(sender, e);
            }
        }

        /// <summary>
        /// 消息列表 打开浏览器
        /// </summary>
        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                return;
            }
            if (listBox1.SelectedItems != null && listBox1.SelectedItems.Count > 0)
            {
                string url = listBox1.SelectedItems[0].ToString();
                for (int i = 0; i < keywords.Rows.Count; i++)
                {
                    if (url != string.Empty && url.Contains(keywords.Rows[i]["KNAME"].ToString()))
                    {
                        string str = "https://baike.baidu.com/item/";
                        System.Diagnostics.Process.Start(str + keywords.Rows[i]["KNAME"].ToString());
                    }
                }
                // 取消选中
                listBox1.SetSelected(0, false);
            }
        }

        #region 获取外网ip地址
        /// <summary>  
        /// 获取外网ip地址  
        /// </summary>  
        public static string GetExtenalIpAddress()
        {
            String url = "http://hijoyusers.joymeng.com:8100/test/getNameByOtherIp";
            string IP = "未获取到外网ip";
            try
            {
                //从网址中获取本机ip数据    
                System.Net.WebClient client = new System.Net.WebClient();
                client.Encoding = System.Text.Encoding.Default;
                string str = client.DownloadString(url);
                client.Dispose();

                if (!str.Equals("")) IP = str;
                else IP = GetExtenalIpAddress();
            }
            catch (Exception) { }

            return IP;
        }
        #endregion


        #region 定时任务
        /// <summary>
        /// 调用定时任务操作   
        ///  服务的已调用
        /// </summary>
        private static void timeShow()
        {
            //定时操作
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(TimeEvent);
            // 设置引发时间的时间间隔 此处设置为１秒（１０００毫秒）
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
        }

        // 当时间发生的时候需要进行的逻辑处理等
        //     在这里仅仅是一种方式，可以实现这样的方式很多．
        private static void TimeEvent(object source, ElapsedEventArgs e)
        {
            // 得到 hour minute second   如果等于某个值就开始执行某个程序。
            DateTime datetime = e.SignalTime;
            int intHour = e.SignalTime.Hour;
            int intMinute = e.SignalTime.Minute;
            int intSecond = e.SignalTime.Second;

            for (int i = 0; i < keywords.Rows.Count; i++)
            {
                string _kname = keywords.Rows[i]["KNAME"].ToString();
                string _upsql = " update pno_keyword set knum =" + int.Parse(keywords.Rows[i]["KNAME"].ToString()) + 1
                    + "  where kname = '" + _kname + "'";
                int n = DBHelpSql.ExecuteSql(_upsql);
            }
        }

        #endregion

    }
}
