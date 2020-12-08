using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class GameControl : MonoBehaviour
{
    private CubePos nowCube = new CubePos(0, 1, 0);
    public float CubeChangePlaceSpees = 0.5f;
    public Transform cubeToPlace;
    private float camMovetoYPosition, CamMoveSpeed = 2f;

    public GameObject [] cubesToCreate;
    public GameObject cubeToCreate, allCubes, VFX;
    private Rigidbody allCubesRb;
    public GameObject[] CanvasStartPage;
    private Coroutine showCubePlace;

    private List<GameObject> possibleCubesToCreate = new List<GameObject>();

    public Text ScoreTxt;

    public Color[] bGColor;
    private Color toCameraColor;

    private bool islose, firstCube, musicOn;

    private List<Vector3> allCUbesPos = new List<Vector3>
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 1),
        new Vector3(-1, 0, -1),
        new Vector3(1, 0, -1),
        new Vector3(-1, 0, 1),
    };

    private Transform mainCam;
    private int prevCountMaxHor;

    private void Start()
    {
        if (PlayerPrefs.GetInt("score") < 5)
            possibleCubesToCreate.Add(cubesToCreate[0]);
        else if (PlayerPrefs.GetInt("score") < 10)
                AddPossibleCubes(2);
        else if (PlayerPrefs.GetInt("score") < 15)
            AddPossibleCubes(3);        
        else if (PlayerPrefs.GetInt("score") < 25) 
            AddPossibleCubes(4);       
        else if (PlayerPrefs.GetInt("score") < 35) 
            AddPossibleCubes(5);        
        else if (PlayerPrefs.GetInt("score") < 50) 
            AddPossibleCubes(6);        
        else if (PlayerPrefs.GetInt("score") < 70) 
            AddPossibleCubes(7);        
        else if (PlayerPrefs.GetInt("score") < 100) 
            AddPossibleCubes(8);        
        else if (PlayerPrefs.GetInt("score") < 150) 
            AddPossibleCubes(9);
        else
            AddPossibleCubes(10);



        toCameraColor = Camera.main.backgroundColor;
        mainCam = Camera.main.transform;
        camMovetoYPosition = 5.9f + nowCube.y - 1f;
        ScoreTxt.text = "<size=60><color=#FF1200>BEST:</color></size> " + PlayerPrefs.GetInt("score") + " \n<size=50>NOW</size> 0";

        showCubePlace = StartCoroutine(ShowCubePlace());
        allCubesRb = allCubes.GetComponent<Rigidbody>();

      
    }

    private void Update()
    {
       


        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && cubeToPlace != null && !EventSystem.current.IsPointerOverGameObject())
        {
#if !UNITY_EDITOR
            if (Input.GetTouch(0).phase != TouchPhase.Began)
                return;
#endif


            if (!firstCube)
            {
                firstCube = true;
                foreach (GameObject obj in CanvasStartPage)
                    Destroy(obj);
            }
            GameObject CreateCube = null;
            if (possibleCubesToCreate.Count == 1)
                CreateCube = possibleCubesToCreate[0];
            else
                CreateCube = possibleCubesToCreate[UnityEngine.Random.Range(0, possibleCubesToCreate.Count)];

            GameObject newCube = Instantiate(
                CreateCube,
                cubeToPlace.position,
                Quaternion.identity) as GameObject;

            newCube.transform.SetParent(allCubes.transform);
            nowCube.SetVector(cubeToPlace.position);
            allCUbesPos.Add(nowCube.GetVector());

            if (PlayerPrefs.GetString("Music") != "No")
                GetComponent<AudioSource>().Play();

            Instantiate(VFX, cubeToPlace.position, Quaternion.identity);

            allCubesRb.isKinematic = true;
            allCubesRb.isKinematic = false;

            SpawnPositions();
            MoveCameraChangeBG();
                    
            }
        if(!islose && allCubesRb.velocity.magnitude > 0.3f)
        {
            Destroy(cubeToPlace.gameObject);
            islose = true;
            StopCoroutine(showCubePlace);
           GameObject.Find("Rotator").gameObject.GetComponent<RotsteCamera>().Rotate=true;

        }
        mainCam.localPosition = Vector3.MoveTowards(mainCam.localPosition,
            new Vector3(mainCam.localPosition.x, camMovetoYPosition, mainCam.localPosition.z),
            CamMoveSpeed * Time.deltaTime);

        if (Camera.main.backgroundColor != toCameraColor)
            Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, toCameraColor, Time.deltaTime / 1.5f);
        
    }
    

    IEnumerator ShowCubePlace()
    {
        while(true)
        {
            SpawnPositions();

            yield return new WaitForSeconds(CubeChangePlaceSpees);
        }
    }

    private void SpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        if(IsPositionEmpty(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z)) 
            && nowCube.x + 1 != cubeToPlace.position.x) 
            positions.Add(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z))
            && nowCube.x - 1 != cubeToPlace.position.x)
            positions.Add(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z))
            && nowCube.y + 1 != cubeToPlace.position.y)
            positions.Add(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z))
            && nowCube.y - 1 != cubeToPlace.position.y)
            positions.Add(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z + 1))
            && nowCube.z + 1 != cubeToPlace.position.z)
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z + 1));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z - 1))
            && nowCube.z - 1 != cubeToPlace.position.z)
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z - 1));


        if (positions.Count > 1)
            cubeToPlace.position = positions[UnityEngine.Random.Range(0, positions.Count)];
        else if (positions.Count == 0)
            islose = true;
        else
            cubeToPlace.position = positions[0];
    }

    private bool IsPositionEmpty(Vector3 targetPos)
    {
        if (targetPos.y == 0)
            return false;



        foreach(Vector3 pos in allCUbesPos)
        {
            allCUbesPos.add
            if (pos.x == targetPos.x && pos.y == targetPos.y && pos.z == targetPos.z)
                return false;
        }
        return true;
    }

    private void MoveCameraChangeBG()
    {
        int MaxX = 0, MaxY = 0, MaxZ = 0, MaxHor;

        foreach(Vector3 pos in allCUbesPos)
        {
            if (Mathf.Abs(Convert.ToInt32(pos.x)) > MaxX)
                MaxX = Convert.ToInt32(pos.x);

            if (Convert.ToInt32(pos.y) > MaxY)
                MaxY= Convert.ToInt32(pos.y);

            if (Mathf.Abs(Convert.ToInt32(pos.z)) > MaxZ)
                MaxZ = Convert.ToInt32(pos.z);
        }
        MaxY--;
        if(PlayerPrefs.GetInt("score") < MaxY)
            PlayerPrefs.SetInt("score", MaxY);

        ScoreTxt.text = "<size=60><color=#FF1200>BEST:</color></size> " + PlayerPrefs.GetInt("score") + " \n<size=50>NOW:</size>" + MaxY;


        camMovetoYPosition = 5.9f + nowCube.y - 1f;


        MaxHor = MaxX > MaxZ ? MaxX : MaxZ;
        if(MaxHor % 3 == 0 && prevCountMaxHor != MaxHor)
        {
            

            mainCam.localPosition -= new Vector3(0, 0, 4f);
            prevCountMaxHor = MaxHor;
        }
        if (MaxY >= 75)
            toCameraColor = bGColor[6];
        if (MaxY >= 50)
            toCameraColor = bGColor[5];
        if (MaxY >= 30)
            toCameraColor = bGColor[4];
        if (MaxY >= 20)
            toCameraColor = bGColor[3];
        if (MaxY >= 10)
            toCameraColor = bGColor[2];
        else if (MaxY >= 5)
            toCameraColor = bGColor[1];
        else if (MaxY >= 2)
            toCameraColor = bGColor[0];
    }

    private void AddPossibleCubes(int till)
    {
        for (int i = 0; i < till; i++)
            possibleCubesToCreate.Add(cubesToCreate[i]);
    }

}
struct CubePos
{
    public int x, y, z;

    public CubePos(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 GetVector()
    {
        return new Vector3(x, y, z);
    }
    public void SetVector(Vector3 pos) {
        x = Convert.ToInt32(pos.x);
        y = Convert.ToInt32(pos.y);
        z = Convert.ToInt32(pos.z);
    } 
}