using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketUtil;
using System.Data.SqlClient;

namespace webClient
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  窗体加载样式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmLogin_Load(object sender, EventArgs e)
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


        /// <summary>
        /// websocket客户端登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string userpwd = textBox2.Text.Trim();
            if (username != "" && userpwd != "")
            {
                SqlParameter[] param = { 
                                       new SqlParameter("@username",username),
                                       new SqlParameter("@userpwd",userpwd)
                                       };
                string sql = " select name from users where username=@username and userpwd=@userpwd and userzt='1' ";
                object obj = DBHelpSql.ExecuteScalar(sql, param);
                if (obj != null)
                {
                    // 登录后获取当前登录用户名称
                    string name = obj.ToString();
                    frmClient frm = new frmClient();
                    frm.clientName = name;
                    frm.username = username;
                    frm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("用户名或密码输入有误");
                }
            }
            else
            {
                MessageBox.Show("请输入用户名或密码");
            }
        }
    }
}
