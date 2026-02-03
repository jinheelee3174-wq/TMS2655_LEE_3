using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMS2655_LEE_3
{
    //
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private TcpClient _client;
        private NetworkStream _stream;



        public MainWindow()
        {
            InitializeComponent();
            
        }

       

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            string ip = txtIpAddress.Text.Trim();

            if (!int.TryParse(txtPort.Text.Trim(), out int port))
            {
                Log("포트 숫자 X");
                return;
            }

            try
            {
                _client = new TcpClient();
                _client.Connect(ip, port);
                _stream = _client.GetStream();

                Log(string.Format("연결 성공: {0}:{1}", ip, port));

            }
            catch (Exception ex)
            {
                Log("연결 실패: " + ex.Message);
            }
        }

        private void BtnDisConnect_Click(object sender, RoutedEventArgs e)
        {
            _stream?.Close();
            _client?.Close();

            Log("연결 해제");

        }


        private void Log(string message)
        {
       
            this.Dispatcher.Invoke(() =>
            //지정된 대리자 동기적 실행
            {
                txtLog.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            });
        }

    

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if(_stream ==null)
            {
                Log("연결되지않음");

                return;
            }

            try
            {
                
            }

            catch
            {

            }

        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnGetResult_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private byte[] BuildCommand(string cmd2)
        {
            byte SYN = 0x16;
            byte SOH = 0x01;
            byte STX = 0x02;
            byte ETX = 0X03;
            
        }
        
    }
}
