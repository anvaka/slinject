using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace AssemblyToInject
{
    public static class GoBabyGo
    {
        public static void Inject()
        {
            MessageBox.Show("31337");
            //Debug.WriteLine("Hello form Inject: Dispatching to UI thread");
            //Application.Current.RootVisual.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("31337")));
            //Debug.WriteLine("Hello from Inject: done.");
        }
    }
}
