using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

public class MAP
{

    static public int LmapY;
    static public int LmapX;
    static public int total_cell;

    //public static MapStatus[,] Lmap;
    public static Hashtable Lmap_ht;
    public static List<MapStatus> feed_map = new List<MapStatus>(); //餌のセルだけをこのリストに入れる。
    public static List<MapStatus> wall_map = new List<MapStatus>();	//壁のセルだけをこのリストに入れる。
    //private MapStatus ms;

    public MAP() { }
    public MAP(int LmapY, int LmapX)
    {
        MAP.LmapX = LmapX;
        MAP.LmapY = LmapY;
        total_cell = 0;
        //MAP.Lmap = new MapStatus[LmapY,LmapX];
        MAP.Lmap_ht = new Hashtable();
        for (int y = 0; y < LmapY; y++)
        {
            for (int x = 0; x < LmapX; x++)
            {
                //Lmap[y,x] = new MapStatus(x,y,0,false);
                Lmap_ht.Add("X_" + x + ",Y_" + y, new MapStatus(x, y, 0, false));
            }
        }
    }
    /*
    public void setFeed(int y,int x){
        Lmap[y][x].setFeed();
    }
     */
    /*
    public void setWall(int y,int x){
        Lmap[y][x].setWall();
    }
     */
    public void addWallMap(int x, int y, int n)
    {
        //MAP.wall_map.Add(Lmap[y,x]);
        //Lmap[y,x].setWall();
        //Lmap[y,x].addPopulation();
        MapStatus ms;
        ms = (MapStatus)Lmap_ht["X_" + x + ",Y_" + y];
        MAP.wall_map.Add(ms);
        ms.setWall();
        ms.addPopulation();
    }
    public static void addFeedMap(int x, int y, Color c)
    {
        MapStatus ms;
        ms = (MapStatus)Lmap_ht["X_" + x + ",Y_" + y];
        ms.setFeed(c);
        //MAP.feed_map.Add(Lmap[y,x]);
        //Lmap[y,x].setFeed(c);
        MAP.feed_map.Add(ms);
    }
    public static void removeFeedMap(MapStatus remove_ms)
    {
        MAP.feed_map.Remove(remove_ms);
        remove_ms.delFeed();
    }
    public static bool checkFeed(int x, int y)
    {
        MapStatus ms = (MapStatus)Lmap_ht["X_" + x + ",Y_" + y];
        //return (MAP.Lmap[y,x].feed);
        if (ms == null) return (false);
        else return (ms.feed);
    }
    /*
     * ハッシュテーブルから検索して取得する
     */
    public static MapStatus getHashLmap(int x, int y)
    {
        return (MapStatus)MAP.Lmap_ht["X_" + x + ",Y_" + y];
    }
    /*
     * リサイズされた時の処理
     */
    public static void resizeHashLamp(){
        for (int y = 0; y < LmapY; y++)
        {
            for (int x = 0; x < LmapX; x++)
            {
                if (!Lmap_ht.ContainsKey("X_" + x + ",Y_" + y)) Lmap_ht.Add("X_" + x + ",Y_" + y, new MapStatus(x, y, 0, false));
            }
        }
    }
    public static void addTotalCell()
    {
        total_cell++;
    }
    public static void removeTotalCell()
    {
        if (total_cell > 0) total_cell--;
    }
    
	public int arroundCells(int x,int y){
		int yr1 = y-1;
		int xr1 = x-1;
		int ya1 = y+1;
		int xa1 = x+1;
		if(yr1 < 0) yr1 = 0;if(ya1 >= LmapY) ya1 = LmapY-1;
		if(xr1 < 0) xr1 = 0;if(xa1 >= LmapX) xa1 = LmapX-1;
        int xryr = (getHashLmap(xr1, yr1) != null) ? getHashLmap(xr1, yr1).population : 0;
        int xyr = (getHashLmap(x, yr1) != null) ? getHashLmap(x, yr1).population : 0;
        int xayr = (getHashLmap(xa1, yr1) != null) ? getHashLmap(xa1, yr1).population : 0;
        int xry = (getHashLmap(xr1, y) != null) ? getHashLmap(xr1, y).population : 0;
        int xay = (getHashLmap(xa1, y) != null) ? getHashLmap(xa1, y).population : 0;
        int xrya = (getHashLmap(xr1, ya1) != null) ? getHashLmap(xr1, ya1).population : 0;
        int xra = (getHashLmap(x, ya1) != null) ? getHashLmap(x, ya1).population : 0;
        int xaya = (getHashLmap(xa1, ya1) != null) ? getHashLmap(xa1, ya1).population : 0;

        int total = xryr + xyr + xayr + xry + xay + xrya + xra + xaya;

        return (total);
	}
     
}
