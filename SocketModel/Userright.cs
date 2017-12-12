using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketModel
{
   public class Userright
    {

        private int uid;

        public int Uid
        {
            get { return uid; }
            set { uid = value; }
        }

        private string uguid;

        public string Uguid
        {
            get { return uguid; }
            set { uguid = value; }
        }

        private string uname;

        public string Uname
        {
            get { return uname; }
            set { uname = value; }
        }

        private string upwd;

        public string Upwd
        {
            get { return upwd; }
            set { upwd = value; }
        }

        private string upower;

        public string Upower
        {
            get { return upower; }
            set { upower = value; }
        }

        private string ustatic;

        public string Ustatic
        {
            get { return ustatic; }
            set { ustatic = value; }
        }

        public Userright(int uid, string uguid, string uname, string upwd, string upower, string ustatic)
        {
            // TODO: Complete member initialization
            this.uid = uid;
            this.uguid = uguid;
            this.uname = uname;
            this.upwd = upwd;
            this.upower = upower;
            this.ustatic = ustatic;
        }




    }
}
