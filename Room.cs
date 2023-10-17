using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int Width;
    public int Height;
    public int X;
    public int Y;

    private bool updatedDoors = false;
        
    private SpriteRenderer minimapIconRender;

    public Room(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Door leftDoor;
    public Door rightDoor;
    public Door topDoor;
    public Door bottomDoor;

    public List<Door> doors = new List<Door>();

    public bool objBroken;

    [System.Obsolete]
    void Start()
    {
        if (RoomController.instance == null)
        {
            Debug.Log("잘못된 씬에서 재생을 눌렀습니다."); //룸 컨트롤러 인스턴스 생성이 안된 씬에서 누름...
            return;
        }

        Door[] ds = GetComponentsInChildren<Door>();
        foreach (Door d in ds)
        {
            doors.Add(d);
            switch (d.doorType)
            {
                case Door.DoorType.Left:
                    leftDoor = d;
                    break;
                case Door.DoorType.Right:
                    rightDoor = d;
                    break;
                case Door.DoorType.Top:
                    topDoor = d;
                    break;
                case Door.DoorType.Bottom:
                    bottomDoor = d;
                    break;
                default:
                    break;
            }
        }
        //print("No6");
        RoomController.instance.RegisterRoom(this); //해당 방 등록하기.

        
        GameObject go = transform.Find("MinimapIcon").gameObject;
        if (go!=null) minimapIconRender = go.GetComponent<SpriteRenderer>();

    }




    public bool IsLockableRoom()
    {
        if (GetLeft() != null && GetRight() == null && GetTop() == null && GetBottom() == null) return true;
        else if (GetLeft() == null && GetRight() != null && GetTop() == null && GetBottom() == null) return true;
        else if (GetLeft() == null && GetRight() == null && GetTop() != null && GetBottom() == null) return true;
        else if (GetLeft() == null && GetRight() == null && GetTop() == null && GetBottom() != null) return true;
        else return false;
    }

    private void Update() //갱신 파트 반드시 고쳐줘야 함.
    {
        if (name.Contains("Sec") || name.Contains("Treasure") || name.Contains("Shop") || name.Contains("End") && !updatedDoors) //해당 문이 보스방이면서 갱신된 문이 아니면 갱신하기.
        {
            RemoveUnconnectedDoors();
            if (IsLockableRoom())
            {
                RoomController.instance.lockableRooms.Add(this);
            }

            updatedDoors = true;
        }
        

        if (RoomController.instance.isDungeonRoomInit)
        {
            if ((!HasEnemy() || !HasPlayer()) && !isOpenRoom) //적이 없거나 플레이어가 없는 방 && 룸이 닫힌상태일때.
            {
                StartCoroutine(SetDoorOpenRoutine());
                //print("Door Open End");
            }
            else if (HasEnemy() && HasPlayer() && isOpenRoom) //적 있고 플레이어도 있으면서 룸이 열린상태일때.
            {
                StartCoroutine(SetDoorCloseRoutine());
                //print("Door Close End");
            }
        }

        if (objBroken)
        {
            StartCoroutine(ScanGridGraph());
        }


    }    
    bool isOpenRoom;
    IEnumerator SetDoorOpenRoutine()
    {
        isOpenRoom = true;
        yield return new WaitForSeconds(1f);
        Door[] doors = GetComponentsInChildren<Door>(); //문 가져옴
                                                        
        foreach (Door door in doors)
        {
            if (door.isActiveAndEnabled)
            {
                door.SetDoor(Door.Status.Open);            
                
            }
        }
    }
    IEnumerator SetDoorCloseRoutine()
    {
        isOpenRoom = false;
        yield return new WaitForSeconds(1f);
        Door[] doors = GetComponentsInChildren<Door>(); //문 가져옴
                                                        //문이 존재하면 잠긴 문 빼고 열기.
        foreach (Door door in doors)
        {
            if (door.isActiveAndEnabled)
            {
              door.SetDoor(Door.Status.Close);                     
            }
        }      
    }
    public void SetLockableDoor()
    {
        //옆방에 문짝이 하나만 있으면 Lockable인거로...
        Room leftRoom = GetLeft();
        Room rightRoom = GetRight();
        Room bottomRoom = GetBottom();
        Room topRoom = GetTop();
        if (leftRoom!=null)
        {
            if (!leftRoom.topDoor.isActiveAndEnabled && !leftRoom.bottomDoor.isActiveAndEnabled && !leftRoom.leftDoor.isActiveAndEnabled)
                this.leftDoor.SetDoor(Door.Status.Lock);            
        }
        if (rightRoom!=null)
        {
            if (!rightRoom.topDoor.isActiveAndEnabled && !rightRoom.bottomDoor.isActiveAndEnabled && !rightRoom.rightDoor.isActiveAndEnabled)
                this.rightDoor.SetDoor(Door.Status.Lock);
        }
        if (topRoom != null)
        {
            if (!topRoom.topDoor.isActiveAndEnabled && !topRoom.leftDoor.isActiveAndEnabled && !topRoom.rightDoor.isActiveAndEnabled)
                this.topDoor.SetDoor(Door.Status.Lock);
        }
        if (bottomRoom != null)
        {
            if (!bottomRoom.bottomDoor.isActiveAndEnabled && !bottomRoom.leftDoor.isActiveAndEnabled && !bottomRoom.rightDoor.isActiveAndEnabled)
                this.bottomDoor.SetDoor(Door.Status.Lock);
        }
    }

    public void RemoveUnconnectedDoors()
    {
        //print("No9");
        foreach (Door door in doors)
        {
            switch (door.doorType)
            {
                case Door.DoorType.Left:
                    if (GetLeft() == null)
                    {
                        door.gameObject.SetActive(false);
                        transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        door.gameObject.SetActive(true);
                        transform.GetChild(0).gameObject.SetActive(false);
                    }
                    break;
                case Door.DoorType.Right:
                    if (GetRight() == null)
                    {
                        door.gameObject.SetActive(false);
                        transform.GetChild(1).gameObject.SetActive(true);
                    }
                    else 
                    { 
                        door.gameObject.SetActive(true);
                        transform.GetChild(1).gameObject.SetActive(false);
                    }
                    break;
                case Door.DoorType.Top:
                    if (GetTop() == null)
                    {
                        door.gameObject.SetActive(false);
                        transform.GetChild(2).gameObject.SetActive(true);
                    }
                    else
                    {
                        door.gameObject.SetActive(true);
                        transform.GetChild(2).gameObject.SetActive(false);
                    }
                    break;
                case Door.DoorType.Bottom:
                    if (GetBottom() == null)
                    {
                        door.gameObject.SetActive(false);
                        transform.GetChild(3).gameObject.SetActive(true);
                    }
                    else
                    {
                        door.gameObject.SetActive(true);
                        transform.GetChild(3).gameObject.SetActive(false);
                    }
                    break;
                default:
                    break;
            }
        }

    }
    public Room GetRight()
    {
        if (RoomController.instance.DoesRoomExist(X + 1, Y)) return RoomController.instance.FindRoom(X + 1, Y);
        return null;
    }
    public Room GetLeft()
    {
        if (RoomController.instance.DoesRoomExist(X - 1, Y)) return RoomController.instance.FindRoom(X - 1, Y);
        return null;
    }
    public Room GetTop()
    {
        if (RoomController.instance.DoesRoomExist(X, Y + 1)) return RoomController.instance.FindRoom(X, Y + 1);
        return null;
    }
    public Room GetBottom()
    {
        if (RoomController.instance.DoesRoomExist(X, Y - 1)) return RoomController.instance.FindRoom(X, Y - 1);
        return null;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(Width, Height, 0));
    }
    public Vector3 GetRoomCentre()
    {
        return new Vector3(X * Width, Y * Height);
    }
    // Update is called once per frame



    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 룸 진입 기준은 카메라워크 바뀌는 것이 기준...

        if (collision.tag == "Player")
        {            
            RoomController.instance.OnPlayerEnterRoom(this); //닿은 문 기준으로 카메라 바뀜.
            //어느쪽으로부터 왔는지 판단해서 스폰지점 잡자.
            Vector3 currPlayerPos = collision.gameObject.transform.position;
            Vector3 roomCenterPos = GetRoomCentre();
            if (currPlayerPos.x < roomCenterPos.x + 2 && currPlayerPos.x > roomCenterPos.x - 2 && currPlayerPos.y < roomCenterPos.y && currPlayerPos.y > roomCenterPos.y - 7)
            {
                //아래로부터
                roomCenterPos.y -= 2.5f;
                collision.gameObject.transform.position = roomCenterPos;
            }
            else if (currPlayerPos.x < roomCenterPos.x + 2 && currPlayerPos.x > roomCenterPos.x - 2 && currPlayerPos.y > roomCenterPos.y && currPlayerPos.y < roomCenterPos.y + 7)
            {
                //위로부터
                roomCenterPos.y += 2.5f;
                collision.gameObject.transform.position = roomCenterPos;
            }
            else if (currPlayerPos.x > roomCenterPos.x && currPlayerPos.x < roomCenterPos.x + 9 && currPlayerPos.y > roomCenterPos.y - 2 && currPlayerPos.y < roomCenterPos.y + 2)
            {
                //오른쪽으로부터
                roomCenterPos.x += 6;
                collision.gameObject.transform.position = roomCenterPos;
            }
            else if (currPlayerPos.x < roomCenterPos.x && currPlayerPos.x > roomCenterPos.x - 9 && currPlayerPos.y > roomCenterPos.y - 2 && currPlayerPos.y < roomCenterPos.y + 2)
            {
                //왼쪽으로부터
                roomCenterPos.x -= 5;
                collision.gameObject.transform.position = roomCenterPos;
            }
            else Debug.Log("room enter bug");

            StartCoroutine(ScanGridGraph());
            //플레이어가 룸에 진입하면 그리드 업데이트해서 ai작동시킨다.

            //입장한 방의 스프라이트 변경
            if(minimapIconRender!=null) minimapIconRender.color = Color.yellow;

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (minimapIconRender != null) minimapIconRender.color = Color.white;
        }
    }

    public bool HasPlayer()
    {
        Collider2D player = Physics2D.OverlapBox(transform.position, new Vector2(Width, Height), 0, 1 << 8);
        if (player == null) return false;
        else return true;
    }
    public bool HasEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapBoxAll(transform.position, new Vector2(Width, Height), 0, 1 << 9);
        if (enemies.Length == 0) return false;
        else return true;
    }

    private IEnumerator ScanGridGraph()
    {
        objBroken = false;
        yield return null;
        AstarPath.active.data.gridGraph.center = this.GetRoomCentre();
        AstarPath.active.Scan();
    }
}
