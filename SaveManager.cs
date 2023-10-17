using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    #region SingleTon
    public static SaveManager instance { get; private set; }

    private float playTime = 0;

    public int selectedSlotIdx = 0;

    public int recentSlotIdx;

    void Awake()
    {
        
        // 인스턴스가 이미 있는지 확인, 이 상태로 설정
        if (instance == null)
            instance = this;

        // 인스턴스가 이미 있는 경우 오브젝트 제거
        else if (instance != this)
            Destroy(gameObject);

        // 이렇게 하면 다음 scene으로 넘어가도 오브젝트가 사라지지 않습니다.
        DontDestroyOnLoad(gameObject);

        ES3.Init();
        
    }
    #endregion
    [SerializeField]private bool initializeDataIfNull = false;
    [SerializeField]private GameData gameData;

    private List<ISave> saveObjects= new List<ISave>();    

    //유니티 라이프사이클 상, OnSceneLoaded는 OnEnable 다음에 호출된다. 
    //즉, 이벤트를 Start나 OnSceneLoaded 메소드에서 구독하면, 게임 시작 시 처음으로 호출되지 않는다.
    //그러므로 OnEnable에서 구독하고
    //OnDisabled에서 구독을 취소한다. 
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {        
        this.saveObjects = FindAllSaveObjects();
        LoadGame(selectedSlotIdx);
    }
    public void OnSceneUnloaded(Scene scene)
    {        
        SaveGame(selectedSlotIdx); 
    }

    public bool isFirstStart;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; //OnEnable에서 구독(씬 활성화 시)
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; //OnDisable에서 구독해제(씬 비활성화시).
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnApplicationQuit()
    {
        //게임 종료 시, 현재 세이브슬롯에 플레이타임만 저장.
        ES3.Save("totalPlayTime", gameData.totalPlayTime, "savedata_0" + selectedSlotIdx + ".es3");
        //게임 종료 시, log파일에 최근 슬롯idx저장.
    }

    private List<ISave> FindAllSaveObjects()
    {
        IEnumerable<ISave> saveObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISave>();
        return new List<ISave>(saveObjects);
    }

    //save 슬롯은 save파일을 다르게 해서 구현한다.
    public void NewGame()
    {
        this.gameData = new GameData();
        
    }
    
    public void LoadGame(int slotIdx)
    {
        //TODO: Load any saved data from a file using ES3
        //if no data can be loaded, initialize to a new game
        if (ES3.FileExists("savedata_0" + slotIdx + ".es3")) //파일 있으면 로드. 
        {
            gameData.playerHp = (float)ES3.Load("hp", "savedata_0" + slotIdx + ".es3");
            gameData.playerStamina = (float)ES3.Load("sp", "savedata_0" + slotIdx + ".es3");
            gameData.keyCount = (int)ES3.Load("keyCount", "savedata_0" + slotIdx + ".es3");
            gameData.coinCount = (int)ES3.Load("coinCount", "savedata_0" + slotIdx + ".es3");
            gameData.totalPlayTime =(float)ES3.Load("totalPlayTime", "savedata_0" + slotIdx + ".es3");
        }
        

        if (this.gameData==null&&initializeDataIfNull)
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }
        //TODO: push the loaded data to all other scripts that need it.

        //외부에 저장된 데이터를 gameData 컨테이너에 넣음.
                
        foreach (ISave saveObject in saveObjects)
        {
            saveObject.LoadData(gameData); //게임 내 변수에 gameData 컨테이너의 값을 넣음
        }
    }

    public void SaveGame(int slotIdx)
    {
        //TODO: pass the data to other scripts so they can update it
        //TODO: save that data to a file using ES3
        if (this.gameData==null)
        {
            Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
            return;
        }
        foreach (ISave saveObject in saveObjects)
        {
            saveObject.SaveData(ref gameData);
        }
        ES3.Save("hp", gameData.playerHp, "savedata_0" + slotIdx + ".es3");
        ES3.Save("sp", gameData.playerStamina, "savedata_0" + slotIdx + ".es3");
        ES3.Save("keyCount", gameData.keyCount, "savedata_0" + slotIdx + ".es3");
        ES3.Save("coinCount", gameData.coinCount, "savedata_0" + slotIdx + ".es3");
        ES3.Save("totalPlayTime", gameData.totalPlayTime, "savedata_0" + slotIdx + ".es3");
    }
    public bool HasGameData()
    {
        return gameData != null; 
    }
    public void ChangeSelectedIdx(int newIdx)
    {
        this.selectedSlotIdx = newIdx;
        LoadGame(newIdx);
    }

}
