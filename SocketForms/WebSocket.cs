using SuperSocket.SocketBase.Config;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;

namespace SocketForms
{
    public class WebSocket
    {

        //----------------------------------------------------websocket 通讯 begin-------------------------------------------------------------//

        DataTable users = new DataTable();
        DataTable DNusers = new DataTable();
        DataTable usersmsg = new DataTable();
        DataTable DNusersmsg = new DataTable();
        WebSocketServer server;
        public string OnStart()
        {
            //定义存储字段
            users.Columns.Add("id", Type.GetType("System.String"));
            users.Columns.Add("name", Type.GetType("System.String"));
            users.Columns.Add("zt", Type.GetType("System.String"));
            DNusers.Columns.Add("id", Type.GetType("System.String"));
            DNusers.Columns.Add("name", Type.GetType("System.String"));
            DNusers.Columns.Add("zt", Type.GetType("System.String"));
            usersmsg.Columns.Add("name", Type.GetType("System.String"));
            usersmsg.Columns.Add("msg", Type.GetType("System.String"));
            DNusersmsg.Columns.Add("name", Type.GetType("System.String"));
            DNusersmsg.Columns.Add("msg", Type.GetType("System.String"));
            //string strPath = System.Configuration.ConfigurationSettings.AppSettings["ImgPath"].ToString();
            var ip = ConfigurationManager.AppSettings["APWebSocketIP"];
            var port = ConfigurationManager.AppSettings["APWebSocketPort"];
            //WebSocket服务器端启动
            server = new WebSocketServer();
            ServerConfig serverConfig = new ServerConfig
            {
                ClearIdleSession = true,//是否清除空闲会话
                IdleSessionTimeOut = 36000,//会话超时时间
                MaxConnectionNumber = 10000,//最大允许的客户端连接数目
                ClearIdleSessionInterval = 3600//清除空闲会话的时间间隔
            };
            if (!server.Setup(ip, int.Parse(port)))
            {
                //处理启动失败消息
                return "0";
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
                    return "2";//已经启动
                }
                else
                {
                    ////调用查询数据方法  添加用户到集合中    状态为不在线
                    //string app = "";
                    //DataSet uall = Pro_selemp(app);
                    //if (uall.Tables.Count > 0)
                    //{
                    //    for (int i = 0; i < uall.Tables[0].Rows.Count; i++)
                    //    {
                    //        if (uall.Tables[0].Rows[i]["app"].ToString().Trim() == "电脑")
                    //        {
                    //            string id = Guid.NewGuid().ToString();
                    //            string name = uall.Tables[0].Rows[i]["name"] + "_" + uall.Tables[0].Rows[i]["age"];
                    //            string zt = "离线";
                    //            DNusers.Rows.Add(id, name, zt);
                    //        }
                    //        else if (uall.Tables[0].Rows[i]["app"].ToString().Trim() == "手机")
                    //        {                                
                    //            string id = Guid.NewGuid().ToString();
                    //            string name = uall.Tables[0].Rows[i]["name"] + "_" + uall.Tables[0].Rows[i]["age"];
                    //            string zt = "离线";
                    //            users.Rows.Add(id,name,zt);
                    //        }
                    //    }                        
                    //}
                    return "1";//启动成功 
                }

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




        //定义当前推送消息用户SessionID
        string KSessionId;
        string VSessionId;
        Dictionary<string, List<string>> msgDictionary = new Dictionary<string, List<string>>();
        private void server_NewMessageReceived(WebSocketSession session, string value)
        {
            string datetimes = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            if (value.StartsWith("K")) //（手机）发来消息
            {
                //解析传入数据
                string bb = "";
                string a = "K:手机:";
                Regex r = new Regex(a);
                Match m = r.Match(value);
                KSessionId = session.SessionID;//给当前手机链接用户反显用（用户提取websocket用户（含状态），第一次链接返回，发送消息返回）
                string[] c = value.Split('#');

                string aa = "K:手机:已链接:";
                Regex rr = new Regex(aa);
                Match mm = rr.Match(value);
                if (mm.Success)
                {
                    bb = c[0].Replace("K:手机:已链接:", "");
                    AddusersIdNaame(session.SessionID, bb);
                }
                else
                {
                    bb = c[0].Replace("K:手机:", "");
                }

                //判断处理对方登录信息,本方登录显示
                if (mm.Success)
                {
                    //对方显示信息（给在线用户发消息）
                    for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                    {
                        if (!String.IsNullOrEmpty(DNusers.Rows[ii]["id"].ToString()) && DNusers.Rows[ii]["zt"].ToString() == "在线")
                            SendMsgToRemotePoint(DNusers.Rows[ii]["id"].ToString(), string.Format("{0}", bb + "已链接，可以与其通讯！     " + datetimes));
                    }
                    //本方反显信息
                    if (!String.IsNullOrEmpty(KSessionId))
                        SendMsgToRemotePoint(KSessionId, string.Format("{0}", bb + "已链接成功！       " + datetimes));
                }
                else if (value == "K:手机:提取电脑在线用户")//给本方返回对方在线用户
                {
                    //本方反显对方在线用户信息
                    string ulist = "";
                    for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                    {
                        ulist = ulist + DNusers.Rows[ii]["name"] + "*" + DNusers.Rows[ii]["zt"] + "#";
                    }
                    if (!String.IsNullOrEmpty(KSessionId))
                        SendMsgToRemotePoint(KSessionId, string.Format("{0}", "获取成功:" + ulist));
                }
                else if (value == "K:手机:提取手机在线用户")//给本方返回对方在线用户
                {
                    //本方反显对方在线用户信息
                    string ulist = "";
                    for (int ii = 0; ii < users.Rows.Count; ii++)
                    {
                        ulist = ulist + users.Rows[ii]["name"] + "*" + users.Rows[ii]["zt"] + "#";
                    }
                    if (!String.IsNullOrEmpty(KSessionId))
                        SendMsgToRemotePoint(KSessionId, string.Format("{0}", "获取成功:" + ulist));
                }
                else if (value == "K:手机:提取所有用户")//给本方返回对方在线用户
                {
                    //本方反显对方在线用户信息
                    string ulist = "";
                    for (int ii = 0; ii < users.Rows.Count; ii++)
                    {
                        ulist = ulist + users.Rows[ii]["name"] + "*" + users.Rows[ii]["zt"] + "#";
                    }
                    for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                    {
                        ulist = ulist + DNusers.Rows[ii]["name"] + "*" + DNusers.Rows[ii]["zt"] + "#";
                    }
                    if (!String.IsNullOrEmpty(KSessionId))
                        SendMsgToRemotePoint(KSessionId, string.Format("{0}", "获取成功:" + ulist));
                }
                else if (value == "K:手机:提取所有在线用户")//给本方返回对方在线用户
                {
                    //本方反显对方在线用户信息
                    string ulist = "";
                    for (int ii = 0; ii < users.Rows.Count; ii++)
                    {
                        if (users.Rows[ii]["zt"].ToString() == "在线")
                        {
                            ulist = ulist + users.Rows[ii]["name"] + "*" + users.Rows[ii]["zt"] + "#";
                        }
                    }
                    for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                    {
                        if (users.Rows[ii]["zt"].ToString() == "在线")
                        {
                            ulist = ulist + DNusers.Rows[ii]["name"] + "*" + DNusers.Rows[ii]["zt"] + "#";
                        }
                    }
                    if (!String.IsNullOrEmpty(KSessionId))
                        SendMsgToRemotePoint(KSessionId, string.Format("{0}", "获取成功:" + ulist));
                }
                else if (value.Contains("K:手机:更新用户"))//给本方返回对方在线用户
                {
                    //本方反显对方在线用户信息
                    string ulist = "";
                    string mb = c[1];//取接收人集合
                    string[] mbd = mb.Split('*');

                    //接收未添加时的用户数量
                    //int userNum = users.Rows.Count;
                    //判断接收了几个用户
                    for (int i = 0; i < mbd.Length - 1; i++)
                    {
                        //接收用户进行比较
                        string nama = mbd[i];
                        bool falg = true;
                        bool falgDn = true;
                        //循环比较两者间是否存在   
                        foreach (DataRow row in users.Rows)
                        {
                            if (nama == row["name"].ToString())
                            {
                                falg = false;
                                break;
                            }
                        }
                        foreach (DataRow row in DNusers.Rows)
                        {
                            if (nama == row["name"].ToString())
                            {
                                falgDn = false;
                                break;
                            }
                        }
                        if (falg)
                        {
                            users.Rows.Add("", nama, "离线");
                        }
                        if (falgDn)
                        {
                            DNusers.Rows.Add("", nama, "离线");
                        }
                    }
                    //返回当前信息到本
                    ulist += "手机:";
                    for (int ii = 0; ii < users.Rows.Count; ii++)
                    {
                        ulist = ulist + users.Rows[ii]["name"] + "*" + users.Rows[ii]["zt"] + "#";
                    }
                    ulist += "@电脑:";
                    for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                    {
                        ulist = ulist + DNusers.Rows[ii]["name"] + "*" + DNusers.Rows[ii]["zt"] + "#";
                    }
                    if (!String.IsNullOrEmpty(KSessionId))
                        SendMsgToRemotePoint(KSessionId, string.Format("{0}", "获取成功:" + ulist));
                }
                else if (m.Success)//判断处理对方发送信息
                {
                    //在线不在线统计字段
                    int zxnum = 0;
                    int bzxnum = 0;
                    //对方显示信息
                    string mb = c[1];//取接收人集合
                    if (mb == "")//目标人为空时，发送对方全部
                    {
                        for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                        {
                            if (!String.IsNullOrEmpty(DNusers.Rows[ii]["id"].ToString()))
                            {
                                if (DNusers.Rows[ii]["zt"].ToString() == "在线")//给在线用户发送信息
                                {
                                    zxnum++;
                                    SendMsgToRemotePoint(DNusers.Rows[ii]["id"].ToString(), string.Format("{0}", bb + "#" + c[2] + "#" + datetimes));
                                }
                                else
                                {
                                    bzxnum++;
                                    //不在线用户记录（发全部用户不用）
                                }
                            }
                        }
                    }
                    //解析发送目标所属端
                    string[] mbd = mb.Split('@');
                    for (int nnn = 0; nnn < mbd.Length - 1; nnn++)
                    {
                        //解析接收人集合
                        string[] jihe = mbd[nnn].Split('&');
                        string[] cc = jihe[1].Split('*');
                        if (jihe[0] == "手机")
                        {
                            for (int i = 0; i < cc.Length - 1; i++)
                            {
                                for (int iii = 0; iii < users.Rows.Count; iii++)//给手机发送消息
                                {
                                    string dddd = users.Rows[iii]["name"].ToString();
                                    if (!String.IsNullOrEmpty(users.Rows[iii]["id"].ToString()) && cc[i] == users.Rows[iii]["name"].ToString())
                                    {
                                        if (users.Rows[iii]["zt"].ToString() == "在线")//给在线用户发送信息
                                        {
                                            zxnum++;
                                            SendMsgToRemotePoint(users.Rows[iii]["id"].ToString(), string.Format("{0}", bb + "#" + c[2] + "#" + datetimes));
                                        }
                                        else
                                        {
                                            bzxnum++;
                                            //不在线用户记录
                                            usersmsg.Rows.Add();
                                            usersmsg.Rows[usersmsg.Rows.Count - 1]["name"] = users.Rows[iii]["name"];
                                            usersmsg.Rows[usersmsg.Rows.Count - 1]["msg"] = bb + "#" + c[2] + "#" + datetimes;
                                        }
                                    }
                                }
                            }
                        }
                        else if (jihe[0] == "电脑")
                        {
                            for (int i = 0; i < cc.Length - 1; i++)
                            {
                                for (int ii = 0; ii < DNusers.Rows.Count; ii++)//给电脑发送消息
                                {
                                    if (!String.IsNullOrEmpty(DNusers.Rows[ii]["id"].ToString()) && cc[i] == DNusers.Rows[ii]["name"].ToString())
                                    {
                                        if (DNusers.Rows[ii]["zt"].ToString() == "在线")//给在线用户发送信息
                                        {
                                            zxnum++;
                                            SendMsgToRemotePoint(DNusers.Rows[ii]["id"].ToString(), string.Format("{0}", bb + "#" + c[2] + "#" + datetimes));
                                        }
                                        else
                                        {
                                            bzxnum++;
                                            //不在线用户记录
                                            DNusersmsg.Rows.Add();
                                            DNusersmsg.Rows[DNusersmsg.Rows.Count - 1]["name"] = DNusersmsg.Rows[ii]["name"];
                                            DNusersmsg.Rows[DNusersmsg.Rows.Count - 1]["msg"] = bb + "#" + c[2] + "#" + datetimes;
                                        }
                                    }
                                }
                            }
                        }

                    }



                    //本方反显信息
                    if (!String.IsNullOrEmpty(KSessionId))
                        SendMsgToRemotePoint(KSessionId, string.Format("{0}", bb + "#" + "在线发送:" + zxnum.ToString() + "离线发送:" + bzxnum.ToString() + "#" + datetimes));
                }
                //页面未链接
                else
                {

                    AddMsgToSessionId(VSessionId);
                }
            }
            else if (value.StartsWith("S"))//（电脑）发来消息
            {
                string a = "S:电脑:";
                string bb = "";
                Regex r = new Regex(a);
                Match m = r.Match(value);
                VSessionId = session.SessionID;
                string[] c = value.Split('#');

                string aa = "S:电脑:已链接:";
                Regex rr = new Regex(aa);
                Match mm = rr.Match(value);
                if (mm.Success)
                {
                    bb = c[0].Replace("S:电脑:已链接:", "");
                    AddDNusersIdNaame(session.SessionID, bb);
                }
                else
                {
                    bb = c[0].Replace("S:电脑:", "");
                }


                //判断处理对方登录信息,本方登录显示

                if (mm.Success)
                {
                    //对方显示信息
                    for (int ii = 0; ii < users.Rows.Count; ii++)
                    {
                        if (!String.IsNullOrEmpty(users.Rows[ii]["id"].ToString()) && users.Rows[ii]["zt"].ToString() == "在线")
                            SendMsgToRemotePoint(users.Rows[ii]["id"].ToString(), string.Format("{0}", bb + "已链接，可以与其通讯！     " + datetimes));
                    }

                    //本方反显信息
                    if (!String.IsNullOrEmpty(VSessionId))
                        SendMsgToRemotePoint(VSessionId, string.Format("{0}", bb + "已链接成功！      " + datetimes));
                }
                else if (value == "S:电脑:提取电脑在线用户")//给本方返回对方在线用户
                {
                    //本方反显对方在线用户信息
                    string ulist = "";
                    for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                    {
                        ulist = ulist + DNusers.Rows[ii]["name"] + "*" + DNusers.Rows[ii]["zt"] + "#";
                    }


                    if (!String.IsNullOrEmpty(VSessionId))
                        SendMsgToRemotePoint(VSessionId, string.Format("{0}", "获取成功:" + ulist));
                }
                else if (value == "S:电脑:提取手机在线用户")//给本方返回对方在线用户
                {
                    //本方反显对方在线用户信息
                    string ulist = "";
                    for (int ii = 0; ii < users.Rows.Count; ii++)
                    {
                        ulist = ulist + users.Rows[ii]["name"] + "*" + users.Rows[ii]["zt"] + "#";
                    }
                    if (!String.IsNullOrEmpty(VSessionId))
                        SendMsgToRemotePoint(VSessionId, string.Format("{0}", "获取成功:" + ulist));
                }
                else if (value.Contains("S:电脑:更新用户"))//给本方返回对方在线用户
                {
                    //本方反显对方在线用户信息
                    string ulist = "";
                    string mb = c[1];//取接收人集合
                    string[] mbd = mb.Split('*');

                    //接收未添加时的用户数量
                    //int userNum = users.Rows.Count;
                    //int DNuserNum = DNusers.Rows.Count;
                    //判断接收了几个用户
                    for (int i = 0; i < mbd.Length - 1; i++)
                    {
                        //接收用户进行比较
                        string nama = mbd[i];
                        bool falg = true;
                        bool falgDn = true;
                        //循环比较两者间是否存在   
                        foreach (DataRow row in users.Rows)
                        {
                            if (nama == row["name"].ToString())
                            {
                                falg = false;
                                break;
                            }
                        }
                        foreach (DataRow row in DNusers.Rows)
                        {
                            if (nama == row["name"].ToString())
                            {
                                falgDn = false;
                                break;
                            }
                        }
                        if (falg)
                        {
                            users.Rows.Add("", nama, "离线");
                        }
                        if (falgDn)
                        {
                            DNusers.Rows.Add("", nama, "离线");
                        }
                    }
                    //返回当前信息到本
                    ulist += "手机:";
                    for (int ii = 0; ii < users.Rows.Count; ii++)
                    {
                        ulist = ulist + users.Rows[ii]["name"] + "*" + users.Rows[ii]["zt"] + "#";
                    }
                    ulist += "@电脑:";
                    for (int ii = 0; ii < DNusers.Rows.Count; ii++)
                    {
                        ulist = ulist + DNusers.Rows[ii]["name"] + "*" + DNusers.Rows[ii]["zt"] + "#";
                    }
                    if (!String.IsNullOrEmpty(VSessionId))
                        SendMsgToRemotePoint(VSessionId, string.Format("{0}", "获取成功:" + ulist));
                }
                else if (m.Success)//判断处理对方发送信息
                {
                    //在线不在线统计字段
                    int zxnum = 0;
                    int bzxnum = 0;

                    //对方显示信息
                    string mb = c[1];//取接收人集合
                    if (mb == "")//目标人为空时，发送对方全部
                    {
                        for (int ii = 0; ii < users.Rows.Count; ii++)
                        {
                            if (!String.IsNullOrEmpty(users.Rows[ii]["id"].ToString()))
                            {
                                if (users.Rows[ii]["zt"].ToString() == "在线")//给在线用户发送信息
                                {
                                    zxnum++;
                                    SendMsgToRemotePoint(users.Rows[ii]["id"].ToString(), string.Format("{0}", bb + "#" + c[2] + "#" + datetimes));
                                }
                                else
                                {
                                    bzxnum++;
                                    //不在线用户记录（发全部用户不用）
                                }
                            }
                        }
                    }
                    //解析发送目标所属端
                    string[] mbd = mb.Split('@');
                    for (int nnn = 0; nnn < mbd.Length - 1; nnn++)
                    {
                        //解析接收人集合
                        string[] jihe = mbd[nnn].Split('&');
                        string[] cc = jihe[1].Split('*');
                        if (jihe[0] == "手机")
                        {
                            for (int i = 0; i < cc.Length - 1; i++)
                            {
                                for (int iii = 0; iii < users.Rows.Count; iii++)//给手机发送消息
                                {
                                    string dddd = users.Rows[iii]["name"].ToString();
                                    if (!String.IsNullOrEmpty(users.Rows[iii]["id"].ToString()) && cc[i] == users.Rows[iii]["name"].ToString())
                                    {
                                        if (users.Rows[iii]["zt"].ToString() == "在线")//给在线用户发送信息
                                        {
                                            zxnum++;
                                            SendMsgToRemotePoint(users.Rows[iii]["id"].ToString(), string.Format("{0}", bb + "#" + c[2] + "#" + datetimes));
                                        }
                                        else
                                        {
                                            bzxnum++;
                                            //不在线用户记录
                                            usersmsg.Rows.Add();
                                            usersmsg.Rows[usersmsg.Rows.Count - 1]["name"] = users.Rows[iii]["name"];
                                            usersmsg.Rows[usersmsg.Rows.Count - 1]["msg"] = bb + "#" + c[2] + "#" + datetimes;
                                        }
                                    }
                                }
                            }
                        }
                        else if (jihe[0] == "电脑")
                        {
                            for (int i = 0; i < cc.Length - 1; i++)
                            {
                                for (int ii = 0; ii < DNusers.Rows.Count; ii++)//给电脑发送消息
                                {
                                    if (!String.IsNullOrEmpty(DNusers.Rows[ii]["id"].ToString()) && cc[i] == DNusers.Rows[ii]["name"].ToString())
                                    {
                                        if (DNusers.Rows[ii]["zt"].ToString() == "在线")//给在线用户发送信息
                                        {
                                            zxnum++;
                                            SendMsgToRemotePoint(DNusers.Rows[ii]["id"].ToString(), string.Format("{0}", bb + "#" + c[2] + "#" + datetimes));
                                        }
                                        else
                                        {
                                            bzxnum++;
                                            //不在线用户记录
                                            DNusersmsg.Rows.Add();
                                            DNusersmsg.Rows[DNusersmsg.Rows.Count - 1]["name"] = DNusersmsg.Rows[ii]["name"];
                                            DNusersmsg.Rows[DNusersmsg.Rows.Count - 1]["msg"] = bb + "#" + c[2] + "#" + datetimes;
                                        }
                                    }
                                }
                            }
                        }

                    }

                    //本方反显信息
                    if (!String.IsNullOrEmpty(VSessionId))
                        SendMsgToRemotePoint(VSessionId, string.Format("{0}", bb + "#" + "在线发送:" + zxnum.ToString() + "离线发送:" + bzxnum.ToString() + "#" + datetimes));

                }
                //页面未链接
                else
                {
                    AddMsgToSessionId(KSessionId);
                }
            }


        }



        /// <summary>
        /// 添加会话消息
        /// </summary>
        /// <param name="value"></param>
        private void AddusersIdNaame(string id, string name)
        {
            if (users.Rows.Count > 0)
            {
                string flag = "0";
                int aaa = 0;
                for (int i = 0; i < users.Rows.Count; i++)
                {
                    if (users.Rows[i]["name"].ToString() == name && users.Rows[i]["id"].ToString() != id)
                    {
                        flag = "1";
                        aaa = i;
                    }
                    if (users.Rows[i]["name"].ToString() == name && users.Rows[i]["id"].ToString() == id)
                    {
                        flag = "2";
                    }
                }

                if (flag == "1")
                {
                    if (users.Rows[aaa]["zt"].ToString() == "在线")
                    {
                        //提取旧ID，并给他发送下线消息
                        string oldid = users.Rows[aaa]["id"].ToString();
                        SendMsgToRemotePoint(oldid, string.Format("{0}", "您的账号在另外手机登录，您已经被强制下线！"));
                        //替换掉旧id
                        users.Rows[aaa]["id"] = id;
                    }
                    else if (users.Rows[aaa]["zt"].ToString() == "离线")
                    {
                        //替换掉旧id、更新状态
                        users.Rows[aaa]["id"] = id;
                        users.Rows[aaa]["zt"] = "在线";
                        //检查是否有未接收的消息，有则发送
                        for (int ii = usersmsg.Rows.Count - 1; ii >= 0; ii--)//涉及删除行，循环要用倒序
                        {
                            if (usersmsg.Rows[ii]["name"].ToString() == users.Rows[aaa]["name"].ToString())
                            {
                                SendMsgToRemotePoint(id, string.Format("{0}", usersmsg.Rows[ii]["msg"]));
                                usersmsg.Rows.RemoveAt(ii);
                            }
                        }
                    }
                }
                else if (flag == "2")
                {
                    if (users.Rows[aaa]["zt"].ToString() == "离线")
                    {
                        users.Rows[aaa]["zt"] = "在线";
                        //检查是否有未接收的消息，有则发送
                        for (int ii = usersmsg.Rows.Count - 1; ii >= 0; ii--)//涉及删除行，循环要用倒序
                        {
                            if (usersmsg.Rows[ii]["name"].ToString() == users.Rows[aaa]["name"].ToString())
                            {
                                SendMsgToRemotePoint(id, string.Format("{0}", usersmsg.Rows[ii]["msg"]));
                                usersmsg.Rows.RemoveAt(ii);
                            }
                        }
                    }
                }
                else
                {
                    users.Rows.Add();
                    users.Rows[users.Rows.Count - 1]["id"] = id;
                    users.Rows[users.Rows.Count - 1]["name"] = name;
                    users.Rows[users.Rows.Count - 1]["zt"] = "在线";
                }
            }
            else
            {
                users.Rows.Add();
                users.Rows[0]["id"] = id;
                users.Rows[0]["name"] = name;
                users.Rows[0]["zt"] = "在线";
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
                        SendMsgToRemotePoint(oldid, string.Format("{0}", "您的账号在另外手机登录，您已经被强制下线！"));
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
        /// 会话关闭
        /// </summary>
        /// <param name="session"></param>
        /// <param name="value"></param>
        private void server_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            //删除电脑用户
            for (int ii = DNusers.Rows.Count - 1; ii >= 0; ii--)
            {
                if (!String.IsNullOrEmpty(DNusers.Rows[ii]["id"].ToString()) && session.SessionID == DNusers.Rows[ii]["id"].ToString())
                {
                    DNusers.Rows[ii]["zt"] = "离线";
                    //DNusers.Rows.RemoveAt(ii);
                }
            }
            //删除手机用户
            for (int i = users.Rows.Count - 1; i >= 0; i--)
            {
                if (!String.IsNullOrEmpty(users.Rows[i]["id"].ToString()) && session.SessionID == users.Rows[i]["id"].ToString())
                {
                    users.Rows[i]["zt"] = "离线";
                    //users.Rows.RemoveAt(i); 
                }
            }

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

        //----------------------------------------------------websocket 通讯 end--------------------------------------------------------------------//
















    }
}
