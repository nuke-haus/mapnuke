using UnityEngine;

/*
 * 
 * To add a new hotkey
 * 1)  Add KeyCode name (1001F)
 * 3)  Add to enum (1001F)
 * 3)  Add case (1001G)
 * 4)  Connect to function via Update in script
 * 
 */


[CreateAssetMenu(fileName = "GameKeybinds", menuName = "GameKeybinds")]
public class Keybindings : ScriptableObject
{
    //1001F
    public KeyCode toggle_road, toggle_mountain, toggle_mountain_pass, toggle_river, toggle_normal, generate_map;
    public enum ActionEnum { TOGGLEROAD, TOGGLEMOUNTAIN, TOGGLEMOUNTAINPASS, TOGGLERIVER, TOGGLESTANDARD, GENERATEMAP };

    //1001G
    public KeyCode CheckKey(ActionEnum key)
    {

        switch(key)
        {
            case ActionEnum.TOGGLEROAD:
                return toggle_road;
            case ActionEnum.TOGGLEMOUNTAIN:
                return toggle_mountain;
            case ActionEnum.TOGGLEMOUNTAINPASS:
                return toggle_mountain_pass;
            case ActionEnum.TOGGLERIVER:
                return toggle_river;
            case ActionEnum.TOGGLESTANDARD:
                return toggle_normal;
            case ActionEnum.GENERATEMAP:
                return generate_map;
            default:
                return KeyCode.None;

        }

    }
}
