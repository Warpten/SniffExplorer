using System;
using System.Windows.Forms;
using SniffExplorer.Enums;

namespace SniffExplorer.UI.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            textBox1.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            textBox1.AutoCompleteCustomSource.AddRange(Enum.GetNames(typeof(OpcodeClient)));
            textBox1.AutoCompleteCustomSource.AddRange(Enum.GetNames(typeof(OpcodeServer)));
        }

        private void LoadSniff(object sender, EventArgs e)
        {

        }
    }
}
