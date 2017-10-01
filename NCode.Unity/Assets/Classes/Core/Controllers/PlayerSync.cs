using System.IO;
using NCode.Client;
using NCode.Core;
using NCode.Core.Utilities;
using UnityEngine;

namespace Assets.Classes.Core.Controllers
{
    [RequireComponent(typeof(NEntityLink))]
    public class PlayerSync : PlayerController
    {

    
    
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

        [SerializeField] protected Vector2 mLastInput;
        [SerializeField] protected Quaternion lastRotation;
        [SerializeField] protected float mLastInputSend = 0f;
        [SerializeField] protected float mNextRB = 0f;
        [SerializeField] protected Vector3 syncPosition;
        [SerializeField] protected Quaternion syncRotation;
        [SerializeField] protected Vector2 syncAxis;
        [SerializeField] private float LerpRate = 15f;

        private float InputThreshold = 0.1f;
        private float RotationThreshold = 10f;

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            lastRotation = transform.rotation;
            Cursor.visible = false;
            if (!nb.IsMine)
            {
                camera.enabled = false;
            }
       
        }

        // Update is called once per frame
        protected override void Update()
        {
            Lerp();
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

                //Movement input X, Y  axis. Checks to see if it has changed enough
                if (Compare.FloatEqual(mLastInput.x, mInput.x, InputThreshold) || Compare.FloatEqual(mLastInput.y, mInput.y, InputThreshold))
                {
                    mLastInputSend = time;
                    mLastInput = mInput;
                    nb.SendRfc(1, Packet.ForwardToAll, false, mInput);
                }

                if (Quaternion.Angle(transform.rotation, lastRotation) > RotationThreshold)
                {
                    lastRotation = transform.rotation;
                    nb.SendRfc(2, Packet.ForwardToAll, false, transform.rotation);
                }
         
                // Since the input is sent frequently, rigidbody only needs to be corrected every couple of seconds.
                // Faster-paced games will require more frequent updates.
                if (mNextRB < time && !jump)
                {
                    mNextRB = time + 1f / rigidbodyUpdates;               
                    nb.SendRfc(3, Packet.ForwardToAll, false, transform.position);

                    //Sets the PlayerObject position on the server
                    BinaryWriter writer = NetworkManager.BeginSend(Packet.ForwardToAll);
                    writer.WriteObject(NUnityTools.Vector3ToV3(transform.position));
                    NetworkManager.EndSend(true);

                }
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        
        }

        void Lerp()
        {
            if (!nb.IsMine)
            {           

                transform.rotation = Quaternion.Lerp(transform.rotation, syncRotation, Time.deltaTime * LerpRate);
            }
        } 

        [RFC(1)]
        protected void SetAxis(Vector2 v)
        {
            mInput = v;    
        }

        [RFC(2)]
        protected void SetDirection(Quaternion v)
        {
            syncRotation = v;
        }

        [RFC(3)]
        protected void SetRB(Vector3 pos)
        {
            transform.position = pos;
        }

    }
}
