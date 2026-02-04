using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using System.Windows.Media.Imaging; 
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
namespace TMS2655_LEE_3
{

   
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
       
            Dispatcher.Invoke(() =>
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
                byte[] packet = MakePacket("ST");
                await _stream.WriteAsync(packet, 0, packet.Length);
                //패킷전체크기를 stream을 통해 전송
                Log("측정 시작 명령을 보냄.");

            }

            catch
            {
                Log("전송 중 오류 발생: " );
            }

        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] packet = MakePacket("SP");
                await _stream.WriteAsync(packet, 0, packet.Length);

                Log("측정 중지 명령을 보냄");
            }

            catch
            {
                Log("전송 중 오류 발생");
            }
        }

        private async void BtnGetResult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] packet = MakePacket("RB");
                await _stream.WriteAsync(packet, 0, packet.Length);

                byte[] recievebuffer = new byte[4096];

                int byteRead = await _stream.ReadAsync(recievebuffer, 0, recievebuffer.Length);

                if(byteRead > 0)
                {
                    string result = Encoding.ASCII.GetString(recievebuffer, 0, byteRead);

                    if(result.Contains("E00"))
                    {
                        int spos = result.IndexOf("S");
                        
                        string high = result.Substring(spos + 1, 3);

                        int mpos = result.IndexOf("M",spos);

                        string mid = result.Substring(mpos + 1, 3);

                        int dpos = result.IndexOf("D",mpos);

                        string low = result.Substring(dpos + 1,3);

                        int ppos = result.LastIndexOf("S");

                        string pulse = result.Substring(ppos + 1, 3);



                        this.Dispatcher.Invoke(() =>
                        {
                            lblSys.Text = high;
                            lblDia.Text = low;
                            lblaverage.Text = mid;
                            lblPulse.Text = pulse;
                        });

                    }

                    Log("측정 결과 수신: " + result);

                }
                else
                {
                    Log("수신된 데이터 없음");
                }
            }

            catch
            {
                Log("전송 중 오류 발생");
            }

        }

        

        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                string path = System.IO.Path.Combine(txtSavePath.Text, "BloodResult.png");

                CaptureControl(ResultArea, path);

                MessageBox.Show(" 저장 성공!\n" + path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 실패: " + ex.Message);
            }
        }

        private void CaptureControl(FrameworkElement element, string filePath)
        {
            Rect rectangle = new Rect(0, 0, element.ActualWidth, element.ActualHeight);

            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(element);

                context.DrawRectangle(brush, null, new Rect(rectangle.Size));
            }

            int width =  (int)element.ActualWidth;
            int height = (int)element.ActualHeight;

            RenderTargetBitmap bitmap = new RenderTargetBitmap
            (
                width,
                height,
                96,
                96,
                PixelFormats.Pbgra32
            );

            bitmap.Render(visual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(stream);
            }
        }

       private byte[] MakePacket(string command)
       {
            List <byte> buffer = new List<byte> { 22, 22, 1, 48, 48, 2 };

            byte[] cmdBytes = Encoding.ASCII.GetBytes(command);

            foreach(byte b in cmdBytes)
            {
                buffer.Add(b);
            }

            buffer.Add(3);

            byte checksum = 0;

            for(int i=2; i<buffer.Count;i++)
            {
                checksum^= buffer[i];
            }

            buffer.Add(checksum);


            return buffer.ToArray();

        }

      

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = "저장 위치를 선택";
            dialog.FileName = "결과화면.jpg";

            if (dialog.ShowDialog() == true)
            {
                txtSavePath.Text = System.IO.Path.GetDirectoryName(dialog.FileName);
                Log("저장 경로 설정: " + txtSavePath.Text);
            }
        }


    }


}
