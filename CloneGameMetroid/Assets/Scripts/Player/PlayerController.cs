﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D theRB;

    [Header("Di chuyển cơ bản")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Kiểm tra ground")]
    public Transform groundPoint;
    private bool isOnGround;
    public LayerMask whatIsGround;

    public Animator anim;

    [Header("Tấn công")]
    public BulletController shotToFire;
    public Transform shotPoint;

    private bool canDoubleJump;

    [Header("Lướt")]
    public float dashSpeed, dashTime;
    private float dashCounter;
    public float waitAfterDashing;
    private float dashRechargeCounter;

    [Header("Tàn Ảnh")]
    public SpriteRenderer theSR, afterImage;
    public float afterImageLifetime, timeBetweenAfterImages;
    private float afterImageCounter;
    public Color afterImageColor;

    [Header("Chuyển dạng bóng")]
    public GameObject standing, ball;
    public float waitToBall;
    private float ballCounter;
    public Animator ballAnim;

    [Header("Đặt bom")]
    public Transform bombPoint;
    public GameObject bomb;

    private PlayerAbilityTracker abilities;

    public bool canMove;

    // Start is called before the first frame update
    void Start()
    {
        abilities = GetComponent<PlayerAbilityTracker>();

        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && Time.timeScale != 0)
        {
            //Lướt
            if (dashRechargeCounter > 0)
            {
                dashRechargeCounter -= Time.deltaTime;
            }
            else
            {

                if (Input.GetButtonDown("Fire2") && standing.activeSelf && abilities.canDash)
                {
                    dashCounter = dashTime;

                    ShowAfterImage();

                    AudioManager.instance.PlaySFXAdjusted(7);
                }
            }

            if (dashCounter > 0)
            {
                dashCounter = dashCounter - Time.deltaTime;

                theRB.velocity = new Vector2(dashSpeed * transform.localScale.x, theRB.velocity.y);

                afterImageCounter -= Time.deltaTime;
                if (afterImageCounter <= 0)
                {
                    ShowAfterImage();
                }

                dashRechargeCounter = waitAfterDashing;
            }
            else
            {

                //di chuyển theo 2 hướng
                theRB.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed, theRB.velocity.y);

                //thay đổi hướng xoay player
                if (theRB.velocity.x < 0)
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else if (theRB.velocity.x > 0)
                {
                    transform.localScale = Vector3.one;
                }
            }

            //Kiểm tra xem có đang ở mặt đất không
            isOnGround = Physics2D.OverlapCircle(groundPoint.position, .2f, whatIsGround);

            //Nhảy
            if (Input.GetButtonDown("Jump") && (isOnGround || (canDoubleJump && abilities.canDoubleJump)))
            {
                if (isOnGround)
                {
                    canDoubleJump = true;

                    AudioManager.instance.PlaySFXAdjusted(12);
                }
                else
                {
                    canDoubleJump = false;

                    anim.SetTrigger("doubleJump");

                    AudioManager.instance.PlaySFXAdjusted(9);
                }

                theRB.velocity = new Vector2(theRB.velocity.x, jumpForce);
            }

            //Tấn công
            if (Input.GetButtonDown("Fire1"))
            {
                if (standing.activeSelf)
                {
                    Instantiate(shotToFire, shotPoint.position, shotPoint.rotation).moveDir = new Vector2(transform.localScale.x, 0f);

                    anim.SetTrigger("shotFired");

                    AudioManager.instance.PlaySFXAdjusted(14);
                }
                else if (ball.activeSelf && abilities.canDropBomb)
                {
                    Instantiate(bomb, bombPoint.position, bombPoint.rotation);

                    AudioManager.instance.PlaySFXAdjusted(13);

                }
            }

            //Chuyển sang dạng bóng
            if (!ball.activeSelf)
            {
                if (Input.GetAxisRaw("Vertical") < -.9f && abilities.canBecomeBall)
                {
                    ballCounter -= Time.deltaTime;
                    if (ballCounter <= 0)
                    {
                        ball.SetActive(true);
                        standing.SetActive(false);

                        AudioManager.instance.PlaySFX(6);
                    }

                }
                else
                {
                    ballCounter = waitToBall;
                }
            }
            else
            {
                //Chuyển về dạng người
                if (Input.GetAxisRaw("Vertical") > .9f)
                {
                    ballCounter -= Time.deltaTime;
                    if (ballCounter <= 0)
                    {
                        ball.SetActive(false);
                        standing.SetActive(true);

                        AudioManager.instance.PlaySFX(10);
                    }

                }
                else
                {
                    ballCounter = waitToBall;
                }
            }
        } else
        {
            theRB.velocity = Vector2.zero;
        }

        //xử lý hoạt ảnh
        if (standing.activeSelf)
        {
            anim.SetBool("isOnGround", isOnGround);
            anim.SetFloat("speed", Mathf.Abs(theRB.velocity.x));
        }

        if(ball.activeSelf)
        {
            ballAnim.SetFloat("speed", Mathf.Abs(theRB.velocity.x));
        }
    }

    //hàm hiển thị tàn ảnh
    public void ShowAfterImage()
    {
        SpriteRenderer image = Instantiate(afterImage, transform.position, transform.rotation);
        image.sprite = theSR.sprite;
        image.transform.localScale = transform.localScale;
        image.color = afterImageColor;

        Destroy(image.gameObject, afterImageLifetime);

        afterImageCounter = timeBetweenAfterImages;
    }
}
