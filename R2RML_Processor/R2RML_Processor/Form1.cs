using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace R2RML_Processor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ClassTest classTest = new ClassTest();
            //classTest.Testings();
            //classTest.HelloWorld(this);
            //classTest.Reading(this);
            //classTest.Writing(this);
            //classTest.WorkingWithGraphs(this);
            //classTest.QueryWithSPARQL(this);
            classTest.r2rml(this);

        }

        private void rchbx_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
            {
                Font font = new Font("Tahoma", 8, FontStyle.Regular);
                rchbx.SelectionFont = font;
                rchbx.SelectionColor = Color.Red;
                //rchbx.SelectedText += (char)(e.KeyValue);
                //rchbx.SelectedText += (char)(e.KeyCode);

            }
            else
            {
                Font font = new Font("Miriam", 14, FontStyle.Italic);
                rchbx.SelectionFont = font;
                rchbx.SelectionColor = Color.Brown;
                //rchbx.SelectedText +=  e.KeyData;
            }
        }

        
    }
}
