using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update

    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGranades;
    public Camera followCamera;

    public int ammo;
    public int coin;
    public int health;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGranades;


    float hAxis;
    float vAxis;

    bool rDown;
    bool jDown; 
    bool fDown;
    bool sDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDoubleJump;
    bool isSprint;
    bool isSwap;
    bool isFireReady;

    Vector3 moveVec;

    Rigidbody rigidbody;
    Animator animator;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Sprint();
        Interation();
        Swap();

    }
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        rDown = Input.GetButton("Run");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        sDown = Input.GetButtonDown("Sprint");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (rDown)
        {
            transform.position += moveVec * speed * Time.deltaTime;

        }
        else
        {
            transform.position += moveVec * speed * 0.3f * Time.deltaTime;
        }

        animator.SetBool("isWalk", moveVec != Vector3.zero);
        animator.SetBool("isRun", rDown);

    }
    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
        if(fDown)
        {
        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        if(Physics.Raycast(ray,out rayHit, 100))
        {
            Vector3 nextVec = rayHit.point - transform.position;
            nextVec.y = 0;
            transform.LookAt(transform.position + nextVec);
        }

        }
    }
    void Jump()
    {
        if (jDown && !isJump)
        {
            rigidbody.AddForce(Vector3.up * 15, ForceMode.Impulse);
            animator.SetBool("isJump", true);
            animator.SetTrigger("doJump");
            isJump = true;
        }
        else if (jDown && isJump && !isDoubleJump)
        {

            rigidbody.AddForce(Vector3.up * 20, ForceMode.Impulse);
            animator.SetTrigger("doDoubleJump");
            isDoubleJump = true;
        }
    }
    void Attack()
    {
        if (equipWeapon == null) return;
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;
        if (fDown && isFireReady && !isSprint && !isSwap)
        {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot" );
            fireDelay = 0;

        }
    }
    void Sprint()
    {
        if (sDown && !isSprint)
        {
            speed *= 2;
            animator.SetTrigger("doSprint");
            isSprint = true;
            Invoke("SprintOut", 1.5f);
        }

    }
    void SprintOut()
    {
        speed *= 0.5f;
        isSprint = false;

    }
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) return;


        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;
        if ((sDown1 || sDown2 || sDown3) && !isJump && !isSprint)
        {
            if (equipWeapon != null) equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("doSwap");

            isSwap = true;
            Invoke("SwapOut", 0.4f);
        }
    }
    void SwapOut()
    {

        isSwap = false;

    }

    void Interation()
    {
        if (iDown && nearObject != null)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            animator.SetBool("isJump", false);
            isJump = false;
            isDoubleJump = false;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGranades].SetActive(true);
                    hasGranades += item.value;
                    if (hasGranades > maxHasGranades)
                        hasGranades = maxHasGranades;
                    break;

            }
            Destroy(other.gameObject);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = other.gameObject;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
    }


}
