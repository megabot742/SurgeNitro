using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AirTrailController : MonoBehaviour
{
    [SerializeField, ReadOnly] private CarControllerBase carController;
    [SerializeField] private List<TrailRenderer> listTrails;
    [SerializeField, Min(0.1f)] private float airTimeToActivate = 0.5f;

    private float airTimer = 0f;
    private bool trailsActive = false;

    private void Awake()
    {
        carController = GetComponentInParent<CarControllerBase>();
        CollectTrail();
        // Đảm bảo trail tắt lúc khởi động
        SetTrailsEmitting(false);
    }
    private void CollectTrail()
    {
        listTrails.Clear();
        var allCameras = GetComponentsInChildren<TrailRenderer>(true); //check and get all cameras
        listTrails.AddRange(allCameras); //add camera to list
    }

    private void FixedUpdate()
    {
        if (!carController.IsGrounded())
        {
            // Xe đang ở trên không
            airTimer += Time.fixedDeltaTime;

            if (!trailsActive && airTimer >= airTimeToActivate)
            {
                SetTrailsEmitting(true);
            }
        }
        else
        {
            // Xe chạm đất → reset timer và tắt trail ngay lập tức
            if (trailsActive || airTimer > 0f)
            {
                airTimer = 0f;
                SetTrailsEmitting(false);
            }
        }
    }

    private void SetTrailsEmitting(bool emit)
    {
        trailsActive = emit;
        foreach (TrailRenderer trail in listTrails)
        {
            if (trail != null)
            {
                trail.emitting = emit;
            }
        }
    }
}
