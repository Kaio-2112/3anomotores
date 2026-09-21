using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Configurações Multi-Jogador")]
        [Tooltip("Defina 1 para Jogador 1 e 2 para Jogador 2")]
        public int PlayerID = 1;
        [Tooltip("Aumento de velocidade base e de corrida por moeda coletada")]
        public float BonusVelocidadePorMoeda = 0.5f;

        [Header("Tags dos Coletáveis")]
        public string TagMoeda = "Coin";
        public string TagEstrela = "Star";

        [Header("Player")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        [Space(10)]
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Cinemachine & Câmera")]
        [SerializeField] private GameObject _mainCamera;
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;
        public Vector2 LookSensitivity = new Vector2(1.5f, 1.0f);

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private Vector3 _cameraStartingLocalPosition;
        private Quaternion _cameraStartingLocalRotation;

        public bool IsRespawning { get; set; } = false;

        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private int _moedasColetadas = 0;
        private int _estrelasColetadas = 0;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;

        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput != null && _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
            {
                Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                foreach (Camera cam in cameras)
                {
                    if ((PlayerID == 1 && cam.gameObject.name.Contains("1")) ||
                        (PlayerID == 2 && cam.gameObject.name.Contains("2")))
                    {
                        _mainCamera = cam.gameObject;
                        break;
                    }
                }

                if (_mainCamera == null && Camera.main != null)
                {
                    _mainCamera = Camera.main.gameObject;
                }
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = transform.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#endif
            VincularCinemachine();
            AssignAnimationIDs();

            if (CinemachineCameraTarget != null)
            {
                _cameraStartingLocalPosition = CinemachineCameraTarget.transform.localPosition;
                _cameraStartingLocalRotation = CinemachineCameraTarget.transform.localRotation;
            }

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            GroundedCheck();
            JumpAndGravity();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        // --- MÉTODOS DE COLETA DE ITENS ---
        public void ColetarMoeda()
        {
            _moedasColetadas++;
            MoveSpeed += BonusVelocidadePorMoeda;
            SprintSpeed += BonusVelocidadePorMoeda;

            PlayerOM.OnCoinCountChanged?.Invoke(PlayerID, _moedasColetadas);
        }

        public void ColetarEstrela()
        {
            _estrelasColetadas++;
            PlayerOM.OnStarCountChanged?.Invoke(PlayerID, _estrelasColetadas);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(TagMoeda))
            {
                Destroy(other.gameObject);
                ColetarMoeda();
            }
            else if (other.CompareTag(TagEstrela))
            {
                Destroy(other.gameObject);
                ColetarEstrela();
            }
        }

        private void VincularCinemachine()
        {
            string nomeVirtualCam = (PlayerID == 1) ? "PlayerFollowCamera1" : "PlayerFollowCamera2";
            GameObject vcamObj = GameObject.Find(nomeVirtualCam);

            if (vcamObj == null)
            {
                Debug.LogError($"[Cinemachine ERRO] Não foi encontrada nenhuma Câmera Virtual chamada '{nomeVirtualCam}' na cena!");
                return;
            }

            Transform rootFilho = transform.Find("PlayerCameraRoot");
            if (rootFilho != null)
            {
                CinemachineCameraTarget = rootFilho.gameObject;
            }

            var vcamV3 = vcamObj.GetComponent<Unity.Cinemachine.CinemachineCamera>();
            if (vcamV3 != null)
            {
                vcamV3.Target.TrackingTarget = CinemachineCameraTarget.transform;
                vcamV3.Target.LookAtTarget = null;
                return;
            }

            var vcamV2 = vcamObj.GetComponent<Unity.Cinemachine.CinemachineVirtualCamera>();
            if (vcamV2 != null)
            {
                vcamV2.Follow = CinemachineCameraTarget.transform;
                vcamV2.LookAt = null;
                return;
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            if (IsRespawning)
            {
                _cinemachineTargetYaw = transform.eulerAngles.y;
                _cinemachineTargetPitch = 0f;
                CinemachineCameraTarget.transform.localPosition = _cameraStartingLocalPosition;
                CinemachineCameraTarget.transform.localRotation = _cameraStartingLocalRotation;
                IsRespawning = false;
                return;
            }

            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * LookSensitivity.x;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * LookSensitivity.y;
            }
            else if (_input.move.sqrMagnitude >= _threshold)
            {
                _cinemachineTargetYaw = Mathf.LerpAngle(_cinemachineTargetYaw, transform.eulerAngles.y, Time.deltaTime * 4.0f);
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed; 
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                float targetRotationAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                
                if (_mainCamera != null)
                {
                    targetRotationAngle += _mainCamera.transform.eulerAngles.y;
                }

                _targetRotation = targetRotationAngle;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator) _animator.SetBool(_animIDJump, true);
                }
                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;
                if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
                else if (_hasAnimator) _animator.SetBool(_animIDFreeFall, true);
                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity) _verticalVelocity += Gravity * Time.deltaTime;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void ResetCameraRotation(float targetYaw)
        {
            _cinemachineTargetYaw = targetYaw;
            _cinemachineTargetPitch = 0f;
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f);
        }
    }
}