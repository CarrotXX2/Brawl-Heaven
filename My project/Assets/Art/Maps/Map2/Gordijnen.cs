using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class BlendShapeLooperWithCooldown : MonoBehaviour
{
    [Header("BlendShape Settings")]
    [Tooltip("Index of the blend shape to animate")]
    public int blendShapeIndex = 0;

    [Header("Animation Settings")]
    [Tooltip("Duration of the animation in seconds")]
    public float animationDuration = 2f;
    [Tooltip("Duration of cooldown between animations in seconds")]
    public float cooldownDuration = 1f;
    [Tooltip("Controls the acceleration (1 = linear)")]
    [Range(0.1f, 5f)] public float curvePower = 2f;
    [Tooltip("Smoothness of the transition")]
    [Range(0.1f, 3f)] public float smoothness = 1f;

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private float cycleTimer;
    private bool isAnimating;
    private AnimationCurve animationCurve;

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        CreateAnimationCurve();
        isAnimating = true;
        cycleTimer = 0f;
    }

    void Update()
    {
        cycleTimer += Time.deltaTime;

        if (isAnimating)
        {
            // Animation phase
            if (cycleTimer <= animationDuration)
            {
                float normalizedTime = cycleTimer / animationDuration;
                float weight = animationCurve.Evaluate(normalizedTime);
                skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, weight * 100f);
            }
            else
            {
                // Transition to cooldown
                isAnimating = false;
                cycleTimer = 0f;
                skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, 0f);
            }
        }
        else
        {
            // Cooldown phase
            if (cycleTimer >= cooldownDuration)
            {
                // Transition back to animation
                isAnimating = true;
                cycleTimer = 0f;
            }
        }
    }

    void CreateAnimationCurve()
    {
        animationCurve = new AnimationCurve();

        // Start at 0
        animationCurve.AddKey(0f, 0f);

        // Peak at middle of animation duration
        animationCurve.AddKey(0.5f, 1f);

        // End back at 0
        animationCurve.AddKey(1f, 0f);

        // Adjust curve handles for smoothness and shape
        for (int i = 0; i < animationCurve.length; i++)
        {
            Keyframe key = animationCurve[i];

            if (i == 1) // At the peak keyframe
            {
                key.inTangent = smoothness * curvePower;
                key.outTangent = -smoothness * curvePower;
            }
            else
            {
                key.inTangent = 0;
                key.outTangent = 0;
            }

            animationCurve.MoveKey(i, key);
        }
    }

}