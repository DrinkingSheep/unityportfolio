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
            Debug.Log("�߸��� ������ ����� �������ϴ�."); //�� ��Ʈ�ѷ� �ν��Ͻ� ������ �ȵ� ������ ����...
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
        RoomController.instance.RegisterRoom(this); //�ش� �� ����ϱ�.

        
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

    private void Update() //���� ��Ʈ �ݵ�� ������� ��.
    {
        if (name.Contains("Sec") || name.Contains("Treasure") || name.Contains("Shop") || name.Contains("End") && !updatedDoors) //�ش� ���� �������̸鼭 ���ŵ� ���� �ƴϸ� �����ϱ�.
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
            if ((!HasEnemy() || !HasPlayer()) && !isOpenRoom) //���� ���ų� �÷��̾ ���� �� && ���� ���������϶�.
            {
                StartCoroutine(SetDoorOpenRoutine());
                //print("Door Open End");
            }
            else if (HasEnemy() && HasPlayer() && isOpenRoom) //�� �ְ� �÷��̾ �����鼭 ���� ���������϶�.
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
        Door[] doors = GetComponentsInChildren<Door>(); //�� ������
                                                        
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
        Door[] doors = GetComponentsInChildren<Door>(); //�� ������
                                                        //���� �����ϸ� ��� �� ���� ����.
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
        //���濡 ��¦�� �ϳ��� ������ Lockable�ΰŷ�...
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
        // �� ���� ������ ī�޶��ũ �ٲ�� ���� ����...

        if (collision.tag == "Player")
        {            
            RoomController.instance.OnPlayerEnterRoom(this); //���� �� �������� ī�޶� �ٲ�.
            //��������κ��� �Դ��� �Ǵ��ؼ� �������� ����.
            Vector3 currPlayerPos = collision.gameObject.transform.position;
            Vector3 roomCenterPos = GetRoomCentre();
            if (currPlayerPos.x < roomCenterPos.x + 2 && currPlayerPos.x > roomCenterPos.x - 2 && currPlayerPos.y < roomCenterPos.y && currPlayerPos.y > roomCenterPos.y - 7)
            {
                //�Ʒ��κ���
                roomCenterPos.y -= 2.5f;
                collision.gameObject.transform.position = roomCenterPos;
            }
            else if (currPlayerPos.x < roomCenterPos.x + 2 && currPlayerPos.x > roomCenterPos.x - 2 && currPlayerPos.y > roomCenterPos.y && currPlayerPos.y < roomCenterPos.y + 7)
            {
                //���κ���
                roomCenterPos.y += 2.5f;
                collision.gameObject.transform.position = roomCenterPos;
            }
            else if (currPlayerPos.x > roomCenterPos.x && currPlayerPos.x < roomCenterPos.x + 9 && currPlayerPos.y > roomCenterPos.y - 2 && currPlayerPos.y < roomCenterPos.y + 2)
            {
                //���������κ���
                roomCenterPos.x += 6;
                collision.gameObject.transform.position = roomCenterPos;
            }
            else if (currPlayerPos.x < roomCenterPos.x && currPlayerPos.x > roomCenterPos.x - 9 && currPlayerPos.y > roomCenterPos.y - 2 && currPlayerPos.y < roomCenterPos.y + 2)
            {
                //�������κ���
                roomCenterPos.x -= 5;
                collision.gameObject.transform.position = roomCenterPos;
            }
            else Debug.Log("room enter bug");

            StartCoroutine(ScanGridGraph());
            //�÷��̾ �뿡 �����ϸ� �׸��� ������Ʈ�ؼ� ai�۵���Ų��.

            //������ ���� ��������Ʈ ����
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
