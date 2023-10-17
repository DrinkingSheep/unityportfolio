using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Pathfinding;
public class RoomInfo
{
    public string name;
    public int X;
    public int Y;
    //룸 정보. 이름과 위치. 
}

public class RoomController : MonoBehaviour
{
    public static RoomController instance;
    string currentWorldName = "Basement";

    RoomInfo currentLoadRoomData;

    Room currRoom;

    Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();

    public List<Room> loadedRooms = new List<Room>();

    bool isLoadingRoom = false;

    bool spawnedBossRoom = false;
    bool spawnedShopRoom = false;
    bool spawnedTreasureRoom = false;
    bool spawnedSecretRoom = false;

    bool updatedRooms = false;

    public bool isDungeonRoomInit = false;

    public List<Room> lockableRooms=new List<Room>(); //방 통로가 하나인 곳. 잠글 수 있다.

    AstarData data;


    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        data = AstarPath.active.data;
    }

    void Update()
    {
        UpdateRoomQueue();
    }
    void UpdateRoomQueue()
    {
        if (isLoadingRoom)
        {
            return;
        }
        if (loadRoomQueue.Count==0)
        {
            if (!spawnedBossRoom)
            {
                StartCoroutine(SpawnBossRoom());
            }
            if (!spawnedShopRoom)
            {
                StartCoroutine(SpawnShopRoom());
            }
            if (!spawnedTreasureRoom&& Random.Range(0,100)>50)
            {
                StartCoroutine(SpawnTreasureRoom());                
            }
            else if (spawnedShopRoom&&spawnedBossRoom&&(spawnedTreasureRoom)&&!updatedRooms) 
            {
                foreach (Room room in loadedRooms)
                {
                    room.RemoveUnconnectedDoors();                   
                }

                updatedRooms = true;

            }
            return;
        }        
        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;
        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
    }

    IEnumerator SpawnBossRoom()
    {
        spawnedBossRoom = true;
        yield return new WaitForSeconds(0.5f); //이거 룸 스폰때마다 조금씩 올려줘야 한다...
        if (loadRoomQueue.Count==0)
        {
            Room bossRoom = loadedRooms[loadedRooms.Count - 1];
            Room tempRoom = new Room(bossRoom.X, bossRoom.Y);
            Destroy(bossRoom.gameObject); //원래 empty room이었던거라 파괴.
            var roomToRemove = loadedRooms.Single(r => r.X == tempRoom.X && r.Y == tempRoom.Y);
            loadedRooms.Remove(roomToRemove);
            LoadRoom("End", tempRoom.X, tempRoom.Y);
        }
    }
    
    IEnumerator SpawnShopRoom()
    {
        spawnedShopRoom = true;
        yield return new WaitForSeconds(0.7f);
        if (loadRoomQueue.Count==0)
        {   
            Room shopRoom = loadedRooms[loadedRooms.Count - 2];
            Room tempRoom = new Room(shopRoom.X, shopRoom.Y);
            Destroy(shopRoom.gameObject);

            var roomToRemove = loadedRooms.Single(r => r.X == tempRoom.X && r.Y == tempRoom.Y);
            loadedRooms.Remove(roomToRemove);
            LoadRoom("Shop", tempRoom.X, tempRoom.Y);
        }
    }
    IEnumerator SpawnTreasureRoom()
    {
        spawnedTreasureRoom = true;
        yield return new WaitForSeconds(0.9f);
        if (loadRoomQueue.Count == 0)
        {
            Room treasureRoom = loadedRooms[loadedRooms.Count-4];
            Room tempRoom = new Room(treasureRoom.X, treasureRoom.Y);
            Destroy(treasureRoom.gameObject);

            var roomToRemove = loadedRooms.Single(r => r.X == tempRoom.X && r.Y == tempRoom.Y);
            loadedRooms.Remove(roomToRemove);
            LoadRoom("Treasure", tempRoom.X, tempRoom.Y);
        }
    }
    
    IEnumerator SpawnSecretRoom()
    {
        spawnedSecretRoom = true;
        yield return new WaitForSeconds(1.1f);
        if (loadRoomQueue.Count == 0)
        {
            Room secretRoom = loadedRooms[loadedRooms.Count - 4];
            Room tempRoom = new Room(secretRoom.X, secretRoom.Y);
            Destroy(secretRoom.gameObject);

            var roomToRemove = loadedRooms.Single(r => r.X == tempRoom.X && r.Y == tempRoom.Y);
            loadedRooms.Remove(roomToRemove);
            LoadRoom("Sec", tempRoom.X, tempRoom.Y);
        }
    }


    public void LoadRoom(string name, int x, int y)
    {        
        if (DoesRoomExist(x,y))
        {
            return;
        }
        RoomInfo newRoomData = new RoomInfo();
        newRoomData.name = name;
        newRoomData.X = x;
        newRoomData.Y = y;

        loadRoomQueue.Enqueue(newRoomData);

    }
    
    IEnumerator LoadRoomRoutine(RoomInfo info)
    {
        //print("No5");
        string roomName = currentWorldName + info.name;

        AsyncOperation loadRoom = SceneManager.LoadSceneAsync(roomName,LoadSceneMode.Additive);
        while(loadRoom.isDone==false)
        {
            yield return null;
        }

        isDungeonRoomInit = true;

    }

    public void RegisterRoom(Room room)
    {
        //print("No7");
        if (!DoesRoomExist(currentLoadRoomData.X,currentLoadRoomData.Y)) //해당 위치에 없어야 생성함.
        {
            room.transform.position = new Vector3(currentLoadRoomData.X * room.Width, currentLoadRoomData.Y * room.Height, 0);
        
            room.X = currentLoadRoomData.X;
            room.Y = currentLoadRoomData.Y;
            room.name = currentWorldName + "-" + currentLoadRoomData.name + " " + room.X + "," + room.Y;
            room.transform.parent = transform;

            isLoadingRoom = false;

            if (loadedRooms.Count==0)
            {
                CameraManager.instance.currRoom = room;
            }

            loadedRooms.Add(room);
            //room.RemoveUnconnectedDoors();

            //그래프 깔기.
            //StartCoroutine(UpdateAstarGridGraph(room));

        }
        else
        {
            Destroy(room.gameObject);
            isLoadingRoom = false;
        }

    }

    
    
    IEnumerator UpdateAstarGridGraph(Room room)
    {
        GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
        int width = room.Width;
        int height = room.Height;
        float nodeSize = 1;

        gg.center = room.GetRoomCentre();
        gg.SetDimensions(width, height, nodeSize);
        var graphToScan = AstarPath.active.data.gridGraph;

        AstarPath.active.Scan(graphToScan);
        yield return new WaitForSeconds(0.1f);
    }

    public bool DoesRoomExist(int x, int y)
    {
        return loadedRooms.Find(item=>item.X==x && item.Y==y)!=null;
    }
    public Room FindRoom(int x, int y)
    {
        return loadedRooms.Find(item => item.X == x && item.Y == y);
    }
    public void OnPlayerEnterRoom(Room room)
    {
        CameraManager.instance.currRoom = room;
        currRoom = room;
    }

    // Update is called once per frame
    
}
