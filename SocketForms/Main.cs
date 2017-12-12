using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketUtil;
using SocketModel;
using System.Data.SqlClient;
using System.Configuration;
using SuperWebSocket;
using SuperSocket.SocketBase.Config;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace SocketForms
{
    public partial class Main : Form
    {
        WebSocketServer server = new WebSocketServer();
        ServerConfig serverConfig = new ServerConfig
        {
            ClearIdleSession = true,//是否清除空闲会话
            IdleSessionTimeOut = 36000,//会话超时时间
            MaxConnectionNumber = 10000,//最大允许的客户端连接数目
            ClearIdleSessionInterval = 3600//清除空闲会话的时间间隔
        };

        #region 初始化本地内存值
        private static DataTable DNusers = new DataTable(); //用户集合
        private static DataTable DNusersmsg = new DataTable();//信息集合
        private static DataTable keywords = new DataTable();//关键词集合
        private static System.Timers.Timer aTimer = new System.Timers.Timer();//定时器初始
        private static Dictionary<string, List<string>> msgDictionary = new Dictionary<string, List<string>>();//消息历史
        #endregion

        public static void initKeyWords()
        {
            try
            {
                string sqlCmd = " select * from pno_keyword ";
                keywords = DBHelpSql.Query(sqlCmd).Tables[0];
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 程序主入口
        /// </summary>
        public Main()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 主窗体加载
        /// </summary>
        private void Main_Load(object sender, EventArgs e)
        {
            //定时操作
            aTimer.Elapsed += new ElapsedEventHandler(TimeEvent);
            // 设置引发时间的时间间隔 此处设置为１秒（１０００毫秒）
            aTimer.Interval = 300000; // 5分钟
            aTimer.Enabled = true;

            initKeyWords();

            string datetimes = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

            Form.CheckForIllegalCrossThreadCalls = false;

            //MessageBox.Show(cz.User.Uguid);
            //定义存储字段
            DNusers.Columns.Add("id", Type.GetType("System.String"));
            DNusers.Columns.Add("name", Type.GetType("System.String"));
            DNusers.Columns.Add("zt", Type.GetType("System.String"));
            DNusersmsg.Columns.Add("name", Type.GetType("System.String"));
            DNusersmsg.Columns.Add("msg", Type.GetType("System.String"));

            var ip = ConfigurationManager.AppSettings["APWebSocketIP"];
            //var ip = "121.40.118.220";
            var port = ConfigurationManager.AppSettings["APWebSocketPort"];

            label2.Text = "IP地址:" + ip + ":" + port;

            if (!server.Setup(ip, int.Parse(port)))
            {
                //处理启动失败消息
                //MessageBox.Show("开启服务器失败");
                this.listBox1.Items.Add(string.Format("{0}", "开启服务器失败:" + datetimes));
                return;
            }
            else
            {
                //新的会话连接时
                server.NewSessionConnected += server_NewSessionConnected;
                //会话关闭
                server.SessionClosed += server_SessionClosed;
                //接收到新的消息时
                server.NewMessageReceived += server_NewMessageReceived;

                if (!server.Start())
                {
                    //处理监听失败消息                    
                    //MessageBox.Show("已经启动");
                    this.listBox1.Items.Add(string.Format("{0}", "服务已经启动:" + datetimes));
                }
                else
                {
                    //MessageBox.Show("启动成功");
                    this.listBox1.Items.Add(string.Format("{0}", "服务启动成功:" + datetimes));
                }

            }

        }

        /// <summary>
        /// 显示当前时间
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString();
        }

        /// <summary>
        /// 程序关闭
        /// </summary>
        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            aTimer.Elapsed += new ElapsedEventHandler(TimeEvent);
            Application.Exit();
        }

        /// <summary>
        /// 新的会话链接
        /// </summary>
        /// <param name="session"></param>
        private void server_NewSessionConnected(WebSocketSession session)
        {
            if (msgDictionary.ContainsKey(session.SessionID))
                msgDictionary[session.SessionID].ForEach(item => session.Send(item));
        }

        //定义当前推送消息用户SessionID
        //string KSessionId;
        string VSessionId;
        private void server_NewMessageReceived(WebSocketSession session, string value)
        {
            DateTime dtimes = DateTime.Now.ToLocalTime();
            string datetimes = dtimes.ToString("yyyy-MM-dd HH:mm:ss");
            string dtime = dtimes.ToString("HH:mm:ss");
            if (value != "")
            {
                //解析传入数据
                string bb = "";
                VSessionId = session.SessionID;//给当前手机链接用户反显用（用户提取websocket用户（含状态），第一次链接返回，发送消息返回）
                string aa = "客户端";
                Regex rr = new Regex(aa);
                Match mm = rr.Match(value);
                if (mm.Success)
                {
                    bb = value.Substring(3, value.IndexOf("已") - 3);
                    //其他客户端显示信息
                    for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                    {
                        if (!String.IsNullOrEmpty(DNusers.Rows[ii]["id"].ToString()) && DNusers.Rows[ii]["zt"].ToString() == "在线"
                            && DNusers.Rows[ii]["name"].ToString() != bb)
                            SendMsgToRemotePoint(DNusers.Rows[ii]["id"].ToString(), string.Format("{0}", bb + "已链接，可以与其通讯！" + datetimes));
                    }

                    //本方反显信息
                    if (!String.IsNullOrEmpty(VSessionId))
                    {
                        SendMsgToRemotePoint(VSessionId, string.Format("{0}", /*bb +*/ "已链接成功！" + datetimes));
                        this.listBox1.Items.Add(string.Format("{0}", bb + "已链接成功！" +
                            datetimes.Substring(datetimes.IndexOf(" ") + 1, datetimes.Length - (datetimes.IndexOf(" ") + 1))));
                    }
                    AddDNusersIdNaame(VSessionId, bb);
                }
                else
                {
                    //客户端发来信息
                    if (value.Contains(":"))
                    {
                        int num = value.IndexOf(":");
                        //获取发送人
                        string sendple = value.Substring(0, num);
                        //获取发送的信息
                        string msg = value.Substring(num + 1, value.Length - (num + 1));
                        //在线不在线统计字段
                        int zxnum = 0;
                        int bzxnum = 0;
                        //对方显示信息
                        for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                        {
                            if (!String.IsNullOrEmpty(DNusers.Rows[ii]["id"].ToString()) && DNusers.Rows[ii]["name"].ToString() != sendple)
                            {
                                if (DNusers.Rows[ii]["zt"].ToString() == "在线")//给在线用户发送信息
                                {
                                    zxnum++;
                                    SendMsgToRemotePoint(DNusers.Rows[ii]["id"].ToString(), string.Format("{0}", sendple + ":" + dtime + ":" + msg));
                                }
                                else
                                {
                                    bzxnum++;
                                    //不在线用户记录
                                    DNusersmsg.Rows.Add();
                                    DNusersmsg.Rows[DNusersmsg.Rows.Count - 1]["name"] = DNusers.Rows[ii]["name"];
                                    DNusersmsg.Rows[DNusersmsg.Rows.Count - 1]["msg"] = sendple + ":" + dtime + ":" + msg;
                                }
                            }
                        }
                        for (int i = 0; i < keywords.Rows.Count; i++)
                        {
                            string str = keywords.Rows[i]["KNAME"].ToString();
                            if (msg.Contains(str))
                            {
                                keywords.Rows[i]["KNUM"] = (int)(int.Parse(keywords.Rows[i]["KNUM"].ToString()) + 1);
                            }
                        }
                        this.listBox1.Items.Add(string.Format("{0}", sendple + ":" + datetimes + ":" + msg));
                    }
                    else
                    {
                        MessageBox.Show("没有信息");
                    }
                }
                //listBox滚动条一直显示最后一条
                this.listBox1.TopIndex = this.listBox1.Items.Count - (int)(this.listBox1.Height / this.listBox1.ItemHeight) + 2;
            }
            else
            {
                return;
                //页面未链接
                //AddMsgToSessionId(VSessionId);
            }
        }

        /// <summary>
        /// 会话关闭
        /// </summary>
        /// <param name="session"></param>
        /// <param name="value"></param>
        private void server_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            string datetimes = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            //删除电脑用户
            for (int ii = DNusers.Rows.Count - 1; ii >= 0; ii--)
            {
                if (!String.IsNullOrEmpty(DNusers.Rows[ii]["id"].ToString()) && session.SessionID == DNusers.Rows[ii]["id"].ToString())
                {
                    DNusers.Rows[ii]["zt"] = "离线";
                    //DNusers.Rows.RemoveAt(ii);
                    this.listBox1.Items.Add(string.Format("{0}", DNusers.Rows[ii]["name"].ToString() + "已离线 " + datetimes));
                    //listBox滚动条一直显示最后一条
                    this.listBox1.TopIndex = this.listBox1.Items.Count - (int)(this.listBox1.Height / this.listBox1.ItemHeight) + 2;
                }
            }

        }

        /// <summary>
        /// 添加会话消息
        /// </summary>
        /// <param name="value"></param>
        private void AddDNusersIdNaame(string id, string name)
        {
            if (DNusers.Rows.Count > 0)
            {
                string flag = "0";
                int aaa = 0;
                for (int i = 0; i < DNusers.Rows.Count; i++)
                {
                    if (DNusers.Rows[i]["name"].ToString() == name && DNusers.Rows[i]["id"].ToString() != id)
                    {
                        flag = "1";
                        aaa = i;
                    }
                    if (DNusers.Rows[i]["name"].ToString() == name && DNusers.Rows[i]["id"].ToString() == id)
                    {
                        flag = "2";
                    }
                }

                if (flag == "1")
                {
                    if (DNusers.Rows[aaa]["zt"].ToString() == "在线")
                    {
                        //提取旧ID，并给他发送下线消息
                        string oldid = DNusers.Rows[aaa]["id"].ToString();
                        SendMsgToRemotePoint(oldid, string.Format("{0}", "您的账号在另外客户端登录，您已经被强制下线！"));
                        //替换掉旧id
                        DNusers.Rows[aaa]["id"] = id;
                    }
                    else if (DNusers.Rows[aaa]["zt"].ToString() == "离线")
                    {
                        //替换掉旧id、更新状态
                        DNusers.Rows[aaa]["id"] = id;
                        DNusers.Rows[aaa]["zt"] = "在线";
                        //检查是否有未接收的消息，有则发送
                        for (int ii = DNusersmsg.Rows.Count - 1; ii >= 0; ii--)//涉及删除行，循环要用倒序
                        {
                            if (DNusersmsg.Rows[ii]["name"].ToString() == DNusers.Rows[aaa]["name"].ToString())
                            {
                                SendMsgToRemotePoint(id, string.Format("{0}", DNusersmsg.Rows[ii]["msg"]));
                                DNusersmsg.Rows.RemoveAt(ii);
                            }
                        }
                    }
                }
                else if (flag == "2")
                {
                    if (DNusers.Rows[aaa]["zt"].ToString() == "离线")
                    {
                        DNusers.Rows[aaa]["zt"] = "在线";
                        //检查是否有未接收的消息，有则发送
                        for (int ii = DNusersmsg.Rows.Count - 1; ii >= 0; ii--)//涉及删除行，循环要用倒序
                        {
                            if (DNusersmsg.Rows[ii]["name"].ToString() == DNusers.Rows[aaa]["name"].ToString())
                            {
                                SendMsgToRemotePoint(id, string.Format("{0}", DNusersmsg.Rows[ii]["msg"]));
                                DNusersmsg.Rows.RemoveAt(ii);
                            }
                        }
                    }
                }
                else
                {
                    DNusers.Rows.Add();
                    DNusers.Rows[DNusers.Rows.Count - 1]["id"] = id;
                    DNusers.Rows[DNusers.Rows.Count - 1]["name"] = name;
                    DNusers.Rows[DNusers.Rows.Count - 1]["zt"] = "在线";
                }
            }
            else
            {
                DNusers.Rows.Add();
                DNusers.Rows[0]["id"] = id;
                DNusers.Rows[0]["name"] = name;
                DNusers.Rows[0]["zt"] = "在线";
            }
        }

        /// <summary>
        /// 发送消息到
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="msg"></param>
        private void SendMsgToRemotePoint(string sessionId, string msg)
        {
            var allSession = server.GetAppSessionByID(sessionId);
            if (allSession != null)
                allSession.Send(msg);
        }

        /// <summary>
        /// 添加会话消息
        /// </summary>
        /// <param name="value"></param>
        private void AddMsgToSessionId(string value)
        {
            if (value != null)
            {
                //消息列表包含页面会话ID
                if (msgDictionary.ContainsKey(value))
                {
                    msgDictionary[value].Add(value);
                }
                //消息列表不包含页面会话ID
                else
                    msgDictionary.Add(value, new List<string>() { value });
            }
        }

        /// <summary>
        /// 结束websocket
        /// </summary>
        /// <returns>结束websocket</returns>
        public void endwebsocket()
        {
            server.Stop();
        }

        /// <summary>
        /// 修改app.config的内容
        /// </summary>
        /// <param name="AppKey">要修改的Key</param>
        /// <param name="AppValue">要修改的值</param>
        public static void SetValue(string AppKey, string AppValue)
        {

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);//获取Configuration对象
            config.AppSettings.Settings["name"].Value = "value";//key的值取textbox.text
            config.Save(ConfigurationSaveMode.Modified);//保存
            ConfigurationManager.RefreshSection("appSettings");//刷新

            //读取为xml的方法
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            xDoc.Load(System.Windows.Forms.Application.ExecutablePath + ".config");

            System.Xml.XmlNode xNode;
            System.Xml.XmlElement xElem1;
            System.Xml.XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");

            xElem1 = (System.Xml.XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            if (xElem1 != null) xElem1.SetAttribute("value", AppValue);
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", AppKey);
                xElem2.SetAttribute("value", AppValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(System.Windows.Forms.Application.ExecutablePath + ".config");

        }



        #region 定时任务

        // 当时间发生的时候需要进行的逻辑处理等
        //     在这里仅仅是一种方式，可以实现这样的方式很多．
        private static void TimeEvent(object source, ElapsedEventArgs e)
        {
            // 得到 hour minute second   如果等于某个值就开始执行某个程序。
            DateTime datetime = e.SignalTime;
            int intHour = e.SignalTime.Hour;
            int intMinute = e.SignalTime.Minute;
            int intSecond = e.SignalTime.Second;
            try
            {
                for (int i = 0; i < keywords.Rows.Count; i++)
                {
                    //keywords.Rows[i]["KNUM"] = int.Parse(keywords.Rows[i]["KNAME"].ToString()) + 1;
                    string _kname = keywords.Rows[i]["KNAME"].ToString();
                    string _upsql = " update pno_keyword set knum =" + int.Parse(keywords.Rows[i]["KNUM"].ToString())
                        + "  where kname = '" + _kname + "'";
                    int n = DBHelpSql.ExecuteSql(_upsql);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("更新字典库出现错误");

                // throw;
            }
        }

        #endregion



    }
}
