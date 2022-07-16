using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FollowCam : MonoBehaviour
{
    public Transform targetTr;
    public Transform camTr;

    private float MaxDistance = 3.5f;
    private float height = 2.0f;

    Transform rigTr;

    //public float damping = 10.0f;

    //public float targetOffset = 2.0f;

    //private Vector3 velocity = Vector3.zero;

    //private float mouseY = 0.0f;

    //public float yMinLimit = -20f;
    //public float yMaxLimit = 80f;

    RaycastHit hit;

    public GameObject Crosshair;

    // Start is called before the first frame update
    void Start()
    {

        rigTr = GetComponent<Transform>();

        // Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        if(GameManager.instance.aimMode == true)
        {
            Crosshair.SetActive(true);
        }
        else if(GameManager.instance.aimMode == false)
        {
            Crosshair.SetActive(false);
        }

        float mouseX = Input.GetAxis("Mouse X");

        Debug.DrawRay(rigTr.position, (camTr.position - rigTr.position), Color.blue);

        if(Physics.Raycast(rigTr.position, (camTr.position - rigTr.position), out hit, MaxDistance, 1 << 8))
        {

            if (hit.point != Vector3.zero) // 레이캐스트 성공시
            {
                if (GameManager.instance.aimMode == false)
                {
                    camTr.localPosition = Vector3.Lerp(camTr.localPosition, camTr.localPosition + Vector3.forward, Time.deltaTime * 10f);
                    camTr.position = hit.point;
                    camTr.Translate(Vector3.forward * 0.5f);
                }
                else if(GameManager.instance.aimMode == true)
                {
                    camTr.localPosition = Vector3.Lerp(camTr.localPosition, new Vector3(0.5f, height - 0.5f, 1.0f), Time.deltaTime * 10f);
                    //camTr.position = hit.point;
                    camTr.Translate(Vector3.forward * 0.5f);
                }
            }
        }
        else
        {
            if (GameManager.instance.aimMode == false)
            {
                camTr.localPosition = Vector3.Lerp(camTr.localPosition, new Vector3(0, height, -MaxDistance), Time.deltaTime * 5f);
            }
            else if(GameManager.instance.aimMode == true)
            {
                camTr.localPosition = Vector3.Lerp(camTr.localPosition, new Vector3(0.5f, height - 0.5f, -MaxDistance + 1.5f), Time.deltaTime * 5f);
            }
        }

        rigTr.transform.position = targetTr.transform.position;


        if (mouseX != 0)
        {
            rigTr.Rotate(Vector3.up * 80.0f * Time.deltaTime * mouseX, Space.World);
        }

        // camTr.LookAt(targetTr.position + (targetTr.up * targetOffset));
    }
}

