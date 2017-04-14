using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace TikaoTestThree
{
    public delegate void ShowMessageDelegateHander(String message);//显示消息
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            clientConServer.ShowMessageEvent += new ShowMessageDelegateHander(ShowHintInfo);
            
        }



        //
        //显示消息
        //
        delegate void AddItemCallback(string text); //多线程调度listbox
        private void ShowHintInfo(String s)
        {
            if (this.systemMsg.InvokeRequired)  
            {  
                AddItemCallback d = new AddItemCallback(ShowHintInfo); 
                this.Invoke(d, new object[] { s });
            }  
            else  
            {
                systemMsg.Items.Add(DateTime.Now + " " + s + Environment.NewLine);
                systemMsg.SelectedIndex = systemMsg.Items.Count - 1;
            }  
        }

        
        /// <summary>
        /// 连接指纹仪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
     
        private void 连接指纹仪ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FingerConnect(sender,e);
        }

        private void 指纹录入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZKFPEngX1.EnrollIndex = 3;
            ZKFPEngX1.BeginEnroll();

            ShowHintInfo("开始录入");
            //FingerClose();//关闭指纹仪

        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            ShowHintInfo(m_clientCon.uploadSqlCom("poqwueieoriuewfcvjfkjghdfuighiufhieurgiufhgeiubrigurehguirebkjfgnuirhgiubnkjgfnbkjdfguioshbgfnbkfshudfighiuerfgbvuidfbhdiufsgbiudfgbdfvbiusdhguierghierbg"));
            if(m_clientCon!=null)
            m_clientCon.SqlSearch();
        }
        private void Register_button_Click(object sender, EventArgs e)
        {

        }

        private void 查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 连接服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            m_clientCon = new clientConServer();

            if(m_clientCon.clientConnect("127.0.0.1", 9999))//连接数据库

            连接指纹仪ToolStripMenuItem.Enabled = true;

        }
    }
}
