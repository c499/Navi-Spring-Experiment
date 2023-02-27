using UnityEngine;

public class BallFade : MonoBehaviour
{
    #region Fields

    protected EventManager eventManager;

    #endregion Fields

    #region Methods

    // Use this for initialization
    private void Start()
    {
        eventManager = FindObjectOfType<EventManager>();
    }


    /// <summary>
    /// Causes any object with the tag "ball" to be deactivated on any collision with a collider this script is attached to.
    /// </summary>
    /// <param name='other'>
    /// Collider of gameObject "other".
    /// </param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            other.gameObject.SetActive(false);
            eventManager.GetComponent<InputVCR>().SyncProperty("Ball has been dropped on: ", transform.name);
        }
    }

    #endregion Methods
}