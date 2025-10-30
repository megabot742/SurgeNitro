using UnityEngine;

[RequireComponent(typeof(CarControllerBase))]
[DisallowMultipleComponent]
public class FrictionUpdater : MonoBehaviour
{
    private CarControllerBase _car;
    private RoadMaterialDetector[] _detectors;

    private float _defaultPeakFrictionSlipAngle;
    private float _defaultMu;
    private float _defaultRollingResistanceCoef;

    private void Awake()
    {
        _car = GetComponent<CarControllerBase>();
        _detectors = GetComponentsInChildren<RoadMaterialDetector>();

        _defaultPeakFrictionSlipAngle = _car.PeakFrictionSlipAngle;
        _defaultMu = _car.Mu;
        _defaultRollingResistanceCoef = _car.RollingResistanceCoef;
    }

    private void FixedUpdate()
    {
        var avgPFSA = 0f;
        var avgMu = 0f;
        var avgRRC = 0f;
        var count = 0;

        foreach (var detector in _detectors)
        {
            if (!detector.Wheel.Grounded)
            {
                continue;
            }

            var mat = detector.Material;
            if (mat == null)
            {
                continue;
            }

            avgPFSA += mat.PeakFrictionSlipAngle;
            avgMu += mat.Mu;
            avgRRC += mat.RollingResistanceCoef;
            count++;
        }

        if (count > 0)
        {
            avgPFSA /= count;
            avgMu /= count;
            avgRRC /= count;
            _car.PeakFrictionSlipAngle = avgPFSA;
            _car.Mu = avgMu;
            _car.RollingResistanceCoef = avgRRC;
        }
        else
        {
            _car.PeakFrictionSlipAngle = _defaultPeakFrictionSlipAngle;
            _car.Mu = _defaultMu;
            _car.RollingResistanceCoef = _defaultRollingResistanceCoef;
        }
    }
}
