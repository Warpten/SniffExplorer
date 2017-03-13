using System.Windows.Forms;

namespace SniffExplorer.Utils
{
    public static class MiscExtensions
    {
        public static void InvokeIfRequired(this Form obj, MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else
            {
                action();
            }
        }
    }
}
