using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract void Fire();
    public abstract void Reload();
    public abstract void AimIn();
    public abstract void AimOut();
    public abstract void View();

}
