// This script is a Manager that controls the the flow and control of the game. It keeps
// track of player data (orb count, death count, total game time) and interfaces with
// the UI Manager. All game commands are issued through the static methods of this class

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    //This class holds a static reference to itself to ensure that there will only be
    //one in existence. This is often referred to as a "singleton" design pattern. Other
    //scripts access this one through its public static methods
    public static EASYFader sceneFader;
    public static Coroutine audioSwitchWorldCoroutine;

    public float deathSequenceDuration = 1.5f;      //How long player death takes before restarting
    public float shadowWorldDuration = 10f;         //How long player can to be into the shadow world
    public float switchTilesDelay = .05f;           //How much time need to switch tiles one world to another

    public static bool inTheShadowWorld;            //Switch between shadow and physic worlds

    static GameManager current;
    static GameObject tiles;
    static GameObject shadowWorldTiles;
    static Tilemap tilesTilemap;
    static Tilemap shadowWorldTilesTilemap;
    static SpriteRenderer physicWorldPlatformsSprites; //доделать
    static SpriteRenderer shadowWorldPlatformsSprites;

    float _totalGameTime;						    //Length of the total game time
    float _shadowWorldDuration;
    float _shadowAbsorption;                        //When it's equals 100 - shadows killing the player
    float _shadowAbsorptionDecreaseDelay = 1f;
    float _alphaTilemapPW = 1f;
    float _alphaTilemapSW = 1f;

    bool isGameOver;							    //Is the game currently over?

    static Coroutine shadowWorldCoroutine;
    static Coroutine absorptionDecreaseCoroutine;
    static Coroutine smoothSwitchingCoroutine;

    PlayerMovement player;

    PostProcessVolume globalPostProcess;
    Vignette vignetteLayer = null;
    ColorGrading colorGradingLayer = null;
    LensDistortion lensDistortion = null;
    GameObject[] lightingObjects;
    SFLight[] sfLights;
    

    void Awake()
    {
        //If a Game Manager exists and this isn't it...
        if (current != null && current != this)
        {
            //...destroy this and exit. There can only be one Game Manager
            Destroy(gameObject);
            return;
        }

        //Set this as the current game manager
        current = this;

        //Persis this object between scene reloads
        DontDestroyOnLoad(gameObject);

        OnEnabled();
    }

    //Do something when scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetComponent();
        //Return to physic world
        if (inTheShadowWorld)
            SwitchWorld();

        current._shadowAbsorption = 0f;

        //Debug.Log("Level Loaded");
        //Debug.Log(scene.name);
        //Debug.Log(mode);
    }
    //Use when need load to scene
    void OnEnabled()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    //Use when need unload the scene
    void OnDesabled()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        //If the game is over, exit
        if (isGameOver)
            return;

        //Update the total game time and tell the UI Manager to update
        _totalGameTime += Time.deltaTime;
        //UIManager.UpdateTimeUI(_totalGameTime);

        //Update vignette and other effect for shadow world
        ShadowPostProcessing();

        if (PlayerInput.switchWorld)
            SwitchWorld();

        if (PlayerInput.restart)
            RestartScene(); //later need to get scene id or name
    }

    void GetComponent()
    {
        globalPostProcess = GameObject.Find("Global Post Processing").GetComponent<PostProcessVolume>();
        globalPostProcess.profile.TryGetSettings(out vignetteLayer);
        globalPostProcess.profile.TryGetSettings(out colorGradingLayer);
        globalPostProcess.profile.TryGetSettings(out lensDistortion);

        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
        tiles = GameObject.FindGameObjectWithTag("Platforms");
        shadowWorldTiles = GameObject.FindGameObjectWithTag("Shadow World Platforms");
        tilesTilemap = tiles.GetComponent<Tilemap>();
        shadowWorldTilesTilemap = shadowWorldTiles.GetComponent<Tilemap>();
        /*
        lightingObjects = GameObject.FindGameObjectsWithTag("Lighting");

        //Может быть понадобится
        
        sfLights = new SFLight[lightingObjects.Length];

        for (int i = 0; i < lightingObjects.Length; i++)
        {
            sfLights[i] = lightingObjects[i].GetComponent<SFLight>();
        }*/
    }

    public static void RegisterSceneFader(EASYFader fader)
    {
        //If there is no current Game Manager, exit
        if (current == null)
            return;

        //Record the scene fader reference
        sceneFader = fader;
    }

    public static bool IsGameOver()
	{
		//If there is no current Game Manager, return false
		if (current == null)
			return false;

		//Return the state of the game
		return current.isGameOver;
	}

	public static void PlayerDied()
	{
		//If there is no current Game Manager, exit
		if (current == null)
			return;

		//Invoke the RestartScene() method after a delay
		current.Invoke("RestartScene", current.deathSequenceDuration);
	}

    public static void SwitchWorld()
    {
        inTheShadowWorld = !inTheShadowWorld;

        if (audioSwitchWorldCoroutine != null)
            current.StopCoroutine(audioSwitchWorldCoroutine);

        if (smoothSwitchingCoroutine != null)
            current.StopCoroutine(smoothSwitchingCoroutine);

        smoothSwitchingCoroutine = current.StartCoroutine(SmoothSwitchingTiles(inTheShadowWorld));
        audioSwitchWorldCoroutine = current.StartCoroutine(AudioManager.PlayAmbientShadowWorld());
        PlayerInput.switchWorld = false;                        //Без этого эта поебала нормально не работает

        if (!inTheShadowWorld)
        {
            absorptionDecreaseCoroutine = current.StartCoroutine(ShadowAbsorptionDecrease(Time.time + current._shadowAbsorptionDecreaseDelay));

            return;
        }

        //Current max stay in shadow world
        current._shadowWorldDuration = Time.time + current.shadowWorldDuration - current._shadowAbsorption;

        shadowWorldCoroutine = current.StartCoroutine(ShadowWorldStaying(Time.time + current.shadowWorldDuration - current._shadowAbsorption));
    }

    static void ShadowPostProcessing()
    {
        current.vignetteLayer.intensity.value = (current._shadowAbsorption / current.shadowWorldDuration) / 2f;
        current.lensDistortion.intensity.value = (current._shadowAbsorption / current.shadowWorldDuration) * -40f;

        //Change tilemaps alpha 
        tilesTilemap.color = new Color(1f, 1f, 1f, current._alphaTilemapPW);
        shadowWorldTilesTilemap.color = new Color(1f, 1f, 1f, current._alphaTilemapSW);
    }

    //Delay after which vignette value is decrease
    static IEnumerator ShadowAbsorptionDecrease(float delay)
    {
        if (shadowWorldCoroutine != null)
            current.StopCoroutine(shadowWorldCoroutine);

        //Screen fader
        if (sceneFader != null)
        {
            sceneFader.value = 1f;
            sceneFader.DoFadeIn();
        }

        //Может быть понадобится
        /*
        //Switch lighing
        for (int i = 0; i < current.sfLights.Length; i++)
            current.sfLights[i].shadowLayers = 1 << 9;
        */
        //Return colors and audio
        while (current.colorGradingLayer.saturation.value < 0)
        {
            yield return new WaitForEndOfFrame();

            current.colorGradingLayer.saturation.value = Mathf.Clamp(current.colorGradingLayer.saturation.value + 1f, -100f, 0f);
        }

        while (current._shadowAbsorption > 0f)
        {
            if (delay <= Time.time)
            {
                yield return new WaitForEndOfFrame();

                current._shadowAbsorption = Mathf.Clamp(current._shadowAbsorption - Time.deltaTime, 0f, current.shadowWorldDuration);
            }
            else
                yield return new WaitForEndOfFrame();
        }
    }

    //Increase shadow absorption when player into the shadow world
    static IEnumerator ShadowWorldStaying(float maxDuration)
    {
        if (absorptionDecreaseCoroutine != null)
            current.StopCoroutine(absorptionDecreaseCoroutine);

        //Screen fader
        if (sceneFader != null)
        {
            sceneFader.value = 1f;
            sceneFader.DoFadeIn();
        }
        //Может быть понадобится
        /*
        //Switch lighing
        for(int i = 0; i < current.sfLights.Length; i++)
            current.sfLights[i].shadowLayers = 1 << 15;
        */
        while (maxDuration > Time.time)
        {
            yield return new WaitForEndOfFrame();

            current._shadowAbsorption += Time.deltaTime;            
            current.colorGradingLayer.saturation.value = Mathf.Clamp(current.colorGradingLayer.saturation.value - 4f, -100f, 100f);

            AudioManager.ambientShadowWorldSource.volume = Mathf.Clamp(current._shadowAbsorption / current.shadowWorldDuration, 0f, 1f);

            if (AudioManager.ambientSource.volume > 0)
                AudioManager.ambientSource.volume -= .05f;
            else
                AudioManager.ambientSource.Pause();
        }

        current.player.isDead = true;
    }

    static IEnumerator SmoothSwitchingTiles(bool shadowWorld)
    {
        if (shadowWorld)
        {
            //Turn off/on collision
            Physics2D.IgnoreLayerCollision(10, 15, false);      //10 - Player layer, 15 - Shadow World Platforms layer
            Physics2D.IgnoreLayerCollision(10, 9);              //10 - Player layer, 9 - Platforms layer
            current.player.groundLayer = 1 << 15;               //15 - Shadow World Platforms layer

            current._alphaTilemapPW = 1f;
            current._alphaTilemapSW = 0f;
            shadowWorldTilesTilemap.color = new Color(1f, 1f, 1f, current._alphaTilemapSW);     //Need here too because the method does not have time to call from the update

            //Smooth switching tiles
            while (shadowWorldTilesTilemap.color.a < 1f)
            {
                yield return new WaitForEndOfFrame();
                current._alphaTilemapPW -= current.switchTilesDelay;
                current._alphaTilemapSW += current.switchTilesDelay;
            }
        }
        else
        {
            //Turn off/on collision
            Physics2D.IgnoreLayerCollision(10, 9, false);       //10 - Player layer, 9 - Platforms layer
            Physics2D.IgnoreLayerCollision(10, 15);             //10 - Player layer, 15 - Shadow World Platforms layer
            current.player.groundLayer = 1 << 9;                //9 - Platfroms layer

            current._alphaTilemapPW = 0f;
            current._alphaTilemapSW = 1f;
            tilesTilemap.color = new Color(1f, 1f, 1f, current._alphaTilemapPW);

            //Smooth switching tiles
            while (tilesTilemap.color.a < 1f)
            {
                yield return new WaitForEndOfFrame();
                current._alphaTilemapPW += current.switchTilesDelay;
                current._alphaTilemapSW -= current.switchTilesDelay;
            }
        }
    }

    //public static void PlayerWon()
    //{
    //	//If there is no current Game Manager, exit
    //	if (current == null)
    //		return;

    //	//The game is now over
    //	current.isGameOver = true;

    //	//Tell UI Manager to show the game over text and tell the Audio Manager to play
    //	//game over audio
    //	UIManager.DisplayGameOverText();
    //	AudioManager.PlayWonAudio();
    //}

    void RestartScene()
	{
        //Play the scene restart audio
        //AudioManager.PlaySceneRestartAudio();

        PlayerInput.restart = false;
        //Reload the current scene
        OnDesabled();
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        OnEnabled();
	}
}
