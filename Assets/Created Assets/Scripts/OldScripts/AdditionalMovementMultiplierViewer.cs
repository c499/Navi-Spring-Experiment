using UnityEngine;
using UnityEngine.UI;

public class AdditionalMovementMultiplierViewer : MonoBehaviour
{
    #region Fields

    public bool displayMultiplier = false;
    public int fontSize = 32;
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
    public Color fullMultiplier = Color.green;
    public Color adjustingMultiplier = Color.yellow;
    public Color zeroMultiplier = Color.red;

    protected VRTK_RoomExtender roomExtender;
    private const float updateInterval = 0.5f;
    private Text text;

    #endregion Fields

    #region Methods

    private void Start()
    {
        roomExtender = FindObjectOfType<VRTK_RoomExtender>();

        text = this.GetComponent<Text>();
        text.fontSize = fontSize;
        text.transform.localPosition = position;
        text.transform.localRotation = rotation;
    }

    private void Update()
    {
        if (text != null)
        {
            if (displayMultiplier)
            {
                float multiplier = roomExtender.additionalMovementMultiplier;
                float defaultMultiplier = roomExtender.defaultAdditionalMovementMultiplier;
                float noMultiplier = 0;

                text.text = System.String.Format("{0:F2} Gain", multiplier);
                text.color = (multiplier > (defaultMultiplier - 0.01) ? fullMultiplier :
                (multiplier > (noMultiplier) ? adjustingMultiplier :
                zeroMultiplier));
            }
            else
            {
                text.text = "";
            }
        }
    }

    #endregion Methods
}