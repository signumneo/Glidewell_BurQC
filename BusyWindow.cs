﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace User_Interface
{
    public partial class BusyWindow : UserControl
    {
        public BusyWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgressBar(int value)
        {
            progressBar1.Value = value;
        }

       
    }
}