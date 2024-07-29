using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static User_Interface.Properties.Settings;
using System.Windows;

namespace User_Interface
{
    public partial class Form1 : Form
    {
        public static Form1 form1Instance;
        public bool[] badScanBurResults = new bool[40];
        public bool[] discardBurResults = new bool[40];
        public string correctLotNumber = "";
        public Form1()
        {
            InitializeComponent();
            try
            {
                //this.Left = 0;
                //this.Width = (int)SystemParameters.WorkArea.Width;
                //this.Top = (int)SystemParameters.WorkArea.Top;
                //this.Height = (int)SystemParameters.WorkArea.Height;
                if (Default.Sorting)
                {
                    side_panel.Height = btn_sorting.Height;
                    side_panel.Top = btn_sorting.Top;
                    btn_TipScan1To1.Visible = false;
                    btn_SideScan.Visible = false;
                    btn_SideScanPerLot.Visible = false;
                    btn_Home.Visible = false;
                    btn_binSettings.Visible = false;
                    btn_RD.Visible = false;
                    btn_rdBinSettings.Visible = false;
                    
                    sorting1.BringToFront();
                  
                    sorting1.Visible = true;
                    form1Instance = this;

                    //sorting1.Width = (int)SystemParameters.WorkArea.Width;
                    //sorting1.Left = side_panel.Width + 10;
                    //sorting1.Height = (int)SystemParameters.WorkArea.Height - 10;
                  
                }
                else
                {
                    //side_panel.Height = btn_TipScan1To1.Height;
                    //side_panel.Top = btn_TipScan1To1.Top;
                    //tipScan1To1.BringToFront();
                    
                    btn_sorting.Visible = false;

                    side_panel.Height = btn_Home.Height;
                    side_panel.Top = btn_Home.Top;
                    topScanUserControl.BringToFront();
                    
                    form1Instance = this;
                }
                //busyWindow1.Visible = false;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            //side_panel.Height = btn_TipScan1To1.Height;
            //side_panel.Top = btn_TipScan1To1.Top;
            //tipScan1To1.BringToFront();
            //form1Instance = this;
          
        }

        public int test;


        public void Form1_Load(object sender, System.EventArgs e)
        {

        }
       
        private void btn_Home_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_Home.Height;
            side_panel.Top = btn_Home.Top;
            topScanUserControl.Visible = false;
            topScanUserControl.Visible = true;
            topScanUserControl.BringToFront();
        }

        private void btn_SpindleBreak_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_SpindleBreak.Height;
            side_panel.Top = btn_SpindleBreak.Top;
            spindleBreakInUserControl1.BringToFront();
        }

        


        private void btn_CloseApplication_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(50);
            System.Windows.Forms.Application.Exit();
        }

        private void btn_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        int mouseX = 0, mouseY = 0;
        bool mouseDown;

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                mouseX = MousePosition.X - 650;
                mouseY = MousePosition.Y - 7;

                this.SetDesktopLocation(mouseX,mouseY);
            }
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void btn_SpindleBreakIn_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
        }



        private void btn_SpindleTest_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_SpindleTest.Height;
            side_panel.Top = btn_SpindleTest.Top;
            settings.BringToFront();
        }

        private void spindleTestUserControl1_Load(object sender, EventArgs e)
        {

        }

        private void btn_SideScan_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_SideScan.Height;
            side_panel.Top = btn_SideScan.Top;
            sideScanUserControl.BringToFront();
        }

        private void btn_SideScanPerLot_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_SideScanPerLot.Height;
            side_panel.Top = btn_SideScanPerLot.Top;
            burSideScanPerLotUserControl.BringToFront();
        }

        private void btn_TipScan1To1_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_TipScan1To1.Height;
            side_panel.Top = btn_TipScan1To1.Top;
            tipScan1To11.BringToFront();
        }


        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void btn_sorting_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_sorting.Height;
            side_panel.Top = btn_sorting.Top;
            sorting1.BringToFront();
        }

        private void btn_RD_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_RD.Height;
            side_panel.Top = btn_RD.Top;
            inHouseBurInspection1.Visible = false;
            inHouseBurInspection1.Visible = true;
            inHouseBurInspection1.BringToFront();

        }

        private void btn_binSettings_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_binSettings.Height;
            side_panel.Top = btn_binSettings.Top;
            binSettings1.BringToFront();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_rdBinSettings.Height;
            side_panel.Top = btn_rdBinSettings.Top;
            rdBinSettings1.BringToFront();
        }

        private void btn_fixtureTransfer_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_fixtureTransfer.Height;
            side_panel.Top = btn_fixtureTransfer.Top;
            fixtureTransfer1.BringToFront();
        }

        private void btn_RDSettings_Click(object sender, EventArgs e)
        {
            side_panel.Height = btn_RDSettings.Height;
            side_panel.Top = btn_RDSettings.Top;
            rdSettings1.BringToFront();
        }


        //public void ShowBusyWindow(bool show)
        //{
        //    if (show)
        //    {
        //        busyWindow1.Visible = true;
        //        busyWindow1.UpdateProgressBar(1);
        //        busyWindow1.BringToFront();
        //    }
        //    else
        //    {
        //        busyWindow1.Visible = false;
        //        busyWindow1.UpdateProgressBar(0);
        //        busyWindow1.SendToBack();

        //    }
        //}

        //public void UpdateBusyWindowProgress(int value)
        //{
        //    busyWindow1.UpdateProgressBar(value);
        //}
    }
}
