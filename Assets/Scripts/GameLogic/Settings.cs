using UnityEngine;
using UnityEngine.UI;

public class Settings : ScriptableObject
{
    //Requirements
    private float _yellowFillRequirement;
    public float YellowFillRequirement { get { return _yellowFillRequirement; } set { _yellowFillRequirement = value; } }

    private float _blueFillRequirement;
    public float BlueFillRequirement { get { return _blueFillRequirement; } set { _blueFillRequirement = value; } }

    private float _greenFillRequirement;
    public float GreenFillRequirement { get { return _greenFillRequirement; } set { _greenFillRequirement = value; } }

    private float _redFillRequirement;
    public float RedFillRequirement { get { return _redFillRequirement; } set { _redFillRequirement = value; } }

    //Values
    private float _yellowValue;
    public float YellowValue { get { return _yellowValue; } set { _yellowValue = value; } }

    private float _greenValue;
    public float GreenValue { get { return _greenValue; } set { _greenValue = value; } }

    private float _redValue;
    public float RedValue { get { return _redValue; } set { _redValue = value; } }


    private float _specialityMultiplier;
    public float SpecialityMultiplier { get { return _specialityMultiplier; } set { _specialityMultiplier = value; } }


    private float _playerHealth;
    public float PlayerHealth { get { return _playerHealth; } set { _playerHealth = value; } }

    public void DefaultSettings ()
    {
        //Set default values
        _yellowFillRequirement = 6f;
        _blueFillRequirement = 15f;
        _greenFillRequirement = 6f;
        _redFillRequirement = 15f;

        _yellowValue = 6f;
        _greenValue = 6f;
        _redValue = 15f;

        _specialityMultiplier = 2f;
    }

    public float GetFillRequirementByType (TileTypes.ESubState state)
    {
        float returnValue = 0;
        switch (state)
        {
            case TileTypes.ESubState.yellow:
                returnValue = _yellowFillRequirement;
                break;
            case TileTypes.ESubState.blue:
                returnValue = _blueFillRequirement;
                break;
            case TileTypes.ESubState.green:
                returnValue = _greenFillRequirement;
                break;
            case TileTypes.ESubState.red:
                returnValue = _redFillRequirement;
                break;
        }

        return returnValue;
    }

    public float GetDamageValueByType(TileTypes.ESubState state)
    {
        float returnValue = 0;
        switch (state)
        {
            case TileTypes.ESubState.yellow:
                returnValue = _yellowValue;
                break;
            case TileTypes.ESubState.green:
                returnValue = _greenValue;
                break;
            case TileTypes.ESubState.red:
                returnValue = _redValue;
                break;
        }

        return returnValue;
    }
}