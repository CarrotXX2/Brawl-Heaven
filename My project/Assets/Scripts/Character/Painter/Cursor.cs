using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private GameObject cursorParticle;

    public void ActivateParticle()
    {
        cursorParticle.gameObject.SetActive(true);
    }

    public void DeactivateParticle()
    {
        cursorParticle.gameObject.SetActive(false);
    }
}
