using UnityEngine;
using System.Collections;

public class TileTypes
{
    public enum ESubState { yellow, blue, green, red };

    public virtual string Name { get { return "Default"; } }

    protected ESubState m_type;

    private Color nYellow = new Color(241/255f, 163/255f, 11/255f);
    private Color nBlue = new Color(0/255f, 80/255f, 239/255f);
    private Color nGreen = new Color(96/255f, 170/255f, 23/255f);
    private Color nRed = new Color(229/255f, 20/255f, 0/255f);

    public ESubState Type
    {
        get
        {
            return m_type;
        }
        set
        {
            m_type = value;
        }
    }

    public Color Color
    {
        get
        {
            if (m_type == ESubState.yellow)
                return nYellow;
            else if (m_type == ESubState.blue)
                return nBlue;
            else if (m_type == ESubState.green)
                return nGreen;
            else if (m_type == ESubState.red)
                return nRed;
            return Color.red;
        }
        set
        {
            if (value == Color.yellow || value == nYellow)
                m_type = ESubState.yellow;
            else if (value == Color.blue || value == nBlue)
                m_type = ESubState.blue;
            else if (value == Color.green || value == nGreen)
                m_type = ESubState.green;
            else if (value == Color.red || value == nRed)
                m_type = ESubState.red;
        }
    }

    public Sprite Sprite
    {
        get
        {
            if (m_type == ESubState.yellow)
                return Resources.LoadAll<Sprite>("Images/TilesNormal128x128")[0];
            else if (m_type == ESubState.blue)
                return Resources.LoadAll<Sprite>("Images/TilesNormal128x128")[1];
            else if (m_type == ESubState.green)
                return Resources.LoadAll<Sprite>("Images/TilesNormal128x128")[2];
            else if (m_type == ESubState.red)
                return Resources.LoadAll<Sprite>("Images/TilesNormal128x128")[3];
            return Resources.LoadAll<Sprite>("Images/TilesNormal128x128")[0];
        }
    }

    public Sprite SelectedSprite
    {
        get
        {
            if (m_type == ESubState.yellow)
                return Resources.LoadAll<Sprite>("Images/TilesSelected128x128")[0];
            else if (m_type == ESubState.blue)
                return Resources.LoadAll<Sprite>("Images/TilesSelected128x128")[1];
            else if (m_type == ESubState.green)
                return Resources.LoadAll<Sprite>("Images/TilesSelected128x128")[2];
            else if (m_type == ESubState.red)
                return Resources.LoadAll<Sprite>("Images/TilesSelected128x128")[3];
            return Resources.LoadAll<Sprite>("Images/TilesSelected128x128")[0];
        }
    }

    public Sprite SpecialitySprite
    {
        get
        {
            if (m_type == ESubState.yellow)
                return Resources.LoadAll<Sprite>("Images/TilesSpecial128x128")[0];
            else if (m_type == ESubState.blue)
                return Resources.LoadAll<Sprite>("Images/TilesSpecial128x128")[1];
            else if (m_type == ESubState.green)
                return Resources.LoadAll<Sprite>("Images/TilesSpecial128x128")[2];
            else if (m_type == ESubState.red)
                return Resources.LoadAll<Sprite>("Images/TilesSpecial128x128")[3];
            return Resources.LoadAll<Sprite>("Images/TilesSpecial128x128")[0];
        }
    }

    //Hex
    public Sprite HexSprite
    {
        get
        {
            if (m_type == ESubState.yellow)
                return Resources.LoadAll<Sprite>("Images/TilesHexa128x128")[0];
            else if (m_type == ESubState.blue)
                return Resources.LoadAll<Sprite>("Images/TilesHexa128x128")[1];
            else if (m_type == ESubState.green)
                return Resources.LoadAll<Sprite>("Images/TilesHexa128x128")[2];
            else if (m_type == ESubState.red)
                return Resources.LoadAll<Sprite>("Images/TilesHexa128x128")[3];
            return Resources.LoadAll<Sprite>("Images/TilesHexa128x128")[0];
        }
    }
    public Sprite HexSpriteSelected
    {
        get
        {
            if (m_type == ESubState.yellow)
                return Resources.LoadAll<Sprite>("Images/TilesHexaSelected128x128")[0];
            else if (m_type == ESubState.blue)
                return Resources.LoadAll<Sprite>("Images/TilesHexaSelected128x128")[1];
            else if (m_type == ESubState.green)
                return Resources.LoadAll<Sprite>("Images/TilesHexaSelected128x128")[2];
            else if (m_type == ESubState.red)
                return Resources.LoadAll<Sprite>("Images/TilesHexaSelected128x128")[3];
            return Resources.LoadAll<Sprite>("Images/TilesHexaSelected128x128")[0];
        }
    }
}
