using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SniffExplorer.Core;
using SniffExplorer.Core.Packets;
using SniffExplorer.Core.Packets.Parsing;

namespace SniffExplorer.UI.Forms
{
    public partial class MainForm : Form
    {
        private BinaryProcessor Processor { get; set; }

        public MainForm()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            _filterTextBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();

            _opcodeListView.GetColumn(0).AspectGetter = model => model.ToString();
            _opcodeListView.ItemChecked += (o, args) => {
                var selectedPackets = _opcodeListView.CheckedObjects.Cast<string>();
                _detailListView.Objects = PacketStore.GetPackets(selectedPackets);
            };

            _detailListView.GetColumn(0).AspectGetter = model =>
                (model as PacketStore.Record)?.Opcode.ToString();
            _detailListView.GetColumn(1).AspectGetter = model =>
                (model as PacketStore.Record)?.TimeStamp.ToString("dd/MM/yyyy hh:mm:ss.ffffff");

            _detailListView.CellClick += (o, cellClickArgs) => {
                if (cellClickArgs.Model != null)
                    _detailedPacketView.SelectedObject = ((PacketStore.Record) cellClickArgs.Model).Packet;
            };

            var opcodeFilter = new OpcodeFilter(_opcodeListView);
            _filterTextBox.TextChanged += (o, _) => opcodeFilter.FilterValue = _filterTextBox.Text;
        }

        private void LoadSniff(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog {Filter = @"PKT files|*.pkt"};
            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            Processor = new BinaryProcessor();
            Processor.OnPacketParsed += PacketStore.Insert;
            Task.Factory.StartNew(() =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                Processor.Process(fileDialog.FileName);
                stopwatch.Stop();
                Invoke((MethodInvoker)(() =>
                {
                    _filterTextBox.AutoCompleteCustomSource.Clear();
                    _filterTextBox.AutoCompleteCustomSource.AddRange(Enum.GetNames(typeof(Opcodes)));

                    _opcodeListView.Enabled = true;
                    _opcodeListView.Objects = PacketStore.GetAvailablePackets();

                    _filterTextBox.Enabled = true;

                    _sniffLoadProgressBar.Visible = false;

                    toolStripStatusLabel1.Text = $"{Processor.Count} packets parsed in {stopwatch.Elapsed}";
                }));
            });
        }

        private void InvokeIfRequired(MethodInvoker action)
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }
    }
}
