using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMousePosition : MonoBehaviour
{
    [SerializeField] private int MyHandleId;

    [SerializeField]private float smoothTime, FallSpeed, DropRadious = 5, GrabRadious = 1;
    [SerializeField]private AnimationCurve FallCurve;
    [SerializeField] RotationCheck CubeRotation;

    private Transform _hand;

    private bool _isRightHand;

    //[SerializeField] private AudioSource HoverSound;

    public bool isMoving, ObjectGrabbed, BlockInputOrder;
    private Vector3 clickPosition;

    private Vector3 velocity = Vector3.zero;
    private Vector3 CurrentPosition, FallPointTarget;

    private bool mouseIsClose, blockDirectionsCheck = false;
    //[SerializeField] bool handleIsOccupied;

    private float TimeStamp, TravelLength, _modelBounds;

    private SphereCollider Grabcollider;

    [SerializeField] private bool[] _blockDirections; // +x -x +z -z
    public Vector3 _originPos;

    private void Awake()
    {
        _originPos = transform.localPosition;

        _modelBounds = 0.6f; //(CubeRotation.transform.GetChild(0).gameObject.GetComponentInChildren<Renderer>().bounds.extents.x) *2;
        // Debug.Log("ModelBounds" + _modelBounds);

        Grabcollider = GetComponent<SphereCollider>();
        
        if (transform.localPosition.x > CubeRotation.transform.localPosition.x) /// Om +x
        {
            MyHandleId = 0;
        }
        else if(transform.localPosition.x < CubeRotation.transform.localPosition.x) /// Om -x
        {
            MyHandleId = 1;
        }
        else if(transform.localPosition.z > CubeRotation.transform.localPosition.z) /// Om +z
        {
            MyHandleId = 2;
        }
        else if(transform.localPosition.z < CubeRotation.transform.localPosition.z) /// Om -z
        {
            MyHandleId = 3;
        }
        else
        {
            Debug.Log("Shits f�cked");
        }
    }

    private void Start()
    {
        CubeFallingDirection(0);
        Grabcollider.radius = GrabRadious;
    }

    //void OnMouseEnter()
    //{
    //    mouseIsClose = true;
    //    //HoverSound.Play();
    //}
    //void OnMouseExit()
    //{
    //    mouseIsClose = false;
    //    //HoverSound.Stop();

    //    if (ObjectGrabbed)
    //        LetGoOfTarget();
    //}

    private void OnTriggerEnter(Collider other)
    {
        //TODO: Add check before we assume that the object is the right one to grab
        
        if (other.gameObject.layer == 10 && /*!handleIsOccupied && */ !BlockInputOrder)
        {
            //handleIsOccupied = true;
            mouseIsClose = true;
            _hand = other.transform;
            var handIdentifier = _hand.gameObject.GetComponent<HandIdentifier>();
            if(handIdentifier != null)
            {
                _isRightHand = handIdentifier.IsRightHand;
            } else
            {
                Debug.LogError("HandIdentifier is missing");
            }
            var phoneAFriend = _hand.gameObject.GetComponentInParent<PhoneAFriend>();
            if (phoneAFriend != null)
            {
                phoneAFriend.ActivateSister(true);
            } else
            {
                Debug.LogError("PhoneAFriend is missing");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10 /*&& other.transform == _hand && handleIsOccupied*/)
        {
            //andleIsOccupied = false;
            mouseIsClose = false;

            if (ObjectGrabbed)
                LetGoOfTarget();
        }
    }

    public void CubeFallingDirection(int FallDirection)
    {
        // Debug.Log("Fall Direction "+FallDirection);

        switch (MyHandleId)
        {
            case 3: /// -z
            {
                    // Debug.Log("-z: " + FallDirection);

                    switch (FallDirection)
                    {
                        case 2: /// Over
                            {
                                FallPointTarget = (_originPos + (Vector3.forward * _modelBounds) * 2);
                                break;
                            }
                        case 1: /// Towards
                            {
                                FallPointTarget = (_originPos + (Vector3.down + Vector3.back) * _modelBounds);
                                break;
                            }
                        default: /// Return
                            {
                                FallPointTarget = _originPos;
                                break;
                            }
                    }
                    break;
                }
            case 2: /// +z
            {
                    // Debug.Log("+z: " + FallDirection);

                    switch (FallDirection)
                    {
                        case 2: /// Over
                            {
                                FallPointTarget = (_originPos + (Vector3.back * _modelBounds) * 2);
                                break;
                            }
                        case 1: /// Towards
                            {
                                FallPointTarget = (_originPos + (Vector3.down + Vector3.forward) * _modelBounds);
                                break;
                            }
                        default: /// Return
                            {
                                FallPointTarget = _originPos;
                                break;
                            }
                    }
                    break;
                }
            case 1: /// -x
            {
                    // Debug.Log("-x: " + FallDirection);

                    switch (FallDirection)
                    {
                        case 2: /// Over
                            {
                                FallPointTarget = (_originPos + (Vector3.right * _modelBounds) * 2);
                                break;
                            }
                        case 1: /// Towards
                            {
                                FallPointTarget = (_originPos + (Vector3.down + Vector3.left) * _modelBounds);
                                break;
                            }
                        default: /// Return
                            {
                                FallPointTarget = _originPos;
                                break;
                            }
                    }
                    break;
                }
            case 0: /// +x
            {
                    // Debug.Log("+x: " + FallDirection);

                    switch (FallDirection)
                    {
                        case 2: /// Over
                            {
                                FallPointTarget = (_originPos + (Vector3.left * _modelBounds ) * 2);

                                break;
                            }
                        case 1: /// Towards
                            {
                                FallPointTarget = (_originPos + (Vector3.down + Vector3.right) * _modelBounds);
                                break;
                            }
                        default: /// Return
                            {
                                FallPointTarget = _originPos;
                                break;
                            }
                    }
                    break;
            }
            default:
            {
                FallPointTarget = _originPos;
                // Debug.Log("Hand ID not found, no fall direction");
                break;
            }
        }
    }

    public void LetGoOfTarget()
    {
        if (_hand == null) return;

        CubeRotation.DenyHandleInput(false, -1);

        CurrentPosition = transform.localPosition;
        TimeStamp = Time.time; //Keep a note of the time the movement started
        TravelLength = Mathf.Clamp(Vector3.Distance(CurrentPosition, FallPointTarget), 0.0001f, Mathf.Infinity); //Calculate the journey length

        ObjectGrabbed = false;
        //handleIsOccupied = false;

        Grabcollider.radius = GrabRadious;

        _hand.GetComponentInParent<PhoneAFriend>().ActivateSister(true);

        _hand = null;
    }

    void Update()
    {
        if (!BlockInputOrder)
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) && mouseIsClose && _isRightHand == true) /// Grab Object
            {
                updateHands();
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch) && mouseIsClose && _isRightHand == false) /// Grab Object
            {
                updateHands();
            }

            if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) && ObjectGrabbed && _isRightHand == true)
            {
                LetGoOfTarget();
            }

            if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch) && ObjectGrabbed && _isRightHand == false)
            {
                LetGoOfTarget();
            }

            if (ObjectGrabbed && _hand != null)
            {
                Vector3 MousePositionDelta =
                    clickPosition - transform.parent.InverseTransformPoint(_hand.position);
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, _originPos + MousePositionDelta * -1, ref velocity, smoothTime);

                Debug.DrawLine(transform.position, _originPos + MousePositionDelta * -1, Color.cyan, 0.001f);
            }
            else if (isMoving)
            {
                float distCovered = (Time.time - TimeStamp) * FallSpeed; //Distance moved equals elapsed time times speed

                float TravelLerp = distCovered / TravelLength; //Fraction of distance completed equals current distance divided by total distance

                transform.localPosition = Vector3.Lerp(CurrentPosition, FallPointTarget, FallCurve.Evaluate(TravelLerp)); // Set position as a fraction of the distance between the markers through an animation curve

                if (TravelLerp >= 1)
                {
                    transform.localPosition = FallPointTarget;
                    isMoving = false;
                }
            }

            if (blockDirectionsCheck)
            {
                transformBlockDirection();
            }
        }
    }

    private void updateHands()
    {
        if (_hand == null) return;

        CubeRotation.HandleCheckIn(MyHandleId);

        CubeRotation.DenyHandleInput(true, MyHandleId);

        ObjectGrabbed = true;
        isMoving = true;
        Grabcollider.radius = DropRadious;
        clickPosition = transform.parent.InverseTransformPoint(_hand.position);
        _hand.GetComponentInParent<PhoneAFriend>().ActivateSister(false);
        FindObjectOfType<ManagerAudio>().Play("boxPlopp");

    }

    private void transformBlockDirection()
    {
        if (_blockDirections[0])  // +x
        {
            if (_originPos.x < transform.localPosition.x)
                transform.localPosition = new Vector3(_originPos.x, transform.localPosition.y, transform.localPosition.z);
        }
        if (_blockDirections[1])  // -x
        {
            if (_originPos.x > transform.localPosition.x)
                transform.localPosition = new Vector3(_originPos.x, transform.localPosition.y, transform.localPosition.z);
        }
        if (_blockDirections[2])  // +z
        {
            if (_originPos.z < transform.localPosition.z)
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _originPos.z);
        }
        if (_blockDirections[3])  // -z
        {
            if (_originPos.z > transform.localPosition.z)
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _originPos.z);
        }
    }

    public void setBlockers(Vector2Int CurrentPosition)
    {
        _blockDirections = FindObjectOfType<LevelManager>().CheckAdjacentTiles(CurrentPosition);
        if (_blockDirections != null)
        {
            blockDirectionsCheck = true;
        }
        else
        {
            blockDirectionsCheck = false;
            // Debug.Log("BlockArray is null");
        }
    }
}
