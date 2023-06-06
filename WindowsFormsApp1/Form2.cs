using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public int SizeGrid { get; set; }
        public int SizeTile { get; set; }
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (int.TryParse(((int)numericUpDown1.Value).ToString(), out int newSizeGrid))
            {
                SizeGrid = newSizeGrid;
            }


            if (int.TryParse(((int)numericUpDown2.Value).ToString(), out int newSizeTile))
            {
                SizeTile = newSizeTile;
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
