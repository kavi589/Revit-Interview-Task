using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace BeamHoleCreation.Presentation
{
    public partial class frmHole : Form
    {
        public frmHole()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                //InitialDirectory = @"D:\",
                Title = "Browse XML Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "xml",
                Filter = "xml files (*.xml)|*.xml",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtBrowse.Text = openFileDialog1.FileName;
                
                XmlSerializer serializer = new XmlSerializer(typeof(Hole));
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName, System.Text.Encoding.Default))
                {
                    CommonData.HoleData = (Hole)serializer.Deserialize(sr);
                    this.txtData.LoadFile(openFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                }
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CommonData.Tolerance = string.IsNullOrEmpty(this.txtTolerance.Text)?0: double.Parse(this.txtTolerance.Text);
            Command.externalEvent.Raise();
            this.Close();
        }

        private void txtTolerance_KeyPress(object sender, KeyPressEventArgs e)
        {
        }
       
    }
}
