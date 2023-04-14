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
                animationToPlay = "Sword Walk";
                break;
            case Animation.Idle:
                animationToPlay = "Sword Stance";
                break;
            case Animation.Default:
                break;
            default:
                break;
        }

        StartAnimation(unit, animationToPlay);
    }

    static void StartAnimation(GameObject unit, string animation)
    {
        string gender = unit.name.Split(' ')[0];
        unit.GetComponent<Animator>().Play($"{gender} {animation}");
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
