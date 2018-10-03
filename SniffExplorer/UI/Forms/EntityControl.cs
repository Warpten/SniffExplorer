using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SniffExplorer.Core;

namespace SniffExplorer.UI.Forms
{
    public partial class EntityControl : UserControl
    {
        private Entity _entity;
        public Entity Entity
        {
            get => _entity;
            set {
                _entity = value;
                Refresh();
            }
        }

        public EntityControl()
        {
            InitializeComponent();
        }

        public override void Refresh()
        {
            sourceSpellControl.Entity = Entity;

            base.Refresh();
        }
    }
}
