using System.Collections;
using BrightIdeasSoftware;

namespace SniffExplorer.UI.Forms
{
    public class OpcodeFilter : IListFilter
    {
        public OpcodeFilter(ObjectListView olv)
        {
            View = olv;
        }

        private ObjectListView View { get; }
        private string _filter;

        public string FilterValue
        {
            get { return _filter; }
            set
            {
                View.ListFilter = null;
                _filter = value;
                View.ListFilter = this;
            }
        }

        public IEnumerable Filter(IEnumerable modelObjects)
        {
            foreach (var model in modelObjects)
            {
                if (!(model is string))
                    continue;

                if (string.IsNullOrEmpty(FilterValue) || model.ToString().Contains(FilterValue))
                    yield return model;
            }
        }
    }
}
