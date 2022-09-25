using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCheck : MonoBehaviour
{
    Vector3 DebugRotationRayTargetPos, DebugObjectCenterPos, _RotationTargetHandlePosition, OriginPosition;
    
    [SerializeField]private GameObject MetaTarget;

    [SerializeField] private FollowMousePosition[] RotationTargetHandles;

    private GameObject Father;

    [SerializeField]private float RotationDiff, HandleToPivotAngle;
    
    private float _TargetHypotenuse;
    private int _Falldirection;
    private GameObject _ChildModel;
    [SerializeField] private bool _RestrictRotationTargetMovement = true, _RestrictetSideMovement = true;

    //private Vector3 pivot = Vector3.zero;
    private Vector3 rotationAxis = Vector3.zero;

    private bool _lockToXAxis, _lockToZAxis;
    private Vector3 _Pivot1 = Vector3.zero, _Pivot2 = Vector3.zero;
    private Vector2Int _gridPosition;
    public Vector2Int GridPosition
    {
        set => _gridPosition = value;
    }
    private float _ChildModelBounds;
    private float _SecondPivotCounterAngle;

    private int _CurrentHandle = 0;

    void Start()
    {
        FindObjectOfType<LevelManager>();

        Father = transform.root.gameObject;

        _ChildModel = transform.GetChild(0).gameObject;

        _ChildModelBounds = 0.3f; /*_ChildModel.GetComponentInChildren<Renderer>().bounds.extents.x :) */;
        _TargetHypotenuse = Mathf.Sqrt((_ChildModelBounds * _ChildModelBounds) * 2);

        _gridPosition = LevelManager.Vector3ToGridPosition(Father.transform.position);
        if(Father.transform.position.y != 0.3f)
            Father.transform.position += new Vector3(0, _ChildModelBounds, 0);

        // DebugLogger.Log("GridPosition: " + gridPosition);
        SetNewPosition();
        HandleCheckIn(0);

        _RotationTargetHandlePosition = RotationTargetHandles[_CurrentHandle].transform.localPosition;
    }

    public void SetNewPosition()
    {
        // offset should be based on exact grid positions and only update on completed rolls.
        // Debug.Log($"pos magnitude {transform.localPosition.magnitude}");
        if (transform.localPosition.magnitude > _ChildModelBounds*1.75f/*0.95f*/)
        {
            Vector3 offset = transform.localPosition;
            offset.y = 0;
                
            Vector3 ChildGlobalRotation = _ChildModel.transform.eulerAngles;

            FindObjectOfType<ManagerAudio>().Play("boxDrop");

            var direction = (transform.position - Father.transform.position);
            var newGridPos = LevelManager.Vector3ToGridPosition(transform.position);
            // DebugLogger.Log($"Direction: {direction}, current: {gridPosition}, target: {newGridPos}");

            if ((newGridPos - _gridPosition).magnitude == 1 && !LevelManager.TileIsOccupied(newGridPos))
            {
                DebugLogger.Log($"New position: {(newGridPos - _gridPosition).magnitude} Occupied: {LevelManager.TileIsOccupied(newGridPos)}");
                LevelManager.MoveBox(_gridPosition, newGridPos);
                _gridPosition = newGridPos;
                Father.transform.position = LevelManager.GridMap[_gridPosition] + new Vector3(0, _ChildModelBounds, 0);
            }
            else
            {
                DebugLogger.Log($"New position: {(newGridPos - _gridPosition).magnitude} Occupied: {LevelManager.TileIsOccupied(newGridPos)}");
                FindObjectOfType<ManagerAudio>().Play("error");
            }

            Debug.Log($"GRIDPOS {_gridPosition}");
            //foreach (FollowMousePosition handle in RotationTargetHandles)
            //    handle.setBlockers(gridPosition);
        
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            ChildGlobalRotation.x = Mathf.Round(ChildGlobalRotation.x / 90) * 90;
            ChildGlobalRotation.y = Mathf.Round(ChildGlobalRotation.y / 90) * 90;
            ChildGlobalRotation.z = Mathf.Round(ChildGlobalRotation.z / 90) * 90;

            _ChildModel.transform.localEulerAngles = ChildGlobalRotation;

            if (LevelManager.IsTileGoal(_gridPosition))
            {
                var elevator = GetComponentInChildren<Elevator>();
                if (elevator)
                    elevator.GoalReached = true;
            }
        }
        else
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    public void HandleCheckIn(int HandleID)
    {
        _CurrentHandle = HandleID;
        switch (HandleID)
        {
            case 0: // +x
                {
                    /// N�r handtag = +x: Rotera i Z, Negativ Rot = Pivot1, Posetiv Rot = Pivot2 (-45) ///

                    // Debug.Log("Handle Check In +x");

                    _lockToXAxis = true;
                    _lockToZAxis = false;

                    _Pivot1 = RotationTargetHandles[0].transform.localPosition;
                    _Pivot1.y = _Pivot1.y - (_ChildModelBounds * 2);
                    _Pivot2 = RotationTargetHandles[1].transform.localPosition;
                    _Pivot2.y = _Pivot2.y - (_ChildModelBounds * 2);

                    _SecondPivotCounterAngle = -45;
                    
                    break;
                }
            case 1: // -x
                {
                    /// N�r handtag = -x: Rotera i Z, Posetiv Rot = Pivot1, Negativ Rot = Pivot2 (+45) ///

                    // Debug.Log("Handle Check In -x");

                    _lockToXAxis = true;
                    _lockToZAxis = false;

                    _Pivot1 = RotationTargetHandles[1].transform.localPosition;
                    _Pivot1.y = _Pivot1.y - (_ChildModelBounds * 2);
                    _Pivot2 = RotationTargetHandles[0].transform.localPosition;
                    _Pivot2.y = _Pivot2.y - (_ChildModelBounds * 2);

                    _SecondPivotCounterAngle = 45;

                    break;
                }
            case 2: // +z
                {
                    /// N�r handtag = +z: Rotera i X, Posetiv Rot = Pivot1, Negativ Rot = Pivot2 (+45) ///

                    // Debug.Log("Handle Check In +z");

                    _lockToZAxis = true;
                    _lockToXAxis = false;

                    _Pivot1 = RotationTargetHandles[2].transform.localPosition;
                    _Pivot1.y = _Pivot1.y - (_ChildModelBounds * 2);
                    _Pivot2 = RotationTargetHandles[3].transform.localPosition;
                    _Pivot2.y = _Pivot2.y - (_ChildModelBounds * 2);

                    _SecondPivotCounterAngle = 45;

                    break;
                }
            case 3: // -z
                {
                    /// N�r handtag = -z: Rotera i X, Negativ Rot = Pivot1, Posetiv Rot = Pivot2 (-45) ///

                    // Debug.Log("Handle Check In -z");

                    _lockToZAxis = true;
                    _lockToXAxis = false;

                    _Pivot1 = RotationTargetHandles[3].transform.localPosition;
                    _Pivot1.y = _Pivot1.y - (_ChildModelBounds * 2);
                    _Pivot2 = RotationTargetHandles[2].transform.localPosition;
                    _Pivot2.y = _Pivot2.y - (_ChildModelBounds * 2);

                    _SecondPivotCounterAngle = -45;

                    break;
                }
            default: //null
                {
                    // Debug.Log("Handle Check In null");

                    _lockToXAxis = false;
                    _lockToZAxis = false;

                    break;
                }
        }
    }

    bool SwitchPositionReset = true;

    void Update()
    {
        if(RotationTargetHandles[_CurrentHandle].isMoving)
        {
            // Switch pivot depending on cube rotation
            _RotationTargetHandlePosition = RotationTargetHandles[_CurrentHandle].transform.localPosition;

            /// +X
            if (_CurrentHandle == 0)
            {
                float FixedZAxisAngle = transform.eulerAngles.z;
                if(FixedZAxisAngle > 180)
                {
                    FixedZAxisAngle = (360 + (FixedZAxisAngle*-1))*-1;
                }

                if(FixedZAxisAngle > 85 || FixedZAxisAngle < -85)
                {
                    RotationTargetHandles[_CurrentHandle].LetGoOfTarget();
                }

                if (FixedZAxisAngle < 0) /// Towards
                {
                    if (SwitchPositionReset == true) // Reset Cube position to prevent unwanted displacement
                    {
                        SwitchPositionReset = false;
                        this.gameObject.transform.localPosition = Vector3.zero;
                    }

                    HandleToPivotAngle = Vector3.SignedAngle((_RotationTargetHandlePosition - _Pivot1), Vector3.up, Vector3.forward) * -1;

                    RotationDiff = HandleToPivotAngle - transform.localEulerAngles.z;

                    transform.RotateAround(Father.transform.TransformPoint(_Pivot1), Vector3.forward, RotationDiff);

                    Debug.DrawLine(_RotationTargetHandlePosition, _Pivot1, Color.green, 0.001f);
                    
                    if (FixedZAxisAngle <= -45 && _Falldirection != 1)
                    {
                        _Falldirection = 1;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                    else if (FixedZAxisAngle >= -45 && _Falldirection != 0)
                    {
                        _Falldirection = 0;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                }
                else /// Over
                {
                    if (SwitchPositionReset == false) // Reset Cube position to prevent unwanted displacement
                    {
                        SwitchPositionReset = true;
                        this.gameObject.transform.localPosition = Vector3.zero;
                    }

                    HandleToPivotAngle = (Vector3.SignedAngle((_RotationTargetHandlePosition - _Pivot2), Vector3.up, Vector3.forward) + _SecondPivotCounterAngle) * -1;

                    RotationDiff = HandleToPivotAngle - transform.localEulerAngles.z;

                    transform.RotateAround(Father.transform.TransformPoint(_Pivot2), Vector3.forward, RotationDiff);

                    Debug.DrawLine(_RotationTargetHandlePosition, _Pivot2, Color.green, 0.001f);
                    
                    if (FixedZAxisAngle >= 45 && _Falldirection != 2)
                    {
                        _Falldirection = 2;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                    else if (FixedZAxisAngle <= 45 && _Falldirection != 0)
                    {
                        _Falldirection = 0;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                }
            }

            /// -X
            if (_CurrentHandle == 1)
            {
                float FixedZAxisAngle = transform.eulerAngles.z;
                if (FixedZAxisAngle > 180)
                {
                    FixedZAxisAngle = (360 + (FixedZAxisAngle * -1)) * -1;
                }

                if (FixedZAxisAngle > 85 || FixedZAxisAngle < -85)
                {
                    RotationTargetHandles[_CurrentHandle].LetGoOfTarget();
                }

                if (FixedZAxisAngle > 0) /// Towards
                {
                    if (SwitchPositionReset == true) // Reset Cube position to prevent unwanted displacement
                    {
                        SwitchPositionReset = false;
                        this.gameObject.transform.localPosition = Vector3.zero;
                    }

                    HandleToPivotAngle = Vector3.SignedAngle((_RotationTargetHandlePosition - _Pivot1), Vector3.up, Vector3.forward) * -1;

                    RotationDiff = HandleToPivotAngle - transform.localEulerAngles.z;

                    transform.RotateAround(Father.transform.TransformPoint(_Pivot1), Vector3.forward, RotationDiff);

                    Debug.DrawLine(_RotationTargetHandlePosition, _Pivot1, Color.green, 0.001f);

                    if (FixedZAxisAngle >= 45 && _Falldirection != 1)
                    {
                        _Falldirection = 1;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                    else if (FixedZAxisAngle <= 45 && _Falldirection != 0)
                    {
                        _Falldirection = 0;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                }
                else /// Over
                {
                    if (SwitchPositionReset == false) // Reset Cube position to prevent unwanted displacement
                    {
                        SwitchPositionReset = true;
                        this.gameObject.transform.localPosition = Vector3.zero;
                    }

                    HandleToPivotAngle = (Vector3.SignedAngle((_RotationTargetHandlePosition - _Pivot2), Vector3.up, Vector3.forward) + _SecondPivotCounterAngle) * -1;

                    RotationDiff = HandleToPivotAngle - transform.localEulerAngles.z;

                    transform.RotateAround(Father.transform.TransformPoint(_Pivot2), Vector3.forward, RotationDiff);

                    Debug.DrawLine(_RotationTargetHandlePosition, _Pivot2, Color.green, 0.001f);

                    if (FixedZAxisAngle <= -45 && _Falldirection != 2)
                    {
                        _Falldirection = 2;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                    else if (FixedZAxisAngle >= -45 && _Falldirection != 0)
                    {
                        _Falldirection = 0;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                }
            }

            /// +Z
            if (_CurrentHandle == 2)
            {
                float FixedXAxisAngle = transform.eulerAngles.x;
                if (FixedXAxisAngle > 180)
                {
                    FixedXAxisAngle = (360 + (FixedXAxisAngle * -1)) * -1;
                }
                
                if (FixedXAxisAngle > 85 || FixedXAxisAngle < -85)
                {
                    RotationTargetHandles[_CurrentHandle].LetGoOfTarget();
                }

                if (FixedXAxisAngle > 0) /// Towards
                {
                    if (SwitchPositionReset == true) // Reset Cube position to prevent unwanted displacement
                    {
                        SwitchPositionReset = false;
                        this.gameObject.transform.localPosition = Vector3.zero;
                    }

                    HandleToPivotAngle = Vector3.SignedAngle((_RotationTargetHandlePosition - _Pivot1), Vector3.up, Vector3.right) * -1;

                    RotationDiff = HandleToPivotAngle - transform.localEulerAngles.x;

                    transform.RotateAround(Father.transform.TransformPoint(_Pivot1), Vector3.right, RotationDiff);

                    Debug.DrawLine(_RotationTargetHandlePosition, _Pivot1, Color.green, 0.001f);

                    if (FixedXAxisAngle >= 45 && _Falldirection != 1)
                    {
                        _Falldirection = 1;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                    else if (FixedXAxisAngle <= 45 && _Falldirection != 0)
                    {
                        _Falldirection = 0;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection); 
                    }
                }
                else /// Over
                {
                    if (SwitchPositionReset == false) // Reset Cube position to prevent unwanted displacement
                    {
                        SwitchPositionReset = true;
                        this.gameObject.transform.localPosition = Vector3.zero;
                    }

                    HandleToPivotAngle = (Vector3.SignedAngle((_RotationTargetHandlePosition - _Pivot2), Vector3.up, Vector3.right) + _SecondPivotCounterAngle) * -1;

                    RotationDiff = HandleToPivotAngle - transform.localEulerAngles.x;

                    transform.RotateAround(Father.transform.TransformPoint(_Pivot2), Vector3.right, RotationDiff);

                    Debug.DrawLine(_RotationTargetHandlePosition, _Pivot2, Color.green, 0.001f);

                    if (FixedXAxisAngle <= -45 && _Falldirection != 2)
                    {
                        _Falldirection = 2;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                    else if (FixedXAxisAngle >= -45 && _Falldirection != 0)
                    {
                        _Falldirection = 0;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                }
            }

            /// -Z
            if (_CurrentHandle == 3)
            {
                float FixedXAxisAngle = transform.eulerAngles.x;
                if (FixedXAxisAngle > 180)
                {
                    FixedXAxisAngle = (360 + (FixedXAxisAngle * -1)) * -1;
                }
                
                if (FixedXAxisAngle > 85 || FixedXAxisAngle < -85)
                {
                    RotationTargetHandles[_CurrentHandle].LetGoOfTarget();
                }

                if (FixedXAxisAngle < 0) /// Towards
                {
                    if (SwitchPositionReset == true) // Reset Cube position to prevent unwanted displacement
                    {
                        SwitchPositionReset = false;
                        this.gameObject.transform.localPosition = Vector3.zero;
                    }

                    HandleToPivotAngle = Vector3.SignedAngle((_RotationTargetHandlePosition - _Pivot1), Vector3.up, Vector3.right) * -1;

                    RotationDiff = HandleToPivotAngle - transform.localEulerAngles.x;

                    transform.RotateAround(Father.transform.TransformPoint(_Pivot1), Vector3.right, RotationDiff);

                    Debug.DrawLine(_RotationTargetHandlePosition, _Pivot1, Color.green, 0.001f);

                    if (FixedXAxisAngle <= -45 && _Falldirection != 1)
                    {
                        _Falldirection = 1;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                    else if (FixedXAxisAngle >= -45 && _Falldirection != 0)
                    {
                        _Falldirection = 0;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                }
                else /// Over
                {
                    if (SwitchPositionReset == false) // Reset Cube position to prevent unwanted displacement
                    {
                        SwitchPositionReset = true;
                        this.gameObject.transform.localPosition = Vector3.zero;
                    }

                    if (FixedXAxisAngle > 85 || FixedXAxisAngle < -85)
                    {
                        RotationTargetHandles[_CurrentHandle].LetGoOfTarget();
                    }

                    HandleToPivotAngle = (Vector3.SignedAngle((_RotationTargetHandlePosition - _Pivot2), Vector3.up, Vector3.right) + _SecondPivotCounterAngle) * -1;

                    RotationDiff = HandleToPivotAngle - transform.localEulerAngles.x;

                    transform.RotateAround(Father.transform.TransformPoint(_Pivot2), Vector3.right, RotationDiff);

                    Debug.DrawLine(_RotationTargetHandlePosition, _Pivot2, Color.green, 0.001f);

                    if (FixedXAxisAngle >= 45 && _Falldirection != 2)
                    {
                        _Falldirection = 2;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                    else if (FixedXAxisAngle <= 45 && _Falldirection != 0)
                    {
                        _Falldirection = 0;
                        RotationTargetHandles[_CurrentHandle].CubeFallingDirection(_Falldirection);
                    }
                }
            }

            if (_RestrictRotationTargetMovement) /// Kombinera dessa tv�
            {
                RestrictRotationTargetMovement();
            }

            if (_RestrictetSideMovement)
            {
                RestrictSideMovement();
            }
        }
        else if (transform.localPosition.magnitude > 0)
        {
            RotationTargetHandles[_CurrentHandle].transform.localPosition = RotationTargetHandles[_CurrentHandle]._originPos;
            SetNewPosition();
        }

        debugDraw();
    }

    private void LateUpdate()
    {
        if (rotationAxis == transform.right && transform.localEulerAngles.z > 0)
            SetNewPosition();
        else if (rotationAxis == transform.forward && transform.localEulerAngles.x > 0)
            SetNewPosition();
    }

    private void RestrictRotationTargetMovement() /// Weird wacky shit happening here, h�ll ett �ga p�!
    {
        float MaxRadius = _TargetHypotenuse + 0.0001f; //TargetHypotenuse defines the radious
        float MinRadius = _TargetHypotenuse;
        float distance = Vector3.Distance(_RotationTargetHandlePosition, transform.localPosition); //distance from Rotation Target to Box

        if (distance > MaxRadius) //If the distance is less than the MaxRadius.
        {
            Vector3 fromOriginToObject = _RotationTargetHandlePosition - transform.localPosition;
            fromOriginToObject *= MaxRadius / distance;
            RotationTargetHandles[_CurrentHandle].transform.localPosition = transform.localPosition + fromOriginToObject;
        }
        else if (distance < MinRadius) //If the distance is greater than the MinRadius.
        {
            Vector3 fromOriginToObject = _RotationTargetHandlePosition - transform.localPosition;
            fromOriginToObject *= MaxRadius / distance;
            RotationTargetHandles[_CurrentHandle].transform.localPosition = transform.localPosition + fromOriginToObject;
        }
    }

    private void RestrictSideMovement()
    {
        if (_lockToXAxis)
        {
            RotationTargetHandles[_CurrentHandle].transform.localPosition = new Vector3(RotationTargetHandles[_CurrentHandle].transform.localPosition.x, RotationTargetHandles[_CurrentHandle].transform.localPosition.y, RotationTargetHandles[_CurrentHandle]._originPos.z);
        }
        else if (_lockToZAxis)
        {
            RotationTargetHandles[_CurrentHandle].transform.localPosition = new Vector3(RotationTargetHandles[_CurrentHandle]._originPos.x, RotationTargetHandles[_CurrentHandle].transform.localPosition.y, RotationTargetHandles[_CurrentHandle].transform.localPosition.z);
        }
        else
        {
            Debug.Log("Rotation Check, Restricted Movement: Fuck");
        }
    }

    private void debugDraw()
    {
        /// Debug
        DebugRotationRayTargetPos = MetaTarget.transform.position; //!!Do not use!!
        DebugObjectCenterPos = this.gameObject.transform.position; //!!Do not use!!
        ///Local Up
        Debug.DrawLine(DebugObjectCenterPos, DebugRotationRayTargetPos , Color.blue, 0.001f);
        ///Global Up
        Debug.DrawLine(DebugObjectCenterPos, new Vector3(DebugObjectCenterPos.x, (DebugObjectCenterPos.y +2), DebugObjectCenterPos.z), Color.red, 0.001f);
    }

    public void DenyHandleInput(bool deactivateHandles, int incomingIndex)
    {
        for (int index = 0; index < RotationTargetHandles.Length; index++)
        {
            if (index == incomingIndex)
                continue;

            RotationTargetHandles[index].BlockInputOrder = deactivateHandles;
        }
    }
}
