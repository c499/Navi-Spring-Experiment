namespace VRTK
{
    using UnityEngine;
    using System.Collections;
    /*
     The concept that the VR user has a physical in game presence which is accomplished by adding a collider and a rigidbody at the position the user is standing within their play area. This physical collider and rigidbody will prevent the user from ever being able to walk through walls or intersect other collidable objects. The height of the collider is determined by the height the user has the headset at, so if the user crouches then the collider shrinks with them, meaning it's possible to crouch and crawl under low ceilings.
The following script parameters are available:
Headset Y Offset: The box collider which is created for the user is set at a height from the user's● headset position. If the collider is required to be lower to allow for room between the play area collider and the headset then this offset value will shorten the height of the generated box collider. Ignore Grabbed Collisions: If this is checked then any items that are grabbed with the controller● will not collide with the box collider and rigid body on the play area. This is very useful if the user is required to grab and wield objects because if the collider was active they would bounce off the
play area collider.
An example of the VRTK_PlayerPresence script can be viewed in the scene SteamVR_Unity_Toolkit/Examples/017_CameraRig_TouchpadWalking. The scene has a collection of walls and slopes that can be traversed by the user with the touchpad but the user cannot pass through the objects as they are collidable and the rigidbody physics won't allow the intersection to occur.

         */
    public class VRTK_PlayerPresence : MonoBehaviour
    {
        public float headsetYOffset = 0.2f;
        public bool ignoreGrabbedCollisions = true;
        public bool resetPositionOnCollision = true;

        private Transform headset;
        private Rigidbody rb;
        private BoxCollider bc;
        private Vector3 lastGoodPosition;
        private bool lastGoodPositionSet = false;
        private float highestHeadsetY = 0f;
        private float crouchMargin = 0.5f;
        private float lastPlayAreaY = 0f;

        public Transform GetHeadset()
        {
            return headset;
        }

        private void Start()
        {
            Utilities.SetPlayerObject(this.gameObject, VRTK_PlayerObject.ObjectTypes.CameraRig);

            lastGoodPositionSet = false;
            headset = DeviceFinder.HeadsetTransform();
            CreateCollider();
            InitHeadsetListeners();

            var controllerManager = GameObject.FindObjectOfType<SteamVR_ControllerManager>();
            InitControllerListeners(controllerManager.left);
            InitControllerListeners(controllerManager.right);
        }

        private void InitHeadsetListeners()
        {
            if (headset.GetComponent<VRTK_HeadsetCollisionFade>())
            {
                headset.GetComponent<VRTK_HeadsetCollisionFade>().HeadsetCollisionDetect += new HeadsetCollisionEventHandler(OnHeadsetCollision);
            }
        }

        private void OnGrabObject(object sender, ObjectInteractEventArgs e)
        {
            Physics.IgnoreCollision(this.GetComponent<Collider>(), e.target.GetComponent<Collider>(), true);
        }

        private void OnUngrabObject(object sender, ObjectInteractEventArgs e)
        {
            if (e.target.GetComponent<VRTK_InteractableObject>() && !e.target.GetComponent<VRTK_InteractableObject>().IsGrabbed())
            {
                Physics.IgnoreCollision(this.GetComponent<Collider>(), e.target.GetComponent<Collider>(), false);
            }
        }

        private void OnHeadsetCollision(object sender, HeadsetCollisionEventArgs e)
        {
            if (resetPositionOnCollision && lastGoodPositionSet)
            {
                SteamVR_Fade.Start(Color.black, 0f);
                this.transform.position = lastGoodPosition;
              
            }
        }

        private void CreateCollider()
        {
            rb = this.gameObject.AddComponent<Rigidbody>();
            rb.mass = 100;
            rb.freezeRotation = true;

            bc = this.gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 1f, 0f);
            bc.size = new Vector3(0.25f, 1f, 0.25f);

            this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        private void UpdateCollider()
        {
            var playAreaHeightAdjustment = 0.009f;
            var newBCYSize = (headset.transform.position.y - headsetYOffset) - this.transform.position.y;
            var newBCYCenter = (newBCYSize != 0 ? (newBCYSize / 2) + playAreaHeightAdjustment : 0);

            bc.size = new Vector3(bc.size.x, newBCYSize, bc.size.z);
            bc.center = new Vector3(headset.localPosition.x, newBCYCenter, headset.localPosition.z);
        }

        private void SetHeadsetY()
        {
            //if the play area height has changed then always recalc headset height
            var floorVariant = 0.005f;
            if (this.transform.position.y > lastPlayAreaY + floorVariant || this.transform.position.y < lastPlayAreaY - floorVariant)
            {
                highestHeadsetY = 0f;
            }

            if (headset.transform.position.y > highestHeadsetY)
            {
                highestHeadsetY = headset.transform.position.y;
            }

            if (headset.transform.position.y > highestHeadsetY - crouchMargin)
            {
                lastGoodPositionSet = true;
                lastGoodPosition = this.transform.position;
            }

            lastPlayAreaY = this.transform.position.y;
        }

        private void Update()
        {
            SetHeadsetY();
            UpdateCollider();
        }

        private void InitControllerListeners(GameObject controller)
        {
            if (controller)
            {
                var grabbingController = controller.GetComponent<VRTK_InteractGrab>();
                if (grabbingController && ignoreGrabbedCollisions)
                {
                    grabbingController.ControllerGrabInteractableObject += new ObjectInteractEventHandler(OnGrabObject);
                    grabbingController.ControllerUngrabInteractableObject += new ObjectInteractEventHandler(OnUngrabObject);
                }
            }
        }
    }
}