using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public AudioClip AudioClick;

    public InputField SwampMin;
    public InputField SwampMax;
    public InputField MountainMin;
    public InputField MountainMax;
    public InputField HighlandMin;
    public InputField HighlandMax;
    public InputField WasteMin;
    public InputField WasteMax;
    public InputField FortMin;
    public InputField FortMax;
    public InputField FarmMin;
    public InputField FarmMax;
    public InputField ForestMin;
    public InputField ForestMax;
    public InputField LakeMin;
    public InputField LakeMax;
    public InputField LargeMin;
    public InputField LargeMax;
    public InputField SmallMin;
    public InputField SmallMax;
    public InputField CaveLakeMin;
    public InputField CaveLakeMax;

    public InputField RoadMin;
    public InputField RoadMax;
    public InputField RiverMin;
    public InputField RiverMax;
    public InputField DeepRiverMin;
    public InputField DeepRiverMax;
    public InputField CliffMin;
    public InputField CliffMax;
    public InputField CliffPassMin;
    public InputField CliffPassMax;
    public InputField ProcNameChance;
    public InputField NumCaveEntrances;
    public InputField UnderworldCaveFreq;

    public Toggle ClassicMountains;

    public Text ProvPercent;
    public Text ConnPercent;
    public Text ProvSizePercent;
    public Text PlainsPercent;
    public Text ConnNormalPercent;
    public Text NormSizePercent;

    public Button AcceptButton;
    private bool m_invalid = false;

    public void Initialize()
    {
        update_textboxes();
        update_labels();
    }

    private void update_textboxes()
    {
        SwampMin.text = GeneratorSettings.s_generator_settings.SwampFreq.MinInt.ToString();
        SwampMax.text = GeneratorSettings.s_generator_settings.SwampFreq.MaxInt.ToString();
        WasteMin.text = GeneratorSettings.s_generator_settings.WasteFreq.MinInt.ToString();
        WasteMax.text = GeneratorSettings.s_generator_settings.WasteFreq.MaxInt.ToString();
        MountainMin.text = GeneratorSettings.s_generator_settings.MountainFreq.MinInt.ToString();
        MountainMax.text = GeneratorSettings.s_generator_settings.MountainFreq.MaxInt.ToString();
        HighlandMin.text = GeneratorSettings.s_generator_settings.HighlandFreq.MinInt.ToString();
        HighlandMax.text = GeneratorSettings.s_generator_settings.HighlandFreq.MaxInt.ToString();
        FortMin.text = GeneratorSettings.s_generator_settings.FortFreq.MinInt.ToString();
        FortMax.text = GeneratorSettings.s_generator_settings.FortFreq.MaxInt.ToString();
        FarmMin.text = GeneratorSettings.s_generator_settings.FarmFreq.MinInt.ToString();
        FarmMax.text = GeneratorSettings.s_generator_settings.FarmFreq.MaxInt.ToString();
        ForestMin.text = GeneratorSettings.s_generator_settings.ForestFreq.MinInt.ToString();
        ForestMax.text = GeneratorSettings.s_generator_settings.ForestFreq.MaxInt.ToString();
        LakeMin.text = GeneratorSettings.s_generator_settings.LakeFreq.MinInt.ToString();
        LakeMax.text = GeneratorSettings.s_generator_settings.LakeFreq.MaxInt.ToString();
        CaveLakeMin.text = GeneratorSettings.s_generator_settings.CaveLakeFreq.MinInt.ToString();
        CaveLakeMax.text = GeneratorSettings.s_generator_settings.CaveLakeFreq.MaxInt.ToString();
        LargeMin.text = GeneratorSettings.s_generator_settings.LargeFreq.MinInt.ToString();
        LargeMax.text = GeneratorSettings.s_generator_settings.LargeFreq.MaxInt.ToString();
        SmallMin.text = GeneratorSettings.s_generator_settings.SmallFreq.MinInt.ToString();
        SmallMax.text = GeneratorSettings.s_generator_settings.SmallFreq.MaxInt.ToString();
        RoadMin.text = GeneratorSettings.s_generator_settings.RoadFreq.MinInt.ToString();
        RoadMax.text = GeneratorSettings.s_generator_settings.RoadFreq.MaxInt.ToString();
        RiverMin.text = GeneratorSettings.s_generator_settings.RiverFreq.MinInt.ToString();
        RiverMax.text = GeneratorSettings.s_generator_settings.RiverFreq.MaxInt.ToString();
        DeepRiverMin.text = GeneratorSettings.s_generator_settings.DeepRiverFreq.MinInt.ToString();
        DeepRiverMax.text = GeneratorSettings.s_generator_settings.DeepRiverFreq.MaxInt.ToString();
        CliffMin.text = GeneratorSettings.s_generator_settings.CliffFreq.MinInt.ToString();
        CliffMax.text = GeneratorSettings.s_generator_settings.CliffFreq.MaxInt.ToString();
        CliffPassMin.text = GeneratorSettings.s_generator_settings.CliffPassFreq.MinInt.ToString();
        CliffPassMax.text = GeneratorSettings.s_generator_settings.CliffPassFreq.MaxInt.ToString();
        ProcNameChance.text = (GeneratorSettings.s_generator_settings.CustomNameFreq * 100).ToString();
        NumCaveEntrances.text = GeneratorSettings.s_generator_settings.NumCaveEntrancesPerPlayer.ToString();
        UnderworldCaveFreq.text = (GeneratorSettings.s_generator_settings.UnderworldCaveFreq * 100).ToString();
    }

    public void ResetSettings()
    {
        GeneratorSettings.s_generator_settings.FortFreq.Reset();
        GeneratorSettings.s_generator_settings.CliffFreq.Reset();
        GeneratorSettings.s_generator_settings.CliffPassFreq.Reset();
        GeneratorSettings.s_generator_settings.DeepRiverFreq.Reset();
        GeneratorSettings.s_generator_settings.FarmFreq.Reset();
        GeneratorSettings.s_generator_settings.ForestFreq.Reset();
        GeneratorSettings.s_generator_settings.HighlandFreq.Reset();
        GeneratorSettings.s_generator_settings.CaveLakeFreq.Reset();
        GeneratorSettings.s_generator_settings.LakeFreq.Reset();
        GeneratorSettings.s_generator_settings.LargeFreq.Reset();
        GeneratorSettings.s_generator_settings.MountainFreq.Reset();
        GeneratorSettings.s_generator_settings.RiverFreq.Reset();
        GeneratorSettings.s_generator_settings.RoadFreq.Reset();
        GeneratorSettings.s_generator_settings.SmallFreq.Reset();
        GeneratorSettings.s_generator_settings.SwampFreq.Reset();
        GeneratorSettings.s_generator_settings.WasteFreq.Reset();
        GeneratorSettings.s_generator_settings.NumCaveEntrancesPerPlayer = 1;
        GeneratorSettings.s_generator_settings.UnderworldCaveFreq = 0.15f;
        GeneratorSettings.s_generator_settings.CustomNameFreq = 0.08f;

        update_textboxes();
        update_labels();
    }

    public void ApplyChanges() // user clicks accept
    {
        GeneratorSettings.s_generator_settings.FortFreq.Update(get_float(FortMin), get_float(FortMax));
        GeneratorSettings.s_generator_settings.CliffFreq.Update(get_float(CliffMin), get_float(CliffMax));
        GeneratorSettings.s_generator_settings.CliffPassFreq.Update(get_float(CliffPassMin), get_float(CliffPassMax));
        GeneratorSettings.s_generator_settings.DeepRiverFreq.Update(get_float(DeepRiverMin), get_float(DeepRiverMax));
        GeneratorSettings.s_generator_settings.FarmFreq.Update(get_float(FarmMin), get_float(FarmMax));
        GeneratorSettings.s_generator_settings.ForestFreq.Update(get_float(ForestMin), get_float(ForestMax));
        GeneratorSettings.s_generator_settings.HighlandFreq.Update(get_float(HighlandMin), get_float(HighlandMax));
        GeneratorSettings.s_generator_settings.LakeFreq.Update(get_float(LakeMin), get_float(LakeMax));
        GeneratorSettings.s_generator_settings.CaveLakeFreq.Update(get_float(CaveLakeMin), get_float(CaveLakeMax));
        GeneratorSettings.s_generator_settings.LargeFreq.Update(get_float(LargeMin), get_float(LargeMax));
        GeneratorSettings.s_generator_settings.MountainFreq.Update(get_float(MountainMin), get_float(MountainMax));
        GeneratorSettings.s_generator_settings.RiverFreq.Update(get_float(RiverMin), get_float(RiverMax));
        GeneratorSettings.s_generator_settings.RoadFreq.Update(get_float(RoadMin), get_float(RoadMax));
        GeneratorSettings.s_generator_settings.SmallFreq.Update(get_float(SmallMin), get_float(SmallMax));
        GeneratorSettings.s_generator_settings.SwampFreq.Update(get_float(SwampMin), get_float(SwampMax));
        GeneratorSettings.s_generator_settings.WasteFreq.Update(get_float(WasteMin), get_float(WasteMax));
        GeneratorSettings.s_generator_settings.CustomNameFreq = get_float(ProcNameChance);
        GeneratorSettings.s_generator_settings.UseClassicMountains = ClassicMountains.isOn;
        GeneratorSettings.s_generator_settings.NumCaveEntrancesPerPlayer = get_int(NumCaveEntrances);
        GeneratorSettings.s_generator_settings.UnderworldCaveFreq = get_float(UnderworldCaveFreq);
        
        gameObject.SetActive(false);
    }

    public void Click()
    {
        GetComponent<AudioSource>().PlayOneShot(AudioClick);
    }

    public void ValidateChanges() // user changed value
    {
        var total_size = get_int(SmallMax) + get_int(LargeMax);

        var total = get_int(SwampMax) +
            get_int(WasteMax) +
            get_int(MountainMax) +
            get_int(HighlandMax) +
            get_int(FarmMax) +
            get_int(ForestMax) +
            get_int(LakeMax);

        var total_conn = get_int(RoadMax) +
            get_int(CliffMax) +
            get_int(CliffPassMax) +
            get_int(RiverMax) +
            get_int(DeepRiverMax);

        var total_plains = 100 - total;
        var total_normal_conn = 100 - total_conn;
        var total_normal_size = 100 - total_size;

        ProvSizePercent.text = "Total: " + total_size + "%";
        ProvPercent.text = "Total: " + total + "%";
        ConnPercent.text = "Total: " + total_conn + "%";
        PlainsPercent.text = "Plains: " + total_plains + "%";
        NormSizePercent.text = "Normal: " + total_normal_size + "%";
        ConnNormalPercent.text = "Normal: " + total_normal_conn + "%";

        m_invalid = false;

        if (total_size > 100)
        {
            ProvSizePercent.color = Color.red;
            m_invalid = true;
        }
        else
        {
            ProvSizePercent.color = Color.blue;
        }

        if (total > 100)
        {
            ProvPercent.color = Color.red;
            m_invalid = true;
        }
        else
        {
            ProvPercent.color = Color.blue;
        }

        if (total_conn > 100)
        {
            ConnPercent.color = Color.red;
            m_invalid = true;
        }
        else
        {
            ConnPercent.color = Color.blue;
        }

        AcceptButton.interactable = !m_invalid;
    }

    private int get_int(InputField f)
    {
        if (f.text == string.Empty)
        {
            f.text = "0";
        }

        var pass = int.TryParse(f.text, out var res);

        if (!pass)
        {
            Debug.LogError("Failed to parse value: " + f.text);
        }

        return res;
    }

    private float get_float(InputField f)
    {
        if (f.text == string.Empty)
        {
            f.text = "0";
        }

        var pass = int.TryParse(f.text, out var res);

        if (!pass)
        {
            Debug.LogError("Failed to parse value: " + f.text);
        }

        if (res > 100)
        {
            res = 100;
        }

        return ((float)res) / 100f;
    }

    private void update_labels()
    {
        var total_size = GeneratorSettings.s_generator_settings.SmallFreq.MaxInt + GeneratorSettings.s_generator_settings.LargeFreq.MaxInt;

        var total = GeneratorSettings.s_generator_settings.SwampFreq.MaxInt +
            GeneratorSettings.s_generator_settings.WasteFreq.MaxInt +
            GeneratorSettings.s_generator_settings.MountainFreq.MaxInt +
            GeneratorSettings.s_generator_settings.HighlandFreq.MaxInt +
            GeneratorSettings.s_generator_settings.FarmFreq.MaxInt +
            GeneratorSettings.s_generator_settings.ForestFreq.MaxInt +
            GeneratorSettings.s_generator_settings.LakeFreq.MaxInt;

        var total_conn = GeneratorSettings.s_generator_settings.RoadFreq.MaxInt +
            GeneratorSettings.s_generator_settings.CliffFreq.MaxInt +
            GeneratorSettings.s_generator_settings.CliffPassFreq.MaxInt +
            GeneratorSettings.s_generator_settings.RiverFreq.MaxInt +
            GeneratorSettings.s_generator_settings.DeepRiverFreq.MaxInt;

        var total_plains = 100 - total;
        var total_normal_conn = 100 - total_conn;
        var total_normal_size = 100 - total_size;

        ProvSizePercent.text = "Total: " + total_size + "%";
        ProvPercent.text = "Total: " + total + "%";
        ConnPercent.text = "Total: " + total_conn + "%";
        PlainsPercent.text = "Plains: " + total_plains + "%";
        NormSizePercent.text = "Normal: " + total_normal_size + "%";
        ConnNormalPercent.text = "Normal: " + total_normal_conn + "%";

        m_invalid = false;

        if (total_size > 100)
        {
            ProvSizePercent.color = Color.red;
            m_invalid = true;
        }
        else
        {
            ProvSizePercent.color = Color.blue;
        }

        if (total > 100)
        {
            ProvPercent.color = Color.red;
            m_invalid = true;
        }
        else
        {
            ProvPercent.color = Color.blue;
        }

        if (total_conn > 100)
        {
            ConnPercent.color = Color.red;
            m_invalid = true;
        }
        else
        {
            ConnPercent.color = Color.blue;
        }

        AcceptButton.interactable = !m_invalid;
    }
}
