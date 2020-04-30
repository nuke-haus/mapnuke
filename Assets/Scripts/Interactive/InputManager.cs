using UnityEngine;

/*
 * 
 * This class should be left alone.
 * Add hotkeys in Keybindings.cs
 * 
 */

public class InputManager : MonoBehaviour
{
    
    public Keybindings keybindings;
    public static InputManager instance;

    private void Awake()
    {
        //Singleton
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }


    public bool KeyDown(Keybindings.ActionEnum key)
    {

        if(Input.GetKeyDown(keybindings.CheckKey(key)))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

}
