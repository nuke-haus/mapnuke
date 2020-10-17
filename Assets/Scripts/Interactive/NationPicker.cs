
using UnityEngine;
using UnityEngine.UI;

public class NationPicker : MonoBehaviour
{
    public Dropdown NationDropdown;
    public Dropdown TeamDropdown;

    public string NationName
    {
        get
        {
            return NationDropdown.options[NationDropdown.value].text;
        }
    }

    public int TeamNum
    {
        get
        {
            var i = 1;
            var text = TeamDropdown.options[TeamDropdown.value].text;
            var pass = int.TryParse(text, out i);

            return i;
        }
    }

    public void SetTeamplay(bool b)
    {
        var tf = NationDropdown.GetComponent<RectTransform>();

        TeamDropdown.interactable = b;
        TeamDropdown.gameObject.SetActive(b);

        if (b)
        {
            tf.offsetMax = new Vector2(-70f, -2f);
        }
        else
        {
            tf.offsetMax = new Vector2(-2f, -2f);
        }
    }

    public void Initialize()
    {
        var block = NationDropdown.colors;
        block.normalColor = new Color(0.4f, 0.8f, 0.5f);
        block.highlightedColor = new Color(0.4f, 1.0f, 0.4f);
        NationDropdown.colors = block;

        block = TeamDropdown.colors;
        block.normalColor = new Color(0.5f, 0.7f, 0.9f);
        block.highlightedColor = new Color(0.5f, 0.9f, 1.0f);
        TeamDropdown.colors = block;
    }
}
