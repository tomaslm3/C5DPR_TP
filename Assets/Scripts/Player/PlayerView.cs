using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkMecanimAnimator))]
public class PlayerView : MonoBehaviour
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    private NetworkMecanimAnimator _mecanim;

    private void Awake()
    {
        _mecanim = GetComponent<NetworkMecanimAnimator>();
    }

    public void SetMovementParameter(Vector2 movement)
    {
        _mecanim.Animator.SetFloat(MoveX, movement.x);
        _mecanim.Animator.SetFloat(MoveY, movement.y);
        _mecanim.Animator.SetBool(IsMoving, movement.sqrMagnitude > 0.01f);
    }

    public void PlayShoot()
    {
        _mecanim.Animator.SetTrigger(Shoot);
    }
}