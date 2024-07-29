using System;
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
    public partial class PDFViewer : Form
    {
        public PDFViewer()
        {
            InitializeComponent();
        }

        private void txtboxPDFViewer_TextChanged(object sender, EventArgs e)
        {

        }

        public void axPDFReader_Enter(object sender, EventArgs e)
        {

        }

        private void btn_ScanTempFolder_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            if (fd.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(fd.FileName);
                axPDFReader.src = fd.FileName;
            }
            else
            {
                MessageBox.Show("Select PDF you want to read");
            }

        }
    }
}
