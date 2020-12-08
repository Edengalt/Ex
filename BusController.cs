using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class BusController : MonoBehaviour
{
    [SerializeField] private GameObject UpDownBody;
    [SerializeField] private float movementSpeed;
    [SerializeField] ParticleSystem RotationParticles;

    [SerializeField] AudioSource RotationSound;
    [SerializeField] AudioSource Win;


    private bool endGame;
    private Direction currentDirection;
    private Vector3 moveDelta;
    private float moveScale = 0f;
    private float turnCount;

    private bool hasRotated;
    private bool IsInsideCrossRoad;

    private LevelGenerator levelGenerator;
    private BusAnimator busAnimator;
    private Rigidbody rigidbody;

    private void Awake()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
        rigidbody = GetComponent<Rigidbody>();
        busAnimator = GetComponent<BusAnimator>();
    }

    private void Start()
    {    
        //Настройка направления
        currentDirection = levelGenerator.firstDirection;
        GetMoveDeltaFromDirection();
        transform.eulerAngles = GetRotationYFromDirection();

        //Add WiggleAnimation position random changing for bus body simulating moving
        StartCoroutine(WiggleAnimation(UpDownBody));
    }

    private void OnEnable()
    {
        GameManager.Instance.OnStart += StartLevel;
        levelGenerator.OnLevelGenerated += ResetLevel;
    }

    //Въехал в стену
    void OnCollisionEnter(Collision collision)
    {
        GameManager.Instance.OnLose();
        endGame = true;
        StopCoroutine(WiggleAnimation(UpDownBody));
        Debug.Log("Collision");
    }

    //Находится на перекрёстке
    private void OnTriggerStay(Collider other)
    {
        IsInsideCrossRoad = true;
    }

    //Выехал с перекрёстка
    private void OnTriggerExit(Collider other)
    {
        IsInsideCrossRoad = false;
        hasRotated = false;
    }

    void Update()
    {
        if (!endGame)
        {
            if (IsInsideCrossRoad)
            {
                if (!hasRotated)
                {
                    if (TurnLeftInput())
                    {
                        
                        if (currentDirection == Direction.Right)
                            currentDirection = Direction.Up;
                        else if (currentDirection == Direction.Left)
                            currentDirection = Direction.Down;
                        else if (currentDirection == Direction.Down)
                            currentDirection = Direction.Right;
                        else 
                            currentDirection = Direction.Left;

                        turnCount++;
                        GetMoveDeltaFromDirection();
                        busAnimator.TurnLeftAnimation();
                        hasRotated = true;

                        RotationParticles.Play();
                        RotationSound.Play();

                    }
                    if (TurnRightInput())
                    {
                        
                        if (currentDirection == Direction.Left)
                            currentDirection = Direction.Up;
                        else if (currentDirection == Direction.Right)
                            currentDirection = Direction.Down;
                        else if (currentDirection == Direction.Down)
                            currentDirection = Direction.Left;
                        else
                            currentDirection = Direction.Right;

                        turnCount++;
                        GetMoveDeltaFromDirection();
                        busAnimator.TurnRightAnimation();
                        hasRotated = true;

                        RotationParticles.Play();
                        RotationSound.Play();
                    }
                }
            }

            transform.position = Vector3.Lerp(transform.position, transform.position + moveDelta * moveScale, Time.deltaTime * movementSpeed);

            if (turnCount >= levelGenerator.Turns)
            {
                StartCoroutine(Finish());
            }

            
        }
        else
        {
            Debug.Log("GameOver");
        }        
    }

    private void StartLevel()
    {
        moveScale = 1f;
    }

    private void ResetLevel()
    {
        moveScale = 0f;
        transform.position = new Vector3(0f, 0f, 0f);

        currentDirection = levelGenerator.firstDirection;
        GetMoveDeltaFromDirection();
        transform.eulerAngles = GetRotationYFromDirection();
    }

    // TODO(vlad): Mobile Input, swipe
    private bool TurnLeftInput()
    {
        return Input.GetMouseButton(0) && Input.mousePosition.x < (Screen.width / 2) || Input.GetKeyDown(KeyCode.A);
    }

    private bool TurnRightInput()
    {
        return Input.GetMouseButton(0) && Input.mousePosition.x > (Screen.width / 2) || Input.GetKeyDown(KeyCode.D);
    }

    public void GetMoveDeltaFromDirection()
    {
        Vector3 oldMoveDelta = moveDelta;
        Vector3 newMoveDelta = moveDelta;
        switch (currentDirection)
        {
            case Direction.Up:
                newMoveDelta = new Vector3(0.0f, 0.0f, 1.0f);
                break;
            case Direction.Right:
                newMoveDelta = new Vector3(1.0f, 0.0f, 0.0f);
                break;
            case Direction.Left:
                newMoveDelta = new Vector3(-1.0f, 0.0f, 0.0f);
                break;
            case Direction.Down:
                newMoveDelta = new Vector3(0.0f, 0.0f, -1f);
                break;
        }

        StartCoroutine(LerpMoveDelta(oldMoveDelta, newMoveDelta, 1.5f));
    }

    private Vector3 GetRotationYFromDirection()
    {
        switch (currentDirection)
        {
            case Direction.Up:
                return new Vector3(0.0f, 90.0f, 0.0f);
            case Direction.Right:
                return new Vector3(0.0f, 180.0f, 0.0f);
            case Direction.Left:
                return Vector3.zero;

            default: return Vector3.zero;
        }
    }

    private IEnumerator Finish()
    {
        //TODO(alex): Bus slows and stops
        
        yield return new WaitForSeconds(2f);
        moveDelta = Vector3.Lerp(moveDelta, new Vector3(0f, 0f, 0f), 0.5f);
        GameManager.Instance.OnFinish();
        Win.Play();
    }

    IEnumerator WiggleAnimation(GameObject obj)
    {
        while (!endGame)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.1f, 0.3f));
            float UpDownBorders = Mathf.Clamp(obj.transform.position.y + UnityEngine.Random.Range(-0.1f, 0.1f), -0.1f, 0.1f);
            obj.transform.DOMoveY(UpDownBorders, 1f);
            
        }
    }
    
    IEnumerator LerpMoveDelta(Vector3 origin, Vector3 target, float time)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            moveDelta = Vector3.Lerp(origin, target, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}