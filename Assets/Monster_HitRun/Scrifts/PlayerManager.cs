using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager PlayerManagerIstance;
    private float distance = 0;
    private Vector3 player;

    public int lvPlayer;
    public Text textlv;

    public Camera cameramain;
    public Transform Ballpos;

    public float speed;
    public float swipespees;
    private Transform localtrans;
    public Vector3 lastMouPos;
    private Vector3 mousePos;
    private Vector3 newPosfortrans;
    private Touch touch;
    private float speedleftright = 0.016f;

    public bool Move = true;
    public Animator anim;
    private Rigidbody rigidbody;
    private Collider collider;

    public PathType pathsystem = PathType.Linear;
    private Vector3[] pathval = new Vector3[8];
    private GameObject[] pathval_ = new GameObject[8];
    private List<GameObject> listpoint = new List<GameObject>();
    private List<Vector3> listvector = new List<Vector3>();


    public bool therotation = true;
    public bool thewall = false;

    private void Awake()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
        collider = gameObject.GetComponent<Collider>();
        anim = GetComponent<Animator>();
        PlayerManagerIstance = this;

    }
    private void Start()
    {
        therotation = true;
        localtrans = GetComponent<Transform>();
        textlv.text = "Lv " + lvPlayer.ToString();
        thewall = false;
    }
    void Update()
    {
        transform.position = new Vector3((Mathf.Clamp(transform.position.x, distance - 4.2f, distance + 4.2f)), transform.position.y, transform.position.z);
        textlv.text = "Lv " + lvPlayer.ToString();

        if (MenuManager.MenuManagerIstance.GameStace && Move)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.MoveTowards(transform.position.z, 1000, speed * Time.deltaTime));
            MovePlayer();
        }

        var ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 13
            ))
        {
            Debug.DrawRay(transform.position, transform.forward * 13, Color.red);
            if (hit.transform.gameObject.tag == "enemies")
            {
                Enemies shot = hit.transform.gameObject.GetComponent<Enemies>();
                Collider rgenemies = hit.transform.gameObject.GetComponent<Collider>();
                if (!shot.shotted)
                {
                    if (lvPlayer >= shot.lvenemies)
                    {
                        rgenemies.isTrigger = true;
                        Pool_manager.Pool_managerInstance.spawnpool_enemy("bong", Ballpos, hit.transform.gameObject);
                        shot.shotted = true;
                        anim.SetLayerWeight(1, 1f);
                    }

                }

            }

        }
        else
            anim.SetLayerWeight(1, 0f);

    }
    public void MovePlayer()
    {
        PlayRun();
        if (Input.GetMouseButton(0))
        {
            mousePos = cameramain.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 18));
            float xDiff = mousePos.x - lastMouPos.x;

            if (thewall)
            {
                newPosfortrans.x = localtrans.position.x;
            }
            else
            {
                //if (xDiff < 0.01)
                {
                    newPosfortrans.x = localtrans.position.x + xDiff * swipespees * Time.deltaTime;
                    newPosfortrans.y = localtrans.position.y;
                    newPosfortrans.z = localtrans.position.z;
                    localtrans.position = newPosfortrans/* + localtrans.forward * speed * Time.deltaTime*/;
                    // vector direct
                    //trai -a phai +a;
                    // dis lastmouse mouse;
                    //a- ;
                    //Debug.Log("e diff" + xDiff);
                    lastMouPos = mousePos;
                }

                //if (xDiff >= 0 && xDiff < 0.1f)
                //    transform.rotation = Quaternion.Euler(0, 30, 0);
                //if (xDiff < 0 && xDiff > -0.1f)
                //    transform.rotation = Quaternion.Euler(0, -30, 0);

            }

        }
        if (!thewall)
        {

            if (Input.touchCount > 0)
            {
                Debug.Log("hehehe");
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    transform.position = new Vector3(transform.position.x + touch.deltaPosition.x * speedleftright, transform.position.y, transform.position.z);

                }

            }
            
            if (transform.position.y > 3.5)
            {
                Debug.Log("nhay xuong");
                rigidbody.velocity = new Vector3(0, -2.5f, 0);
            }
            
            }
        if (transform.position.y <= .05f)
        {
            anim.SetLayerWeight(1, 0f);
            anim.SetBool("IsRun", true);
            anim.SetBool("IsJump", false);
            anim.SetBool("IsJumpWall", false);
        }
        if (transform.position.y < -0.3f)
        {
            gameObject.SetActive(false);
            MenuManager.MenuManagerIstance.GameStace = false;
            MenuManager.MenuManagerIstance.BonusEndgame.gameObject.SetActive(true);
        }
        if (lvPlayer <= 0)
        {
            gameObject.SetActive(false);
            MenuManager.MenuManagerIstance.GameStace = false;
            MenuManager.MenuManagerIstance.BonusEndgame.gameObject.SetActive(true);
        }

    }

    public void PlayRun()
    {
        anim.SetBool("IsRun", true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag ("Lo_xo"))
        {
            rigidbody.velocity = new Vector3(0, 10f, 0);
            Debug.Log("aaaa");
            anim.SetBool("IsJump", true);
            anim.SetBool("IsRun", false);

            rigidbody.velocity = new Vector3(0, 8.5f, -3);
            Debug.Log("aaaa");
            anim.SetBool("IsJump", true);
            anim.SetBool("IsRun", false);

        }
        if (other.CompareTag("Lo_xo_right"))
        {
            therotation = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);

            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.MoveTowards(transform.position.z, 1000, speed * Time.deltaTime));
            Cameractl.CameractlIstance.rotationcamera = true;
            Cameractl.CameractlIstance.right = true;
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x + 4.2f, transform.position.y + 1f, transform.position.z), 50 * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, 25f), 50f * Time.deltaTime);
            rigidbody.isKinematic = true;
            thewall = true;

        }
        if (other.CompareTag("Lo_xo_left"))
        {
            therotation = false;
            Cameractl.CameractlIstance.rotationcamera = true;
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x + -4.2f, transform.position.y + 1f, transform.position.z), 50 * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, -25f), 50f * Time.deltaTime);
            rigidbody.isKinematic = true;
            thewall = true;

            //transform.position = new Vector3(transform.position.x + -3.6f, transform.position.y + 1.2f, transform.position.z);
            //transform.Rotate(0, 0, -25f);
            //rigidbody.isKinematic = true;
        }
        //if (other.transform.tag == "Trap_gai")
        //{
        //    gameObject.SetActive(false);
        //    MenuManager.MenuManagerIstance.GameStace = false;
        //    MenuManager.MenuManagerIstance.YouLose.gameObject.SetActive(true);

        //}
        if (other.CompareTag( "in"))
        {

            Destroy(other.gameObject);
            Cameractl.CameractlIstance.flash = true;
            player = transform.position;
            distance = 500;
            transform.position = new Vector3(transform.position.x + 500f, transform.position.y, transform.position.z);

        }
        if (other.CompareTag("out"))
        {
            distance = 0;
            transform.position = player;
            Cameractl.CameractlIstance.flash = true;
        }

        if (other.CompareTag("road_1"))
        {
            GameObject child = other.transform.GetChild(0).gameObject;
            toList(child);
            MovePaval(pathval_);
        }
        if (other.CompareTag("road_2_1"))
        {

            toList(other.gameObject);
            MovePaval(pathval_);
        }
        if (other.CompareTag ("road_2_2"))
        {
            toList(other.gameObject);
            MovePaval(pathval_);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Plane"))
        {

            if (lvPlayer > 0 && lvPlayer <= 100)
            {
                //transform.localScale = new Vector3(33, 33, 33);//scale0%
                //speed = 14;
            }
            if (lvPlayer > 100 && lvPlayer <= 200)
            {
                transform.localScale = new Vector3(36, 36, 36);//scale 10%
                speed = 15;
            }
            if (lvPlayer > 200 && lvPlayer <= 300)
            {
                transform.localScale = new Vector3(39, 39, 39);//scale 20%
                speed = 16;
            }
            if (lvPlayer > 300 && lvPlayer <= 400)
            {
                transform.localScale = new Vector3(42, 42, 42); //scale 30%
                speed = 16;
            }
            if (lvPlayer > 400 && lvPlayer <= 500)
            {
                transform.localScale = new Vector3(45, 45, 45); //scale 40%
                speed = 16.5f;
            }
            if (lvPlayer > 500)
            {
                transform.localScale = new Vector3(48, 48, 48);//scale  50%
                speed = 16.5f;
            }
        }
        if (collision.transform.CompareTag("Lo_xo"))
        {
            rigidbody.velocity = new Vector3(0, 8.5f, -3);
            Debug.Log("aaaa");
            anim.SetBool("IsJump", true);
            anim.SetBool("IsRun", false);

        }
        if (collision.transform.CompareTag("Lo_xo_Finish"))
        {
            rigidbody.velocity = new Vector3(0, 11f, 0);
            speed =19;

            Debug.Log("aaaa");
            anim.SetBool("IsJump", true);
            anim.SetBool("IsRun", false);

        }
        if (collision.transform.CompareTag( "Trap_gai"))
        {
            Debug.Log("vao trap gai");
            Move = false;
            //rigidbody.AddForce(new Vector3(0, 0, -1) * 500);
            rigidbody.isKinematic = true;
            collider.isTrigger = true;
            StartCoroutine(timeskip()); 

        }
    }

    public void ToListVector(GameObject[] pavalpoint)
    {
        for (int i = 0; i < pavalpoint.Length; i++)
        {
            pathval[i] = pavalpoint[i].transform.position;
        }
    }
    public void MovePaval(GameObject[] pavalpoint)
    {
        Move = false;
        ToListVector(pavalpoint);
        rigidbody.isKinematic = true;
        transform.DOPath(pathval, 1.5f, pathsystem).SetEase(Ease.Linear).OnComplete(() =>
       {

           Move = true;
           rigidbody.isKinematic = false;
       });
    }
    IEnumerator timeskip()
    {  
        lvPlayer -= 10;
        yield return new WaitForSeconds(0.15f);
        Move = true;
        
        yield return new WaitForSeconds(0.5F);
        rigidbody.isKinematic = false;
        collider.isTrigger = false;
    }
    public void toList(GameObject dt)
    {
        Debug.Log(dt.transform.childCount);
        for (int i = 0; i < dt.transform.childCount; i++)
        {
            pathval_[i] = dt.transform.GetChild(i).gameObject;
        }
    }
    
}











