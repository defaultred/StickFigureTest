using UnityEngine;


public class FaceTrackingExample : MonoBehaviour
{
    public float FaceTrackingTimeout = 0.1f;
    public float TimeToReturnToDefault = 0.5f;
    public float SmoothTime = 0.1f;
    public KinectBinder Kinect;
    public Transform Model;
    public PoseAnimator ModelAnimator;

    public GameObject Flames;

    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _smoothedRotation;
    private Vector3 _currentPosVelocity;
    private Vector3 _currentRotVelocity;

    private AnimationUnits _animUnits, _targetAnimUnits;
    private AnimationUnits _currentAuVelocity;

    private bool _isInitialized;
    private bool _hasNewData;
    private float _waitTimer;
    private float _timeOfLastFrame;
    private float _gravityStartTime;
    private AnimationUnits _startGravityAu;
    private Vector3 _startGravityPos;
    private Vector3 _startGravityRot;

    private AnimationUnits _accAu;
    private float[] _morphCoefs = new float[7];

    private Vector3 _userInitialPosition;

    public enum TrackingMode
    {
        UserFace,
        Gravity,
        ComputerControlled,
    }

    public TrackingMode CurrentMode { get; set; }


    void Start ()
    {
        _position = _userInitialPosition;
        _rotation = Model.transform.rotation.eulerAngles;
        Kinect.FaceTrackingDataReceived += ProcessFaceTrackingData;
    }
    

    void ProcessFaceTrackingData(float au0, float au1, float au2, float au3, float au4, float au5, float posX, float posY, float posZ, float rotX, float rotY, float rotZ)
    {
        _hasNewData = true;
        var animUnits = new AnimationUnits(au0, au1, au2, au3, au4, au5);
        _position = new Vector3(posX, posY, posZ);
        _rotation = new Vector3(rotX, rotY, rotZ);
        
        // We amplify the position to exagerate the head movements.
        _position *= 10;
        SetCurrentAUs(animUnits);
    }


    private void SetCurrentAUs(AnimationUnits animUnits)
    {
        const float weight = 0.8f;
        for (int i = 0; i < 6; i++)
        {
            _accAu[i] = animUnits[i] * weight + _accAu[i] * (1 - weight);
        }

        animUnits = _accAu;

        _targetAnimUnits.LipRaiser = MapLipRaiserValue(animUnits.LipRaiser);
        _targetAnimUnits.JawLowerer = MapJawLowererValue(animUnits.JawLowerer);
        _targetAnimUnits.LipStretcher = MapLipStretcherValue(animUnits.LipStretcher);
        _targetAnimUnits.BrowLowerer = MapBrowLowererValue(animUnits);
        _targetAnimUnits.LipCornerDepressor = MapLipCornerDepressorValue(animUnits.LipCornerDepressor);
        _targetAnimUnits.OuterBrowRaiser = MapOuterBrowRaiserValue(animUnits.OuterBrowRaiser);
    }


    #region Au Range Calibration
    /**
     * Notes on the Animation Units remapping:
     * In this part we simply that the raw data from the kinect and filter/re-mapped it.
     * In some cases we amplify it, we in others we use a bit of logic to manipulate the data.
     * 
     * The magic numbers only reflect what we found worked well for the experience we wanted to setup.
     */
    // AU0
    private float MapLipRaiserValue(float coef)
    {
        return coef;
    }

    // AU1
    private float MapJawLowererValue(float coef)
    {
        return coef;
    }

    // AU2
    private float MapLipStretcherValue(float coef)
    {
        return coef;
    }

    // AU3
    private float MapBrowLowererValue(AnimationUnits animUnits)
    {
        if (animUnits.OuterBrowRaiser > 0f)
            return Mathf.Clamp(animUnits.BrowLowerer - animUnits.OuterBrowRaiser, -1f, 1.7f);

        return Mathf.Clamp(animUnits.BrowLowerer - 3 * animUnits.OuterBrowRaiser, -1f, 1.7f);
    }

    // AU4
    private float MapLipCornerDepressorValue(float coef)
    {
        return 2 * coef;
    }

    // AU5
    private float MapOuterBrowRaiserValue(float coef)
    {
        return Mathf.Clamp(coef * 2, -1f, 1f);
    }
    #endregion


    void Update()
    {
        if (_hasNewData)
        {
            _hasNewData = false;
            _timeOfLastFrame = Time.time;
            ProcessFaceData();
        }
        else
        {
            float timeSinceLastFrame = Time.time - _timeOfLastFrame;
            if (timeSinceLastFrame > TimeToReturnToDefault + FaceTrackingTimeout)
            {
                ManipulateMaskAutomatically();
            }
            else if (timeSinceLastFrame > FaceTrackingTimeout)
            {
                ReturnMaskToNeutralState();
            }
        }

        UpdateTransform();
        UpdateAUs();

        CheckForSpecialPoses();
    }


    private void ProcessFaceData()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            InitializeUserData();
        }

        CurrentMode = TrackingMode.UserFace;
    }


    private void InitializeUserData()
    {
        _userInitialPosition = _position;
    }


    // Perform random faces automatically to fill the gaps whenever we do not have any input data from kinect.
    private void ManipulateMaskAutomatically()
    {
        CurrentMode = TrackingMode.ComputerControlled;
        _waitTimer -= Time.deltaTime;
        if (_waitTimer > 0)
            return;

        _waitTimer = 3f;

        _rotation = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        _targetAnimUnits = new AnimationUnits(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f),
                                          Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }


    private void ReturnMaskToNeutralState()
    {
        if (CurrentMode != TrackingMode.Gravity)
        {
            InitializeGravity();
            CurrentMode = TrackingMode.Gravity;
        }

        ApplyGravityToParams();
    }


    // By gravity we mean the force that will pull the mask back to its neutral state.
    private void InitializeGravity()
    {
        _isInitialized = false;
        _gravityStartTime = Time.time;
        _startGravityAu = _animUnits;
        _startGravityPos = _position;
        _startGravityRot = _rotation;
    }


    private void ApplyGravityToParams()
    {
        float time = Mathf.Clamp01((Time.time - _gravityStartTime) / TimeToReturnToDefault);
        _position = Vector3.Lerp(_startGravityPos, _userInitialPosition, time);
        _rotation = Vector3.Lerp(_startGravityRot, new Vector3(0, 0, 0), time);

        var animUnits = new AnimationUnits();
        animUnits.Au012 = Vector3.Lerp(_startGravityAu.Au012, Vector3.zero, time);
        animUnits.Au345 = Vector3.Lerp(_startGravityAu.Au345, Vector3.zero, time);
        SetCurrentAUs(animUnits);
    }


    private void UpdateTransform()
    {
        // Apply some smoothing to both the position and rotation. The raw input data is quite noisy.
        _smoothedRotation = Vector3.SmoothDamp(_smoothedRotation, _rotation, ref _currentRotVelocity, SmoothTime);
        Model.rotation = Quaternion.Euler(_smoothedRotation);
        Model.position = Vector3.SmoothDamp(Model.position, _position - _userInitialPosition, ref _currentPosVelocity, SmoothTime);
    }


    private void UpdateAUs()
    {
        // Smooth the animation units as the data received directly by the kinect is noisy.
        _animUnits.Au012 = Vector3.SmoothDamp(_animUnits.Au012, _targetAnimUnits.Au012, ref _currentAuVelocity.Au012, SmoothTime);
        _animUnits.Au345 = Vector3.SmoothDamp(_animUnits.Au345, _targetAnimUnits.Au345, ref _currentAuVelocity.Au345, SmoothTime);

        UpdateLipRaiser(_animUnits.LipRaiser);
        UpdateJawLowerer(_animUnits.JawLowerer);
        UpdateLipStretcher(_animUnits.LipStretcher);
        UpdateBrowLowerer(_animUnits.BrowLowerer);
        UpdateLipCornerDepressor(_animUnits.LipCornerDepressor);
        UpdateOuterBrowRaiser(_animUnits.OuterBrowRaiser);
    }

    #region Specific AU Updates
    /**
     * Note on animating the mask:
     * In this example we use a pose animation technology to animate the mask.
     * You could just as easily use unity animation system and control the animation timeline yourself,
     * or control the animations by direct manipulation of the bones. Whatever suits you best.
     * 
     * In general, we have one or two specific pose (animation) per Animation Unit (AU).
     * This setup was simply motived by its ease of use.
     * 
     * Finally you will notice that sometimes we do not use the full range or that we use some magic numbers.
     * These were hand tweaked so that we could better exagerate certain expressions. In that case we decided
     * to go benefit the user experience instead of plain data accuracy.
     */
    private void UpdateLipRaiser(float coef)
    {
        _morphCoefs[0] = coef;
        ModelAnimator.SetWeight(0, coef);
    }

    private void UpdateJawLowerer(float coef)
    {
        // The jaw lowerer animation unit has no negative range.
        _morphCoefs[1] = Mathf.Max(0, coef);
        ModelAnimator.SetWeight(1, Mathf.Max(0, coef));
    }

    private void UpdateLipStretcher(float coef)
    {
        // The lip stretcher animation has 2 animations simply because it was easier to design that way.
        // One represents the Animation Unit range [-1, 0] and the other is for [0, 1].
        _morphCoefs[2] = Mathf.Clamp(-1.5f * coef, 0, 1.5f);
        _morphCoefs[3] = Mathf.Clamp(coef, -0.7f, 1);
        ModelAnimator.SetWeight(2, Mathf.Clamp(-1.5f * coef, 0, 1.5f));
        ModelAnimator.SetWeight(3, Mathf.Clamp(coef, -0.7f, 1));
    }

    private void UpdateBrowLowerer(float coef)
    {
        _morphCoefs[4] = coef;
        ModelAnimator.SetWeight(4, coef);
    }

    private void UpdateLipCornerDepressor(float coef)
    {
        _morphCoefs[5] = Mathf.Clamp(coef, -0.15f, 1);
        ModelAnimator.SetWeight(5, Mathf.Clamp(coef, -0.15f, 1));
    }

    private void UpdateOuterBrowRaiser(float coef)
    {
        _morphCoefs[6] = coef;
        ModelAnimator.SetWeight(6, coef);
    }
    #endregion


    private void CheckForSpecialPoses()
    {
        if (IsHappy())
        {
            if (!Flames.activeSelf)
                Flames.SetActive(true);
        }
        else
        {
            if (Flames.activeSelf)
                Flames.SetActive(false);
        }
    }

    private bool IsHappy()
    {
        return _morphCoefs[1] > 0.6f && _morphCoefs[6] > 0.35f;
    }



}
