using UnityEngine;
using System;
using NCode;
using NCode.Utilities;
using NCode.Core;

[RequireComponent(typeof(NetworkBehaviour))]
public class PlayerSync : PlayerController
{

    float LastPacketTime = 0;
    float LerpRate = 30f;
    float LastVerticalAxisValue = 0;
    float LastHorizontalAxisValue = 0;


    [Range(1f, 20f)]
    public float inputUpdates = 10f;

    /// <summary>
    /// Maximum number of updates per second when synchronizing the rigidbody.
    /// </summary>

    [Range(0.25f, 5f)]
    public float rigidbodyUpdates = 1f;

    /// <summary>
    /// We want to cache the network object (TNObject) we'll use for network communication.
    /// If the script was derived from TNBehaviour, this wouldn't have been necessary.
    /// </summary>

    [SerializeField]
    protected Vector2 mLastInput;
    [SerializeField]
    protected Quaternion lastDirection;
    [SerializeField]
    protected float mLastInputSend = 0f;
    [SerializeField]
    protected float mNextRB = 0f;
    [SerializeField]
    protected Vector3 syncPosition;
    [SerializeField]
    protected Quaternion syncRotation;
    [SerializeField]
    protected Vector2 syncAxis;

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        lastDirection = transform.rotation;
        Cursor.visible = false;
        if (!nb.IsMine)
        {
            camera.enabled = false;
        }
       
    }

    // Update is called once per frame
    protected override void Update()
    {

        if (!nb.IsMine) return;

        
        base.Update();

        float time = Time.time;
        float delta = time - mLastInputSend;
        float delay = 1f / inputUpdates;

        // Don't send updates more than 20 times per second
        if (delta > 0.05f)
        {
            // The closer we are to the desired send time, the smaller is the deviation required to send an update.
            float threshold = Mathf.Clamp01(delta - delay) * 0.5f;

            if (Compare.FloatEqual(lastDirection.y, transform.rotation.y, 0.02f))
            {
                Tools.Print("Rotating");
                lastDirection = transform.rotation;
                nb.SendRFC(2, Packet.RFC, false, transform.rotation);
            }
            
            // If the deviation is significant enough, send the update to other players
            if (Compare.FloatEqual(mLastInput.x, mInput.x, threshold) || Compare.FloatEqual(mLastInput.y, mInput.y, threshold))
            {
                mLastInputSend = time;
                mLastInput = mInput;
                nb.SendRFC(1, Packet.RFC, false, mInput);
            }

            //Wont send anything unless its changed
            if (lastBool != running)
            {
                nb.SendRFC(5, Packet.RFC, false, running);
                lastBool = running;
            }

            // Since the input is sent frequently, rigidbody only needs to be corrected every couple of seconds.
            // Faster-paced games will require more frequent updates.
            if (mNextRB < time && !jump)
            {
                mNextRB = time + 1f / rigidbodyUpdates;
                nb.SendRFC(3, Packet.RFC, false, gameObject.transform.position, gameObject.transform.rotation);
            }

            if (jump)
            {
                //Sends the Jump trigger
                nb.SendRFC(4, Packet.RFC, false);
            }
            
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        Lerp();
    }

    void Lerp()
    {
        if (!nb.IsMine)
        {
            mInput = Vector2.Lerp(mInput, syncAxis, Time.deltaTime * LerpRate);
            transform.rotation = Quaternion.Lerp(transform.rotation, syncRotation, Time.deltaTime * LerpRate);
        }
    }

    [RFC(1)]
    protected void SetAxis(Vector2 v)
    {
        syncAxis = v;    
    }
    [RFC(2)]
    protected void SetDirection(Quaternion v)
    {
        syncRotation = v;
    }
    [RFC(3)]
    protected void SetRB(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;     
    }
    [RFC(4)]
    protected void JumpSet()
    {
        Tools.Print("JumpSet FAM");
        jump = true;
    }
    [RFC(5)]
    protected void RunSet(bool run)
    {
        running = run;
    }

}
