using MeshSplitting.Splitters;
//Disclaimer - Took little help on splitters from github
using System;
using UnityEngine;


   
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(LineRenderer))]
public class CameraLineSplitter : MonoBehaviour
{

    //The minimum distance from camera to object to initiate the cut
    public float CutPlaneDistance = 10f;

    //Length of plane that will do the cut
    public float CutPlaneSize = 2f;

    private LineRenderer _lineRenderer;
    private Camera _camera;
    private Transform _transform;

    private bool _inCutMode = false;
    private bool _hasStartPos = false;
    private Vector3 _startPos;
    private Vector3 _endPos;

    private void Awake()
    {
        _transform = transform;
        _lineRenderer = GetComponent<LineRenderer>();
        _camera = GetComponent<Camera>();

        _lineRenderer.enabled = false;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Allow cut only when space is pressed
            _inCutMode = true;
           
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
     
            _inCutMode = false;
            _lineRenderer.enabled = false;
            _hasStartPos = false;
           
           
        }

        if (_inCutMode)
        {
            //Record coordinates of cut when space is released
            if (Input.GetMouseButtonDown(0))
            {
                //Capture position from point A to point B and create a Line
                _startPos = GetMousePosInWorld();
                _hasStartPos = true;
            }
            else if (_hasStartPos && Input.GetMouseButtonUp(0))
            {
                _endPos = GetMousePosInWorld();
                if (_startPos != _endPos)
                    CreateCutPlane();

                _hasStartPos = false;
                _lineRenderer.enabled = false;
            }

            if (_hasStartPos)
            {
                _lineRenderer.enabled = true;
                _lineRenderer.SetPosition(0, _startPos);
                _lineRenderer.SetPosition(1, GetMousePosInWorld());
            }
        }
    }

    private Vector3 GetMousePosInWorld()
    {
        //Convert mouse position to ray
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        return ray.origin + ray.direction * CutPlaneDistance;
    }

    private void CreateCutPlane()
    {
        //Record positions of line and create a plane accordingly
        Vector3 center = Vector3.Lerp(_startPos, _endPos, .5f);
        Vector3 cut = (_endPos - _startPos).normalized;
        Vector3 fwd = (center - _transform.position).normalized;
        Vector3 normal = Vector3.Cross(fwd, cut).normalized;

        //Create new object of cutplane. This object will be used in other script to apply the cut
        GameObject goCutPlane = new GameObject("CutPlane", typeof(BoxCollider), typeof(Rigidbody), typeof(SplitterSingleCut));

        goCutPlane.GetComponent<Collider>().isTrigger = true;
        Rigidbody bodyCutPlane = goCutPlane.GetComponent<Rigidbody>();
        bodyCutPlane.useGravity = false;
        bodyCutPlane.isKinematic = true;

        Transform transformCutPlane = goCutPlane.transform;
        transformCutPlane.position = center;
        transformCutPlane.localScale = new Vector3(CutPlaneSize, .01f, CutPlaneSize);
        transformCutPlane.up = normal;
        float angleFwd = Vector3.Angle(transformCutPlane.forward, fwd);
        transformCutPlane.RotateAround(center, normal, normal.y < 0f ? -angleFwd : angleFwd);
    }
}

