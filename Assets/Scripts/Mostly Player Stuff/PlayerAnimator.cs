using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Movement movement;
    public Animator anim;
    private void Update()
    {
        //Movement Anim
        float movespeed = Mathf.Abs(movement.moveInput.x);
        anim.SetFloat("WalkSpeed", movespeed);
    }
}
