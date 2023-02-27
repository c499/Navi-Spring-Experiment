namespace VRTK
{
    using UnityEngine;
    using UnityEngine.UI;

    public class DJVR_ObjectTooltip : MonoBehaviour
    {
        #region Fields

        [TextArea(3, 10)]
        public string displayText;

        public bool useName = false;
        public float objectNumber;
        public int fontSize = 14;
        public Font displayFont;
        public Vector2 containerSize = new Vector2(0.1f, 0.03f);
        public Transform drawLineFrom;
        public Transform drawLineTo;
        public float lineWidth = 0.001f;
        public bool facePlayer = true;

        [Header("Colour Settings")]
        public bool adoptColourFromObject = true;

        public Color fontColor = Color.black;
        public Color containerColor = Color.black;
        public Color lineColor = Color.black;
        private string nameText;
        private LineRenderer line;

        #endregion Fields

        #region Methods

        public void Reset()
        {
            SetContainer();
            SetText("UITextFront");
            SetText("UITextReverse");
            SetLine();
            if (drawLineTo == null && this.transform.parent != null)
            {
                drawLineTo = this.transform.parent;
            }
        }

        public void SetText(string name)
        {
            var tmpText = this.transform.FindChild("TooltipCanvas/" + name).GetComponent<Text>();
            tmpText.material = Resources.Load("UIText") as Material;

            //Debug.Log("Name" + transform.root.GetComponentInParent<Transform>().name);
            string nameText = transform.root.GetComponentInParent<Transform>().name;

            if (useName == true)
            {
                tmpText.text = nameText;
            }
            else
            {
                tmpText.text = displayText;
            }
            tmpText.color = fontColor;
            tmpText.fontSize = fontSize;
            tmpText.font = displayFont;
        }

        private void Start()
        {
            Reset();
            FacePlayer();
        }

        private void SetContainer()
        {
            this.transform.FindChild("TooltipCanvas").GetComponent<RectTransform>().sizeDelta = containerSize;

            var rectTransfrom = this.transform.FindChild("TooltipCanvas").GetComponent<RectTransform>();

            float fixRectTransform = -90;
            rectTransfrom.rotation = Quaternion.Euler(new Vector3(rectTransfrom.rotation.x, fixRectTransform, rectTransfrom.rotation.z));

            var tmpContainer = this.transform.FindChild("TooltipCanvas/UIContainer");
            tmpContainer.GetComponent<Image>().material = Resources.Load("Valve Bright") as Material;
            tmpContainer.GetComponent<RectTransform>().sizeDelta = containerSize;

            if (adoptColourFromObject)
            {
                tmpContainer.GetComponent<Image>().material.color = transform.GetComponentInParent<Renderer>().material.color;
            }
            else
            {
                tmpContainer.GetComponent<Image>().material.color = containerColor;
            }

            this.transform.FindChild("TooltipCanvas").GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 20;
        }

        private void SetLine()
        {
            line = transform.FindChild("Line").GetComponent<LineRenderer>();
            line.material = Resources.Load("TooltipLine") as Material;

            if (adoptColourFromObject)
            {
                line.material.color = transform.GetComponentInParent<Renderer>().material.color;
                line.SetColors(transform.GetComponentInParent<Renderer>().material.color, transform.GetComponentInParent<Renderer>().material.color);
            }
            else
            {
                line.material.color = lineColor;
                line.SetColors(lineColor, lineColor);
            }
            line.SetWidth(lineWidth, lineWidth);

            if (drawLineFrom == null)
            {
                drawLineFrom = transform;
            }
        }

        private void DrawLine()
        {
            if (drawLineTo)
            {
                line.SetPosition(0, drawLineFrom.position);
                line.SetPosition(1, drawLineTo.position);
            }
        }

        private void Update()
        {
            DrawLine();
            FacePlayer();
        }

        private void FacePlayer()
        {
            if (facePlayer)
            {
                Vector3 camPos = Camera.main.transform.position;

                transform.LookAt(
                    new Vector3(0, camPos.y, 0)
                    );

                
            }
        }

        #endregion Methods
    }
}