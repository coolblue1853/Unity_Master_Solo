// Leaf.cs
using UnityEngine;

public class Leaf
{
    public RectInt Area;
    public Leaf Left, Right;
    public RectInt? Room;

    private const int RoomMin = 10;
    private const int Margin = 3;
    private const int MinSize = RoomMin + Margin * 2;
    private const float SplitThreshold = 1.25f;

    public Leaf(RectInt area)
    {
        this.Area = area;
    }

    public bool Split()
    {
        if (Left != null || Right != null) return false;

        bool splitHorizontally = Random.value > 0.5f;
        if (Area.width > Area.height && Area.width / (float)Area.height >= SplitThreshold)
            splitHorizontally = false;
        else if (Area.height > Area.width && Area.height / (float)Area.width >= SplitThreshold)
            splitHorizontally = true;

        int max = (splitHorizontally ? Area.height : Area.width) - MinSize;
        if (max <= MinSize) return false;

        int split = Random.Range(MinSize, max);
        if (splitHorizontally)
        {
            Left = new Leaf(new RectInt(Area.x, Area.y, Area.width, split));
            Right = new Leaf(new RectInt(Area.x, Area.y + split, Area.width, Area.height - split));
        }
        else
        {
            Left = new Leaf(new RectInt(Area.x, Area.y, split, Area.height));
            Right = new Leaf(new RectInt(Area.x + split, Area.y, Area.width - split, Area.height));
        }

        return true;
    }

    public void CreateRooms()
    {
        if (Left != null || Right != null) 
        {
            // 자식 노드에서 재귀적으로 방 생성
            if (Left != null) Left.CreateRooms();
            if (Right != null) Right.CreateRooms();
        }
        else 
        {
            // 리프 노드일 때만 방 생성 로직 실행
            int maxRoomWidth = Area.width - Margin * 2;
            int maxRoomHeight = Area.height - Margin * 2;

            if (maxRoomWidth < RoomMin || maxRoomHeight < RoomMin)
            {
                return;
            }

            int roomWidth = Random.Range(RoomMin, maxRoomWidth + 1);
            int roomHeight = Random.Range(RoomMin, maxRoomHeight + 1);

            int roomX = Area.x + Random.Range(Margin, Area.width - roomWidth - Margin + 1);
            int roomY = Area.y + Random.Range(Margin, Area.height - roomHeight - Margin + 1);

            Room = new RectInt(roomX, roomY, roomWidth, roomHeight);
        }
    }
}