using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;

namespace ScreenRecorder
{
	internal enum FrameBufferState
	{
		Free, Busy
	}

	internal class FrameBuffer: Queue<Image<Bgr, Byte>>//IntPtr>
	{
		private object locker = new object();
		private FrameBufferState bufferState;

		public static readonly int MAX_SIZE = 500;

		public bool ThreadSafeEnqueue(Image<Bgr, Byte> item)
		{
			bool output = false;
			while (bufferState == FrameBufferState.Busy)
				Thread.Sleep(10);

			lock (locker)
			{
				bufferState = FrameBufferState.Busy;
				if (this.Count <= MAX_SIZE)
				{
					this.Enqueue(item);
					output = true;
				}
				bufferState = FrameBufferState.Free;
			}

			return output;
		}

		public /*IntPtr*/Image<Bgr, Byte> ThreadSafeDequeue()
		{
			/*IntPtr*/ Image<Bgr, Byte> output = null;//IntPtr.Zero;
			while (bufferState == FrameBufferState.Busy)
				Thread.Sleep(10);

			lock (locker)
			{
				bufferState = FrameBufferState.Busy;
				if (this.Count > 0) output = this.Dequeue();
				bufferState = FrameBufferState.Free;
			}

			return output;
		}
	}
}
