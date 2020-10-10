using UnityEngine;
public class FireCell
{
    public FireSettings settings;
    public Vector3 position;
    public float radius;
    public FireState state;

    private float _currentTemperature;
    private float _burningTime;

    public FireCell()
    {
        state = FireState.Normal;
        _currentTemperature = 0;
        _burningTime = 0;
    }

    public bool HasReachedBurningTemperature()
    {
        if(state == FireState.OnFire)
        {
            return false;
        }
        _currentTemperature += settings.temperatureChangePerFrame;
        if(_currentTemperature >= settings.initialBurningTemperature)
        {
            state = FireState.OnFire;
            return true;
        }
        return false;
    }

    public bool IsStillBurning(float timeElapsed)
    {
        if(state == FireState.BurntDown || state == FireState.Normal)
        {
            return true;
        }
        _burningTime += timeElapsed;
        if(_burningTime >= settings.burningTime)
        {
            state = FireState.BurntDown;
            return false;
        }
        return true;
    }
}
