using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ScreenRecorder
{
	internal enum CaptureState
	{
		Running, Stopped,
	}

    internal class ScreenCapture
    {
		private Thread captureThread;
		private CaptureState captureState;
		public decimal FrameNumber { get; private set; }

		public FrameBuffer Frames;

		public ScreenCapture(FrameBuffer Frames)
		{
			this.captureThread = new Thread(new ThreadStart(captureMethod));
			this.Frames = Frames;
			this.FrameNumber = 0;
		}

		public void BeginCapture()
		{
			this.captureState = CaptureState.Running;
			this.captureThread.Start();
		}

		public void EndCapture()
		{
			this.captureState = CaptureState.Stopped;
		}

		private void captureMethod()
		{
			while (this.captureState == CaptureState.Running)
			{
				Image<Bgr, byte> image = new Image<Bgr, byte>(captureOneFrame());
				if (this.Frames.ThreadSafeEnqueue(image))//.Ptr))
					this.FrameNumber++;
			}
		}

		private Bitmap captureOneFrame()
		{
			Bitmap bitmap = new Bitmap(Recorder.ScreenSize.Width, Recorder.ScreenSize.Height);

			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.CopyFromScreen(Recorder.StartPoint, new Point(0, 0),
					new Size(bitmap.Width, bitmap.Height));
			}

			return bitmap;
		}
    }
}
