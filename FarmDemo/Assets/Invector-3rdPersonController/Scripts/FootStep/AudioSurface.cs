using System;
using UnityEngine;
using System.Collections.Generic;
[Serializable]
public class AudioSurface
{
	public string name;
	public AudioSource source;                  // The AudioSource that will play the clips.
    public List<string> TextureNames;           // The tag on the surfaces that play these sounds.
    public List<AudioClip> audioClips;          // The different clips that can be played on this surface.    

    private FisherYatesRandom randomSource = new FisherYatesRandom();       // For randomly reordering clips.

    public void PlayRandomClip()
    {
        // If there are no clips to play return.
        if (audioClips == null || audioClips.Count == 0)
            return;

		// initialize variable if not already started
		if (randomSource == null) 		
			randomSource = new FisherYatesRandom();

        // Find a random clip and play it.
        int index = randomSource.Next(audioClips.Count);
        source.PlayOneShot(audioClips[index]);
    }
}