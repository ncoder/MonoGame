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
﻿
using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;


namespace Microsoft.Xna.Framework.Audio
{
    public sealed class SoundEffect : IDisposable
    {
		private int buffer;
		private string _name = "";
        internal List<SoundEffectInstance> attachedInstances = new List<SoundEffectInstance>();

		internal SoundEffect(string fileName)
		{
			AudioEngine.EnsureInit();

			buffer = AL.GenBuffer(); 
		
			var error = AL.GetError();
			if (error != ALError.NoError)
			{
				throw new OpenALException(error, "borked generation. ALError: " + error.ToString());
			}

			XRamExtension XRam = new XRamExtension();
			if (XRam.IsInitialized) 
				XRam.SetBufferMode(1, ref buffer, XRamExtension.XRamStorage.Hardware); 
			
			
			
			AudioReader sound = new AudioReader(fileName);
			var sounddata = sound.ReadToEnd();
			AL.BufferData(buffer, sounddata.SoundFormat.SampleFormatAsOpenALFormat, sounddata.Data, sounddata.Data.Length, sounddata.SoundFormat.SampleRate);
			error = AL.GetError();
			if ( error != ALError.NoError )
			{
			   throw new OpenALException(error, "unable to read " + fileName);
			}			
			
			_name = Path.GetFileNameWithoutExtension(fileName);
		}
		
		//SoundEffect from playable audio data
		internal SoundEffect(string name, byte[] data)
		{
			throw new NotImplementedException();
			_name = name;
		}
		
		
		public SoundEffect(byte[] buffer, int sampleRate, AudioChannels channels)
		{
			throw new NotImplementedException();
			_name = "";
		}
		
        public bool Play()
        {				
			return Play(MasterVolume, 0.0f, 0.0f);
        }

        public bool Play(float volume, float pitch, float pan)
        {
			if ( MasterVolume > 0.0f )
			{
				SoundEffectInstance instance = CreateInstance();
				instance.Volume = volume;
				instance.Pitch = pitch;
				instance.Pan = pan;
				instance.Play();
				return true;
			}
			return false;
        }
		
		public TimeSpan Duration 
		{ 
			get
			{
				// todo:
				return new TimeSpan(0);
			}
		}

        public string Name
        {
            get
            {
				return _name;
            }
        }
		
		public SoundEffectInstance CreateInstance ()
		{
			var instance = new SoundEffectInstance(buffer, this);
            attachedInstances.Add (instance);
			return instance;
		}

        internal void UnlinkInstance(SoundEffectInstance instance)
        {
            attachedInstances.Remove(instance);
        }
		
		#region IDisposable Members

        public void Dispose()
        {
            while(attachedInstances.Count != 0)
            {
                attachedInstances[0].Dispose(); // this will come back and call UnlinkInstance, removing items from attachedInstances
            }

			AL.DeleteBuffer(buffer ); // free previously reserved Handles
			
            var error = AL.GetError();
            if (error != ALError.NoError)
            {
                throw new OpenALException(error, "borked dispose. ALError: " + error.ToString());
            }

			buffer = 0;
        }

        #endregion
		
		static float _masterVolume = 1.0f;
		public static float MasterVolume 
		{ 
			get
			{
				return _masterVolume;
			}
			set
			{
				_masterVolume = value;	
			}
		}
    }
}

