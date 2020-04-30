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
    public KeyCode toggleRoad, toggleMountain, toggleMountainPass, toggleRiver, toggleNormal, generateMap;
    public enum ActionEnum { toggleRoad, toggleMountain, toggleMountainPass, toggleRiver, toggleStandard, generateMap };

    //1001G
    public KeyCode CheckKey(ActionEnum key)
    {

        switch(key)
        {
            case ActionEnum.toggleRoad:
                return toggleRoad;
            case ActionEnum.toggleMountain:
                return toggleMountain;
            case ActionEnum.toggleMountainPass:
                return toggleMountainPass;
            case ActionEnum.toggleRiver:
                return toggleRiver;
            case ActionEnum.toggleStandard:
                return toggleNormal;
            case ActionEnum.generateMap:
                return generateMap;
            default:
                return KeyCode.None;

        }

    }
}
