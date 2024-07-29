using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace User_Interface
{
    public partial class InstructionsUserControl : UserControl
    {      
        private void InstructionsUserControl_Load(object sender, EventArgs e)
        {
            string file_directory = Directory.GetCurrentDirectory();
            string file_path = file_directory + "\\" + "instructions.pdf";
            Console.WriteLine(file_path);


            if (File.Exists(file_path))
            {
                try
                {
                    axPDFReader.src = file_path;
                }
                catch
                {

                }
            }
            else
            {
                Console.WriteLine("file does not exist");
            
            }
        }

        public InstructionsUserControl()
        {
            InitializeComponent();
        }
       
    }
}
