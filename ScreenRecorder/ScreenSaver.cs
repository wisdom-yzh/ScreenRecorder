using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ScreenRecorder
{
	internal enum SaverState
	{
		Running, Stopped
	}

	internal class ScreenSaver: IDisposable
	{
		private IntPtr videoWriter = IntPtr.Zero;
		private Thread saveThread;
		private SaverState saverState;

		public FrameBuffer Frames;

		public ScreenSaver(FrameBuffer buffer)
		{
			saveThread = new Thread(new ThreadStart(saveMethod));
			this.Frames = buffer;
			this.saverState = SaverState.Stopped;
		}

		public void Dispose()
		{
			if (this.videoWriter != IntPtr.Zero)
			{
				CvInvoke.cvReleaseVideoWriter(ref this.videoWriter);
				this.videoWriter = IntPtr.Zero;
			}
		}

		public void BeginSave(int fpsWanted, string fileName)
		{
			this.Dispose();
			videoWriter = CvInvoke.cvCreateVideoWriter(fileName, CvInvoke.CV_FOURCC('P', 'I', 'M', '1'),
				fpsWanted, Recorder.ScreenSize, true);
			this.saverState = SaverState.Running;
			this.saveThread.Start();
		}

		public void EndSave()
		{
			this.saverState = SaverState.Stopped;
			while (saveThread.ThreadState != ThreadState.Stopped)
				Thread.Sleep(10);

			CvInvoke.cvReleaseVideoWriter(ref this.videoWriter);
			this.videoWriter = IntPtr.Zero;
		}

		private void saveMethod()
		{
			while (this.saverState == SaverState.Running)
			{
				Image<Bgr, Byte> image = Frames.ThreadSafeDequeue();
				if (image != null) //IntPtr.Zero)
				{
					CvInvoke.cvWriteFrame(videoWriter, image);
					image.Dispose();
				}
			}
		}

		
	}
}
