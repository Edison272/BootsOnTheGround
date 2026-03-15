using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level");
        //SceneManager.LoadScene("CoreLevel", LoadSceneMode.Additive);


        // SceneController.SCENE_CONTROLLER
        //     .NewTransition()
        //     .Load(SceneDatabase.Slots.Level, SceneDatabase.Scenes.Level)
        //     .Unload(SceneDatabase.Slots.Menu)
        //     .WithOverlay()
        //     .WithClearUnusedAssets()
        //     .Perform();
    }

    public void Settings()
    {
        
    }
}
