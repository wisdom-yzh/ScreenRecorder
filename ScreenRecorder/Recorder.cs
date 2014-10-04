using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ScreenRecorder
{
	public enum RecorderState
	{
		StopCapture, Finish, Start,
	}

	public class Recorder
	{
		public readonly static Size DefaultSize = new Size(Screen.PrimaryScreen.Bounds.Width,
			Screen.PrimaryScreen.Bounds.Height);
		public readonly static Point DefaultPoint = new Point(0, 0);

		public static Point StartPoint;
		public static Size ScreenSize;

		private ScreenCapture[] screenCaptures = new ScreenCapture[2];
		private ScreenSaver screenSaver;
		private FrameBuffer frameBuffer;

		public RecorderState State { get; private set; }

		public Recorder()
		{
			this.State = RecorderState.Finish;
		}

		public decimal FrameNumber
		{
			get
			{
				decimal num = 0;
				for (int i = 0; i < screenCaptures.Length; i++)
					num += screenCaptures[i].FrameNumber;
				return num;
			}
		}

		public void StartRecord(string fileName, int fpsWanted)
		{
			frameBuffer = new FrameBuffer();

			this.State = RecorderState.Start;
			for (int i = 0; i < screenCaptures.Length; i++)
			{
				screenCaptures[i] = new ScreenCapture(frameBuffer);
				screenCaptures[i].BeginCapture();
			}
			this.screenSaver = new ScreenSaver(frameBuffer);
			this.screenSaver.BeginSave(fpsWanted, fileName);
		}

		public void EndRecord()
		{	
			for (int i = 0; i < screenCaptures.Length; i++)
				screenCaptures[i].EndCapture();
			this.State = RecorderState.StopCapture;

			while (frameBuffer.Count > 0) ;
			this.screenSaver.EndSave();
			this.screenSaver.Dispose();

			this.State = RecorderState.Finish;
		}
	}
}
