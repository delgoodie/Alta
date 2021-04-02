using UnityEngine;

public static class Chips
{
    public static string[] Names = new string[25]{
    "Air",
    "Andesite",
    "Basalt",
    "Dacite",
    "Breccia",
    "Amphibolite",
    "Anthracite",
    "Coal",
    "Gneiss",
    "Hornfels",
    "Lapis Lazuli",
    "Marble",
    "Mariposite",
    "Novaculite",
    "Phyllite",
    "Quartzite",
    "Schist",
    "Skarn",
    "Slate",
    "Soapstone",
    "Obsidian",
    "Iron Ore",
    "Diatomite",
    "Charoitite",
    "Chalk"
    };

    public static Color[] Colors = new Color[25]{
        new Color(0f, 0f, 0f),
        new Color(0.411f, 0.435f, 0.435f),
        new Color(0.266f, 0.266f, 0.266f),
        new Color(0.623f, 0.572f, 0.537f),
        new Color(0.501f, 0.321f, 0.270f),
        new Color(0.286f, 0.305f, 0.321f),
        new Color(0.321f, 0.301f, 0.286f),
        new Color(0.219f, 0.227f, 0.223f),
        new Color(0.674f, 0.678f, 0.674f),
        new Color(0.360f, 0.356f, 0.349f),
        new Color(0.313f, 0.313f, 0.529f),
        new Color(0.690f, 0.690f, 0.682f),
        new Color(0.588f, 0.682f, 0.631f),
        new Color(0.741f, 0.737f, 0.717f),
        new Color(0.341f, 0.274f, 0.239f),
        new Color(0.580f, 0.568f, 0.529f),
        new Color(0.549f, 0.462f, 0.360f),
        new Color(0.686f, 0.556f, 0.454f),
        new Color(0.337f, 0.360f, 0.349f),
        new Color(0.400f, 0.423f, 0.403f),
        new Color(0.066f, 0.066f, 0.066f),
        new Color(0.607f, 0.466f, 0.443f),
        new Color(0.862f, 0.862f, 0.862f),
        new Color(0.462f, 0.349f, 0.623f),
        new Color(0.780f, 0.776f, 0.756f)
    };
    public static bool[] Opaque = new bool[5]{
        true,
        true,
        true,
        false,
        false
    };
    public static Object[] Pointer = new Object[5]{
        null,
        null,
        null,
        null,
        null
    };

    public static bool[] PathBlocking = new bool[5]{
        true,
        true,
        true,
        false,
        true,
    };
}
[System.Serializable]
public struct Chip
{
    public byte iso;
    public byte type;
    public ushort data;
    public Chip(byte i, byte t, ushort d)
    {
        iso = i;
        type = t;
        data = d;
    }
}