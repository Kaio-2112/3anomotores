using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Configuração de Controlo do Teclado")]
        [Tooltip("Marque se este GameObject for o Jogador 2 para usar as Setinhas")]
        public bool isPlayerTwo = false;

        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        private void Update()
        {
            // Leitura direta do teclado para evitar bloqueios do PlayerInput em teclado único
            if (Keyboard.current != null)
            {
                float x = 0f;
                float y = 0f;

                if (!isPlayerTwo)
                {
                    // Jogador 1: WASD + Espaço + Shift
                    if (Keyboard.current.wKey.isPressed) y += 1f;
                    if (Keyboard.current.sKey.isPressed) y -= 1f;
                    if (Keyboard.current.dKey.isPressed) x += 1f;
                    if (Keyboard.current.aKey.isPressed) x -= 1f;

                    if (Keyboard.current.spaceKey.wasPressedThisFrame) jump = true;
                    sprint = Keyboard.current.leftShiftKey.isPressed;
                }
                else
                {
                    // Jogador 2: Setinhas + Enter/Numpad0 + RightShift
                    if (Keyboard.current.upArrowKey.isPressed) y += 1f;
                    if (Keyboard.current.downArrowKey.isPressed) y -= 1f;
                    if (Keyboard.current.rightArrowKey.isPressed) x += 1f;
                    if (Keyboard.current.leftArrowKey.isPressed) x -= 1f;

                    if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame) jump = true;
                    sprint = Keyboard.current.rightShiftKey.isPressed;
                }

                move = new Vector2(x, y).normalized;
            }
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value) { MoveInput(value.Get<Vector2>()); }
        public void OnLook(InputValue value) { if (cursorInputForLook) LookInput(value.Get<Vector2>()); }
        public void OnJump(InputValue value) { JumpInput(value.isPressed); }
        public void OnSprint(InputValue value) { SprintInput(value.isPressed); }
#endif

        public void MoveInput(Vector2 newMoveDirection) { move = newMoveDirection; }
        public void LookInput(Vector2 newLookDirection) { look = newLookDirection; }
        public void JumpInput(bool newJumpState) { jump = newJumpState; }
        public void SprintInput(bool newSprintState) { sprint = newSprintState; }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}