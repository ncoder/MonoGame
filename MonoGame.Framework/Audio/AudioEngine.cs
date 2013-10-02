#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
namespace Microsoft.Xna.Framework.Audio
{
	public class AudioEngine : IDisposable
	{
		private bool _disposed;
		private AudioContext ac;
		
		public const int ContentVersion = 39;
		
		// default auto-initialized currentInstance. 
		// needs this to fit XNA usage mode with OpenAL.
		private static AudioEngine currentInstance;
		
		internal static AudioEngine CurrentInstance
		{
			get 
			{
				if(currentInstance == null)
				{
					// first instance automatically sets current instance.
					new AudioEngine(null);
				}
				return currentInstance;
			}
			
			set
			{
				currentInstance = value;
			}
			
		}
		
		internal static void EnsureInit()
		{
			var instance = CurrentInstance;	
			if(instance == null)
			{
				throw new Exception("Unknown error");
			}
		}
		
		
		private void Init()
		{
			ac = new AudioContext();
			
			ALError error = AL.GetError();
			if (error != ALError.NoError)
			{
				throw new OpenALException(error, "borked audio context init. ALError: " + error.ToString());
			}

			_disposed = false;
			
/*			Source = AL.GenSource();
			Buffer = AL.GenBuffer(); 
			
			error = AL.GetError();
			if (error != ALError.NoError)
			{
				throw new OpenALException(error, "borked generation. ALError: " + error.ToString());
			}*/
			
			
			if(currentInstance == null)
			{
				currentInstance = this;	
			}
		}
		
		private void Cleanup()
		{
			ac.Dispose();
			ac = null;
		}
		
		internal Dictionary<string, WaveBank> Wavebanks = new Dictionary<string, WaveBank>();
 		
		public AudioEngine (string settingsFile)
		{
			Init();
		}
		
		public AudioEngine (string settingsFile, TimeSpan lookAheadTime, string rendererId)
		{
			Init();
		}
		
		#region IDisposable implementation
	    public void Dispose() 
	    {
	        Dispose(true);
	
	        // Use SupressFinalize in case a subclass
	        // of this type implements a finalizer.
	        GC.SuppressFinalize(this);      
	    }
	
	    protected virtual void Dispose(bool disposing)
	    {
	        // If you need thread safety, use a lock around these 
	        // operations, as well as in your methods that use the resource.
	        if (!_disposed)
	        {
	            if (disposing) {
					Cleanup();
	            }
	
	            // Indicate that the instance has been disposed.
	            _disposed = true;
				
				if(currentInstance == this)
				{
					currentInstance = null;
				}

				// managed resource cleanup here.
	        }
	    }		
		#endregion
	}
}

