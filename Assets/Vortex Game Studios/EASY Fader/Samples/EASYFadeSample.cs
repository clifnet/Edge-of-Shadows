/*
*
* EASYFadeSample.cs
*
* Version 1.0.0
*
* Developed by Vortex Game Studios LTDA ME. (http://www.vortexstudios.com)
* Authors:		Alexandre Ribeiro de Sa (@alexribeirodesa)
*
*/

using UnityEngine;
using System.Collections;

public class EASYFadeSample : MonoBehaviour {
	private EASYFader easyFader;

	void Start() {
		easyFader = MonoBehaviour.FindObjectOfType<EASYFader>();
		// Set the EASY Fader events
		easyFader.OnFadeInComplete( this.OnFadeInComplete );
		easyFader.OnFadeOutComplete( this.OnFadeOutComplete );
	}

	public void OnClickFadeIn() {
		easyFader.DoFadeIn();
	}

	public void OnClickFadeOut() {
		easyFader.DoFadeOut();
	}

	public void OnFadeInComplete() {
		Debug.Log( "EASFader Event => Fade-In Completed!" );
	}

	public void OnFadeOutComplete() {
		Debug.Log( "EASFader Event => Fade-Out Completed!" );
	}
}
