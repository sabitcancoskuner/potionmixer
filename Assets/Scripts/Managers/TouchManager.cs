using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class TouchManager : Singleton<TouchManager>
{
    #region Events
    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;
    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;
    #endregion

    private InputActions inputActions;
    private Camera mainCamera;

    protected override void Init()
    {
        inputActions = new InputActions();

        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        inputActions.Touch.PrimaryContact.started += context => StartTouchPrimary(context);
        inputActions.Touch.PrimaryContact.canceled += context => EndTouchPrimary(context);
    }

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnStartTouch != null)
        {
            OnStartTouch(
                Utils.ScreenToWorldPoint(mainCamera, inputActions.Touch.PrimaryPosition.ReadValue<Vector2>()),
                (float)context.startTime);
        }
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnEndTouch != null)
        {
            OnEndTouch(
                Utils.ScreenToWorldPoint(mainCamera, inputActions.Touch.PrimaryPosition.ReadValue<Vector2>()),
                (float)context.time);
        }
    }

    public Vector2 GetPrimaryPosition()
    {
        return Utils.ScreenToWorldPoint(mainCamera, inputActions.Touch.PrimaryPosition.ReadValue<Vector2>());
    }
}
