using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using ScreenRecorder;
using GlobalMouseHook;

namespace 屏幕录像机
{
	public partial class FrmMain : Form
	{	
		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow();

		private Recorder recorder = new Recorder();
		private decimal prevFrameNumber = 0;
		private SaveFileDialog fileDlg = new SaveFileDialog();
		private MouseHook mouseHook;
		private Point []sizePoints = new Point[2];
		private int clickCount = 0;
		private bool fullScreenMode = true;

		private Graphics gDraw;

		public FrmMain()
		{
			InitializeComponent();

			gDraw = Graphics.FromHwnd(GetDesktopWindow());

			fileDlg.Filter = "mpeg视频文件|*.mpg";
			fileDlg.Title = "视频文件保存位置";
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (recorder.State == RecorderState.Finish)
			{
				if (fileDlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(fileDlg.FileName))
				{
					if (this.getRecorderArea())
					{
						this.WindowState = FormWindowState.Minimized;
						recorder.StartRecord("$temp.mpg", 20);
						button1.Text = "end";
						timer1.Enabled = true;
					}
				}
			}
			else
			{
				recorder.EndRecord();
				while (recorder.State != RecorderState.Finish)
					;
				timer1.Enabled = false;

				if (File.Exists(fileDlg.FileName))
					File.Delete(fileDlg.FileName);
				File.Copy("$temp.mpg", fileDlg.FileName);
				File.Delete("$temp.mpg");
				button1.Text = "start";
			}
		}

		private bool getRecorderArea()
		{
			if (MessageBox.Show("是否录制整个屏幕?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				Recorder.ScreenSize = Recorder.DefaultSize;
				Recorder.StartPoint = Recorder.DefaultPoint;
				return true;
			}

			MessageBox.Show("关闭此对话框后,请分别点击屏幕上两点,以此作为录制区域的依据");

			mouseHook = new MouseHook();
			mouseHook.OnMouseActivity += Global_Click;
			clickCount = 0;
			mouseHook.Start();
			while (clickCount != 2)
				Application.DoEvents();
			mouseHook.Stop();

			if (sizePoints[0].X > sizePoints[1].X)
			{
				int t = sizePoints[0].X;
				sizePoints[0].X = sizePoints[1].X;
				sizePoints[1].X = t;
			}

			if (sizePoints[0].Y > sizePoints[1].Y)
			{
				int t = sizePoints[0].Y;
				sizePoints[0].Y = sizePoints[1].Y;
				sizePoints[1].Y = t;
			}

			fullScreenMode = false;
			Recorder.StartPoint = sizePoints[0];
			Recorder.ScreenSize = new Size(sizePoints[1].X - sizePoints[0].X, sizePoints[1].Y - sizePoints[0].Y);
			return true;
		}

		
		private void Global_Click(object sender, MouseEventArgs e)
		{
			if (clickCount >= 2) return ;
			else if (e.Clicks == 10)	
			{
				sizePoints[clickCount] = new Point(e.X, e.Y);
				gDraw.DrawImage(Image.FromFile("VV.png"), sizePoints[clickCount] - new Size(36, 36));
				clickCount++;
			}
		}

		private void drawRegion()
		{
			gDraw.DrawRectangle(Pens.Blue, sizePoints[0].X, sizePoints[0].Y,
				sizePoints[1].X - sizePoints[0].X, sizePoints[1].Y - sizePoints[0].Y);
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			label1.Text = "帧数 = " + recorder.FrameNumber + "\n瞬时FPS = " + 
				((double)(recorder.FrameNumber - prevFrameNumber) / 3.0 * 10).ToString("##.##");

			prevFrameNumber = recorder.FrameNumber;
			if (recorder.State == RecorderState.StopCapture) button1.Text = "wait";

			if (!this.fullScreenMode)
			{
				this.drawRegion();
			}
		}
	}
}
