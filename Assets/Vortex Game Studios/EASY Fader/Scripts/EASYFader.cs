/*
*
* EASYFader.shader
* Create a very nice and easy to use screen fade.
*
* Version 1.0.0
*
* Developed by Vortex Game Studios LTDA ME. (http://www.vortexstudios.com)
* Authors:		Alexandre Ribeiro de Sa (@alexribeirodesa)
*
*/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EASYFader : FilterBehavior {
	public enum EASYFaderState {
		Idle = 0,
		In = 1,
		Out = 2
	}
	
	public enum EASYFaderType {
		Default = 0,
		Bright = 1,
		Texture = 2
	}

	public Color color = Color.black;
	public EASYFaderType type = EASYFaderType.Default;
	private EASYFaderState _state = EASYFaderState.Idle;
	public EASYFaderState state {
		get { return _state; }
	}

	public Texture2D texture = null;
	public bool textureFill = true;
	public float textureScale = 1.0f;

	public float interval = 1.0f;

	[Range( 0.0f, 1.0f )]
	public float value = 0.0f;

	public delegate void EASYFaderCallback();
	protected EASYFaderCallback callback_OnFadeInComplete = null;
	protected EASYFaderCallback callback_OnFadeOutComplete = null;

    public void OnFadeInComplete( EASYFaderCallback callback ) {
		callback_OnFadeInComplete = callback;
	}

	public void OnFadeOutComplete( EASYFaderCallback callback ) {
		callback_OnFadeOutComplete = callback;
	}

	void Update() {
        if(GameManager.sceneFader == null)
            GameManager.RegisterSceneFader(this);

        if ( _state != EASYFaderState.Idle ) {
			if ( _state == EASYFaderState.In ) {
				value -= Time.deltaTime / interval;
				if( value < 0.0f ) {
					value = 0.0f;
					_state = EASYFaderState.Idle;

					if ( callback_OnFadeInComplete != null )
						callback_OnFadeInComplete.Invoke();
				}
			} else if ( _state == EASYFaderState.Out ) {
				value += Time.deltaTime / interval;
				if ( value > 1.0f ) {
					value = 1.0f;
					_state = EASYFaderState.Idle;

					if ( callback_OnFadeOutComplete != null )
						callback_OnFadeOutComplete.Invoke();
				}
			}
		}
	}

	public void DoFadeIn() {
		_state = EASYFaderState.In;
	}

	public void DoFadeOut() {
		_state = EASYFaderState.Out;
	}

	void OnRenderImage( RenderTexture source, RenderTexture destination ) {
		if ( value <= 0.0f ) {
			Graphics.Blit( source, destination );
			return;
		}

		this.material.SetColor( "_Color", color );
		this.material.SetTexture( "_Texture", texture );
		this.material.SetFloat( "_TextureFill", System.Convert.ToSingle( textureFill ) );
		this.material.SetFloat( "_TextureAspectX", (float)( source.width / ( texture.width * textureScale ) ) );
		this.material.SetFloat( "_TextureAspectY", (float)( source.height / ( texture.height * textureScale ) ) );
		this.material.SetFloat( "_Type", (float)( type ) );
		this.material.SetFloat( "_Value", value );

		Graphics.Blit( source, destination, this.material );
	}
}
