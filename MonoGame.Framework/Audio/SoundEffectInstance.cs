#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using Statements
using System;
using OpenTK.Audio.OpenAL;
#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
	public sealed class SoundEffectInstance : IDisposable
	{
		private int source;
        private SoundEffect sourceInstance;
		private bool _disposed = true;
#if RESOURCE_TRACKERS
        public long lastUsed;
#endif

		internal SoundEffectInstance(int buffer, SoundEffect parent)
		{
            sourceInstance = parent;
			source = AL.GenSource();
			var error = AL.GetError();
			if(error != ALError.NoError)
			{
				throw new OpenALException(error, "error creating source.");
			}

			_disposed = false;
			AL.Source(source, ALSourcei.Buffer, buffer ); // attach the buffer to a source
			
			error = AL.GetError();
			if(error != ALError.NoError)
			{
				throw new OpenALException(error, "error creating source");
			}
		}
		
		public void Dispose()
		{
	        if (!_disposed)
	        {
				AL.DeleteSource(source);
				source = 0;

				// Indicate that the instance has been disposed.
	            _disposed = true;
				// managed resource cleanup here.

                var error = AL.GetError();
                if (error != ALError.NoError)
                {
#if DEBUG
                    throw new OpenALException(error, "borked dispose. ALError: " + error.ToString());
#else
                    Console.WriteLine("borked dispose. ALError: " + error.ToString());
#endif
                }
                
                sourceInstance.UnlinkInstance(this);
                sourceInstance = null;
            }
			
			GC.SuppressFinalize(this);

        }

        private void CheckError()
        {
            var error = AL.GetError();
            if (error != ALError.NoError)
            {
#if DEBUG
                throw new OpenALException(error, "ALError: " + error.ToString());
#else
                Console.WriteLine("ALError: " + error.ToString());
#endif
            }

        }
	
		public void Apply3D (AudioListener listener, AudioEmitter emitter)
		{
			throw new NotImplementedException();
		}
		
		public void Apply3D (AudioListener[] listeners,AudioEmitter emitter)
		{
			throw new NotImplementedException();
		}		
		
		public void Pause ()
		{
			AL.SourcePause(source);
            CheckError();

		}
		
		public void Play ()
		{
#if RESOURCE_TRACKERS
            lastUsed = DateTime.Now.Ticks;
#endif

			AL.SourcePlay(source);
            CheckError();

		}
		
		public void Resume ()
		{
			Play();

		}
		
		public void Stop ()
		{
			AL.SourceStop(source);
            CheckError();

		}
		
		public void Stop (bool immediate)
		{
			Stop();
		}
		
		public bool IsDisposed 
		{ 
			get
			{
				return _disposed;
			}
		}
		
		public bool IsLooped 
		{ 
            get
            {
                bool r = false;
                AL.GetSource(source, ALSourceb.Looping, out r);
                CheckError();

                return r;
            }
            
            set
            {
                AL.Source(source, ALSourceb.Looping, value);
                CheckError();

            }
		}
		
		public float Pan 
		{ 
			get
			{
				throw new NotImplementedException();
			}
			
			set
			{
				throw new NotImplementedException();
			}
		}
		
		public float Pitch { 
			get
			{
				float r = 0;
				AL.GetSource(source, ALSourcef.Pitch, out r);
                CheckError();

				return (float) (Math.Log(r) / Math.Log(2));
			}
			set
			{
				AL.Source(source, ALSourcef.Pitch, (float) (Math.Pow(2, value)));
                CheckError();

			}
		}
		
		public SoundState State 
		{ 
			get
			{
                var state = AL.GetSourceState(source);
                switch(state)
                {
                case ALSourceState.Initial:
                    return SoundState.Stopped;
                case ALSourceState.Paused:
                    return SoundState.Paused;
                case ALSourceState.Playing:
                    return SoundState.Playing;
                case ALSourceState.Stopped:
                    return SoundState.Stopped;
                default:
                    return SoundState.Stopped;                    
                }
			} 
		}
		
		public float Volume
		{ 
			get
			{
				float r = 0;
				AL.GetSource(source, ALSourcef.Gain, out r);
                CheckError();
				return r;
			}
			
			set
			{
                AL.Source(source, ALSourcef.Gain, value);
                CheckError();
            }
		}		
	}
}
