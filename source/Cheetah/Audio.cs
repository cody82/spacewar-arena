using System.IO;
using System;
using OpenTK.Audio;
using Cheetah;

namespace Cheetah.Audio
{
	
	

    public class OpenTkSound : Sound
    {
        public OpenTkSound()
            : base(null)
        {
        }
    }
	
    public class OpenTkChannel : Channel
    {
    }

    public class OpenTkAudio : IAudio
    {
		AudioContext context;
		
		public OpenTkAudio()
		{
			context=new AudioContext();
			AudioReader sound = new AudioReader("audio/pulselaser.wav");
			
	int MyBuffers;
AL.GenBuffers( 1, out MyBuffers );

		
			AL.BufferData(MyBuffers, sound.ReadToEnd());
			if ( AL.GetError() != ALError.NoError )
			{
				throw new Exception();
			}
			
			
			uint MySources;
AL.GenSources( 1, out MySources ); // gen 2 Source Handles

AL.Source( MySources, ALSourcei.Buffer, MyBuffers ); // attach the buffer to a source

AL.SourcePlay( MySources); // start playback
AL.Source( MySources, ALSourceb.Looping, true ); // source loops infinitely


		}
		
        #region IAudio Members

        public Sound Load(Stream s)
        {
            return new OpenTkSound();
        }

        public Channel Play(Sound sound, Vector3 pos,bool loop)
        {
            return new OpenTkChannel();
        }
        public bool IsPlaying(Channel channel)
        {
            return true;
        }

        public void SetListener(Vector3 pos, Vector3 forward, Vector3 up)
        {
        }

        public void SetPosition(Channel channel, Vector3 pos)
        {
        }

        public void Stop(Channel channel)
        {
        }

        public void Free(Sound sound)
        {
        }

        #endregion

        #region ITickable Members

        public void Tick(float dtime)
        {
        }

        #endregion

        public void Dispose()
        {
        }
    }

}
