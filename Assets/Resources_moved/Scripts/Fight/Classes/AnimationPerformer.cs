using UnityEngine;

public static class AnimationPerformer
{
    public static void PerformAnimation(Animation animation, GameObject obj)
    {
        if (!obj.TryGetComponent<Animator>(out var animator))
            return;

		string animationToPlay = "";

        switch (animation)
        {
            case Animation.Attack:
                animationToPlay = "Attack 1";
                break;
            case Animation.TakeDamage:
                animationToPlay = "Damage Light";
                break;
			case Animation.Die:
				animationToPlay = "Die";
				break;
			case Animation.Move:
                animationToPlay = "Walk";
                break;
            case Animation.Idle:
                animationToPlay = "Idle";
                break;
            case Animation.ShowcaseIdle:
                animationToPlay = "Showcase Idle";
                break;
			case Animation.ShowcaseAttack:
				animationToPlay = "Showcase Attack";
				break;
			case Animation.Default:
                break;
            default:
                break;
        }

		animator.Play(animationToPlay);
    }
}

public enum Animation
{
    Attack,
    TakeDamage,
    Die,
    Move,
    Idle,
    ShowcaseIdle,
    ShowcaseAttack,
    Default
}
