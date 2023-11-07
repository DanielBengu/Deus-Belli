using UnityEngine;

public static class AnimationPerformer
{
    public static void PerformAnimation(Animation animation, GameObject unit)
    {
        string animationToPlay = "";

        switch (animation)
        {
            case Animation.Attack:
                animationToPlay = "Attack 1";
                break;
            case Animation.TakeDamage:
                animationToPlay = "Damage Light";
                break;
            case Animation.Move:
                animationToPlay = "Walk";
                break;
            case Animation.Idle:
                animationToPlay = "Idle";
                break;
            case Animation.Default:
                break;
            default:
                break;
        }

        unit.GetComponent<Animator>().Play(animationToPlay);
    }
}

public enum Animation
{
    Attack,
    TakeDamage,
    Move,
    Idle,
    Default
}
