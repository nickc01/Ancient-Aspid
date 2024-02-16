/*using UnityEngine;
/// <summary>
/// Attacks enemies via raycasting, rather than using collisions. Can be used to attack enemies that only have triggers attached
/// </summary>
public class DamageScanner : MonoBehaviour
{
    [field: SerializeField]
    public LayerMask AttackLayers;

    [field: SerializeField]
    bool attackTriggers;

    private void Reset()
    {
        AttackLayers = LayerMask.GetMask("Enemies");
    }
}*/
