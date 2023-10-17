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
        
        // �ν��Ͻ��� �̹� �ִ��� Ȯ��, �� ���·� ����
        if (instance == null)
            instance = this;

        // �ν��Ͻ��� �̹� �ִ� ��� ������Ʈ ����
        else if (instance != this)
            Destroy(gameObject);

        // �̷��� �ϸ� ���� scene���� �Ѿ�� ������Ʈ�� ������� �ʽ��ϴ�.
        DontDestroyOnLoad(gameObject);

        ES3.Init();
        
    }
    #endregion
    [SerializeField]private bool initializeDataIfNull = false;
    [SerializeField]private GameData gameData;

    private List<ISave> saveObjects= new List<ISave>();    

    //����Ƽ ����������Ŭ ��, OnSceneLoaded�� OnEnable ������ ȣ��ȴ�. 
    //��, �̺�Ʈ�� Start�� OnSceneLoaded �޼ҵ忡�� �����ϸ�, ���� ���� �� ó������ ȣ����� �ʴ´�.
    //�׷��Ƿ� OnEnable���� �����ϰ�
    //OnDisabled���� ������ ����Ѵ�. 
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
        SceneManager.sceneLoaded += OnSceneLoaded; //OnEnable���� ����(�� Ȱ��ȭ ��)
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; //OnDisable���� ��������(�� ��Ȱ��ȭ��).
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnApplicationQuit()
    {
        //���� ���� ��, ���� ���̺꽽�Կ� �÷���Ÿ�Ӹ� ����.
        ES3.Save("totalPlayTime", gameData.totalPlayTime, "savedata_0" + selectedSlotIdx + ".es3");
        //���� ���� ��, log���Ͽ� �ֱ� ����idx����.
    }

    private List<ISave> FindAllSaveObjects()
    {
        IEnumerable<ISave> saveObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISave>();
        return new List<ISave>(saveObjects);
    }

    //save ������ save������ �ٸ��� �ؼ� �����Ѵ�.
    public void NewGame()
    {
        this.gameData = new GameData();
        
    }
    
    public void LoadGame(int slotIdx)
    {
        //TODO: Load any saved data from a file using ES3
        //if no data can be loaded, initialize to a new game
        if (ES3.FileExists("savedata_0" + slotIdx + ".es3")) //���� ������ �ε�. 
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

        //�ܺο� ����� �����͸� gameData �����̳ʿ� ����.
                
        foreach (ISave saveObject in saveObjects)
        {
            saveObject.LoadData(gameData); //���� �� ������ gameData �����̳��� ���� ����
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
