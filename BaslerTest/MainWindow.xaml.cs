using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BaslerTest
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BaslerCamera.Instance.CameraImageEvent += Camera_CameraImageEvent;
        }

        private void Camera_CameraImageEvent(Bitmap bmp)
        {
            pictureBox1.Invoke(new MethodInvoker(delegate
            {
                Bitmap old = pictureBox1.Image as Bitmap;
                pictureBox1.Image = bmp;
                if (old != null)
                    old.Dispose();
            }));
        }

        private void Grab_Click(object sender, RoutedEventArgs e)
        {
            BaslerCamera.Instance.KeepShot();
        }

        private void GrabOnce_Click(object sender, RoutedEventArgs e)
        {
            BaslerCamera.Instance.OneShot();
        }
    }
}
