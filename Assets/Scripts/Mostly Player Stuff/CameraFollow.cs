using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform Player;

    [SerializeField] Vector3 PlayerPos;
    [SerializeField] Vector3 CameraPos;

    [SerializeField] Transform Bound1;
    [SerializeField] Transform Bound2;

    [SerializeField]
    private bool lockY;
    [SerializeField]
    private bool lockX;

    private void Update()
    {
        PlayerPos = Player.position;
    }

    private void LateUpdate()
    {
        //Clamping cam.
        float XDir = lockX ? Mathf.Clamp(PlayerPos.x, Bound1.position.x, Bound2.position.x) : PlayerPos.x;

        float yDir = lockY ? Mathf.Clamp(PlayerPos.y, Bound1.position.y, Bound2.position.y) : PlayerPos.y;

        transform.position = new Vector3(XDir, yDir, -10);
    }
}