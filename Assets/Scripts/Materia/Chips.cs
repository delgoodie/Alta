using UnityEngine;

public static class Chips
{
    public const byte Air = 0x00;
    #region OPAQUE
    public const byte Dirt = 0x01;
    public const byte Rock = 0x02;
    public const byte Grass = 0x03;
    #endregion
    #region TRANSPARENT
    public const byte Grey = 0x04;
    #endregion
    public static Color[] Colors = new Color[5]{
        Color.clear,
        new Color(0.337f, 0.176f, 0.023f),
        Color.black,
        new Color(0.058f, 0.560f, 0.435f),
        Color.grey,
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