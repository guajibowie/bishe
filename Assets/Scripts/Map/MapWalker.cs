using UnityEngine;

public class MapWalker
{
    public Vector2 _position;
    public Vector2 _direction;
    public float _chanceToChange;
    public MapWalker(Vector2 position,Vector2 direction,float chanceToChange)
    {
        _position = position;
        _direction = direction;
        _chanceToChange = chanceToChange;
    }
    public bool Change()
    {
        return UnityEngine.Random.value < _chanceToChange;
    }

    public void UpdatePosition()
    {
        _position += _direction; 
    }

}
