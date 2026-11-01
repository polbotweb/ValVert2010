using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(CharacterController))]
public class SimpleFPS : MonoBehaviour {
    public float speed = 4f, runSpeed = 8f, mouseSens = 0.1f, gravity = -9.81f, jump = 1.2f;
    public Transform cam;
    float pitch, vy; CharacterController cc;
    void Start(){ cc=GetComponent<CharacterController>(); Cursor.lockState=CursorLockMode.Locked; }
    void Update(){
        var kb=Keyboard.current; var m=Mouse.current;
        if(m!=null){ Vector2 d=m.delta.ReadValue()*mouseSens; transform.Rotate(0,d.x,0);
            pitch=Mathf.Clamp(pitch-d.y,-85,85); if(cam) cam.localEulerAngles=new Vector3(pitch,0,0);}
        Vector3 mv=Vector3.zero;
        if(kb!=null){ if(kb.wKey.isPressed)mv+=transform.forward; if(kb.sKey.isPressed)mv-=transform.forward;
            if(kb.dKey.isPressed)mv+=transform.right; if(kb.aKey.isPressed)mv-=transform.right;
            if(cc.isGrounded&&kb.spaceKey.wasPressedThisFrame) vy=Mathf.Sqrt(jump*-2f*gravity);}
        float s=(kb!=null&&kb.leftShiftKey.isPressed)?runSpeed:speed;
        if(cc.isGrounded&&vy<0)vy=-2f; vy+=gravity*Time.deltaTime;
        cc.Move((mv.normalized*s+Vector3.up*vy)*Time.deltaTime);
    }
}
