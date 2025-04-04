using System.Collections;
using UnityEngine;

public class DelayedActivator : MonoBehaviour
{
    public GameObject objectToActivate;
    public GameObject objectToDeactivate;
    public float delay = 0.5f;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(ActivateThenDeactivate());
            print("Test 2");
        }
    }

    public void ActivateAndDeactivateWithDelay()
    {
        CoroutineRunner.Instance.RunCoroutine(ActivateThenDeactivate());
    }
    
    private IEnumerator ActivateThenDeactivate()
    {
        yield return new WaitForSeconds(delay);

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }
    }
}
