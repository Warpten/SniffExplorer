using System;
using System.Windows.Forms;
using SniffExplorer.Enums;
using SniffExplorer.Packets.Parsing;
using SniffExplorer.Utils;

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

            BinaryProcessor.OnOpcodeParsed += opcode => this.InvokeIfRequired(() => {
                toolStripStatusLabel1.Text = $@"Parsed {opcode} ...";
            });
            BinaryProcessor.OnSniffLoaded += () => this.InvokeIfRequired(() => {
                var l = PacketStore.Count;
            });
        }

        private void LoadSniff(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog {Filter = @"PKT files|*.pkt"};

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            BinaryProcessor.Process(fileDialog.FileName);
        }
    }
}
