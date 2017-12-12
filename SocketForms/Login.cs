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
using System.Drawing.Drawing2D;
using SocketModel;
using System.Data.SqlClient;

namespace SocketForms
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        public void bianyuan() 
        {
            skinEngine1.SkinFile = "SilverColor2.ssk";
            //第一个
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(pictureBox1.ClientRectangle);
            Region region = new Region(gp);
            pictureBox1.Region = region;
            gp.Dispose();
            region.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string userName = textBox1.Text.ToString().Trim();
            string userPwd = textBox2.Text.ToString().Trim();

            if (userName != "")
            {
                if (userPwd != "")
                {
                    //string sql = " select * from Userright where uname=@uname and upwd=@upwd ";
                    SqlParameter[] param = { 
                                           new SqlParameter("@uname",userName),
                                           new SqlParameter("@wpwd",userPwd),
                                       };
                    DataSet users = DBHelpSql.RunProcedureGetDS("pro_selUser", param);


                    //object nnn = DBHelpSql.GetSingle("select * from Userright ");
                    if (users.Tables[0].Rows.Count > 0)
                    {
                        cz.User = new Userright(
                            (int)users.Tables[0].Rows[0]["uid"],
                            users.Tables[0].Rows[0]["uguid"].ToString(),
                            users.Tables[0].Rows[0]["uname"].ToString(),
                            users.Tables[0].Rows[0]["upwd"].ToString(),
                            users.Tables[0].Rows[0]["upower"].ToString(),
                            users.Tables[0].Rows[0]["ustatic"].ToString()
                            );
                        Main form = new Main();
                        form.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("用户名或密码不正确");
                    }
                }
                else
                {
                    MessageBox.Show("密码不能为空");
                }
            }
            else
            {
                MessageBox.Show("用户名不能为空");
            }
        }

        

       

        protected override void WndProc(ref Message m)
        {
            //拦截双击标题栏、移动窗体的系统消息
            if (m.Msg != 0xA3 && m.Msg != 0x0003 && m.WParam != (IntPtr)0xF012)
            {
                base.WndProc(ref m);
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            bianyuan();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {  
            textBox2.Text = "";      
        }

    }
}
