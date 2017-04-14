using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TikaoTestThree
{

    public partial class Form1
    {

        private int FMatchType=0, fpcHandle=0;
        private bool FAutoIdentify;
        private string str;

        //
        //连接指纹仪
        //
        public void FingerConnect(object sender, EventArgs e)
        {
            if (ZKFPEngX1.InitEngine() == 0)
            {
                连接指纹仪ToolStripMenuItem.Enabled = false;
                指纹录入ToolStripMenuItem.Enabled = true;
                
                //FMatchType = 2;//1:N或者1:1
                //ZKFPEngX1.EnrollIndex = 1;

                ShowHintInfo("指纹仪连接成功");

                ZKFPEngX1.CancelCapture();//暂停使用
                ZKFPEngX1.CancelEnroll();

                ZKFPEngX1.BeginCapture();//开启使用

                //ZKFPEngX1.EngineValid = false;
                //ZKFPEngX1.BeginCapture();
                //FingerClose();

            }
            else
            {
                ShowHintInfo("指纹仪连接失败");
            }
        }


        //
        //获取指纹仪传输图像
        //
        private void ZKFPEngX1_OnImageReceived(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnImageReceivedEvent e)
        {

            Graphics g = FingerImage.CreateGraphics();
            Bitmap bmp = new Bitmap(FingerImage.Width, FingerImage.Height);

            g = Graphics.FromImage(bmp);
            int dc = g.GetHdc().ToInt32();
            ZKFPEngX1.PrintImageAt(dc, 0, 0, bmp.Width, bmp.Height);
            g.Dispose();
            FingerImage.Image = bmp;

        }
        //
        //验证,查找指纹信息
        //
        private void axZKFPEngX1_OnCapture(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnCaptureEvent e)
        {
            //
            //从数据库获取指纹信息验证指纹
            //

            bool RegChang = true;
            StreamReader s = File.OpenText("e:\\testtwo.txt");//测试从本地获取
            string tem =null;
            while ((tem+= s.ReadLine()) != null)
            { break; }
            s.Close();
            object FRegTemplate = ZKFPEngX1.DecodeTemplate1(tem);//获取数据库指纹信息

            object myFregTemplate = e.aTemplate;

            if(ZKFPEngX1.VerFingerFromStr(ref tem,ZKFPEngX1.GetTemplateAsString(),true, ref RegChang))//获取数据

           // if (ZKFPEngX1.VerFinger(ref FRegTemplate, myFregTemplate, true, ref RegChang))//匹配函数
            {
                ShowHintInfo("指纹验证成功");//指纹查找成功,入队并获取数据库学生信息
            }
            else
            {
                ShowHintInfo("指纹验证失败,请联系管理员");
            }
        }

        //
        //读写三次指纹信息
        //
        private void ZKFPEngX1_OnFeatureInfo(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnFeatureInfoEvent e)
        {

            String strTemp = null;
            if (e.aQuality != 0)
            {
                strTemp = strTemp + "指纹采样错误重新采样";
            }
            else
            {
                strTemp = strTemp + "指纹采样正确";
                if (ZKFPEngX1.EnrollIndex != 1)
                {
                    if (ZKFPEngX1.IsRegister)
                    {
                        if (ZKFPEngX1.EnrollIndex - 1 > 0)
                        {
                            strTemp = strTemp + '\n' + "还需录入" + Convert.ToString(ZKFPEngX1.EnrollIndex - 1) + "次";
                        }
                    }
                }
            }

            ShowHintInfo(strTemp);
        }
        //
        //指纹三次读写完毕时调用
        //保存指纹信息
        //
        private void ZKFPEngX1_OnEnroll(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnEnrollEvent e)
        {
            if (e.actionResult)
            {
                String FingerData=ZKFPEngX1.GetTemplateAsString();

                ShowHintInfo("指纹保存成功");
    
                //
                //上传数据库未写

              //  for (int i = 0; i < 100;i++)
                   ShowHintInfo(m_clientCon.uploadSqlCom(FingerData));//上传指纹数据

            }
            else
            {
                ShowHintInfo("指纹信息不一致请重新采样");
            }

            

        }
        //
        //断开指纹仪的连接
        //
        private void FingerClose()
        {
            ZKFPEngX1.EndEngine();
            ShowHintInfo("指纹仪已断开");
            连接指纹仪ToolStripMenuItem.Enabled = true;
            指纹录入ToolStripMenuItem.Enabled = false;
        }
    
    }
}
